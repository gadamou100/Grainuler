using Grainuler.Abstractions;
using Grainuler.DataTransferObjects;
using Grainuler.DataTransferObjects.Events;
using Grainuler.DataTransferObjects.Exceptions;
using Grainuler.DataTransferObjects.ExtensionMethods;
using Grainuler.DataTransferObjects.Triggers;
using Orleans;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using System.Diagnostics;

namespace Grainuler
{
    [StorageProvider(ProviderName = ScheduleTaskGrainBuilder.ProviderStorageName)]
    public class ScheduleTaskGrain : JournaledGrain<ScheduleTaskGrainState>, IScheduleTaskGrain, ITaskCompletedEventObserver, IAsyncObserver<TaskEvent>
    {
        
        private IAsyncStream<TaskEvent> _stream;
        private IAsyncStream<TaskEvent> _managerStream;

        private readonly IPayloadInvoker _payloadInvoker;
        private  Payload _payload;
        private Dictionary<string,ReactiveTrigger> _reactiveTriggers;
        private Guid? streamId;
        private readonly IClusterClient _clusterClient;
        private  Dictionary<string,StreamSubscriptionHandle<TaskEvent>> _subscriptions;

        private readonly Dictionary<string,(IGrainReminder Reminder, Trigger Trigger)> _grainReminders;
        public ScheduleTaskGrain(IClusterClient clusterClient, IPayloadInvoker payloadInvoker)
        {

            _grainReminders = new Dictionary<string, (IGrainReminder Reminder, Trigger Trigger)>();
            _clusterClient = clusterClient;
            _subscriptions = new Dictionary<string, StreamSubscriptionHandle<TaskEvent>>();
            _payloadInvoker = payloadInvoker;
        }

        public override async Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider(ScheduleTaskGrainBuilder.StreamProviderName);
            //todo retrieve it from the state.
            streamId =  State?.StreamId;
            if (streamId == null)
                streamId = Guid.NewGuid();
            
            _stream = streamProvider.GetStream<TaskEvent>(streamId.Value, "default");
            _managerStream = streamProvider.GetStream<TaskEvent>(Guid.Empty,"default");
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
                    await RegisterScheduleTrigger(key, scheduleTrigger);
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

        private async Task RegisterScheduleTrigger(string? key, ScheduleTrigger trigger)
        {
            string reminderName = $"Schedule_{key}_{trigger.TriggerId}";
            var grainReminder = await RegisterOrUpdateReminder(
          reminderName,
          trigger.TriggerTime,
          trigger.RepeatTime);
            if (_grainReminders.ContainsKey(reminderName))
                _grainReminders[reminderName] = (grainReminder, trigger);
            else
                _grainReminders.Add(reminderName, (grainReminder, trigger));

            await RaiseTaskRegisteredEvent(trigger.TriggerId);
        }


        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if(_grainReminders.TryGetValue(reminderName,out var grainReminder))
            {
                if (grainReminder.Trigger.HasExpired())
                {
                    await UnregisterReminder(grainReminder.Reminder);
                    await RaiseTriggerExpiredEvent(grainReminder.Trigger.TriggerId);


                    if (HaveAllRemindersExpired())
                        await RaiseTaskExpiredEvent();

                }
                else
                {
                    await RaiseTaskStartedEvent(grainReminder.Trigger.TriggerId);
                    await Execute(grainReminder.Trigger);
                }
            }
           

        }

