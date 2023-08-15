namespace Grainuler.DataTransferObjects.Triggers
{
    public abstract class Trigger
    {
        public TimeSpan MaxRetryPeriod { get; set; } = TimeSpan.MaxValue;
        public TimeSpan WaitTimeWithinRetries { get; set; } = TimeSpan.FromSeconds(10);
        public bool IsExpnentailBackoffRetry { get; set; } = false;
        public ushort MaxRetryNumber { get; set; } = 3;
        public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.MaxValue;
        public DateTime ExpireDate { get; set; } = DateTime.MaxValue;
        public abstract string TriggerId { get; }

    }

}
