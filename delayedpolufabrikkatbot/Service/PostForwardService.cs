using delayedpolufabrikkatbot.Interfaces;
using MongoDB.Bson;
using System.Text.RegularExpressions;
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

        public async void ForwardPostToModeration(Update update, ITelegramBotClient botClient, IUserRepository userRepository, IPostSubmitionRepository postSubmitionRepository, Models.PostCreationSession session)
        {
            // Создаем новый пост и сохраняем его в базе
            var userId = update.Message.From.Id;
            var user = await userRepository.GetUserByTelegramId(userId);

            // Генерация CallbackData с ObjectId поста
            var inlineKeyboard = new InlineKeyboardMarkup(new[] {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("10 очков", $"add_reputation:{user.Id}:{session.PostId}:10"),
                InlineKeyboardButton.WithCallbackData("20 очков", $"add_reputation:{user.Id}:{session.PostId}:20"),
                InlineKeyboardButton.WithCallbackData("50 очков", $"add_reputation:{user.Id}:{session.PostId}:50")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Игнорировать", $"ignore_post:{session.PostId}")
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
                var regex = new Regex("add_reputation:(?<userId>\\w+):(?<postId>\\w+):(?<reputation>\\d+)");
                var match = regex.Match(callbackData);

                var userId = match.Groups["userId"].Value;
                var postId = match.Groups["postId"].Value;
                var reputation = match.Groups["reputation"].Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(reputation))
                {
                    await botClient.AnswerCallbackQuery(
                                callbackQueryId: callbackQuery.Id,
                                text: "Ошибка: некорректный идентификатор поста.",
                                showAlert: true
                            );
                    return;
                }

                await postSubmitionRepository.UpdatePostReputation(ObjectId.Parse(postId), int.Parse(reputation));

                await userRepository.AddReputationToUser(long.Parse(userId), int.Parse(reputation));

                await botClient.AnswerCallbackQuery(
                    callbackQueryId: callbackQuery.Id,
                    text: $"Репутация +{reputation} добавлена пользователю {userId}."
                );

                await botClient.EditMessageReplyMarkup(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    replyMarkup: null
                );
            }
            else if (callbackData.StartsWith("ignore_post:"))
            {
                await botClient.DeleteMessage(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId
                    );

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

