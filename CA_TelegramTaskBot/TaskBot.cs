using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CA_TelegramTaskBot.Models;


namespace CA_TelegramTaskBot
{
    public class TaskBot
    {
        public readonly TelegramBotClient _botClient;
        private readonly Dictionary<long, List<DailyReport>> _userReports;
        private readonly Dictionary<int, Coordinator> _coordinators;

        public TaskBot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _userReports = new Dictionary<long, List<DailyReport>>();
            _coordinators = new Dictionary<int, Coordinator>();
        }

        public async Task StartAsync()
        {
            var me = await _botClient.GetMe();
            Console.WriteLine($"Bot {me.Username} is starting...");
            var reciverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: reciverOptions);

            Console.WriteLine("Bot is running. Press any key to exit.");
        }
        private async Task HandleUpdateAsync(
                ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message) return;
            // explain syntax: this checks if update.Message is null. If it is null, the method returns immediately.
            // If it is not null, the message variable is assigned the value of update.
            // Message for use in the rest of the method.
            if (message.Text is not { } messageText) return;

            var chatId = message.Chat.Id;
            var username = message.From?.Username ?? "Unknown";
            Console.WriteLine($"Received from {username}: {messageText}");

            // Route commands vs regular messages
            if (messageText.StartsWith("/"))
            {
                await HandleCommandAsync(chatId, messageText, cancellationToken);
                return;
            }

            var report = MessageParser.ParseMessage(messageText);
            if (report != null)
            {
                    if (!_userReports.ContainsKey(chatId))
                        _userReports[chatId] = new List<DailyReport>();
                    // check for duplicate entry for the same day and participant
                    
                    var existingReportIndex = _userReports[chatId].FindIndex(r=>r.Day==report.Day && r.Participant.ParticipantId == report.Participant.ParticipantId);
                    bool isReplacement = existingReportIndex >= 0;
                        if (isReplacement)
                        {
                            _userReports[chatId][existingReportIndex] = report;// replace 
                        }
                        else { 
                            _userReports[chatId].Add(report);// add new
                        }

                            TrackCoordinator(report);

                        string statusEmoji = isReplacement ? "🔄" : "✅";
                        string statusText = isReplacement ? "تم تحديث بيانات" : "تم حفظ بيانات";


                        await botClient.SendMessage(
                           chatId: chatId,
                           text: $"{statusEmoji} {statusText}: {report.Participant.ParticipantName}\n" +
                                 $"📅 اليوم: {report.Day}\n" +
                                 $"📊 المجموع: {report.TotalPoints}/70\n" +
                                 $"📝 إجمالي السجلات: {_userReports[chatId].Count}\n\n" +
                                 $"استخدم /generate لإنشاء ملف Excel.",
                           cancellationToken: cancellationToken);
                        }
            else
            {
                // Malformed message — inform the user
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ صيغة الرسالة غير صحيحة.\n\n" +
                          "الصيغة المطلوبة:\n" +
                          "اليوم: 1\n" +
                          "اسم المشارك : الاسم\n" +
                          "رقم المشارك: 123\n" +
                          "رقم المشرف: 500\n" +
                           "اسم المشرف: الاسم\n" +
                          "المهام:\n" +
                          "اسم المهمة: 10",
                    cancellationToken: cancellationToken);
            }
        }

        private void TrackCoordinator(DailyReport report)
        {
            var coorId = report.Coordinator.CoordinatorId;
            var coorName = report.Coordinator.CoordinatorName;
            if (!_coordinators.ContainsKey(coorId))
            {
                _coordinators[coorId] = new Coordinator(coorId, coorName);
            }
            _coordinators[coorId].AddParticipant(report.Participant);
        }


        private async Task HandleCommandAsync(
            long chatId, string command, CancellationToken cancellationToken)
        {
            string response;

            switch (command.ToLower())
            {
                case "/start":
                    response = "👋 مرحباً! أرسل لي بطاقات المتابعة الرمضانية وسأحولها إلى Excel!\n\n" +
                               "الأوامر المتاحة:\n" +
                               "/help - المساعدة\n" +
                               "/preview - معاينة الرسائل\n" +
                               "/generate - إنشاء ملف Excel\n" +
                               "/stats - إحصائيات\n" +
                               "/clear - مسح جميع الرسائل";
                    break;

                case "/help":
                    response = "📚 الأوامر المتاحة:\n" +
                               "/start - رسالة الترحيب\n" +
                               "/help - عرض المساعدة\n" +
                               "/preview - معاينة الرسائل المحفوظة\n" +
                               "/generate - إنشاء ملف Excel\n" +
                               "/stats - عرض الإحصائيات\n" +
                               "/clear - مسح جميع الرسائل";
                    break;

                case "/preview":
                    if (!_userReports.ContainsKey(chatId) || _userReports[chatId].Count == 0)
                    {
                        response = "📭 لا توجد رسائل محفوظة.";
                    }
                    else
                    {
                        var reports = _userReports[chatId];
                        var preview = string.Join("\n",
                            reports.Select(r =>
                                $"📅 يوم {r.Day} | {r.Participant.ParticipantName} | المجموع: {r.TotalPoints}"));
                        response = $"📝 الرسائل المحفوظة ({reports.Count}):\n\n{preview}";
                    }
                    break;

                case "/stats":
                    if (!_coordinators.Any())
                    {
                        response = "📭 لا توجد إحصائيات بعد.";
                    }
                    else
                    {
                        var statsLines = _coordinators.Select(kvp =>
                            $"👤 المشرف {kvp.Value.CoordinatorName} ({kvp.Key}): {kvp.Value.Participants.Count} مشارك");
                        response = $"📊 الإحصائيات:\n\n{string.Join("\n", statsLines)}";
                    }
                    break;

                case "/clear":
                    if (_userReports.ContainsKey(chatId))
                        _userReports[chatId].Clear();
                    response = "✅ تم مسح جميع الرسائل!";
                    break;

                case "/generate":
                    if (!_userReports.ContainsKey(chatId) || _userReports[chatId].Count == 0)
                    {
                        response = "📭 لا توجد رسائل لإنشاء الملف. أرسل بطاقات المتابعة أولاً!";
                    }
                    else
                    {
                        await GenerateExcelForUser(chatId, cancellationToken);
                        return;
                    }
                    break;

                default:
                    response = "❌ أمر غير معروف. استخدم /help لعرض الأوامر.";
                    break;
            }

            await _botClient.SendMessage(
                chatId: chatId, text: response, cancellationToken: cancellationToken);
        }

        private async Task GenerateExcelForUser(long chatId, CancellationToken ct)
        {
            try
            {

                await _botClient.SendMessage(
                     chatId, "⏳ جاري إنشاء ملف Excel...", cancellationToken: ct);

                // Parse all stored messages into DailyReports
                List<DailyReport> reports = _userReports[chatId];

                if (reports.Count == 0)
                {
                    await _botClient.SendMessage(
                        chatId, "❌ لم يتم العثور على بيانات صالحة. تحقق من صيغة الرسائل.",
                        cancellationToken: ct);
                    return;
                }

                var filename = $"Ramadan_Tasks_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                ExcelGenerator.GenerateExcel(reports, filename);

                var uniqueParticipants = reports.Select(r => r.Participant.ParticipantId).Distinct().Count();
                var uniqueCoordinators = reports
                   .Select(r => r.Participant.CoordinatorId).Distinct().Count();
                var totalDays = reports.Select(r => r.Day).Distinct().Count();


                using (var stream = File.OpenRead(filename))
                {
                    var file = InputFile.FromStream(stream, filename);
                    await _botClient.SendDocument(
                        chatId, file,
                        caption: $"✅ تم إنشاء الملف!\n\n" +
                                 $"📊 {reports.Count} سجل\n" +
                                 $"👥 {uniqueParticipants} مشارك\n" +
                                 $"👤 {uniqueCoordinators} مشرف\n" +
                                 $"📅 {totalDays} يوم",
                        cancellationToken: ct);

                    File.Delete(filename); // Clean up the file after sending
                }

            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(chatId: chatId, text: $"❌ خطأ أثناء إنشاء الملف: {ex.Message}", cancellationToken: ct);
                return;
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;


        }
    }
}
