using Grainuler.Abstractions;
using Grainuler.DataTransferObjects;
using Grainuler.DataTransferObjects.Events;
using Grainuler.DataTransferObjects.Exceptions;
using Grainuler.DataTransferObjects.Triggers;
using Orleans;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using System.Diagnostics;

namespace Grainuler
{
    [StorageProvider(ProviderName = ProviderStorageName)]
    public class ScheduleTaskGrain : JournaledGrain<ScheduleTaskGrainState>, IScheduleTaskGrain, ITaskCompletedEventObserver, IAsyncObserver<TaskEvent>
    {
        public const string ProviderStorageName = "Grainuler_Store";
        public const string StreamProviderName = "Grainuler";
        private IAsyncStream<TaskEvent> _stream;
        private  Payload _payload;
        private Dictionary<string,ReactiveTrigger> _reactiveTriggers;
        private Guid? streamId;
        private readonly IClusterClient _clusterClient;
        private  Dictionary<string,StreamSubscriptionHandle<TaskEvent>> _subscriptions;

        private readonly Dictionary<string,(IGrainReminder Reminder, Trigger Trigger)> _grainReminders;
        public ScheduleTaskGrain(IClusterClient clusterClient)
        {

            _grainReminders = new Dictionary<string, (IGrainReminder Reminder, Trigger Trigger)>();
            _clusterClient = clusterClient;
            _subscriptions = new Dictionary<string, StreamSubscriptionHandle<TaskEvent>>();
        }

        public override async Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider(StreamProviderName);
            //todo retrieve it from the state.
            streamId =  State?.StreamId;
            if (streamId == null)
                streamId = Guid.NewGuid();
            
            _stream = streamProvider.GetStream<TaskEvent>(streamId.Value, "default");
            await InitiateReactivedTriggers();
            await  base.OnActivateAsync();
        }

        private async Task InitiateReactivedTriggers()
        {
            _reactiveTriggers = new Dictionary<string, ReactiveTrigger>();
            var triggers = State?.InitiationParameter?.Triggers ?? Array.Empty<Trigger>();
            foreach (var trigger in triggers)
            {
                if(trigger is ReactiveTrigger reactiveTrigger)
                {
                    _ = _reactiveTriggers.Remove(reactiveTrigger.TriggerId);
                    await SetSubscription(reactiveTrigger.TaskId);
                    _reactiveTriggers.Add(reactiveTrigger.TriggerId, reactiveTrigger);
                }
            }
        }

        public Task<Guid> GetStreamId() => Task.FromResult(streamId ?? default);

        public async Task Initiate(ScheduleTaskGrainInitiationParameter parameter)
        {
            State.InitiationParameter =parameter;
            State.StreamId = streamId;
            this.GetPrimaryKey(out var key);

            _payload = parameter.Payload;
            var triggers = parameter?.Triggers?
                .GroupBy(p=>p.TriggerId)
                .ToDictionary(p=>p.Key,p=>p.First())
                ?? new Dictionary<string, Trigger>();
       


            foreach (var trigger in triggers.Values)
            {
                if (trigger is ScheduleTrigger scheduleTrigger)
                    await RegisterScheduleTrigger(key, scheduleTrigger, scheduleTrigger);
                else if (trigger is ReactiveTrigger reactiveTrigger )
                {
                    if (reactiveTrigger.TaskId != key && !_reactiveTriggers.ContainsKey(reactiveTrigger.TriggerId))
                    {
                        await SetSubscription(reactiveTrigger.TaskId);
                        _reactiveTriggers.Add(reactiveTrigger.TriggerId, reactiveTrigger);
                    }
                }
                
            }   
        }

