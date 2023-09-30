using Grainuler.Abstractions;
using Grainuler.DataTransferObjects;
using Grainuler.DataTransferObjects.Enums;
using Grainuler.DataTransferObjects.Events;
using TaskStatus = Grainuler.DataTransferObjects.Enums.TaskStatus;

namespace Grainuler
{
    public class ScheduleTaskGrainState
    {
        public bool Succeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public uint RetriesNumber { get; set; }
        public ulong ExecutionNumber { get; set; }
        public string TriggerId { get; set; }
        public Guid? StreamId { get; set; }
        public ScheduleTaskGrainInitiationParameter InitiationParameter { get; set; }
        public TaskStatus CurrentStatus { get; set; }

        public void Apply(TaskSuccedEvent @event)
        {
            Succeded = true;
            Message = @event.Message;
            StartTime = @event.StartTime;
            EndTime = @event.EndTime;
            RetriesNumber = @event.RetriesNumber;
            ExecutionNumber = @event.ExecutionNumber;
            InitiationParameter = @event.InitiationParameter;
            TriggerId = @event.TriggerId;
        }
        public void Apply(TaskFailedEvent @event)
        {
            Succeded = false;
            Message = @event.Message;
            StartTime = @event.StartTime;
            EndTime = @event.EndTime;
            RetriesNumber = @event.RetriesNumber;
            ExecutionNumber = @event.ExecutionNumber;
            InitiationParameter = @event.InitiationParameter;
            TriggerId = @event.TriggerId;
        }
    }
}