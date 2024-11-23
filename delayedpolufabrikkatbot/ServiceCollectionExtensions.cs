using delayedpolufabrikkatbot;
using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Options;
using delayedpolufabrikkatbot.Repositories;
using delayedpolufabrikkatbot.Service;
using MongoDB.Driver;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostSubmitionRepository, PostSubmitionRepository>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPostCreationSessionService, PostCreationSessionService>();
        services.AddScoped<IPostForwardService, PostForwardService>();
        services.AddScoped<IMessageHandlerService, MessageHandlerService>();
        services.AddScoped<ICallbackQueryHandlerService, CallbackQueryHandlerService>();
        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["delayedpolufabrikkatbot:MongodbConnection"];
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        var client = new MongoClient(settings);

        services.AddSingleton(client);
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