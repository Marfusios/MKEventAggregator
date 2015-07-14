using MKEventAggregator;
using MKEventAggregator.Plugins;
using Xunit;

namespace EventAggregator_Tests
{
    public class EventCommunicatorTests
    {

        // ***************************
        // TEST SUBSCRIBING
        // ***************************

        [Fact]
        public void PublishToAllSubscribeToAll()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParent = initParentCommunicator(aggregator);
            var communicatorChild = initChildCommunicator(aggregator, communicatorParent);

            communicatorChild.SubscribeToAll();

            communicatorParent.PublishToAll();
            Assert.Same(communicatorParent, communicatorChild.LastPublishedEvent.Sender);

            var msg = aggregator.GetEvent<Event<MockEventForChildCommunicator>>();
            msg.Publish(null, new MockEventForChildCommunicator(this));

            Assert.Same(this, communicatorChild.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToAllSubscribeToParent()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParent = initParentCommunicator(aggregator);
            var communicatorChild = initChildCommunicator(aggregator, communicatorParent);

            communicatorChild.SubscribeToParent();


            communicatorParent.PublishToAll();
            var receiviedEventFromParent = communicatorChild.LastPublishedEvent;
            Assert.Same(communicatorParent, receiviedEventFromParent.Sender);

            var msg = aggregator.GetEvent<Event<MockEventForChildCommunicator>>();
            msg.Publish(null, new MockEventForChildCommunicator(this));

            Assert.Same(communicatorParent, communicatorChild.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToAllSubscribeToParentTwoLevel()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParent = initParentCommunicator(aggregator);
            var communicatorChild = initChildCommunicator(aggregator, communicatorParent);
            var communicatorChild2 = initChildCommunicator(aggregator, communicatorChild);

            communicatorChild2.SubscribeToParent();

            communicatorParent.PublishToAll();
            var receiviedEventFromParent = communicatorChild2.LastPublishedEvent;
            Assert.Same(communicatorParent, receiviedEventFromParent.Sender);

            var msg = aggregator.GetEvent<Event<MockEventForChildCommunicator>>();
            msg.Publish(null, new MockEventForChildCommunicator(this));

            Assert.Same(communicatorParent, communicatorChild2.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToAllSubscribeToClosestParent()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParent = initParentCommunicator(aggregator);
            var communicatorChild = initChildCommunicator(aggregator, communicatorParent);
            var communicatorChild2 = initChildCommunicator(aggregator, communicatorChild);

            communicatorChild2.SubscribeToClosestParent();

            communicatorParent.PublishToAll();
            Assert.Null(communicatorChild2.LastPublishedEvent);

            communicatorChild.PublishToAll();
            Assert.Same(communicatorChild, communicatorChild2.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToAllSubscribeToNotOther()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParentMain = initParentCommunicator(aggregator);
            var communicatorParentOther = initParentCommunicator(aggregator);

            var communicatorChild = initChildCommunicator(aggregator, communicatorParentMain);

            communicatorChild.SubscribeToNotOther();

            communicatorParentOther.PublishToAll();
            Assert.Null(communicatorChild.LastPublishedEvent);

            communicatorParentMain.PublishToAll();
            Assert.Same(communicatorParentMain, communicatorChild.LastPublishedEvent.Sender);

            communicatorChild.PublishToAll();
            Assert.Same(communicatorChild, communicatorChild.LastPublishedEvent.Sender);
        }

        [Fact]
        public void PublishToAllSubscribeToSiblingChildAndParent()
        {
            IEventAggregator aggregator = new EventAggregator();


            var communicatorParentMain = initParentCommunicator(aggregator);

            var communicatorMainChild = initChildCommunicator(aggregator, communicatorParentMain);
            var communicatorMainChild2 = initChildCommunicator(aggregator, communicatorMainChild);
            var communicatorOtherChild = initChildCommunicator(aggregator, communicatorParentMain);
            var communicatorOtherChild2 = initChildCommunicator(aggregator, communicatorOtherChild);


            communicatorMainChild.SubscribeToSiblingChildAndParent();

            communicatorMainChild2.PublishToAll();
            Assert.Null(communicatorMainChild.LastPublishedEvent);

            communicatorOtherChild.PublishToAll();
            Assert.Null(communicatorMainChild.LastPublishedEvent);

            communicatorOtherChild2.PublishToAll();
            Assert.Same(communicatorOtherChild2, communicatorMainChild.LastPublishedEvent.Sender);

            communicatorParentMain.PublishToAll();
            Assert.Same(communicatorParentMain, communicatorMainChild.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToAllSubscribeToSiblingChild()
        {
            IEventAggregator aggregator = new EventAggregator();


            var communicatorParentMain = initParentCommunicator(aggregator);

            var communicatorMainChild = initChildCommunicator(aggregator, communicatorParentMain);
            var communicatorMainChild2 = initChildCommunicator(aggregator, communicatorMainChild);
            var communicatorOtherChild = initChildCommunicator(aggregator, communicatorParentMain);
            var communicatorOtherChild2 = initChildCommunicator(aggregator, communicatorOtherChild);


            communicatorMainChild.SubscribeToSiblingChild();

            communicatorMainChild2.PublishToAll();
            Assert.Null(communicatorMainChild.LastPublishedEvent);

            communicatorOtherChild.PublishToAll();
            Assert.Null(communicatorMainChild.LastPublishedEvent);

            communicatorParentMain.PublishToAll();
            Assert.Null(communicatorMainChild.LastPublishedEvent);

            communicatorOtherChild2.PublishToAll();
            Assert.Same(communicatorOtherChild2, communicatorMainChild.LastPublishedEvent.Sender);
        }


        // ***************************
        // TEST PUBLISHING
        // ***************************

        [Fact]
        public void PublishToChildSubscribeToAll()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParent = initParentCommunicator(aggregator);
            var communicatorOther = initParentCommunicator(aggregator);
            var communicatorChild = initChildCommunicator(aggregator, communicatorParent);

            communicatorChild.SubscribeToAll();

            communicatorOther.PublishToChild();
            Assert.Null(communicatorChild.LastPublishedEvent);

            communicatorParent.PublishToChild();
            Assert.Same(communicatorParent, communicatorChild.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToChildSubscribeToAllTwoLevel()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorMainParent = initParentCommunicator(aggregator);
            var communicatorOtherParent = initParentCommunicator(aggregator);
            var communicatorMainChild = initChildCommunicator(aggregator, communicatorMainParent);
            var communicatorOtherChild = initChildCommunicator(aggregator, communicatorOtherParent);

            communicatorMainChild.SubscribeToAll();
            communicatorOtherChild.SubscribeToAll();

            communicatorMainParent.PublishToChild();
            Assert.Null(communicatorOtherChild.LastPublishedEvent);
            Assert.Same(communicatorMainParent, communicatorMainChild.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToSiblingChildSubscribeToAll()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParent = initParentCommunicator(aggregator);

            var communicatorMainParent = initChildCommunicator(aggregator, communicatorParent);
            var communicatorOtherParent = initChildCommunicator(aggregator, communicatorParent);
            var communicatorMainChild = initChildCommunicator(aggregator, communicatorMainParent);
            var communicatorOtherChild = initChildCommunicator(aggregator, communicatorOtherParent);

            communicatorMainChild.SubscribeToAll();
            communicatorOtherChild.SubscribeToAll();

            communicatorMainParent.PublishToSiblingChild();
            Assert.Null(communicatorMainChild.LastPublishedEvent);
            Assert.Same(communicatorMainParent, communicatorOtherChild.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToClosestParentSubscribeToAll()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParentRoot = initParentCommunicator(aggregator);
            var communicatorParent = initChildCommunicator(aggregator, communicatorParentRoot);

            var communicatorMainParent = initChildCommunicator(aggregator, communicatorParent);
            var communicatorOtherParent = initChildCommunicator(aggregator, communicatorParent);
            var communicatorMainChild = initChildCommunicator(aggregator, communicatorMainParent);

            communicatorParent.SubscribeToAll();
            communicatorMainParent.SubscribeToAll();
            communicatorOtherParent.SubscribeToAll();


            communicatorMainChild.PublishToClosestParent();


            Assert.Null(communicatorParent.LastPublishedEvent);
            Assert.Null(communicatorOtherParent.LastPublishedEvent);
            Assert.Same(communicatorMainChild, communicatorMainParent.LastPublishedEvent.Sender);
        }


        [Fact]
        public void PublishToParentSubscribeToClosestChild()
        {
            IEventAggregator aggregator = new EventAggregator();

            var communicatorParentRoot = initParentCommunicator(aggregator);
            var communicatorParent = initChildCommunicator(aggregator, communicatorParentRoot);

            var communicatorMainParent = initChildCommunicator(aggregator, communicatorParent);
            var communicatorOtherParent = initChildCommunicator(aggregator, communicatorParent);
            var communicatorMainChild = initChildCommunicator(aggregator, communicatorMainParent);

            communicatorParent.SubscribeToClosestChild();
            communicatorMainParent.SubscribeToClosestChild();
            communicatorOtherParent.SubscribeToClosestChild();
            communicatorParentRoot.SubscribeToAll();


            communicatorMainChild.PublishToParent();


            Assert.Null(communicatorParent.LastPublishedEvent);
            Assert.Null(communicatorOtherParent.LastPublishedEvent);
            Assert.Same(communicatorMainChild, communicatorMainParent.LastPublishedEvent.Sender);
            Assert.Same(communicatorMainChild, communicatorParentRoot.LastPublishedEvent.Sender);
        }



        private MockParentEventCommunicator initParentCommunicator(IEventAggregator aggregator)
        {
            var comm = new MockParentEventCommunicator();
            comm.SetEventAggregator(aggregator);
            return comm;
        }

        private MockChildEventCommunicator initChildCommunicator(IEventAggregator aggregator, IEventCommunicator parent)
        {
            var comm = new MockChildEventCommunicator(parent);
            comm.SetEventAggregator(aggregator);
            return comm;
        }
    }
}
