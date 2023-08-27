using Grainuler.DataTransferObjects;
using Grainuler.DataTransferObjects.Triggers;

namespace Grainuler.Abstractions
{
    public interface IScheduleTaskGrainBuilder
    {
        ScheduleTaskGrainInitiationParameter ScheduleTaskGrainConstructorParameter { get; }
        IScheduleTaskGrain? Grain { get; }

        IScheduleTaskGrainBuilder AddPayload(Payload payload);
        IScheduleTaskGrainBuilder AddTrigger(Trigger trigger);

        IScheduleTaskGrainBuilder AddPayload(Type type, string methodName, object[]? constructorParameters = null, object[]? methodParameters = null, bool isStatic = false, Type[]? genericArguments = null, Type[]? constructorGenericArguments = null);
        IScheduleTaskGrainBuilder AddPayload(string assemblyName, string assemblyPath, string className, string methodName, object[]? constructorParameters = null, object[]? methodParameters = null, bool isStatic = false, Type[]? genericArguments = null, Type[]? constructorGenericArguments = null);
        IScheduleTaskGrainBuilder AddOnScheduleTrigger(TimeSpan triggerTimeSpan, TimeSpan repeatTimeSpan, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        IScheduleTaskGrainBuilder AddOnSuccededTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        IScheduleTaskGrainBuilder AddOnFailureTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);

        IScheduleTaskGrainBuilder AddOnRetryTrigger(string taskId, ushort fromRetry, ushort toRetry, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);


        IScheduleTaskGrainBuilder AddPayloadType(Type type);
        IScheduleTaskGrainBuilder AddPayloadMethod(string methodName);
        IScheduleTaskGrainBuilder AddConstructorParameters(params object[] constructorArguments);
        IScheduleTaskGrainBuilder AddMethodParameters(params object[] methodParameters);
        IScheduleTaskGrainBuilder AddConstructorGenericArguments(params Type[]? constructorGenericArguments);
        IScheduleTaskGrainBuilder AddMethodGenericArguments(params Type[]? methodGenericArguments);
        IScheduleTaskGrainBuilder AddIsMethodStatic(bool value);
        IScheduleTaskGrainBuilder AddOnScheduleTriggerSubscription(TimeSpan triggerTimeSpan, TimeSpan repeatTimeSpan);
        IScheduleTaskGrainBuilder AddOnSuccededTriggerSubscription(string taskId);
        IScheduleTaskGrainBuilder AddOnFailureTriggerSubscription(string taskId);
        IScheduleTaskGrainBuilder AddOnRetryTriggerSubscription(string taskId, ushort fromRetry=1, ushort toRetry=ushort.MaxValue);

        IScheduleTaskGrainBuilder AddMaxRetryNumber(ushort maxRetryNumber);
        IScheduleTaskGrainBuilder AddMaxRetryPeriod(TimeSpan retryPeriod);
        IScheduleTaskGrainBuilder AddWaitTimeWithinRetries(TimeSpan waitTimeWithinRetries);
        IScheduleTaskGrainBuilder AddExpireTimespan(TimeSpan expireTimespan);
        IScheduleTaskGrainBuilder AddExpireDate(DateTime expireDate);
        IScheduleTaskGrainBuilder AddIsExponentialBackoffRetry(bool value);




        Task Trigger();
    }

}