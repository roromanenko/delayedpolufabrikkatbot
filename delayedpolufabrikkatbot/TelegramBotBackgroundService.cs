using delayedpolufabrikkatbot.Service;
using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using delayedpolufabrikkatbot.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace delayedpolufabrikkatbot
{
    public class TelegramBotBackgroundService : BackgroundService
    {
        private const string mainInfo = "Информация";
        private const string newPost = "Создать пост";
        private const string profile = "Профиль";
        private const string shop = "Магазин";

        private readonly ILogger<TelegramBotBackgroundService> _logger;
        private readonly TelegramOptions _telegramOptions;
        private readonly MongoDbOptions _mongoDbOptions;
        private readonly IServiceProvider _serviceProvider;

        public TelegramBotBackgroundService(ILogger<TelegramBotBackgroundService> logger, IOptions<TelegramOptions> telegramOptions, IOptions<MongoDbOptions> mongoDbOptions, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _telegramOptions = telegramOptions.Value;
            _mongoDbOptions = mongoDbOptions.Value;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var botClient = new TelegramBotClient(_telegramOptions.Token);

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = [],
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                await botClient.ReceiveAsync(
                    updateHandler: HandleUpdateAsync,
                    errorHandler: HandleErrorAsync,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken);
            }
        }

        async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
        }



        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if(update.Type != Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    return;
                }

                using IServiceScope scope = _serviceProvider.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var postSubmitionRepository = scope.ServiceProvider.GetRequiredService<IPostSubmitionRepository>();
                var postCreationSessionService = scope.ServiceProvider.GetRequiredService<IPostCreationSessionService>();

                Content content = new Content();

                var text = update.Message.Text;
                if (text == "/start")
                {
                    await userRepository.AddIfNotExist(update.Message.From.Id);
                    await botClient.SendMessage(update.Message.Chat.Id, content.GetInfo(), replyMarkup: GetButtons());
                    return;
                }
                var user = await userRepository.GetUserByTelegramId(update.Message.From.Id);

				if (postCreationSessionService.IsCreationSessionStarted(user.Id, out var session))
				{
					if (session.WaitingForTitle)
					{
						await postSubmitionRepository.UpdaterPostTitleAndUserID(user.Id, update.Message.Text);
						session.CurrentStep = PostCreationStep.WaitingForContent;
						await botClient.SendMessage(update.Message.Chat.Id, content.GetPostInfo());
						return;
					}
					if (session.WaitingForContent)
					{
						postCreationSessionService.FinishPostCreationSession(user.Id);
						await botClient.SendMessage(update.Message.Chat.Id, "Публикация отправлена на модерацию!", replyMarkup: GetButtons());
						//переслать пост на модерацию
						return;
					}
				}

                switch (text)
                {
                    case mainInfo:
                        await botClient.SendMessage(update.Message.Chat.Id, content.GetInfo(), replyMarkup: GetButtons());
                        break;
                    case newPost:
						postCreationSessionService.StartPostCreationSession(user.Id);
						await botClient.SendMessage(update.Message.Chat.Id, $"Напишите название вашей публикации. Это название нужно для того, что бы вы смогли в будущем отслеживать её статус");
                        break;
                    case profile:
                        var userPosts = await postSubmitionRepository.GetLastPostsByUserId(user.Id, 10);
                        await botClient.SendMessage(update.Message.Chat.Id, content.GetProfileInfo(user, userPosts), replyMarkup: GetButtons());
                        break;
                    case shop:
                        await botClient.SendMessage(update.Message.Chat.Id, "Shop", replyMarkup: GetButtons());
                        break;
                    default:
						await botClient.SendMessage(update.Message.Chat.Id, "Используйте существующие команды", replyMarkup: GetButtons());
						break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{
                        new KeyboardButton { Text = mainInfo}, new KeyboardButton { Text = newPost}, },
                    new List<KeyboardButton>{
                        new KeyboardButton { Text = profile}, new KeyboardButton { Text = shop}, }
            }
            };
        }

    }
}
