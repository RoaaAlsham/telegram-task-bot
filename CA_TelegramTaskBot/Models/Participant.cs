

namespace CA_TelegramTaskBot.Models
{
    
    public class Participant
    {
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public int CoordinatorId { get; set; }

        public Participant(int participantId, string participantName, int coordinatorId)
        {
            ParticipantId = participantId;
            ParticipantName = participantName;
            CoordinatorId = coordinatorId;
        }
    }
}
