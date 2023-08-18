using Grainuler.Abstractions;
using Grainuler.DataTransferObjects.Events;
using Grainuler.DataTransferObjects.Exceptions;
using Orleans;
using Orleans.Streams;

namespace Grainuler
{
    public class TaskCompletedEventObserver : Grain, IAsyncObserver<TaskEvent>, ITaskCompletedEventObserver
    {

        private readonly IClusterClient _clusterClient;
        private Dictionary<string, StreamSubscriptionHandle<TaskEvent>> _subscriptions;

        public TaskCompletedEventObserver(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
            _subscriptions = new Dictionary<string, StreamSubscriptionHandle<TaskEvent>>();
        }

        public async Task SetSubscription(string taskId)
        {
            var taskGrain = _clusterClient.GetGrain<IScheduleTaskGrain>(taskId);
            var streamProvider = GetStreamProvider(ScheduleTaskGrainBuilder.StreamProviderName);
            var streamId = await taskGrain.GetStreamId();
            var stream = streamProvider.GetStream<TaskEvent>(streamId, "default");
            if (stream != null)
            {
                var subscription = await stream.SubscribeAsync(this);
                if (!_subscriptions.ContainsKey(taskId))
                    _subscriptions.Add(taskId, subscription);
                else
                {
                    var oldSubscription = _subscriptions[taskId];
                    await oldSubscription.UnsubscribeAsync();
                    _subscriptions[taskId] = subscription;
                }
            }
        }

        public async override Task OnActivateAsync()
        {
             await base.OnActivateAsync();
          
        }

        public Task OnCompletedAsync()
        {
            Console.WriteLine($"Task Obeservation Completed at {DateTime.Now}");
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }

        public Task OnErrorAsync(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"Task Failed at {DateTime.Now}");

            if (ex is  TaskFailedException failedException)
            {
                Console.WriteLine($"Task Id: {failedException.Event?.TaskId}");
                Console.WriteLine($"Trigger Id: {failedException.Event?.TriggerId}");
                Console.WriteLine($"Execution Number: {failedException.Event?.ExecutionNumber}");
                Console.WriteLine($"Retry Number: {failedException.Event?.RetriesNumber}");
                Console.WriteLine($"Trigger Time: {failedException.Event?.StartTime}");
                Console.WriteLine($"Completed Time: {failedException.Event?.EndTime}");

            }
            Console.WriteLine($"{ex}");
            Console.WriteLine($"{ex.StackTrace}");
            Console.ResetColor();
            return Task.CompletedTask;

        }

        public async Task OnNextAsync(TaskEvent item, StreamSequenceToken token = null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Task Id: {item?.TaskId}");
            Console.WriteLine($"Trigger Id: {item?.TriggerId}");
            Console.WriteLine($"Execution Number: {item?.ExecutionNumber}");
            Console.WriteLine($"Retry Number: {item?.RetriesNumber}");
            Console.WriteLine($"Trigger Time: {item?.StartTime}");
            Console.WriteLine($"Completed Time: {item?.EndTime}");
            Console.WriteLine($"Message: {item?.Message}");
            return;
        }

        public async Task Unsubscribe(string taskId)
        {
            var subscription = _subscriptions.GetValueOrDefault(taskId);
            if (subscription != default)
                await subscription.UnsubscribeAsync();
        }
    }
}
