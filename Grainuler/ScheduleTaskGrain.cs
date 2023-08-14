using Grainuler.Abstractions;
using Grainuler.DataTransferObjects;
using Orleans;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;

namespace Grainuler
{
    [StorageProvider(ProviderName = ProviderStorageName)]
    public class ScheduleTaskGrain : JournaledGrain<ScheduleTaskGrainState>, IScheduleTaskGrain
    {
        public const string ProviderStorageName = "Grainuler_Store";
        public const string StreamProviderName = "Grainuler";
        private IAsyncStream<TaskEvent> _stream;
        private  Payload _payload;
        private IEnumerable<Trigger> _triggers;
        private Guid? streamId;


        private readonly Dictionary<string,(IGrainReminder Reminder, Trigger Trigger)> _grainReminders;
        public ScheduleTaskGrain()
        {

            _grainReminders = new Dictionary<string, (IGrainReminder Reminder, Trigger Trigger)>();
        }

        public override async Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider(StreamProviderName);
            //todo retrieve it from the state.
            streamId =  State?.StreamId;
            if (streamId == null)
                streamId = Guid.NewGuid();
            
            _stream = streamProvider.GetStream<TaskEvent>(streamId.Value, "default");
            //await _stream.SubscribeAsync(new TaskCompletedEventObserver());
            await  base.OnActivateAsync();
        }
        public Task<Guid> GetStreamId() => Task.FromResult(streamId ?? default);

        public async Task Initiate(ScheduleTaskGrainInitiationParameter parameter)
        {
            State.InitiationParameter =parameter;
            State.StreamId = streamId;
            this.GetPrimaryKey(out var key);

            _payload = parameter.Payload;
            _triggers = parameter.Triggers;
       


            foreach (var trigger in _triggers)
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
            }
          
            
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
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Executing at {DateTime.Now}");
                    await PayloadInvoker.Invoke(_payload);
                    isCurrentExecutionAttempSuccess = true;
                    Console.WriteLine($"Finish Execution at {DateTime.Now}");
                    Console.ResetColor();
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
    }
}
