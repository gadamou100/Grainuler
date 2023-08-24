namespace Grainuler.DataTransferObjects.Events
{
    public record TaskFailedEvent : TaskEvent
    {
        public ICollection<(Exception Exception, DateTime Occurence)> Exceptions { get; set; }
        public TaskFailedEvent()
        {
            Exceptions = new List<(Exception Exception, DateTime Occurence)>();
        }

    }
}