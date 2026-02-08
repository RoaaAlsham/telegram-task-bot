

namespace CA_TelegramTaskBot.Models
{
    public class Coordinator
    {
        public int CoordinatorId { get; set; }
        public string CoordinatorName { get; set; }

        public List<Participant> Participants { get; set; }
        public Coordinator(int coordinatorId, string coordinatorName)
        {
            CoordinatorId = coordinatorId;
            //CoordinatorName = coordinatorName;
            Participants = new List<Participant>();
            CoordinatorName = coordinatorName;
        }
        public bool AddParticipant(Participant participant) {
            if (Participants.Any(p => p.ParticipantId == participant.ParticipantId)) { 
                return false; // Participant with the same ID already exists
            }
            Participants.Add(participant);
            return true;
        }
        public override string ToString()
        {
            return $"(ID: {CoordinatorId})";
        }
    }
}
