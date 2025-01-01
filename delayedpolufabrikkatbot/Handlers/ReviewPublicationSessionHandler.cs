using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models.Sessions;
using delayedpolufabrikkatbot.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Handlers
{
	public class ReviewPublicationSessionHandler : BaseSessionHandler<ReviewPublicationSession>
	{
		private readonly IPostSubmitionRepository _postSubmitionRepository;
		private readonly IUserRepository _userRepository;

		public ReviewPublicationSessionHandler(ICacheManager cacheManager, IPostSubmitionRepository postSubmitionRepository, IUserRepository userRepository) : base(cacheManager)
		{
			_postSubmitionRepository = postSubmitionRepository;
			_userRepository = userRepository;
		}

		protected override async Task<bool> ProcessSessionAndReturnIsSessionFinished(ITelegramBotClient botClient, Update update, ReviewPublicationSession session)
		{
			var callbackQuery = update.CallbackQuery;

			switch (session.PublicationResolution)
			{
				case PublicationResolution.Approved:
					await _postSubmitionRepository.UpdatePostReputation(session.PostId, session.Reputation);
					await _userRepository.AddReputationToUser(session.TelegramUserId, session.Reputation);
					await botClient.AnswerCallbackQuery(
						callbackQueryId: callbackQuery.Id,
						text: $"Репутация +{session.Reputation} добавлена пользователю {session.TelegramUserId}."
					);
					await botClient.EditMessageReplyMarkup(
						chatId: callbackQuery.Message.Chat.Id,
						messageId: callbackQuery.Message.MessageId,
						replyMarkup: null
					);
					break;
				case PublicationResolution.Ignored:
					await botClient.DeleteMessage(
						chatId: callbackQuery.Message.Chat.Id,
						messageId: callbackQuery.Message.MessageId
					);
					await botClient.AnswerCallbackQuery(
						callbackQueryId: callbackQuery.Id,
						text: "Сообщение удалено, репутация не добавлена."
					);
					break;
				default:
					await botClient.AnswerCallbackQuery(
					callbackQueryId: callbackQuery.Id,
					text: "Ошибка: некорректный идентификатор поста.",
					showAlert: true
					);
					break;
			}

			return true;
		}
	}
}
