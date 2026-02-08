

namespace CA_TelegramTaskBot.Models
{
    public class DailyReport
    {
        public DailyReport(int day, Participant participant, List<DailyTask> tasks, Coordinator coordinator)
        {
            Day = day;
            Participant = participant;
            Tasks = tasks;
            Coordinator = coordinator;
        }

        // represent one parsed message, one participants data for one day
        public int Day { get; set; }
        public Participant Participant { get; set; }

        public Coordinator Coordinator { get; set; }
        public List<DailyTask> Tasks { get; set; }

        public double TotalPoints => Tasks.Sum(t => t.TaskValue); // computed property on demand

        public string TasksSummary => string.Join(", ", Tasks.Select(t=>t.ToString()));

    }
}
