

using System;
using System.Threading;
using MKEventAggregator;
using MKEventAggregator.Plugins;
using Xunit;

namespace EventAggregator_Tests
{
    public class BackgroundEventSubscriptionTests
    {
        [Fact]
        public void ShouldReceiveDelegateOnDifferentThread()
        {
            ManualResetEvent completeEvent = new ManualResetEvent(false);
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            SynchronizationContext calledSyncContext = null;
            Action<object> action = delegate
            {
                calledSyncContext = SynchronizationContext.Current;
                completeEvent.Set();
            };

            IDelegateReference actionDelegateReference = new MockDelegateReference() { Target = action };
            IDelegateReference filterDelegateReference = new MockDelegateReference() { Target = (Predicate<object>)delegate { return true; } };

            var eventSubscription = new BackgroundEventSubscription<object>(actionDelegateReference, filterDelegateReference, null, EventCommunicatorsRelationship.All);


            var publishAction = eventSubscription.GetExecutionStrategy();

            Assert.NotNull(publishAction);

            publishAction.Invoke(null);

            completeEvent.WaitOne(5000);
            
            Assert.NotEqual(SynchronizationContext.Current, calledSyncContext);
        }
    }
}
