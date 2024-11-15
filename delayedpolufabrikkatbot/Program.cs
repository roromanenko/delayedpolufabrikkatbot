using delayedpolufabrikkatbot;
using delayedpolufabrikkatbot.Service;
using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Options;
using delayedpolufabrikkatbot.Repositories;
using MongoDB.Driver;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostSubmitionRepository, PostSubmitionRepository>();
builder.Services.AddScoped<IPostCreationSessionService, PostCreationSessionService>();

builder.Services.AddScoped(opt =>
{
    string connectionString = builder.Configuration["delayedpolufabrikkatbot:MongodbConnection"];

    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    var client = new MongoClient(settings);
    return client;
});

builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(nameof(MongoDbOptions)));

builder.Services.AddHostedService<TelegramBotBackgroundService>();

builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.Telegram));

builder.Services.AddMemoryCache();

var host = builder.Build();
host.Run();