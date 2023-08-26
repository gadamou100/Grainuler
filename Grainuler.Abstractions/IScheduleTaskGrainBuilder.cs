using Grainuler.DataTransferObjects;

namespace Grainuler.Abstractions
{
    public interface IScheduleTaskGrainBuilder
    {
        ScheduleTaskGrainInitiationParameter ScheduleTaskGrainConstructorParameter { get; }
        IScheduleTaskGrain? Grain { get; }

        IPayloadedScheduleTaskGrainBuilder AddPayload(Type type, string methodName, object[]? constructorParameters=null, object[]? methodParameters=null, bool isStatic=false, Type[]? genericArguments = null, Type[]? constructorGenericArguments = null);
        IPayloadedScheduleTaskGrainBuilder AddPayload(string assemblyName, string assemblyPath, string className, string methodName, object[]? constructorParameters=null, object[]? methodParameters=null, bool isStatic=false, Type[]? genericArguments = null, Type[]? constructorGenericArguments = null);
        IScheduleTaskGrainBuilder AddOnScheduleTrigger(TimeSpan triggerTimeSpan, TimeSpan repeatTimeSpan, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        IScheduleTaskGrainBuilder AddOnSuccededTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        IScheduleTaskGrainBuilder AddOnFailureTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);

        IScheduleTaskGrainBuilder AddOnRetryTrigger(string taskId, ushort fromRetry, ushort toRetry, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);

        Task Trigger();
       
    }

}