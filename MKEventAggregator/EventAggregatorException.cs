using System;

namespace MKEventAggregator
{
    public class EventAggregatorException : Exception
    {
        public EventAggregatorException()
        {

        }

        public EventAggregatorException(string msg)
            : base(msg)
        {

        }

        public EventAggregatorException(string msg, Exception inner)
            : base(msg, inner)
        {

        }
    }
}
