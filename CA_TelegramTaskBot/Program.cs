using CA_TelegramTaskBot;

Console.WriteLine("Starting Task Bot...");
DotNetEnv.Env.Load();
string? token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if(string.IsNullOrEmpty(token))
{
    Console.WriteLine("Error: TELEGRAM_BOT_TOKEN environment variable is not set.");
    return;
}
var bot = new TaskBot(token);
await bot.StartAsync();
Console.ReadKey();
