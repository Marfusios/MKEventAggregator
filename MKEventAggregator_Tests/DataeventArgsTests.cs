

using System;
using MKEventAggregator;
using Xunit;

namespace EventAggregator_Tests
{
    
    public class DataeventArgsTests
    {
        [Fact]
        public void CanPassData()
        {
            DataEventArgs<int> e = new DataEventArgs<int>(32);
            Assert.Equal(32, e.Value);
        }

        [Fact]
        public void IsEventArgs()
        {
            DataEventArgs<string> dea = new DataEventArgs<string>("");
            EventArgs ea = dea as EventArgs;
            Assert.NotNull(ea);
        }
    }
}