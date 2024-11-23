using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace delayedpolufabrikkatbot.Service
{
    public class PostForwardService : IPostForwardService
    {
        const long moderationChatId = 7477125681;
		private readonly IPostReviewSessionService _postReviewSessionService;

		public PostForwardService(IPostReviewSessionService postReviewSessionService)
        {
			_postReviewSessionService = postReviewSessionService;
		}

        public async void ForwardPostToModeration(Update update, ITelegramBotClient botClient, IUserRepository userRepository, IPostSubmitionRepository postSubmitionRepository, Models.PostCreationSession session)
        {
            // Создаем новый пост и сохраняем его в базе
            var userId = update.Message.From.Id;
            var user = await userRepository.GetUserByTelegramId(userId);

			var inlineKeyboard = GetReviewButtonsAndSetupSessions(session.PostId.Value, userId);

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

            if (_postReviewSessionService.TryGetReviewSessionItem(callbackData, out var session))
            {
				if(session.PublicationResolution == PublicationResolution.Approved)
				{
					await postSubmitionRepository.UpdatePostReputation(session.PostId, session.Reputation);

					await userRepository.AddReputationToUser(session.TelegramUserId, session.Reputation);

					await botClient.AnswerCallbackQuery(
						callbackQueryId: callbackQuery.Id,
						text: $"Репутация +{session.Reputation} добавлена пользователю {session.TelegramUserId}."
					);

					await botClient.EditMessageReplyMarkup(
						chatId: callbackQuery.Message.Chat.Id,
						messageId: callbackQuery.Message.MessageId,
						replyMarkup: null
					);
				}
				if(session.PublicationResolution == PublicationResolution.Ignored)
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

				await botClient.AnswerCallbackQuery(
					callbackQueryId: callbackQuery.Id,
					text: "Ошибка: некорректный идентификатор поста.",
					showAlert: true
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

		private InlineKeyboardMarkup GetReviewButtonsAndSetupSessions(ObjectId postId, long telegramUserId)
		{
			var reputation10Guid = Guid.NewGuid().ToString();
			_postReviewSessionService.AddReviewSessionItem(reputation10Guid, new ReviewPublicationSession
			{
				PostId = postId,
				TelegramUserId = telegramUserId,
				PublicationResolution = PublicationResolution.Approved,
				Reputation = 10
			});
			var reputation20Guid = Guid.NewGuid().ToString();
			_postReviewSessionService.AddReviewSessionItem(reputation20Guid, new ReviewPublicationSession
			{
				PostId = postId,
				TelegramUserId = telegramUserId,
				PublicationResolution = PublicationResolution.Approved,
				Reputation = 20
			});
			var reputation50Guid = Guid.NewGuid().ToString();
			_postReviewSessionService.AddReviewSessionItem(reputation50Guid, new ReviewPublicationSession
			{
				PostId = postId,
				TelegramUserId = telegramUserId,
				PublicationResolution = PublicationResolution.Approved,
				Reputation = 50
			});
			var ignoreGuid = Guid.NewGuid().ToString();
			_postReviewSessionService.AddReviewSessionItem(reputation50Guid, new ReviewPublicationSession
			{
				PostId = postId,
				TelegramUserId = telegramUserId,
				PublicationResolution = PublicationResolution.Ignored,
				Reputation = 0
			});

			var inlineKeyboard = new InlineKeyboardMarkup(new[] {
			new[]
			{
				InlineKeyboardButton.WithCallbackData("10 очков", reputation10Guid),
				InlineKeyboardButton.WithCallbackData("20 очков", reputation20Guid),
				InlineKeyboardButton.WithCallbackData("50 очков", reputation50Guid)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("Игнорировать", ignoreGuid)
			}
			});

			return inlineKeyboard;
		}
    }
}

