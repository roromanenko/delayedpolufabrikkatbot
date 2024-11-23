using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using MongoDB.Bson;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace delayedpolufabrikkatbot.Service
{
    public class PostForwardService : IPostForwardService
    {
        const long moderationChatId = 7477125681;

        public PostForwardService()
        {

        }

        public async void ForwardPostToModeration(Update update, ITelegramBotClient botClient, IUserRepository userRepository, IPostSubmitionRepository postSubmitionRepository)
        {
            // Создаем новый пост и сохраняем его в базе
            var userId = update.Message.From.Id;
            var user = await userRepository.GetUserByTelegramId(userId);
            var post = new PostSubmition
            {
                UserId = user.Id,
                PostTitle = update.Message.Text ?? "Без названия",
                Date = DateTime.UtcNow
            };

            await postSubmitionRepository.UpdaterPostTitleAndUserID(user.Id, post.PostTitle);

            // Генерация CallbackData с ObjectId поста
            var inlineKeyboard = new InlineKeyboardMarkup(new[] {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("10 очков", $"add_reputation:{user.Id}:10"),
                InlineKeyboardButton.WithCallbackData("20 очков", $"add_reputation:{user.Id}:20"),
                InlineKeyboardButton.WithCallbackData("50 очков", $"add_reputation:{user.Id}:50")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Игнорировать", $"ignore_post:{post.Id}")
            }
            });


            if (update.Message.Text != null)
            {
                // Text
                await botClient.SendMessage(chatId: moderationChatId, text: $"Новая публикация от пользователя {update.Message.From.Id}:\n\n{update.Message.Text}", replyMarkup: inlineKeyboard);
            }
            else if (update.Message.Photo != null)
            {
                // Photo
                var fileId = update.Message.Photo.Last().FileId;
                var caption = update.Message.Caption ?? "Без подписи";

                await botClient.SendPhoto(chatId: moderationChatId, photo: fileId, caption: $"{caption}\n\nНовая публикация от пользователя {update.Message.From.Id}", replyMarkup: inlineKeyboard);
            }
            else if (update.Message.Video != null)
            {
                // Video
                var fileId = update.Message.Video.FileId;
                await botClient.SendVideo(chatId: moderationChatId, video: fileId, caption: $"Новая публикация от пользователя {update.Message.From.Id}", replyMarkup: inlineKeyboard);
            }
            else if (update.Message.Audio != null)
            {
                // Audio
                var fileId = update.Message.Audio.FileId;
                await botClient.SendAudio(chatId: moderationChatId, audio: fileId, caption: $"Новая публикация от пользователя {update.Message.From.Id}", replyMarkup: inlineKeyboard);
            }
        }

        public async Task HandleCallbackQuery(CallbackQuery callbackQuery, ITelegramBotClient botClient, IUserRepository userRepository, IPostSubmitionRepository postSubmitionRepository)
        {
            var callbackData = callbackQuery.Data;

            if (callbackData.StartsWith("add_reputation:"))
            {
                var dataParts = callbackData.Split(':');
                if (dataParts.Length == 4)
                {
                    var userId = long.Parse(dataParts[1]);
                    if (ObjectId.TryParse(dataParts[2], out var postId))
                    {
                        var reputationPoints = int.Parse(dataParts[3]);

                        // Обновляем репутацию поста
                        await postSubmitionRepository.UpdatePostReputation(postId, reputationPoints);

                        // Обновляем репутацию пользователя
                        await userRepository.AddReputationToUser(userId, reputationPoints);

                        // Уведомляем модератора об успешном добавлении репутации
                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            text: $"Репутация +{reputationPoints} добавлена пользователю {userId}."
                        );

                        // Убираем кнопки из сообщения в чате модерации
                        await botClient.EditMessageReplyMarkup(
                            chatId: callbackQuery.Message.Chat.Id,
                            messageId: callbackQuery.Message.MessageId,
                            replyMarkup: null
                        );
                    }
                    else
                    {
                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            text: "Ошибка: некорректный идентификатор поста.",
                            showAlert: true
                        );
                    }
                }
            }
            else if (callbackData.StartsWith("ignore_post:"))
            {
                var dataParts = callbackData.Split(':');
                if (dataParts.Length == 2 && ObjectId.TryParse(dataParts[1], out var postId))
                {
                    // Удаляем сообщение из чата модерации
                    await botClient.DeleteMessage(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId
                    );

                    // Уведомляем модератора
                    await botClient.AnswerCallbackQuery(
                        callbackQueryId: callbackQuery.Id,
                        text: "Сообщение удалено, репутация не добавлена."
                    );
                }
                else
                {
                    await botClient.AnswerCallbackQuery(
                        callbackQueryId: callbackQuery.Id,
                        text: "Ошибка: некорректный идентификатор поста.",
                        showAlert: true
                    );
                }
            }
        }
    }
}

