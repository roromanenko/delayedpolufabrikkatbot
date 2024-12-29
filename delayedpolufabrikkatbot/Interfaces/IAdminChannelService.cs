using Telegram.Bot.Types;
using Telegram.Bot;
using delayedpolufabrikkatbot.Models.Sessions;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface IAdminChannelService
    {
        Task ForwardPostToModeration(Update update, ITelegramBotClient botClient, PostCreationSession session);

    }
}