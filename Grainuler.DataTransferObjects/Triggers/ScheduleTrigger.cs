namespace Grainuler.DataTransferObjects.Triggers
{
    public class ScheduleTrigger : Trigger
    {
        public TimeSpan TriggerTime { get; set; }
        public TimeSpan RepeatTime { get; set; }

        public override string TriggerId => $"{TriggerTime}_{RepeatTime}";

        public ScheduleTrigger()
        {

        }

        public ScheduleTrigger(TimeSpan triggerTime, TimeSpan repeatTime)
        {
            TriggerTime = triggerTime;
            RepeatTime = repeatTime;
        }
    }

}
