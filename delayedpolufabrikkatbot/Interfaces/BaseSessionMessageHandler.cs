using delayedpolufabrikkatbot.Models.Sessions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Interfaces
{
	public abstract class BaseSessionMessageHandler<T> where T : BaseUserSession
	{
		private readonly ICacheManager _cacheManager;

		public BaseSessionMessageHandler(ICacheManager cacheManager)
		{
			_cacheManager = cacheManager;
		}

		public async Task HandleSessionMessage(ITelegramBotClient botClient, Update update, T session)
		{
			if (await ProcessSessionAndReturnIsSessionFinished(botClient, update, session))
			{
				_cacheManager.Remove(session.UserId);
			}
		}

		protected abstract Task<bool> ProcessSessionAndReturnIsSessionFinished(ITelegramBotClient botClient, Update update, T session);
	}
}
