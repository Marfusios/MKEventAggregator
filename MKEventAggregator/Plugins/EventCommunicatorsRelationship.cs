using System;

namespace MKEventAggregator.Plugins
{
    [Flags]
    public enum EventCommunicatorsRelationship
    {
        All = 0,
        Same = 1, 
        Sibling = 2, 
        SiblingChild = 4,
        ClosestChild = 8, 
        Child = 16, 
        ClosestParent = 32,
        Parent = 64, 
        Other = 128
    }
}
