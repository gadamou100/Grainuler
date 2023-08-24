namespace Grainuler.DataTransferObjects.Triggers
{
    public class TaskFailedTrigger : ReactiveTrigger
    {
        public const string TriggerPrefix = "Task_Failed_";
        public override string TriggerId => $"{TriggerPrefix}{TaskId}";
    }

}