        private async Task RegisterScheduleTrigger(string? key, ScheduleTrigger trigger, ScheduleTrigger scheduleTrigger)
        {
            string reminderName = $"Schedule_{key}_{trigger.TriggerId}";
            var grainReminder = await RegisterOrUpdateReminder(
          reminderName,
          scheduleTrigger.TriggerTime,
          trigger.RepeatTime);
            if (_grainReminders.ContainsKey(reminderName))
                _grainReminders[reminderName] = (grainReminder, trigger);
            else
                _grainReminders.Add(reminderName, (grainReminder, trigger));
        }


        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if(_grainReminders.TryGetValue(reminderName,out var grainReminder))
            {
                if (HasReminderExpire(grainReminder.Trigger))
                        await UnregisterReminder(grainReminder.Reminder);
                else
                    await Execute(grainReminder.Trigger);
            }
           

        }

        private bool HasReminderExpire(Trigger trigger)
        {
            return GetDateFromTimespanAddition(trigger.ExpireTimeSpan) < DateTime.UtcNow || trigger.ExpireDate < DateTime.UtcNow;
        }

        private async Task Execute(Trigger trigger)
        {
            ushort currentRetry = 0;
            var startExecutionTime = DateTime.UtcNow;
            var retryUntil = GetDateFromTimespanAddition(trigger.MaxRetryPeriod);
            var waitTime = trigger.WaitTimeWithinRetries;
            bool isCurrentExecutionAttempSuccess=false;
            Exception? lastException = null;
            do
            {
                try
                {
                    Trace.WriteLine($"Executing at {DateTime.Now}");
                    await PayloadInvoker.Invoke(_payload);
                    isCurrentExecutionAttempSuccess = true;
                    Trace.WriteLine($"Finish Execution at {DateTime.Now}");
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (currentRetry > trigger.MaxRetryNumber || retryUntil < DateTime.UtcNow)
                        break;
                    if(trigger.IsExpnentailBackoffRetry && currentRetry!=0)
                        waitTime = waitTime *2;
                    await Task.Delay(waitTime);
                    currentRetry++;
                }
            }while (true);
            var stopExecutionTime = DateTime.UtcNow;
            if (isCurrentExecutionAttempSuccess)
                await RaiseSuccedEvent(currentRetry, startExecutionTime, stopExecutionTime, trigger.TriggerId);
            else
                await RaiseFailureEvent(currentRetry, startExecutionTime, stopExecutionTime,trigger.TriggerId, lastException);
        }

        private static DateTime GetDateFromTimespanAddition(TimeSpan timeSpan)
        {
            try
            {
                if(timeSpan == TimeSpan.MaxValue)
                    return DateTime.MaxValue;
                var result = DateTime.UtcNow.Add(timeSpan);
                return result;
            }
            catch
            {

                return DateTime.MaxValue;
            }
        }
        private async Task RaiseSuccedEvent(ushort retryNumber, DateTime startTime, DateTime endTime, string triggerId) 
        {
            this.GetPrimaryKey(out var key);
            var @event = new TaskEvent
            {
                TaskId =  key,
                EndTime = endTime,
                StartTime = startTime,
                ExecutionNumber = State.ExecutionNumber + 1,
                RetriesNumber = retryNumber,
                InitiationParameter = State.InitiationParameter,
                TriggerId = triggerId
            };
            base.RaiseEvent(@event);
            await ConfirmEvents();
            await _stream.OnNextAsync(@event);
        }
        private async Task RaiseFailureEvent(ushort retryNumber, DateTime startTime, DateTime endTime, string triggerId,Exception? e)

        {
            this.GetPrimaryKey(out var key);
            var @event = new TaskFailedEvent
            {
                TaskId = key,
                EndTime = endTime,
                StartTime = startTime,
                ExecutionNumber = State.ExecutionNumber + 1,
                RetriesNumber = retryNumber,
                InitiationParameter = State.InitiationParameter,
                TriggerId = triggerId
            };
            var taskFailedException = new TaskFailedException(@event, e);
            base.RaiseEvent(@event);
            await ConfirmEvents();
            await _stream.OnErrorAsync(taskFailedException);
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public async Task OnErrorAsync(Exception ex)
        {
            if(ex is TaskFailedException failedException)
            {
                var trigger = _reactiveTriggers.GetValueOrDefault($"Task_Failed_{failedException.Event.TaskId}");
                if(trigger != default)
                    await Execute(trigger);
            }
        }

        public async Task OnNextAsync(TaskEvent item, StreamSequenceToken token = null)
        {
            var trigger = _reactiveTriggers.GetValueOrDefault($"Task_Completed_{item.TaskId}");
            if (trigger != default)
                await Execute(trigger);
        }

        public async Task SetSubscription(string taskId)
        {
            IAsyncStream<TaskEvent> stream = await GetStreamFromTask(taskId);
            if(stream != null)
            {
                var subscription = await stream.SubscribeAsync(this);
                if(!_subscriptions.ContainsKey(taskId))
                    _subscriptions.Add(taskId, subscription);
                else
                {
                    var oldSubscription = _subscriptions[taskId];
                    await oldSubscription.UnsubscribeAsync();
                    _subscriptions[taskId] = subscription;
                }
            }

        }
        public async Task Unsubscribe(string taskId)
        {
            var subscription = _subscriptions.GetValueOrDefault(taskId);
            if(subscription!= default)
                await subscription.UnsubscribeAsync();
        }

        private async Task<IAsyncStream<TaskEvent>> GetStreamFromTask(string taskId)
        {
            var taskGrain = _clusterClient.GetGrain<IScheduleTaskGrain>(taskId);
            var streamProvider = GetStreamProvider(StreamProviderName);
            var streamId = await taskGrain.GetStreamId();
            var stream = streamProvider.GetStream<TaskEvent>(streamId, "default");
            return stream;
        }
    }
}
