namespace EventAggregator_Tests
{
    public class MockEventForChildCommunicator
    {
        public object Sender { get; private set; }

        public MockEventForChildCommunicator(object sender)
        {
            Sender = sender;
        }
    }
}