using Grainuler.DataTransferObjects.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grainuler.DataTransferObjects
{
    public record TaskManagerDTO
    {
        public TaskEvent Event { get; set; }
        public DateTime? NextRun { get; set; } 
    }
}
