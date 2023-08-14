namespace Grainuler.DataTransferObjects
{



    public class ScheduleTaskGrainInitiationParameter
    {
        public Payload Payload { get; set; }
        public ICollection<Trigger> Triggers { get; set; }


        public ScheduleTaskGrainInitiationParameter()
        {
            Triggers = new List<Trigger>();
        }
    }

    public struct Payload
    {
        public string AssemblyName { get; set; }
        public string AssemblyPath { get; set; }

        public string ClassName { get; set; }
        public object[] TypeArguments { get; set; }
        public string MethodName { get; set; }
        public object[] MethodArguments { get; set; }



    }

    public struct Trigger
    {
        public TimeSpan TriggerTime { get; set; }
        public TimeSpan RepeatTime { get; set; }
        public TimeSpan MaxRetryPeriod { get; set; } = TimeSpan.MaxValue;
        public TimeSpan WaitTimeWithinRetries { get; set; } = TimeSpan.FromSeconds(10);
        public bool IsExpnentailBackoffRetry { get; set; } = false;
        public ushort MaxRetryNumber { get; set; } = 3;
        public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.MaxValue;
        public DateTime ExpireDate { get; set; } = DateTime.MaxValue;
        public string TriggerId => $"{TriggerTime}_{RepeatTime}";

    }
}
