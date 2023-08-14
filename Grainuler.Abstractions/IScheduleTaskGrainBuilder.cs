namespace Grainuler.Abstractions
{
    public interface IScheduleTaskGrainBuilder
    {
        IScheduleTaskGrainBuilder AddPayload(Type type, string methodName, object[] typeArguments, object[] methodArguments);
        IScheduleTaskGrainBuilder AddTrigger(TimeSpan triggerTimeSpan, TimeSpan repeatTimeSpan, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null);
        Task<IScheduleTaskGrainBuilder> Trigger();
    }
}