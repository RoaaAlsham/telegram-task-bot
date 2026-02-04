using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramTaskBot
{
    public class TaskBot
    {
        private readonly TelegramBotClient _botClient;

        private readonly Dictionary<long, List<string>> _userMessages;
        public TaskBot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _userMessages = new Dictionary<long, List<string>>();
        }
        public async Task StartAsync()
        {
            var me = await _botClient.GetMe(); // Ensure the correct method name is used
            Console.WriteLine($"Bot {me.Username} is starting...");

            // set up receiving options
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
               updateHandler: HandleUpdateAsync,
               errorHandler: HandleErrorAsync,
               receiverOptions: receiverOptions
           );

            Console.WriteLine("Press any key to stop the bot.");
        }
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            if (message.Text is not { } messageText)
                return;
            var ChatId = message.Chat.Id;
            var username = message.From?.Username ?? "Unknown User";
            Console.WriteLine($"Received a message from {username}: {messageText}");

            //// echo the message back
            //await botClient.SendMessage( 
            //    chatId: ChatId,
            //    text: $"You said: {messageText}",
            //    cancellationToken: cancellationToken
            //);

            // handle commands
            if (messageText.StartsWith("/")) { 
                await HandleCommandAsync(ChatId, messageText, cancellationToken);
                return;
            }

            // store the commands
            if (!_userMessages.ContainsKey(ChatId))
            {
                _userMessages[ChatId] = new List<string>();
            }
            _userMessages[ChatId].Add(messageText);


            await botClient.SendMessage(
                chatId: ChatId,
                text: $"✅ Message stored! Total: {_userMessages[ChatId].Count}\n" +
                      $"Use /preview to see or /generate to create Excel.",
                cancellationToken: cancellationToken);
        }

        private async Task HandleCommandAsync(long chatId, string command, CancellationToken cancellationToken) {
            string response = command.ToLower();
            switch(response)
            {
                case "/start":
                response = "👋 Welcome! Send me task messages and I'll convert them to Excel!\n\n" +
                      "Format:\n" +
                      "person name: John\n" +
                      "task one: 10\n" +
                      "task two: 5\n\n" +
                      "Use /help for more info.";
                break;

                case "/help":
                response = "📚 Available Commands:\n" +
                          "/start - Welcome message\n" +
                          "/help - Show this help\n" +
                          "/preview - Preview stored messages\n" +
                          "/generate - Create Excel file\n" +
                          "/clear - Clear all messages";
                break;

                case "/preview":
                    if (!_userMessages.ContainsKey(chatId) || _userMessages[chatId].Count == 0)
                    {
                        response = "No messages stored yet.";
                    }
                    else {
                        var messages = string.Join("\n\n", _userMessages[chatId]);
                        response = $"📝 Stored Messages ({_userMessages[chatId].Count}):\n\n{messages}";
                    }
                    break;
                case "/clear":
                    if (_userMessages.ContainsKey(chatId)) {
                        _userMessages[chatId].Clear();
                    }
                    response = "✅ All messages cleared!";
                    break;
                case "/generate":
                    if (!_userMessages.ContainsKey(chatId) || _userMessages[chatId].Count == 0)
                    {
                        response = "No messages to generate from. Send some task messages first!";
                    }
                    else {
                        GenerateExcelForUser(chatId, cancellationToken);
                        return;
                    }
                        break;

                default:
                    response = "❌ Unknown command. Use /help to see available commands.";
                    break;
            }
            ;

            await _botClient.SendMessage(chatId: chatId,
                text: response,
                cancellationToken: cancellationToken
            );
        }

        private async Task GenerateExcelForUser(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                await _botClient.SendMessage(chatId: chatId, "⏳ Generating Excel file...", cancellationToken: cancellationToken);

                var taskData = MessageParser.ParseMessages(_userMessages[chatId]);

                if (taskData.Count == 0) { 
                   await  _botClient.SendMessage(chatId, "No valid task data found, check your message FORMAT.", cancellationToken: cancellationToken);
                    return;
                }
                var fileName = $"Tasks_{chatId}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                ExcelGenerator.GenerateExcel(taskData, fileName);

                // send the file 
                using (var stream = System.IO.File.OpenRead(fileName)) {
                    InputFile file = InputFile.FromStream(stream, fileName);
                    await _botClient.SendDocument(chatId,file, caption: $"✅ Excel file created!\n\n" +
                        $"📊 {taskData.Count} tasks from {taskData.Select(t => t.Person).Distinct().Count()} people",
                cancellationToken: cancellationToken);
                }
                System.IO.File.Delete(fileName);


                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Want to add more data or start fresh? Use /clear to reset.",
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: $"❌ Error generating Excel: {ex.Message}",
                    cancellationToken: cancellationToken);
            }
        }
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error occurred: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
