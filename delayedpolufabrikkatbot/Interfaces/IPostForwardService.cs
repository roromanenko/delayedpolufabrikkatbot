using Telegram.Bot.Types;
using Telegram.Bot;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface IPostForwardService
    {
        void ForwardPostToModeration(Update update, ITelegramBotClient botClient, IUserRepository userRepository, IPostSubmitionRepository postSubmitionRepository);

        Task HandleCallbackQuery(CallbackQuery callbackQuery, ITelegramBotClient botClient, IUserRepository userRepository, IPostSubmitionRepository postSubmitionRepository);
    }
}