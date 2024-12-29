using Telegram.Bot.Types;
using Telegram.Bot;
using delayedpolufabrikkatbot.Models.Sessions;

namespace delayedpolufabrikkatbot.Interfaces
{
	public interface ISessionMessageHandler
	{
		Task HandleSessionMessage(ITelegramBotClient botClient, Update update, BaseUserSession session);
	}
}
