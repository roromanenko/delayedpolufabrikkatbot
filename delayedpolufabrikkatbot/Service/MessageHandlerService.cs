using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace delayedpolufabrikkatbot.Service
{
    public class MessageHandlerService : IMessageHandlerService
    {
        private readonly IServiceProvider _serviceProvider;

        private const string mainInfo = "Информация";
        private const string newPost = "Создать пост";
        private const string profile = "Профиль";
        private const string shop = "Магазин";

        public MessageHandlerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var postForwardService = scope.ServiceProvider.GetRequiredService<IPostForwardService>();
            var postCreationSessionService = scope.ServiceProvider.GetRequiredService<IPostCreationSessionService>();
            var postSubmitionRepository = scope.ServiceProvider.GetRequiredService<IPostSubmitionRepository>();

            Content content = new Content();

            var text = update.Message.Text;
            var userId = update.Message.From.Id;

            // Регистрация нового пользователя
            await userRepository.AddIfNotExist(userId);

            var user = await userRepository.GetUserByTelegramId(userId);

            if (postCreationSessionService.IsCreationSessionStarted(user.Id, out var session))
            {
                if (session.WaitingForTitle)
                {
                    await postSubmitionRepository.UpdaterPostTitleAndUserID(user.Id, text);
                    session.CurrentStep = PostCreationStep.WaitingForContent;
                    await botClient.SendMessage(update.Message.Chat.Id, "Напишите содержание публикации.");
                    return;
                }

                if (session.WaitingForContent)
                {
                    postCreationSessionService.FinishPostCreationSession(user.Id);
                    await botClient.SendMessage(update.Message.Chat.Id, "Публикация отправлена на модерацию.");
                    postForwardService.ForwardPostToModeration(update, botClient, userRepository, postSubmitionRepository);
                    return;
                }
            }

            switch (text)
            {
                case mainInfo:
                    await botClient.SendMessage(update.Message.Chat.Id, content.GetInfo(), replyMarkup: GetButtons());
                    break;
                case newPost:
                    postCreationSessionService.StartPostCreationSession(user.Id);
                    await botClient.SendMessage(update.Message.Chat.Id, "Напишите название вашей публикации. Это название нужно для того, чтобы вы смогли в будущем отслеживать её статус.");
                    break;
                case profile:
                    var userPosts = await postSubmitionRepository.GetLastPostsByUserId(user.Id, 10);
                    await botClient.SendMessage(update.Message.Chat.Id, content.GetProfileInfo(user, userPosts), replyMarkup: GetButtons());
                    break;
                case shop:
                    await botClient.SendMessage(update.Message.Chat.Id, "Shop", replyMarkup: GetButtons());
                    break;
                default:
                    await botClient.SendMessage(update.Message.Chat.Id, "Используйте существующие команды", replyMarkup: GetButtons());
                    break;
            }
        }

        private IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton> { new KeyboardButton(mainInfo), new KeyboardButton(newPost) },
                new List<KeyboardButton> { new KeyboardButton(profile), new KeyboardButton(shop) }
            }
            };
        }
    }

}