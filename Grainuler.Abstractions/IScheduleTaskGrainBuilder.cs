namespace Grainuler.Abstractions
{
    public interface IScheduleTaskGrainBuilder
    {
        IScheduleTaskGrainBuilder AddPayload(Type type, string methodName, object[] typeArguments, object[] methodArguments, bool isStatic);
        IScheduleTaskGrainBuilder AddOnScheduleTrigger(TimeSpan triggerTimeSpan, TimeSpan repeatTimeSpan, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        IScheduleTaskGrainBuilder AddOnSuccededTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        IScheduleTaskGrainBuilder AddOnFailureTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);

        Task<IScheduleTaskGrainBuilder> Trigger();
        IScheduleTaskGrainBuilder AddPayload(string assemblyName, string assemblyPath, string className, string methodName, object[] typeArguments, object[] methodArguments, bool isStatic);
    }
}