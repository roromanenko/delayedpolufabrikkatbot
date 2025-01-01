using delayedpolufabrikkatbot;
using delayedpolufabrikkatbot.Handlers;
using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models.Sessions;
using delayedpolufabrikkatbot.Options;
using delayedpolufabrikkatbot.Repositories;
using delayedpolufabrikkatbot.Service;
using delayedpolufabrikkatbot.Utilities;
using MongoDB.Driver;
using System;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IPostSubmitionRepository, PostSubmitionRepository>();
		services.AddScoped<IAdminChannelService, AdminChannelService>();
        services.AddScoped<IRootMessageHandler, RootMessageHandler>();
		services.AddScoped<ICacheManager, CacheManager>();
		services.AddScoped<ISessionHandler, SessionHandler>();
		services.AddScoped<BaseSessionHandler<PostCreationSession>, PostCreationSessionHandler>();


        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
		services.AddScoped(opt =>
		{
			var connectionString = configuration["delayedpolufabrikkatbot:MongodbConnection"];
			var settings = MongoClientSettings.FromConnectionString(connectionString);
			settings.ServerApi = new ServerApi(ServerApiVersion.V1);
			var client = new MongoClient(settings);

			return client;
		});

        services.Configure<MongoDbOptions>(configuration.GetSection(nameof(MongoDbOptions)));
        return services;
    }

    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<TelegramBotBackgroundService>();
        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.Telegram));
        return services;
    }
}