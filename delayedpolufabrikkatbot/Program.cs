using delayedpolufabrikkatbot.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRepositories();

builder.Services.AddServices();

builder.Services.AddMongoDb(builder.Configuration);

builder.Services.AddTelegramBot(builder.Configuration);

builder.Services.AddMemoryCache();

var host = builder.Build();
host.Run();