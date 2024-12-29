using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using delayedpolufabrikkatbot.Models.Sessions;
using delayedpolufabrikkatbot.Repositories;
using delayedpolufabrikkatbot.Service;
using delayedpolufabrikkatbot.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace delayedpolufabrikkatbot.Handlers
{
	public class PostCreationSessionMessageHandler : BaseSessionMessageHandler<PostCreationSession>
	{
		private readonly IPostSubmitionRepository _postSubmitionRepository;
		private readonly IAdminChannelService _adminChannelService;

		public PostCreationSessionMessageHandler(ICacheManager cacheManager, IPostSubmitionRepository postSubmitionRepository, IAdminChannelService adminChannelService) : base(cacheManager)
		{
			_postSubmitionRepository = postSubmitionRepository;
			_adminChannelService = adminChannelService;
		}

		protected override async Task<bool> ProcessSessionAndReturnIsSessionFinished(ITelegramBotClient botClient, Update update, PostCreationSession session)
		{
			switch (session.CurrentStep)
			{
				case PostCreationStep.WaitingForTitle:
					session.PostId = await _postSubmitionRepository.CreatePostWithTitleAndUserID(session.UserId, update.Message.Text);
					session.CurrentStep = PostCreationStep.WaitingForContent;
					await botClient.SendMessage(update.Message.Chat.Id, "Напишите содержание публикации.");
					return false;
				case PostCreationStep.WaitingForContent:
					await botClient.SendMessage(update.Message.Chat.Id, "Публикация отправлена на модерацию.");
					await _adminChannelService.ForwardPostToModeration(update, botClient, session);
					return true;
				default:
					throw new InvalidDataException($"Unknown session step: {session.CurrentStep}");
			}
		}
	}
}
