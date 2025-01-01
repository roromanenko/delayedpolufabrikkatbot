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
			if(!await botClient.TestApi(stoppingToken))
			{
				_logger.LogCritical("Incorrect telegram token");
			}

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
            };

			// Main cycle
            while (!stoppingToken.IsCancellationRequested)
            {
                await botClient.ReceiveAsync(HandleUpdateAsync, HandleErrorAsync, receiverOptions, stoppingToken);
            }
        }

        private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var messageHandler = scope.ServiceProvider.GetRequiredService<IRootHandler>();

			try
			{
				return update.Type switch
				{
					UpdateType.Unknown
					or UpdateType.InlineQuery
					or UpdateType.ChosenInlineResult
					or UpdateType.EditedMessage
					or UpdateType.ChannelPost
					or UpdateType.EditedChannelPost
					or UpdateType.ShippingQuery
					or UpdateType.PreCheckoutQuery
					or UpdateType.Poll
					or UpdateType.PollAnswer
					or UpdateType.MyChatMember
					or UpdateType.ChatMember
					or UpdateType.ChatJoinRequest
					or UpdateType.MessageReaction
					or UpdateType.MessageReactionCount
					or UpdateType.ChatBoost
					or UpdateType.RemovedChatBoost
					or UpdateType.BusinessConnection
					or UpdateType.BusinessMessage
					or UpdateType.EditedBusinessMessage
					or UpdateType.DeletedBusinessMessages
					or UpdateType.PurchasedPaidMedia => throw new NotImplementedException(),

					UpdateType.Message => messageHandler.HandleMessageAsync(botClient, update, cancellationToken),
					UpdateType.CallbackQuery => messageHandler.HandleCallbackQueryAsync(botClient, update, cancellationToken),
					_ => throw new ArgumentOutOfRangeException($"Unknown Update Type: {update.Type}"),
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке обновления.");
				return Task.CompletedTask;
			}
		}

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
			_logger.LogError($"Error message: {exception.Message}\nFull error: {exception}");
            return Task.CompletedTask;
        }
    }

}
