namespace Grainuler.DataTransferObjects.Triggers
{
    public abstract class ReactiveTrigger : Trigger
    {
        public string TaskId { get; set; }
        public ReactiveTrigger()
        {
        }

        protected ReactiveTrigger(string taskId)
        {
            TaskId = taskId;
        }
    }

}
