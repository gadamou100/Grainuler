using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grainuler.DataTransferObjects.Events
{
    public record TaskRetriedEvent : TaskEvent
    {
        public Exception Exception { get; set; }
        public ushort CurrentRetry { get; set; }
    }
}
