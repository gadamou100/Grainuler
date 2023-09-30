using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grainuler.DataTransferObjects.ExtensionMethods
{
    public static class TimeSpanExtensions
    {
        public static DateTime GetDateFromTimespanAddition(this TimeSpan timeSpan)
        {
            try
            {
                if (timeSpan == TimeSpan.MaxValue)
                    return DateTime.MaxValue;
                var result = DateTime.UtcNow.Add(timeSpan);
                return result;
            }
            catch
            {

                return DateTime.MaxValue;
            }
        }

    }
}
