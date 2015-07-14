using System;
using System.Diagnostics;
using MKEventAggregator;
using MKEventAggregator.Plugins;
using Xunit;

namespace EventAggregator_Tests
{
    public class EventCommunicatorAddressTests
    {

        [Fact]
        public void RemoveLastOrNothingParent()
        {
            var communicator = new MockParentEventCommunicator();

            var address = new EventCommunicatorAddress(communicator);

            var countBefore = address.Count;
            address.RemoveLastOrNothing();
            var countAfter = address.Count;

            Assert.Equal(1, countBefore);
            Assert.Equal(countBefore, countAfter);
        }

        [Fact]
        public void RemoveLastOrNothingChild()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild = new MockChildEventCommunicator(communicatorParent);

            var addressChild = new EventCommunicatorAddress(communicatorChild);

            var countBefore = addressChild.Count;
            addressChild.RemoveLastOrNothing();
            var countAfter = addressChild.Count;

            Assert.Equal(2, countBefore);
            Assert.Equal(1, countAfter);
        }

        [Fact]
        public void RemoveLastShouldThrowsException()
        {
            var communicator = new MockParentEventCommunicator();

            var address = new EventCommunicatorAddress(communicator);

            Assert.Throws<EventAggregatorException>(() =>
            {
                address.RemoveLast();
            });
        }

        [Fact]
        public void RemoveLast()
        {
            var communicator = new MockParentEventCommunicator();
            var child = new MockChildEventCommunicator(communicator);

            var address = new EventCommunicatorAddress(child);

            var addrString = address.AddressString;
            address.RemoveLast();

            Assert.NotEqual(addrString, address.AddressString);
        }

        [Fact]
        public void ValidationShouldWork()
        {
            var communicator = new MockParentEventCommunicator();
            communicator.CommunicatorIdInternal = Guid.Empty;

            Assert.Throws<EventAggregatorException>(() =>
            {
                var tmp = new EventCommunicatorAddress(null);
            });

            Assert.Throws<EventAggregatorException>(() =>
            {
                var tmp = new EventCommunicatorAddress(communicator);
            });
        }

        [Fact]
        public void ToStringShouldReturnsAddress()
        {
            var communicator = new MockParentEventCommunicator();

            var addr = new EventCommunicatorAddress(communicator);

            Assert.Equal(addr.AddressString, addr.ToString());
        }

        [Fact]
        public void ParentCommunicatorAddressCreated()
        {
            var communicator = new MockParentEventCommunicator();

            var address = new EventCommunicatorAddress(communicator);

            Assert.False(string.IsNullOrEmpty(address.AddressString));
        }


        [Fact]
        public void ChildCommunicatorAddressCreated()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild = new MockChildEventCommunicator(communicatorParent);

            var addressChild = new EventCommunicatorAddress(communicatorChild);

