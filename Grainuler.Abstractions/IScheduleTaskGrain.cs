using Grainuler.DataTransferObjects;
using Orleans;

namespace Grainuler.Abstractions
{
    public interface IScheduleTaskGrain : IGrainWithStringKey, IRemindable
    {
        Task<Guid> GetStreamId();
        Task Initiate(ScheduleTaskGrainInitiationParameter parameter);
        Task<Grainuler.DataTransferObjects.Enums.TaskStatus> GetCurrentStatus();
    }
}
