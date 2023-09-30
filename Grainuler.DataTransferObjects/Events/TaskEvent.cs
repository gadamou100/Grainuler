namespace Grainuler.DataTransferObjects.Events
{
    public abstract record TaskEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string TaskId { get; init; }
        public string Message { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public ushort RetriesNumber { get; init; }
        public ulong ExecutionNumber { get; init; }
        public ScheduleTaskGrainInitiationParameter InitiationParameter { get; init; }
        public string TriggerId { get; init; }
        public double DurationInMinutes => (EndTime - StartTime).TotalMinutes;
    }
}