            Assert.False(string.IsNullOrEmpty(addressChild.AddressString));
        }

        [Fact]
        public void GetRelationshipBetweenNullCommunicator()
        {
            var communicator = new MockParentEventCommunicator();

            var address = new EventCommunicatorAddress(communicator);

            Assert.Throws<ArgumentNullException>(() =>
            {
                var tmp = address.RecognizeRelationship(null);
            });
        }

        [Fact]
        public void GetRelationshipBetweenSameCommunicators()
        {
            var communicator = new MockParentEventCommunicator();

            var address = new EventCommunicatorAddress(communicator);

            var relationship = address.RecognizeRelationship(address);

            Assert.Equal(EventCommunicatorsRelationship.Same, relationship);
        }

        [Fact]
        public void GetRelationshipBetweenNotRelatedCommunicators()
        {
            var communicatorParent1 = new MockParentEventCommunicator();
            var communicatorParent2 = new MockParentEventCommunicator();


            var addressParent1 = new EventCommunicatorAddress(communicatorParent1);
            var addressParent2 = new EventCommunicatorAddress(communicatorParent2);

            var relationshipParent2ToParent1 = addressParent1.RecognizeRelationship(addressParent2);
            var relationshipParent1ToParent2 = addressParent2.RecognizeRelationship(addressParent1);

            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipParent2ToParent1);
            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipParent1ToParent2);
        }

        [Fact]
        public void GetRelationshipBetweenParentAndChildClosest()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild = new MockChildEventCommunicator(communicatorParent);

            var addressParent = new EventCommunicatorAddress(communicatorParent);
            var addressChild = new EventCommunicatorAddress(communicatorChild);

            var relationshipChildToParent = addressParent.RecognizeRelationship(addressChild);
            var relationshipParentToChild = addressChild.RecognizeRelationship(addressParent);

            Assert.Equal(EventCommunicatorsRelationship.ClosestChild | EventCommunicatorsRelationship.Child, relationshipChildToParent);
            Assert.Equal(EventCommunicatorsRelationship.Parent | EventCommunicatorsRelationship.ClosestParent, relationshipParentToChild);
        }


        [Fact]
        public void GetRelationshipBetweenParentAndChild()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild = new MockChildEventCommunicator(communicatorParent);
            var communicatorChildChild = new MockChildEventCommunicator(communicatorChild);

            var addressParent = new EventCommunicatorAddress(communicatorParent);
            var addressChild = new EventCommunicatorAddress(communicatorChildChild);

            var relationshipChildToParent = addressParent.RecognizeRelationship(addressChild);
            var relationshipParentToChild = addressChild.RecognizeRelationship(addressParent);

            Assert.Equal(EventCommunicatorsRelationship.Child, relationshipChildToParent);
            Assert.Equal(EventCommunicatorsRelationship.Parent, relationshipParentToChild);
        }

        [Fact]
        public void GetRelationshipBetweenChildAndChild()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild1 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent);

            var addressChild1 = new EventCommunicatorAddress(communicatorChild1);
            var addressChild2 = new EventCommunicatorAddress(communicatorChild2);

            var relationshipChild2ToChild1 = addressChild1.RecognizeRelationship(addressChild2);
            var relationshipChild1ToChild2 = addressChild2.RecognizeRelationship(addressChild1);

            Assert.Equal(EventCommunicatorsRelationship.Sibling, relationshipChild2ToChild1);
            Assert.Equal(EventCommunicatorsRelationship.Sibling, relationshipChild1ToChild2);
        }

        [Fact]
        public void GetRelationshipBetweenChildAndTwoLevelChildSiblingBranch()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild1 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2Child = new MockChildEventCommunicator(communicatorChild2);

            var addressChild1 = new EventCommunicatorAddress(communicatorChild1);
            var addressChild2 = new EventCommunicatorAddress(communicatorChild2Child);

            var relationshipChild2ToChild1 = addressChild1.RecognizeRelationship(addressChild2);
            var relationshipChild1ToChild2 = addressChild2.RecognizeRelationship(addressChild1);

            Assert.Equal(EventCommunicatorsRelationship.SiblingChild, relationshipChild2ToChild1);
            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipChild1ToChild2);
        }

        [Fact]
        public void GetRelationshipBetweenChildAndThreeLevelChildSiblingBranch()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild1 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2Child = new MockChildEventCommunicator(communicatorChild2);
            var communicatorChild2ChildChild = new MockChildEventCommunicator(communicatorChild2Child);

            var addressChild1 = new EventCommunicatorAddress(communicatorChild1);
            var addressChild2 = new EventCommunicatorAddress(communicatorChild2ChildChild);

            var relationshipChild2ToChild1 = addressChild1.RecognizeRelationship(addressChild2);
            var relationshipChild1ToChild2 = addressChild2.RecognizeRelationship(addressChild1);

            Assert.Equal(EventCommunicatorsRelationship.SiblingChild, relationshipChild2ToChild1);
            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipChild1ToChild2);
        }




        [Fact]
        public void GetRelationshipBetweenChildAndTwoLevelChildSameBranch()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild1 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2Child = new MockChildEventCommunicator(communicatorChild2);

            var addressChild1 = new EventCommunicatorAddress(communicatorChild2);
            var addressChild2 = new EventCommunicatorAddress(communicatorChild2Child);

            var relationshipChild2ToChild1 = addressChild1.RecognizeRelationship(addressChild2);
            var relationshipChild1ToChild2 = addressChild2.RecognizeRelationship(addressChild1);

            Assert.Equal(EventCommunicatorsRelationship.Child | EventCommunicatorsRelationship.ClosestChild, relationshipChild2ToChild1);
            Assert.Equal(EventCommunicatorsRelationship.Parent | EventCommunicatorsRelationship.ClosestParent, relationshipChild1ToChild2);
        }

        [Fact]
        public void GetRelationshipBetweenChildAndThreeLevelChildSameBranch()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild1 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2Child = new MockChildEventCommunicator(communicatorChild2);
            var communicatorChild2ChildChild = new MockChildEventCommunicator(communicatorChild2Child);

            var addressChild1 = new EventCommunicatorAddress(communicatorChild2);
            var addressChild2 = new EventCommunicatorAddress(communicatorChild2ChildChild);

            var relationshipChild2ToChild1 = addressChild1.RecognizeRelationship(addressChild2);
            var relationshipChild1ToChild2 = addressChild2.RecognizeRelationship(addressChild1);

            Assert.Equal(EventCommunicatorsRelationship.Child, relationshipChild2ToChild1);
            Assert.Equal(EventCommunicatorsRelationship.Parent, relationshipChild1ToChild2);
        }



        [Fact]
        public void GetRelationshipBetweenChildAndTwoLevelChildOtherBranch()
        {
            var communicatorParent1 = new MockParentEventCommunicator();
            var communicatorParent2 = new MockParentEventCommunicator();
            var communicatorChild1 = new MockChildEventCommunicator(communicatorParent1);
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent2);
            var communicatorChild2Child = new MockChildEventCommunicator(communicatorChild2);

            var addressChild1 = new EventCommunicatorAddress(communicatorChild1);
            var addressChild2 = new EventCommunicatorAddress(communicatorChild2Child);

            var relationshipChild2ToChild1 = addressChild1.RecognizeRelationship(addressChild2);
            var relationshipChild1ToChild2 = addressChild2.RecognizeRelationship(addressChild1);

            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipChild2ToChild1);
            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipChild1ToChild2);
        }

        [Fact]
        public void GetRelationshipBetweenChildAndThreeLevelChildOtherBranch()
        {
            var communicatorParent1 = new MockParentEventCommunicator();
            var communicatorParent2 = new MockParentEventCommunicator();
            var communicatorChild1 = new MockChildEventCommunicator(communicatorParent1);
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent2);
            var communicatorChild2Child = new MockChildEventCommunicator(communicatorChild2);
            var communicatorChild2ChildChild = new MockChildEventCommunicator(communicatorChild2Child);

            var addressChild1 = new EventCommunicatorAddress(communicatorChild1);
            var addressChild2 = new EventCommunicatorAddress(communicatorChild2ChildChild);

            var relationshipChild2ToChild1 = addressChild1.RecognizeRelationship(addressChild2);
            var relationshipChild1ToChild2 = addressChild2.RecognizeRelationship(addressChild1);

            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipChild2ToChild1);
            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipChild1ToChild2);
        }

        [Fact]
        public void GetRelationshipBetweenThreeLevelChildAndThreeLevelChildOtherBranch()
        {
            var communicatorParent1 = new MockParentEventCommunicator();
            var communicatorParent2 = new MockParentEventCommunicator();
            var communicatorChild1 = new MockChildEventCommunicator(communicatorParent1);
            var communicatorChild1Child = new MockChildEventCommunicator(communicatorChild1);
            var communicatorChild1ChildChild = new MockChildEventCommunicator(communicatorChild1Child);
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent2);
            var communicatorChild2Child = new MockChildEventCommunicator(communicatorChild2);
            var communicatorChild2ChildChild = new MockChildEventCommunicator(communicatorChild2Child);

            var addressChild1 = new EventCommunicatorAddress(communicatorChild1ChildChild);
            var addressChild2 = new EventCommunicatorAddress(communicatorChild2ChildChild);

            var relationshipChild2ToChild1 = addressChild1.RecognizeRelationship(addressChild2);
            var relationshipChild1ToChild2 = addressChild2.RecognizeRelationship(addressChild1);

            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipChild2ToChild1);
            Assert.Equal(EventCommunicatorsRelationship.Other, relationshipChild1ToChild2);
        }


        [Fact]
        public void GoodPerformance()
        {
            var communicatorParent = new MockParentEventCommunicator();
            var communicatorChild2 = new MockChildEventCommunicator(communicatorParent);
            var communicatorChild2Child = new MockChildEventCommunicator(communicatorChild2);
            var communicatorChild2ChildChild = new MockChildEventCommunicator(communicatorChild2Child);

            var rootAddr = new EventCommunicatorAddress(communicatorParent);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                var addr = new EventCommunicatorAddress(communicatorChild2ChildChild);
                var relToRoot = addr.RecognizeRelationship(rootAddr);
                var relFromRoot = rootAddr.RecognizeRelationship(addr);
            }
            sw.Stop();
            Assert.True(sw.ElapsedMilliseconds < 150);
        }
    }
}
