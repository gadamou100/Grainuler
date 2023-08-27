namespace Grainuler.DataTransferObjects.Triggers
{
    public class TaskSuccededTrigger : ReactiveTrigger
    {
        public TaskSuccededTrigger()
        {
        }
        public TaskSuccededTrigger(string taskId) : base(taskId)
        {
        }


        public override string TriggerId => $"Task_Completed_{TaskId}";

    }

}
