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
        private readonly IMemoryCache _memoryCache;

        public TelegramBotBackgroundService(ILogger<TelegramBotBackgroundService> logger, IOptions<TelegramOptions> telegramOptions, IOptions<MongoDbOptions> mongoDbOptions, IServiceProvider serviceProvider, IMemoryCache memoryCache)
        {
            _logger = logger;
            _telegramOptions = telegramOptions.Value;
            _mongoDbOptions = mongoDbOptions.Value;
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
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
                var cacheConroller = scope.ServiceProvider.GetRequiredService<ICacheService>();

                Content content = new Content();
                CacheService cacheService = new CacheService(_memoryCache);

                var text = update.Message.Text;
                if (text == "/start")
                {
                    await userRepository.AddIfNotExist(update.Message.From.Id);
                    await botClient.SendMessage(update.Message.Chat.Id, content.GetInfo(), replyMarkup: GetButtons());
                    return;
                }

                var user = await userRepository.GetUserByTelegramId(update.Message.From.Id);

                if (cacheService.GetCache(update.Message.From.Id) != null)
                {
                    if (cacheService.GetCache(update.Message.From.Id) == "waiting for title")
                    {
                        await postSubmitionRepository.UpdaterPostTitleAndUserID(user.Id, update.Message.Text);
                        cacheService.SetCache(update.Message.From.Id, "waiting for post");
                        await botClient.SendMessage(update.Message.Chat.Id, content.GetPostInfo());
                    }
                    else
                    {
                        //переслать пост на модерацию
                        await botClient.SendMessage(update.Message.Chat.Id, "Публикация отправлена на модерацию!", replyMarkup: GetButtons());
                        cacheConroller.ClearCache(update.Message.From.Id);
                        return;
                    }
                }
                switch (text)
                {
                    case mainInfo:
                        await botClient.SendMessage(update.Message.Chat.Id, content.GetInfo(), replyMarkup: GetButtons());
                        break;
                    case newPost:
                        await botClient.SendMessage(update.Message.Chat.Id, $"Напишите название вашей публикации. Это название нужно для того, что бы вы смогли в будущем отслеживать её статус");
                        cacheService.SetCache(update.Message.From.Id, "waiting for title");
                        break;
                    case profile:
                        var userPosts = await postSubmitionRepository.GetLastPostsByUserId(user.Id, 10);
                        await botClient.SendMessage(update.Message.Chat.Id, content.GetProfileInfo(user, userPosts), replyMarkup: GetButtons());
                        break;
                    case shop:
                        await botClient.SendMessage(update.Message.Chat.Id, "Shop", replyMarkup: GetButtons());
                        break;
                    default:
                        if (cacheService.GetCache(update.Message.From.Id) == null)
                        {
                            await botClient.SendMessage(update.Message.Chat.Id, "Используйте существующие команды", replyMarkup: GetButtons());
                        }
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
