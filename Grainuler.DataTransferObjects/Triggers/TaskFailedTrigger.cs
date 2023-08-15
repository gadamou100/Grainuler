namespace Grainuler.DataTransferObjects.Triggers
{
    public class TaskFailedTrigger : ReactiveTrigger
    {

        public override string TriggerId => $"Task_Failed_{TaskId}";
    }

}
