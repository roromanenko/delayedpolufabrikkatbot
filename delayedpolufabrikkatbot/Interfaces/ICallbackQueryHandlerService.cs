using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface ICallbackQueryHandlerService
    {
        Task HandleCallbackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}