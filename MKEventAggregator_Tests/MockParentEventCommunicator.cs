using System;
using MKEventAggregator;
using MKEventAggregator.Plugins;

namespace EventAggregator_Tests
{
    public class MockParentEventCommunicator : IEventCommunicator
    {
        public  Guid CommunicatorIdInternal = Guid.NewGuid();
        private IEventAggregator _eventAggregator;

        public IEventCommunicator ParentCommunicator { get { return null; } }
        public Guid CommunicatorId { get { return CommunicatorIdInternal; } }
        public MockEventForChildCommunicator LastPublishedEvent;


        public void SetEventAggregator(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void PublishToAll()
        {
            publishToEvent(EventCommunicatorsRelationship.All);
        }

        public void PublishToChild()
        {
            publishToEvent(EventCommunicatorsRelationship.Child);
        }

        public void PublishToSiblingChild()
        {
            publishToEvent(EventCommunicatorsRelationship.ClosestParent);
        }


        public void PublishToClosestParent()
        {
            publishToEvent(EventCommunicatorsRelationship.ClosestParent);
        }


        public void SubscribeToAll()
        {
            subscribeToEvent(EventCommunicatorsRelationship.All);
        }

        private void publishToEvent(EventCommunicatorsRelationship relationship)
        {
            var msg = _eventAggregator.GetEvent<Event<MockEventForChildCommunicator>>();

            msg.Publish(this, new MockEventForChildCommunicator(this), relationship);
        }

        private void subscribeToEvent(EventCommunicatorsRelationship relationship)
        {
            var msg = _eventAggregator.GetEvent<Event<MockEventForChildCommunicator>>();

            msg.Subscribe(x => LastPublishedEvent = x, this, relationship);
        }

    }
}