using System;
using MKEventAggregator;
using MKEventAggregator.Plugins;

namespace EventAggregator_Tests
{
    public class MockChildEventCommunicator : IEventCommunicator
    {
        private readonly Guid _communicatorId = Guid.NewGuid();
        private readonly IEventCommunicator _parentCommunicator;
        private IEventAggregator _eventAggregator;

        public MockChildEventCommunicator(IEventCommunicator parentCommunicator)
        {
            _parentCommunicator = parentCommunicator;
        }

        public IEventCommunicator ParentCommunicator { get { return _parentCommunicator; } }
        public Guid CommunicatorId { get { return _communicatorId; } }
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
            publishToEvent(EventCommunicatorsRelationship.SiblingChild);
        }

        public void PublishToParent()
        {
            publishToEvent(EventCommunicatorsRelationship.Parent);
        }

        public void PublishToClosestParent()
        {
            publishToEvent(EventCommunicatorsRelationship.ClosestParent);
        }




        public void SubscribeToAll()
        {
            subscribeToEvent(EventCommunicatorsRelationship.All);
        }

        public void SubscribeToParent()
        {
            subscribeToEvent(EventCommunicatorsRelationship.Parent);

        }

        public void SubscribeToClosestParent()
        {
            subscribeToEvent(EventCommunicatorsRelationship.ClosestParent);
        }

        public void SubscribeToChild()
        {
            subscribeToEvent(EventCommunicatorsRelationship.Child);
        }

        public void SubscribeToClosestChild()
        {
            subscribeToEvent(EventCommunicatorsRelationship.ClosestChild);
        }

        public void SubscribeToSiblingChildAndParent()
        {
            subscribeToEvent(EventCommunicatorsRelationship.SiblingChild | EventCommunicatorsRelationship.Parent);

        }


        public void SubscribeToSiblingChild()
        {
            subscribeToEvent(EventCommunicatorsRelationship.SiblingChild);
        }

        public void SubscribeToNotOther()
        {
            subscribeToEvent(EventCommunicatorsRelationship.Child | 
                EventCommunicatorsRelationship.Parent | 
                EventCommunicatorsRelationship.Same | 
                EventCommunicatorsRelationship.Sibling | 
                EventCommunicatorsRelationship.SiblingChild
                );
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