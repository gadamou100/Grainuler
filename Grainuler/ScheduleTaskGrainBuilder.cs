using Grainuler.Abstractions;
using Grainuler.DataTransferObjects;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grainuler
{
    public class ScheduleTaskGrainBuilder : IScheduleTaskGrainBuilder
    {
        private readonly ScheduleTaskGrainInitiationParameter _scheduleTaskGrainConstructorParameter;
        private readonly string _taskName;
        private readonly IClusterClient _clusterClient;
        public ScheduleTaskGrainBuilder(IClusterClient clusterClient, string taskName)
        {
            _clusterClient = clusterClient;
            _taskName = taskName;
            _scheduleTaskGrainConstructorParameter = new ScheduleTaskGrainInitiationParameter();
        }

        public IScheduleTaskGrainBuilder AddPayload(Type type, string methodName, object[] typeArguments, object[] methodArguments)
        {
            _scheduleTaskGrainConstructorParameter.Payload = new Payload
            {
                AssemblyName = type.Assembly.FullName ?? string.Empty,
                AssemblyPath = type.Assembly.Location ?? string.Empty,
                ClassName = type.Name,
                MethodName = methodName,
                MethodArguments = methodArguments,
                TypeArguments = typeArguments
            };
            return this;
        }
        public IScheduleTaskGrainBuilder AddTrigger(TimeSpan triggerTimeSpan, TimeSpan repeatTimeSpan, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null)
        {
            maxRetryPeriod ??= TimeSpan.MaxValue;
            waitTimeWithinRetries ??= TimeSpan.FromSeconds(10);
            expiredDate ??= DateTime.MaxValue;
            expireTimeSpan ??= TimeSpan.MaxValue;
            var trigger = new Trigger
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

        public  Task<IScheduleTaskGrainBuilder> Trigger()
        {
            var friend = _clusterClient.GetGrain<IScheduleTaskGrain>(_taskName);
             friend.Initiate(_scheduleTaskGrainConstructorParameter);
            return Task.FromResult(this as IScheduleTaskGrainBuilder);
        }
    }
}
