using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MKEventAggregator.Properties;

namespace MKEventAggregator.Plugins
{
    [DebuggerDisplay("EventCommunicatorAddress = {AddressString} || {GetHashCode()}")]
    public class EventCommunicatorAddress
    {
        private readonly LinkedList<string> _innerAddress = new LinkedList<string>(); 

        public EventCommunicatorAddress(IEventCommunicator communicator)
        {
            validate(communicator);
            createAddress(communicator);
            updateAddressString();
        }

        private EventCommunicatorAddress(LinkedList<string> address)
        {
            _innerAddress = address;
            updateAddressString();
        }

        public string AddressString { get; private set; }

        public int Count { get { return _innerAddress.Count; } }
        public bool CanRemoveLast { get { return Count >= 2; } }


        public EventCommunicatorsRelationship RecognizeRelationship(EventCommunicatorAddress other)
        {
            if(other == null)
                throw new ArgumentNullException("other");

            return recognizeRelationship(other);
        }

        public EventCommunicatorAddress RemoveLast()
        {
            if (!CanRemoveLast)
                throw new EventAggregatorException("Can't remove last address node, there is only one node");
            _innerAddress.RemoveLast();
            updateAddressString();
            return this;
        }

        public EventCommunicatorAddress RemoveLastOrNothing()
        {
            if (CanRemoveLast)
            {
                _innerAddress.RemoveLast();
                updateAddressString();
            }
            return this;
        }
        

        public EventCommunicatorAddress Clone()
        {
            return new EventCommunicatorAddress(new LinkedList<string>(_innerAddress));
        }

        private void validate(IEventCommunicator communicator)
        {
            if(communicator == null)
                throw new EventAggregatorException(Resources.CreateCommunicatorAddressFailedIsNull);
            if(communicator.CommunicatorId.Equals(Guid.Empty))
                throw new EventAggregatorException(Resources.CreateCommunicatorAddressFailedIdNotSet);
        }

        private void createAddress(IEventCommunicator communicator)
        {
            if(communicator.ParentCommunicator != null)
                createAddress(communicator.ParentCommunicator);

            var id = communicator.CommunicatorId.ToString("N");
            _innerAddress.AddLast(id);
        }

        

        private EventCommunicatorsRelationship recognizeRelationship(EventCommunicatorAddress other)
        {
            var relationship = EventCommunicatorsRelationship.All;

            if(isSameRel(other))
                relationship = EventCommunicatorsRelationship.Same;
            
            else if (isClosestParentRel(other))
                relationship = EventCommunicatorsRelationship.ClosestParent | EventCommunicatorsRelationship.Parent;
            else if (isParentRel(other))
                relationship = EventCommunicatorsRelationship.Parent;

            else if (isClosestChild(other))
                relationship = EventCommunicatorsRelationship.ClosestChild | EventCommunicatorsRelationship.Child;
            else if (isChild(other))
                relationship = EventCommunicatorsRelationship.Child;

            else if(isSiblingRel(other))
                relationship = EventCommunicatorsRelationship.Sibling;
            else if (isSiblingChildRel(other))
                relationship = EventCommunicatorsRelationship.SiblingChild;

            else
                relationship = EventCommunicatorsRelationship.Other;
            

            return relationship;
        }


        private bool isSameRel(EventCommunicatorAddress other)
        {
            return this.AddressString.Equals(other.AddressString);
        }

        private bool isClosestParentRel(EventCommunicatorAddress other)
        {
            var thisClone = this.Clone();
            return other.AddressString.Equals(thisClone.RemoveLastOrNothing().AddressString);
        }

        private bool isParentRel(EventCommunicatorAddress otherClone)
        {
            return this.AddressString.Contains(otherClone.AddressString);
        }


        private bool isClosestChild(EventCommunicatorAddress other)
        {
            var otherClone = other.Clone();
            return this.AddressString.Equals(otherClone.RemoveLastOrNothing().AddressString);
        }

        private bool isChild(EventCommunicatorAddress other)
        {
            return other.AddressString.Contains(this.AddressString);
        }

        private bool isSiblingRel(EventCommunicatorAddress other)
        {
            if (this.CanRemoveLast != other.CanRemoveLast)
                return false;

            var thisClone = this.Clone();
            var otherClone = other.Clone();
            return thisClone.RemoveLastOrNothing().AddressString.Equals(otherClone.RemoveLastOrNothing().AddressString);
        }

        private bool isSiblingChildRel(EventCommunicatorAddress other)
        {
            var thisClone = this.Clone();
            return other.AddressString.Contains(thisClone.RemoveLastOrNothing().AddressString);
        }


        private void updateAddressString()
        {
            AddressString = CreateStringAddress(this);
        }


        public static string CreateStringAddress(EventCommunicatorAddress address)
        {
            var builder = new StringBuilder();
            for (var currNode = address._innerAddress.First; currNode != null; currNode = currNode.Next)
            {
                builder.Append(currNode.Value);
                if (currNode.Next != null)
                    builder.Append("@");
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            return AddressString;
        }
    }
}
