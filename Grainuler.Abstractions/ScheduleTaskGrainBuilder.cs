using Grainuler.DataTransferObjects;
using Grainuler.DataTransferObjects.Triggers;
using Orleans;

namespace Grainuler.Abstractions
{
    public class ScheduleTaskGrainBuilder : IPayloadedScheduleTaskGrainBuilder
    {
        private readonly ScheduleTaskGrainInitiationParameter _scheduleTaskGrainConstructorParameter;
        private readonly string _taskName;
        private readonly IClusterClient _clusterClient;
        public const string ProviderStorageName = "Grainuler_Store";
        public const string StreamProviderName = "Grainuler";
        public IScheduleTaskGrain? Grain { get; private set; }
        public ScheduleTaskGrainInitiationParameter ScheduleTaskGrainConstructorParameter => _scheduleTaskGrainConstructorParameter;
        public ScheduleTaskGrainBuilder(IClusterClient clusterClient, string taskName)
        {
            _clusterClient = clusterClient;
            _taskName = taskName;
            _scheduleTaskGrainConstructorParameter = new ScheduleTaskGrainInitiationParameter();
        }

        public IPayloadedScheduleTaskGrainBuilder AddPayload(Type type, string methodName, object[] constructorArguments=null, object[]? methodArguments=null, bool isStatic=false, Type[]? genericArguments=null, Type[]? constructorGenericArguments = null)
        {
            _scheduleTaskGrainConstructorParameter.Payload = new Payload
            {
                AssemblyName = type.Assembly.FullName ?? string.Empty,
                AssemblyPath = type.Assembly.Location ?? string.Empty,
                ClassName = type.Name,
                MethodName = methodName ,
                MethodParameters = methodArguments ?? Array.Empty<object>(),
                ConstructorParameters = constructorArguments ?? Array.Empty<object>(),
                IsStatic = isStatic,
                GenericMethodArguments = GetGenericArguments(genericArguments),
                ConstructorGenericArguments = GetGenericArguments(constructorGenericArguments)


            };
            return this;
        }

      

        public IPayloadedScheduleTaskGrainBuilder AddPayload(string assemblyName, string assemblyPath, string className, string methodName, object[]? constructorParameters=null, object[]? methodParameters=null, bool isStatic=false,  Type[]? genericArguments = null, Type[]? constructorGenericArguments = null)
        {
            _scheduleTaskGrainConstructorParameter.Payload = new Payload
            {
                AssemblyName = assemblyName ?? string.Empty,
                AssemblyPath = assemblyPath ?? string.Empty,
                ClassName = className,
                MethodName = methodName,
                MethodParameters = methodParameters ?? Array.Empty<object>(),
                ConstructorParameters = constructorParameters ?? Array.Empty<object>(),
                IsStatic = isStatic,
                GenericMethodArguments = GetGenericArguments(genericArguments),
                ConstructorGenericArguments = GetGenericArguments(constructorGenericArguments)
            };
            return this;
        }

        private GenericArgument[] GetGenericArguments(Type[] genericArguments)
        {
            if (genericArguments == null || !genericArguments.Any())
                return Array.Empty<GenericArgument>();
            var result = new GenericArgument[genericArguments.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new GenericArgument(genericArguments[i].FullName, genericArguments[i].Assembly.FullName);
            }
            return result;
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

        public IScheduleTaskGrainBuilder AddOnRetryTrigger(string taskId, ushort fromRetry, ushort toRetry, bool isExpnentailBackoffRetry = false, ushort maxRetryNumber = 3, TimeSpan? maxRetryPeriod = null, TimeSpan? waitTimeWithinRetries = null, TimeSpan? expireTimeSpan = null, DateTime? expiredDate = null)
        {
            maxRetryPeriod ??= TimeSpan.MaxValue;
            waitTimeWithinRetries ??= TimeSpan.FromSeconds(10);
            expiredDate ??= DateTime.MaxValue;
            expireTimeSpan ??= TimeSpan.MaxValue;
            var trigger = new TaskRetryTrigger
            {
                TaskId = taskId,
                FromRetry = fromRetry,
                ToRetry = toRetry,
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


        public Task Trigger()
        {
            Grain = _clusterClient.GetGrain<IScheduleTaskGrain>(_taskName);
            Grain.Initiate(_scheduleTaskGrainConstructorParameter);
            return Task.FromResult(this as IScheduleTaskGrainBuilder);
        }

        public IPayloadedScheduleTaskGrainBuilder AddPayloadType(Type type)
        {
            _scheduleTaskGrainConstructorParameter.Payload ??= new Payload();
            _scheduleTaskGrainConstructorParameter.Payload.AssemblyName = type.Assembly.FullName ?? string.Empty;
            _scheduleTaskGrainConstructorParameter.Payload.AssemblyPath = type.Assembly.Location ?? string.Empty;
            _scheduleTaskGrainConstructorParameter.Payload.ClassName = type.Name;


            return this;

        }

        public IPayloadedScheduleTaskGrainBuilder AddPayloadMethod(string methodName)
        {
            _scheduleTaskGrainConstructorParameter.Payload ??= new Payload();
            _scheduleTaskGrainConstructorParameter.Payload.MethodName = methodName;
            return this;
        }

        public IPayloadedScheduleTaskGrainBuilder AddConstructorParameters(params object[] constructorParameters)
        {
            _scheduleTaskGrainConstructorParameter.Payload ??= new Payload();
            _scheduleTaskGrainConstructorParameter.Payload.ConstructorParameters = constructorParameters;
            return this;
        }

        public IPayloadedScheduleTaskGrainBuilder AddMethodParameters(params object[] methodParameters)
        {
            _scheduleTaskGrainConstructorParameter.Payload ??= new Payload();
            _scheduleTaskGrainConstructorParameter.Payload.MethodParameters = methodParameters;
            return this;
        }

        public IPayloadedScheduleTaskGrainBuilder AddConstructorGenericArguments(params Type[]? constructorGenericArguments)
        {
            _scheduleTaskGrainConstructorParameter.Payload ??= new Payload();
            _scheduleTaskGrainConstructorParameter.Payload.ConstructorGenericArguments = GetGenericArguments(constructorGenericArguments);

            return this;
        }

        public IPayloadedScheduleTaskGrainBuilder AddMethodGenericArguments(params Type[]? methodGenericArguments)
        {
            _scheduleTaskGrainConstructorParameter.Payload ??= new Payload();
            _scheduleTaskGrainConstructorParameter.Payload.GenericMethodArguments = GetGenericArguments(methodGenericArguments);
            return this;
        }

        public IPayloadedScheduleTaskGrainBuilder AddIsMethodStatic(bool value)
        {
            _scheduleTaskGrainConstructorParameter.Payload ??= new Payload();
            _scheduleTaskGrainConstructorParameter.Payload.IsStatic = value;
            return this;
        }
    }
}
