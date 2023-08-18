using Grainuler.DataTransferObjects;
using Grainuler.DataTransferObjects.Triggers;
using Orleans;

namespace Grainuler.Abstractions
{
    public class ScheduleTaskGrainBuilder : IScheduleTaskGrainBuilder
    {
        private readonly ScheduleTaskGrainInitiationParameter _scheduleTaskGrainConstructorParameter;
        private readonly string _taskName;
        private readonly IClusterClient _clusterClient;
        public const string ProviderStorageName = "Grainuler_Store";
        public const string StreamProviderName = "Grainuler";
        public ScheduleTaskGrainBuilder(IClusterClient clusterClient, string taskName)
        {
            _clusterClient = clusterClient;
            _taskName = taskName;
            _scheduleTaskGrainConstructorParameter = new ScheduleTaskGrainInitiationParameter();
        }

        public IScheduleTaskGrainBuilder AddPayload(Type type, string methodName, object[] typeArguments, object[] methodArguments, bool isStatic)
        {
            _scheduleTaskGrainConstructorParameter.Payload = new Payload
            {
                AssemblyName = type.Assembly.FullName ?? string.Empty,
                AssemblyPath = type.Assembly.Location ?? string.Empty,
                ClassName = type.Name,
                MethodName = methodName,
                MethodArguments = methodArguments,
                ConstructorArguments = typeArguments,
                IsStatic = isStatic
            };
            return this;
        }

        public IScheduleTaskGrainBuilder AddPayload(string assemblyName, string assemblyPath, string className, string methodName, object[] typeArguments, object[] methodArguments, bool isStatic)
        {
            _scheduleTaskGrainConstructorParameter.Payload = new Payload
            {
                AssemblyName = assemblyName ?? string.Empty,
                AssemblyPath = assemblyPath ?? string.Empty,
                ClassName = className,
                MethodName = methodName,
                MethodArguments = methodArguments,
                ConstructorArguments = typeArguments,
                IsStatic = isStatic
            };
            return this;
        }


        public IScheduleTaskGrainBuilder AddOnScheduleTrigger(TimeSpan triggerTimeSpan, TimeSpan repeatTimeSpan, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null)
        {
            maxRetryPeriod ??= TimeSpan.MaxValue;
            waitTimeWithinRetries ??= TimeSpan.FromSeconds(10);
            expiredDate ??= DateTime.MaxValue;
            expireTimeSpan ??= TimeSpan.MaxValue;
            var trigger = new ScheduleTrigger
            {
                TriggerTime = triggerTimeSpan,
                RepeatTime = repeatTimeSpan,
                IsExpnentailBackoffRetry = isExpnentailBackoffRetry,
                MaxRetryNumber = maxRetryNumber,
                MaxRetryPeriod = maxRetryPeriod.Value,
                WaitTimeWithinRetries = waitTimeWithinRetries.Value,
                ExpireDate = expiredDate.Value,
                ExpireTimeSpan = expireTimeSpan.Value,
            };
            _scheduleTaskGrainConstructorParameter.Triggers.Add(trigger);
            return this;
        }

        public IScheduleTaskGrainBuilder AddOnSuccededTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null)
        {
            maxRetryPeriod ??= TimeSpan.MaxValue;
            waitTimeWithinRetries ??= TimeSpan.FromSeconds(10);
            expiredDate ??= DateTime.MaxValue;
            expireTimeSpan ??= TimeSpan.MaxValue;
            var trigger = new TaskCompletedTrigger
            {
                TaskId = taskId,
                IsExpnentailBackoffRetry = isExpnentailBackoffRetry,
                MaxRetryNumber = maxRetryNumber,
                MaxRetryPeriod = maxRetryPeriod.Value,
                WaitTimeWithinRetries = waitTimeWithinRetries.Value,
                ExpireDate = expiredDate.Value,
                ExpireTimeSpan = expireTimeSpan.Value,
            };
            _scheduleTaskGrainConstructorParameter.Triggers.Add(trigger);
            return this;
        }


        public IScheduleTaskGrainBuilder AddOnFailureTrigger(string taskId, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null)
        {
            maxRetryPeriod ??= TimeSpan.MaxValue;
            waitTimeWithinRetries ??= TimeSpan.FromSeconds(10);
            expiredDate ??= DateTime.MaxValue;
            expireTimeSpan ??= TimeSpan.MaxValue;
            var trigger = new TaskFailedTrigger
            {
                TaskId = taskId,
                IsExpnentailBackoffRetry = isExpnentailBackoffRetry,
                MaxRetryNumber = maxRetryNumber,
                MaxRetryPeriod = maxRetryPeriod.Value,
                WaitTimeWithinRetries = waitTimeWithinRetries.Value,
                ExpireDate = expiredDate.Value,
                ExpireTimeSpan = expireTimeSpan.Value,
            };
            _scheduleTaskGrainConstructorParameter.Triggers.Add(trigger);
            return this;
        }


        public Task<IScheduleTaskGrainBuilder> Trigger()
        {
            var friend = _clusterClient.GetGrain<IScheduleTaskGrain>(_taskName);
            friend.Initiate(_scheduleTaskGrainConstructorParameter);
            return Task.FromResult(this as IScheduleTaskGrainBuilder);
        }
    }
}
