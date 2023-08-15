using Grainuler.DataTransferObjects.Events;
using Orleans;
using Orleans.Streams;

namespace Grainuler.Abstractions
{
    public interface ITaskCompletedEventObserver : IGrainWithStringKey
    {
        Task OnCompletedAsync();
        Task OnErrorAsync(Exception ex);
        Task OnNextAsync(TaskEvent item, StreamSequenceToken token = null);
        Task SetSubscription(string taskId);
        Task Unsubscribe(string taskId);
    }
}