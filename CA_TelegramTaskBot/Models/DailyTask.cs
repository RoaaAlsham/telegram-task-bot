
using System.Xml.Linq;

namespace CA_TelegramTaskBot.Models
{
    public class DailyTask
    {
        private const double MaxTaskValue = 10;
        private const double MinTaskValue = 0;
        public DailyTask(string taskName, double taskValue)
        {
            TaskName = taskName;
            TaskValue = Math.Clamp(taskValue, MinTaskValue, MaxTaskValue);
        }

        public string TaskName { get; set; }
        public double TaskValue { get; set; }

        public override string ToString()
        {
            return $"{TaskName} ({TaskValue})";
        }
    }
}
