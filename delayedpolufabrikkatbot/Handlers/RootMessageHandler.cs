using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models.Sessions;
using delayedpolufabrikkatbot.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace delayedpolufabrikkatbot.Handlers
{
    public class RootMessageHandler : IRootMessageHandler
    {
        private const string mainInfo = "Информация";
        private const string newPost = "Создать пост";
        private const string profile = "Профиль";
        private const string shop = "Магазин";

        private readonly IUserRepository _userRepository;
        private readonly IPostSubmitionRepository _postSubmitionRepository;
        private readonly IAdminChannelService _adminChannelService;
        private readonly ICacheManager _cacheManager;
		private readonly ISessionHandler _sessionMessageHandler;

		public RootMessageHandler(IUserRepository userRepository, IPostSubmitionRepository postSubmitionRepository, IAdminChannelService adminChannelService, ICacheManager cacheManager, ISessionHandler sessionMessageHandler)
        {
            _userRepository = userRepository;
            _postSubmitionRepository = postSubmitionRepository;
            _adminChannelService = adminChannelService;
            _cacheManager = cacheManager;
			_sessionMessageHandler = sessionMessageHandler;
		}

        public async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var userId = update.Message.From.Id;
            // Регистрация нового пользователя
            await _userRepository.AddIfNotExist(userId);
            var user = await _userRepository.GetUserByTelegramId(userId);

            if (_cacheManager.TryGet(user.Id, out BaseSession session))
            {
				await _sessionMessageHandler.HandleSessionMessage(botClient, update, session);
				return;
            }

			Content content = new Content();
			var text = update.Message.Text;
			switch (text)
            {
                case mainInfo:
                    await botClient.SendMessage(update.Message.Chat.Id, content.GetInfo(), replyMarkup: GetRootButtons());
                    break;
                case newPost:
                    _cacheManager.Add(user.Id, new PostCreationSession
                    {
						UserId = user.Id,
						Key = user.Id,
						CurrentStep = PostCreationStep.WaitingForTitle
                    });
                    await botClient.SendMessage(update.Message.Chat.Id, "Напишите название вашей публикации. Это название нужно для того, чтобы вы смогли в будущем отслеживать её статус.");
                    break;
                case profile:
                    var userPosts = await _postSubmitionRepository.GetLastPostsByUserId(user.Id, 10);
                    await botClient.SendMessage(update.Message.Chat.Id, content.GetProfileInfo(user, userPosts), replyMarkup: GetRootButtons());
                    break;
                case shop:
                    await botClient.SendMessage(update.Message.Chat.Id, "Shop", replyMarkup: GetRootButtons());
                    break;
                default:
                    await botClient.SendMessage(update.Message.Chat.Id, "Используйте существующие команды", replyMarkup: GetRootButtons());
                    break;
            }
        }

        public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var callbackQuery = update.CallbackQuery;

            if (_cacheManager.TryGet(callbackQuery.Data, out ReviewPublicationSession session))
            {
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
            }
            else
            {
                await botClient.AnswerCallbackQuery(
                    callbackQueryId: callbackQuery.Id,
                    text: "Ошибка: некорректный идентификатор поста.",
                    showAlert: true
                );
            }

            _cacheManager.Remove(callbackQuery.Data);
        }

        private IReplyMarkup GetRootButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton(mainInfo), new KeyboardButton(newPost) },
                    new List<KeyboardButton> { new KeyboardButton(profile), new KeyboardButton(shop) }
                }
            };
        }
    }

}