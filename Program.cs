using TelegramTaskBot;
Console.WriteLine("Starting Task Bot...");
DotNetEnv.Env.Load();
string? token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
var bot = new TaskBot(token);
await bot.StartAsync();
Console.ReadKey();
