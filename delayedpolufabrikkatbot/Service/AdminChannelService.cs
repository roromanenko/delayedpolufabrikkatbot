using delayedpolufabrikkatbot.Handlers;
using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace delayedpolufabrikkatbot.Service
{
    public class AdminChannelService : IAdminChannelService
    {
        const long moderationChatId = 7477125681;
		private readonly IUserRepository _userRepository;
		private readonly ICacheManager _cacheManager;

		public AdminChannelService(IUserRepository userRepository, ICacheManager cacheManager)
        {
			_userRepository = userRepository;
			_cacheManager = cacheManager;
		}

        public async Task ForwardPostToModeration(Update update, ITelegramBotClient botClient, PostCreationSession session)
        {
            var userId = update.Message.From.Id;
            var user = await _userRepository.GetUserByTelegramId(userId);

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

		private InlineKeyboardMarkup GetReviewButtonsAndSetupSessions(ObjectId postId, long telegramUserId)
		{
			var reputation10Guid = Guid.NewGuid().ToString();
			_cacheManager.Add(reputation10Guid, new ReviewPublicationSession
			{
				Key = reputation10Guid,
				PostId = postId,
				TelegramUserId = telegramUserId,
				PublicationResolution = PublicationResolution.Approved,
				Reputation = 10
			});
			var reputation20Guid = Guid.NewGuid().ToString();
			_cacheManager.Add(reputation20Guid, new ReviewPublicationSession
			{
				Key = reputation20Guid,
				PostId = postId,
				TelegramUserId = telegramUserId,
				PublicationResolution = PublicationResolution.Approved,
				Reputation = 20
			});
			var reputation50Guid = Guid.NewGuid().ToString();
			_cacheManager.Add(reputation50Guid, new ReviewPublicationSession
			{
				Key = reputation50Guid,
				PostId = postId,
				TelegramUserId = telegramUserId,
				PublicationResolution = PublicationResolution.Approved,
				Reputation = 50
			});
			var ignoreGuid = Guid.NewGuid().ToString();
			_cacheManager.Add(ignoreGuid, new ReviewPublicationSession
			{
				Key = ignoreGuid,
				PostId = postId,
				TelegramUserId = telegramUserId,
				PublicationResolution = PublicationResolution.Ignored,
				Reputation = 0
			});

			var inlineKeyboard = new InlineKeyboardMarkup([
				[
					InlineKeyboardButton.WithCallbackData("10 очков", reputation10Guid),
					InlineKeyboardButton.WithCallbackData("20 очков", reputation20Guid),
					InlineKeyboardButton.WithCallbackData("50 очков", reputation50Guid)
				],
				[
					InlineKeyboardButton.WithCallbackData("Игнорировать", ignoreGuid)
				]
			]);

			return inlineKeyboard;
		}
	}
}