        private bool HaveAllRemindersExpired()
        {
            foreach (var item in _grainReminders)
            {
                if(!item.Value.Trigger.HasExpired())
                    return false;
            }
            return true;
        }

      
        private async Task Execute(Trigger trigger)
        {
            ushort currentRetry = 0;
            var startExecutionTime = DateTime.UtcNow;
            var retryUntil = trigger.MaxRetryPeriod.GetDateFromTimespanAddition();
            var waitTime = trigger.WaitTimeWithinRetries;
            bool isCurrentExecutionAttempSuccess=false;
            var exceptions = new List<(Exception Exception, DateTime Occurence)>();
            do
            {
                try
                {
                    if(currentRetry >0)
                        await RaiseRetryEvent(currentRetry,startExecutionTime,DateTime.UtcNow,trigger.TriggerId,exceptions.LastOrDefault().Exception);
                    Trace.WriteLine($"Executing at {DateTime.Now}");
                    var result = await _payloadInvoker.Invoke(_payload);
                    isCurrentExecutionAttempSuccess = true;
                    Trace.WriteLine($"Execution finished at {DateTime.Now}");
                    break;

                }
                catch (Exception e)
                {
                  
                    (bool retriesHaveBeenExhausted, currentRetry, waitTime) = await WaitForRetry(exceptions, e, retryUntil, currentRetry, trigger.MaxRetryNumber, trigger.IsExpnentialBackoffRetry, waitTime);
                    if (retriesHaveBeenExhausted)
                    {
                        Trace.WriteLine($"Execution failed at {DateTime.Now} after {currentRetry} number of retries");
                        break;
                    }
                }

            } while (true);
            var stopExecutionTime = DateTime.UtcNow;
            if (isCurrentExecutionAttempSuccess)
                await RaiseSuccedEvent(currentRetry, startExecutionTime, stopExecutionTime, trigger.TriggerId);
            else
                await RaiseFailureEvent(currentRetry, startExecutionTime, stopExecutionTime,trigger.TriggerId, exceptions);
        }
        private async Task<(bool,ushort,TimeSpan)> WaitForRetry(List<(Exception Exception, DateTime Occurence)> exceptions,Exception currentException,DateTime retryUntil,ushort currentRetry,int maxRetryNumber,bool isExpnentailBackoffRetry, TimeSpan waitTime)
        {
            exceptions.Add((currentException, DateTime.UtcNow));
            if (currentRetry >= maxRetryNumber || retryUntil < DateTime.UtcNow)
            {
                return (true, currentRetry, waitTime);
            }
            if (isExpnentailBackoffRetry && currentRetry != 0)
                waitTime = waitTime * 2;
            await Task.Delay(waitTime);
            currentRetry++;
            
            return (false,currentRetry,waitTime);

        }

        private async Task RaiseTaskRegisteredEvent(string triggerId)
        {
            this.GetPrimaryKey(out var key);
            var now = DateTime.UtcNow;
            var @event = new TaskRegisteredEvent
            {
                TaskId = key,
                EndTime = now,
                StartTime = now,
                InitiationParameter = State.InitiationParameter,
                TriggerId = triggerId
            };
            base.RaiseEvent(@event);
            await ConfirmEvents();
            State.CurrentStatus = DataTransferObjects.Enums.TaskStatus.Registered;
            Task publishObserversTask = _stream.OnNextAsync(@event);
            Task publishManagerTask = _managerStream.OnNextAsync(@event);
            await publishObserversTask;
            await publishManagerTask;

        }

        private async Task RaiseTaskStartedEvent(string triggerId)
        {
            this.GetPrimaryKey(out var key);
            var now = DateTime.UtcNow;
            var @event = new TaskStartedEvent
            {
                TaskId = key,
                EndTime = now,
                StartTime = now,
                InitiationParameter = State.InitiationParameter,
                TriggerId = triggerId
            };
            base.RaiseEvent(@event);
            await ConfirmEvents();
            State.CurrentStatus = DataTransferObjects.Enums.TaskStatus.Started;
            Task publishObserversTask = _stream.OnNextAsync(@event);
            Task publishManagerTask = _managerStream.OnNextAsync(@event);
            await publishObserversTask;
            await publishManagerTask;
        }

        private async Task RaiseTriggerExpiredEvent(string triggerId)
        {
            this.GetPrimaryKey(out var key);
            var now = DateTime.UtcNow;
            var @event = new TriggerExpiredEvent
            {
                TaskId = key,
                EndTime = now,
                StartTime = now,
                InitiationParameter = State.InitiationParameter,
                TriggerId = triggerId
            };
            base.RaiseEvent(@event);
            await ConfirmEvents();
            Task publishObserversTask = _stream.OnNextAsync(@event);
            Task publishManagerTask = _managerStream.OnNextAsync(@event);
            await publishObserversTask;
            await publishManagerTask;

        }
        private async Task RaiseTaskExpiredEvent()
        {
            this.GetPrimaryKey(out var key);
            var now = DateTime.UtcNow;
            var @event = new TriggerExpiredEvent
            {
                TaskId = key,
                EndTime = now,
                StartTime = now,
                InitiationParameter = State.InitiationParameter,
            };
            base.RaiseEvent(@event);
            await ConfirmEvents();
            State.CurrentStatus = DataTransferObjects.Enums.TaskStatus.Expired;
            Task publishObserversTask = _stream.OnNextAsync(@event);
            Task publishManagerTask = _managerStream.OnNextAsync(@event);
            await publishObserversTask;
            await publishManagerTask;

        }


