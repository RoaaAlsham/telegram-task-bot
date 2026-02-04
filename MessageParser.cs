

namespace TelegramTaskBot
{
    public class MessageParser
    {
        public static List<TaskData> ParseMessages(List<string> messages) {
            var taskList = new List<TaskData>();
            string? currentPerson = null;

            foreach (var message in messages) {
                var lines = message.Split("\n");
                foreach (var line in lines) {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                        continue;
                    // check if is a person name line
                    if (trimmedLine.Contains("الاسم") || trimmedLine.Contains("name")) {
                        var parts = trimmedLine.Split(":", StringSplitOptions.TrimEntries);
                        if (parts.Length == 2) {
                            currentPerson = parts[1];
                        }
                    }
                    // check if is a task line
                    else if(trimmedLine.Contains(":") && currentPerson!=null)
                    {
                        var parts = trimmedLine.Split(":", StringSplitOptions.TrimEntries);
                        if (parts.Length == 2) { 
                            var taskName = parts[0];
                            var taskValue = parts[1];
                            if (double.TryParse(taskValue, out double value)) { 
                                taskList.Add(new TaskData(currentPerson, taskName, value));
                            }
                        }
                    }
                }
            }
            return taskList;
        }
    }
}
