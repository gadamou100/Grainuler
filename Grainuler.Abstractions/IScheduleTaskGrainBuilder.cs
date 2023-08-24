using Grainuler.DataTransferObjects;

namespace Grainuler.Abstractions
{
    public interface IScheduleTaskGrainBuilder
    {
        ScheduleTaskGrainInitiationParameter ScheduleTaskGrainConstructorParameter { get; }
        IScheduleTaskGrain? Grain { get; }

        IScheduleTaskGrainBuilder AddPayload(Type type, string methodName, object[]? constructorArguments=null, object[]? methodArguments=null, bool isStatic=false, Type[]? genericArguments = null, Type[]? constructorGenericArguments = null);
        IScheduleTaskGrainBuilder AddPayload(string assemblyName, string assemblyPath, string className, string methodName, object[]? constructorArguments=null, object[]? methodArguments=null, bool isStatic=false, Type[]? genericArguments = null, Type[]? constructorGenericArguments = null);
        IScheduleTaskGrainBuilder AddOnScheduleTrigger(TimeSpan triggerTimeSpan, TimeSpan repeatTimeSpan, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        IScheduleTaskGrainBuilder AddOnSuccededTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        IScheduleTaskGrainBuilder AddOnFailureTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);

        Task Trigger();
       
    }
}