        private async Task RaiseSuccedEvent(ushort retryNumber, DateTime startTime, DateTime endTime, string triggerId) 
        {
            this.GetPrimaryKey(out var key);
            var @event = new TaskSuccedEvent
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
            State.CurrentStatus = DataTransferObjects.Enums.TaskStatus.Succeded;
            Task publishObserversTask = _stream.OnNextAsync(@event);
            Task publishManagerTask = _managerStream.OnNextAsync(@event);
            await publishObserversTask;
            await publishManagerTask;
        }

        private async Task RaiseRetryEvent(ushort currentRetry, DateTime startTime, DateTime endTime, string triggerId, Exception exception)

        {
            this.GetPrimaryKey(out var key);
            var @event = new TaskRetriedEvent
            {
                TaskId = key,
                EndTime = endTime,
                StartTime = startTime,
                ExecutionNumber = State.ExecutionNumber + 1,
                RetriesNumber = currentRetry,
                InitiationParameter = State.InitiationParameter,
                TriggerId = triggerId,
                Exception = exception,
                CurrentRetry = currentRetry
            };
            var taskRetriedException = new TaskRetriedException(@event);
            base.RaiseEvent(@event);
            await ConfirmEvents();
            State.CurrentStatus = DataTransferObjects.Enums.TaskStatus.Retried;
            Task publishObserversTask = _stream.OnErrorAsync(taskRetriedException);
            Task publishManagerTask = _managerStream.OnNextAsync(@event);
            await publishObserversTask;
            await publishManagerTask;
        }

        private async Task RaiseFailureEvent(ushort retryNumber, DateTime startTime, DateTime endTime, string triggerId, List<(Exception Exception, DateTime Occurence)> exceptions)

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
                TriggerId = triggerId,
                Exceptions = exceptions
            };
            var taskFailedException = new TaskFailedException(@event);
            base.RaiseEvent(@event);
            await ConfirmEvents();
            State.CurrentStatus = DataTransferObjects.Enums.TaskStatus.Failed;
            Task publishObserversTask = _stream.OnErrorAsync(taskFailedException);
            Task publishManagerTask = _managerStream.OnNextAsync(@event);
            await publishObserversTask;
            await publishManagerTask;
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public async Task OnErrorAsync(Exception ex)
        {
            if(ex is TaskFailedException failedException)
            {
                var trigger = _reactiveTriggers.GetValueOrDefault($"{TaskFailedTrigger.TriggerPrefix}{failedException.Event.TaskId}");
                if(trigger != default)
                    await Execute(trigger);
            }
            if (ex is TaskRetriedException retryException)
            {
                var trigger = _reactiveTriggers.GetValueOrDefault($"{TaskRetryTrigger.TriggerPrefix}{retryException.Event.TaskId}") as TaskRetryTrigger;
                if (trigger != default && retryException.Event.CurrentRetry >= trigger.FromRetry && retryException.Event.CurrentRetry <= trigger.ToRetry)
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
            var streamProvider = GetStreamProvider(ScheduleTaskGrainBuilder.StreamProviderName);
            var streamId = await taskGrain.GetStreamId();
            var stream = streamProvider.GetStream<TaskEvent>(streamId, "default");
            return stream;
        }

        public async Task<Grainuler.DataTransferObjects.Enums.TaskStatus> GetCurrentStatus()
        {

            DataTransferObjects.Enums.TaskStatus result = State?.CurrentStatus ?? DataTransferObjects.Enums.TaskStatus.NA;
            if(result != DataTransferObjects.Enums.TaskStatus.Expired && HaveAllRemindersExpired())
                await RaiseTaskExpiredEvent();

            return result;
        }
    }
}
