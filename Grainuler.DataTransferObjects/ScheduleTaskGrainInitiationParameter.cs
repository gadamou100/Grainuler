using Grainuler.DataTransferObjects.Triggers;

namespace Grainuler.DataTransferObjects
{



    public class ScheduleTaskGrainInitiationParameter
    {
        public Payload Payload { get; set; }
        public ICollection<Trigger> Triggers { get; set; }


        public ScheduleTaskGrainInitiationParameter()
        {
            Triggers = new List<Trigger>();
        }
    }

}
