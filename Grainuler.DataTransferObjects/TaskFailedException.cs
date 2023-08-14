using System.Runtime.Serialization;

namespace Grainuler.DataTransferObjects
{
    public class TaskFailedException : Exception
    {
        public TaskFailedEvent Event { get; init; }
        public TaskFailedException()
        {
        }

        public TaskFailedException(TaskFailedEvent @event, Exception? innerException) : base(innerException?.Message, innerException)
        {
            Event = @event;
        }


        public TaskFailedException(string? message, TaskFailedEvent @event, Exception? innerException) : base(message, innerException)
        {
            Event = @event;
        }

        public TaskFailedException(string? message) : base(message)
        {
        }

        public TaskFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TaskFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
