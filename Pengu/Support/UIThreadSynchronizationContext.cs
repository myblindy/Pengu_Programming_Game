using System.Collections.Concurrent;
using System.Threading;

namespace Pengu.Support
{
    class UIThreadSynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback callback, object? state)> queue = new ConcurrentQueue<(SendOrPostCallback, object?)>();

        public override void Post(SendOrPostCallback d, object? state) => queue.Enqueue((d, state));

        public void RunAllQueuedActions()
        {
            while (queue.TryDequeue(out var callbackWithState))
                callbackWithState.callback(callbackWithState.state);
        }
    }
}