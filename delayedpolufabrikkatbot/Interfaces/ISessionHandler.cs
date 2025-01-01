using Telegram.Bot.Types;
using Telegram.Bot;
using delayedpolufabrikkatbot.Models.Sessions;

namespace delayedpolufabrikkatbot.Interfaces
{
	public interface ISessionHandler
	{
		Task HandleSessionMessage(ITelegramBotClient botClient, Update update, BaseSession session);
	}
}
