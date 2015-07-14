using System;
using MKEventAggregator;
using Xunit;

namespace EventAggregator_Tests
{
    
    public class EventAggregatorTests
    {
        [Fact]
        public void GetReturnsSingleInstancesOfSameEventType()
        {
            var eventAggregator = new EventAggregator();
            var instance1 = eventAggregator.GetEvent<MockEventBase>();
            var instance2 = eventAggregator.GetEvent<MockEventBase>();

            Assert.Equal(instance2, instance1);
        }


        [Fact]
        public void SubscribeToEventInstance1AndPublishEventButInstance2()
        {
            var eventAggregator = new EventAggregator();
            var published = false;

            var instance1 = eventAggregator.GetEvent<MockEvent>();
            instance1.Subscribe(x => published = true);

            var instance2 = eventAggregator.GetEvent<MockEvent>();
            instance2.Publish(null, "My event");

            Assert.True(published);
        }

        public class MockEventBase : EventBase { }

        public class MockEvent : Event<string> { }
    }
}