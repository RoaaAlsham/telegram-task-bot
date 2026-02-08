using System.Text.RegularExpressions;
using CA_TelegramTaskBot.Models;

namespace CA_TelegramTaskBot
{
    /// <summary>
    /// Parses incoming Arabic Ramadan tracking messages into DailyReport objects.
    /// 
    /// Expected message format:
    /// بطاقة المتابعة الرمضانية 🌙
    /// اليوم: 1
    /// اسم المشارك : القمر الجميل
    /// رقم المشارك:125
    /// رقم المشرف: 500
    /// المهام: 
    /// الاستماع لمقطع التدبر: 10
    /// ...
    /// </summary>
    public class MessageParser
    {
        // ===== REGEX PATTERNS =====
        // Each pattern uses \s* to handle inconsistent spacing around colons.
        // (?:...) is a non-capturing group — we don't need to capture the colon itself.

        // Matches "اليوم: 1" or "اليوم :1" or "اليوم:1"
        private static readonly Regex DayPattern =
            new Regex(@"اليوم\s*:\s*(\d+)", RegexOptions.Compiled);

        // Matches "اسم المشارك : القمر الجميل"
        // (.+) captures everything after the colon (the name)
        private static readonly Regex NamePattern =
            new Regex(@"اسم المشارك\s*:\s*(.+)", RegexOptions.Compiled);

        // Matches "رقم المشارك:125"
        private static readonly Regex ParticipantIdPattern =
            new Regex(@"رقم المشارك\s*:\s*(\d+)", RegexOptions.Compiled);

        // Matches "رقم المشرف: 500"
        private static readonly Regex CoordinatorIdPattern =
            new Regex(@"رقم المشرف\s*:\s*(\d+)", RegexOptions.Compiled);

        // Matches "اسم المشرف: منال العلا"
        private static readonly Regex CoordinatorNamePattern =
            new Regex(@"اسم المشرف\s*:\s*(.+)", RegexOptions.Compiled);

        // Task line: "الاستماع لمقطع التدبر: 10" or "صلاة الضحى:" (no value = 0)
        // (.+?) = lazy match for task name, ([\d.]*) = optional decimal number
        private static readonly Regex TaskPattern =
            new Regex(@"^(.+?)\s*:\s*([\d.]*)\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Strips all emojis and common decorative Unicode characters from text.
        /// Emojis are in various Unicode ranges — this regex covers the main ones.
        /// </summary>
        private static string StripEmojis(string text)
        {
            // This pattern matches common emoji Unicode ranges
            return Regex.Replace(text,
                @"[\u2700-\u27BF\uFE00-\uFE0F\u200B-\u200F\u2028-\u202F" +
                @"\uD83C-\uDBFF\uDC00-\uDFFF\u2600-\u26FF\u2300-\u23FF" +
                @"\u200D\uFE0F]",
                "").Trim();
        }

        /// <summary>
        /// Parse a single Arabic message into a DailyReport.
        /// Returns null if the message is malformed or missing required fields.
        /// </summary>
        public static DailyReport? ParseMessage(string message)
        {
            // STEP 1: Strip emojis so they don't interfere with regex matching
            var cleanMessage = StripEmojis(message);

            // STEP 2: Extract header fields using regex
            var dayMatch = DayPattern.Match(cleanMessage);
            var nameMatch = NamePattern.Match(cleanMessage);
            var participantIdMatch = ParticipantIdPattern.Match(cleanMessage);
            var coordinatorIdMatch = CoordinatorIdPattern.Match(cleanMessage);
            var coordinatorNameMatch = CoordinatorNamePattern.Match(cleanMessage);

            // STEP 3: Validate all required fields are present
            if (!dayMatch.Success || !nameMatch.Success ||
                !participantIdMatch.Success || !coordinatorIdMatch.Success || !coordinatorNameMatch.Success)
            {
                // Message is malformed — return null so the caller can handle it
                return null;
            }

            // STEP 4: Extract values from regex groups
            int day = int.Parse(dayMatch.Groups[1].Value);
            string name = nameMatch.Groups[1].Value.Trim();
            int participantId = int.Parse(participantIdMatch.Groups[1].Value);
            int coordinatorId = int.Parse(coordinatorIdMatch.Groups[1].Value);
            string coordinatorName = coordinatorNameMatch.Groups[1].Value.Trim();
            // STEP 5: Build the Participant object
            var participant = new Participant(participantId, name, coordinatorId);
            var coordinator = new Coordinator(coordinatorId, coordinatorName);
            // STEP 6: Parse tasks — look for lines AFTER "المهام"
            var tasks = new List<DailyTask>();
            var lines = cleanMessage.Split('\n');
            bool inTaskSection = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Once we see "المهام", everything after is a task line
                if (trimmedLine.Contains("المهام"))
                {
                    inTaskSection = true;
                    continue; // Skip the "المهام:" line itself
                }

                // Only parse task lines after we've entered the tasks section
                if (inTaskSection)
                {
                    var taskMatch = TaskPattern.Match(trimmedLine);
                    if (taskMatch.Success)
                    {
                        string taskName = taskMatch.Groups[1].Value.Trim();
                        string rawValue = taskMatch.Groups[2].Value.Trim();

                        // Default to 0 if no value provided (e.g., "صلاة الضحى:")
                        double taskValue = string.IsNullOrEmpty(rawValue) ? 0 : double.Parse(rawValue);

                        // RamadanTask constructor clamps value to [0, 10]
                        tasks.Add(new DailyTask(taskName, taskValue));
                    }
                }
            }

            // STEP 7: Return the assembled DailyReport
            return new DailyReport(day, participant, tasks, coordinator);
        }

        /// <summary>
        /// Parse multiple messages into a list of DailyReports.
        /// Skips malformed messages (returns only successful parses).
        /// </summary>
        public static List<DailyReport> ParseMessages(List<string> messages)
        {
            var reports = new List<DailyReport>();

            foreach (var message in messages)
            {
                var report = ParseMessage(message);
                if (report != null)
                {
                    reports.Add(report);
                }
                // If report is null, the message was malformed — we silently skip it.
                // TO DO : want to log these for debugging.
            }

            return reports;
        }

    }
}
