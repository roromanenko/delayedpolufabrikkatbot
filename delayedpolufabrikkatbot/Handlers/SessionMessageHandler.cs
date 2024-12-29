using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models.Sessions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Handlers
{
	public class SessionMessageHandler : ISessionMessageHandler
	{
		private readonly IServiceProvider _serviceProvider;

		public SessionMessageHandler(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public Task HandleSessionMessage(ITelegramBotClient botClient, Update update, BaseUserSession session)
		{
			var sessionType = session.GetType();
			var handlerType = typeof(BaseSessionMessageHandler<>).MakeGenericType(sessionType);

			// Resolve the handler dynamically
			var handler = _serviceProvider.GetService(handlerType);
			if (handler == null)
			{
				throw new InvalidOperationException($"No handler found for session type {sessionType.Name}");
			}

			var handleMethod = handlerType.GetMethod(nameof(BaseSessionMessageHandler<BaseUserSession>.HandleSessionMessage));
			return (Task)handleMethod?.Invoke(handler, [botClient, update, session]);
		}
	}
}
