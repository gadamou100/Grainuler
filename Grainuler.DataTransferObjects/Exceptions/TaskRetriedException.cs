using Grainuler.DataTransferObjects.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Grainuler.DataTransferObjects.Exceptions
{
    public class TaskRetriedException : Exception
    {
        public TaskRetriedEvent Event { get; init; }

        public TaskRetriedException(TaskRetriedEvent @event)
        {
            Event = @event;
        }

        public TaskRetriedException(string? message, TaskRetriedEvent @event, Exception? innerException) : base(message, innerException)
        {
            Event = @event;
        }

        public TaskRetriedException(string? message) : base(message)
        {
        }

        public TaskRetriedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TaskRetriedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
