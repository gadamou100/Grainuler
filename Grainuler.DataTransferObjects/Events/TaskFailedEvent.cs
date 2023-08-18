using Grainuler.DataTransferObjects.Events;

namespace Grainuler.DataTransferObjects
{
    public class TaskFailedEvent : TaskEvent 
    {
        public ICollection<(Exception Exception, DateTime Occurence)> Exceptions { get; set; }
        public TaskFailedEvent()
        {
            Exceptions = new List<(Exception Exception, DateTime Occurence)>();
        }

    }
}