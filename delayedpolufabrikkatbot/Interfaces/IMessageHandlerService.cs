using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface IMessageHandlerService
    {
        Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}