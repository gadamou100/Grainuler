using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grainuler.DataTransferObjects.Triggers
{
    public class TaskRetryTrigger : ReactiveTrigger
    {
        public const string TriggerPrefix = "Task_Retry_";
        public ushort FromRetry { get; set; } = 1;
        public ushort ToRetry { get; set; }=ushort.MaxValue;
        public override string TriggerId => $"{TriggerPrefix}{TaskId}";
    }
}
