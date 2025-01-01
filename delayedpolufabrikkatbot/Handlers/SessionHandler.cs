using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models.Sessions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Handlers
{
	public class SessionHandler : ISessionHandler
	{
		private readonly IServiceProvider _serviceProvider;

		public SessionHandler(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public Task HandleSessionMessage(ITelegramBotClient botClient, Update update, BaseSession session)
		{
			var sessionType = session.GetType();
			var handlerType = typeof(BaseSessionHandler<>).MakeGenericType(sessionType);

			// Resolve the handler dynamically
			var handler = _serviceProvider.GetService(handlerType);
			if (handler == null)
			{
				throw new InvalidOperationException($"No handler found for session type {sessionType.Name}");
			}

			var handleMethod = handlerType.GetMethod(nameof(BaseSessionHandler<BaseSession>.HandleSessionMessage));
			return (Task)handleMethod?.Invoke(handler, [botClient, update, session]);
		}
	}
}
