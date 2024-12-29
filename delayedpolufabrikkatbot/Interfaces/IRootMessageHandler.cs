using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface IRootMessageHandler
    {
        Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
		Task HandleCallbackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
	}
}