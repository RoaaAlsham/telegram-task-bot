

namespace TelegramTaskBot
{
    public class TaskData
    {
        public TaskData(string person, string task, double value)
        {
            Person = person;
            Task = task;
            Value = value;
        }

        public string Person { get; set; }
        public string Task { get; set; }
        public double Value { get; set; }

    }
}
