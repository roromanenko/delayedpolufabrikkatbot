using delayedpolufabrikkatbot.Models.Sessions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Interfaces
{
	public abstract class BaseSessionHandler<T> where T : BaseSession
	{
		private readonly ICacheManager _cacheManager;

		public BaseSessionHandler(ICacheManager cacheManager)
		{
			_cacheManager = cacheManager;
		}

		public async Task HandleSessionMessage(ITelegramBotClient botClient, Update update, T session)
		{
			if (await ProcessSessionAndReturnIsSessionFinished(botClient, update, session))
			{
				_cacheManager.Remove(session.Key);
			}
		}

		protected abstract Task<bool> ProcessSessionAndReturnIsSessionFinished(ITelegramBotClient botClient, Update update, T session);
	}
}
