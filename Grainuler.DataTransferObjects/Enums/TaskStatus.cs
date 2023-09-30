using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grainuler.DataTransferObjects.Enums
{
    public enum TaskStatus
    {
        NA=-1,
        Registered,
        Started,
        Succeded,
        Retried,
        Failed,
        Expired
    }
}
