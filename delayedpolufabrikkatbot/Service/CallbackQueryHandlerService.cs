using delayedpolufabrikkatbot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace delayedpolufabrikkatbot.Service
{
    public class CallbackQueryHandlerService : ICallbackQueryHandlerService
    {
        private readonly IServiceProvider _serviceProvider;

        public CallbackQueryHandlerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var postForwardService = scope.ServiceProvider.GetRequiredService<IPostForwardService>();
            var postSubmitionRepository = scope.ServiceProvider.GetRequiredService<IPostSubmitionRepository>();

            await postForwardService.HandleCallbackQuery(update.CallbackQuery, botClient, userRepository, postSubmitionRepository);
        }
    }

}
