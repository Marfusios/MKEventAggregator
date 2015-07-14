using System;

namespace MKEventAggregator.Plugins
{
    public interface IEventCommunicator
    {
        Guid CommunicatorId { get; }
        IEventCommunicator ParentCommunicator { get; }

        void SetEventAggregator(IEventAggregator eventAggregator);
    }
}
