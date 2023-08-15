namespace Grainuler.DataTransferObjects.Triggers
{
    public class TaskCompletedTrigger : ReactiveTrigger
    {
        public override string TriggerId => $"Task_Completed_{TaskId}";
    }

}
