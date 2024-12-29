var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddServices();

builder.Services.AddMongoDb(builder.Configuration);

builder.Services.AddTelegramBot(builder.Configuration);

builder.Services.AddMemoryCache();

var host = builder.Build();
host.Run();