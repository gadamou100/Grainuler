namespace Grainuler.DataTransferObjects.Events
{
    public class TaskEvent
    {
        public string TaskId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ushort RetriesNumber { get; set; }
        public ulong ExecutionNumber { get; set; }
        public ScheduleTaskGrainInitiationParameter InitiationParameter { get; set; }
        public string TriggerId { get; set; }
    }
}