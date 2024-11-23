using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Formats.Asn1.AsnWriter;

namespace delayedpolufabrikkatbot
{
    public class TelegramBotBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TelegramOptions _telegramOptions;
        private readonly ILogger<TelegramBotBackgroundService> _logger;


        public TelegramBotBackgroundService(IServiceProvider serviceProvider, IOptions<TelegramOptions> telegramOptions, ILogger<TelegramBotBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _telegramOptions = telegramOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var botClient = new TelegramBotClient(_telegramOptions.Token);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                await botClient.ReceiveAsync(HandleUpdateAsync, HandleErrorAsync, receiverOptions, stoppingToken);
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandlerService>();
            var callbackQueryHandler = scope.ServiceProvider.GetRequiredService<ICallbackQueryHandlerService>();

            if (update.Type == UpdateType.Message)
            {
                await messageHandler.HandleMessageAsync(botClient, update, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                try
                {
                    await callbackQueryHandler.HandleCallbackQueryAsync(botClient, update, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке обновления.");
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}
