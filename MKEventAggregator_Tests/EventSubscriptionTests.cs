

using System;
using System.Collections.Generic;
using MKEventAggregator;
using MKEventAggregator.Plugins;
using Xunit;

namespace EventAggregator_Tests
{
    
    public class EventSubscriptionTests
    {
        [Fact]
        public void NullTargetInActionThrows()
        {
            var actionDelegateReference = new MockDelegateReference()
            {
                Target = null
            };
            var filterDelegateReference = new MockDelegateReference()
            {
                Target = (Predicate<object>)(arg =>
                {
                    return true;
                })
            };

            Assert.Throws<ArgumentException>(
                () =>
                {
                    var eventSubscription = new EventSubscription<object>(actionDelegateReference,
                        filterDelegateReference, null, EventCommunicatorsRelationship.All);
                });
        }

        [Fact]
        public void DifferentTargetTypeInActionThrows()
        {
            var actionDelegateReference = new MockDelegateReference()
            {
                Target = (Action<int>)delegate { }
            };
            var filterDelegateReference = new MockDelegateReference()
            {
                Target = (Predicate<string>)(arg =>
                {
                    return true;
                })
            };

            Assert.Throws<ArgumentException>(
                () =>
                {
                    var eventSubscription = new EventSubscription<string>(actionDelegateReference,
                        filterDelegateReference, null, EventCommunicatorsRelationship.All);
                });
        }

        [Fact]
        public void NullActionThrows()
        {
            var filterDelegateReference = new MockDelegateReference()
            {
                Target = (Predicate<object>)(arg =>
                {
                    return true;
                })
            };

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    var eventSubscription = new EventSubscription<object>(null, filterDelegateReference, null, EventCommunicatorsRelationship.All);
                });
        }

        [Fact]
        public void NullTargetInFilterThrows()
        {
            var actionDelegateReference = new MockDelegateReference()
            {
                Target = (Action<object>)delegate { }
            };

            var filterDelegateReference = new MockDelegateReference()
            {
                Target = null
            };

            Assert.Throws<ArgumentException>(
                () =>
                {
                    var eventSubscription = new EventSubscription<object>(actionDelegateReference,
                        filterDelegateReference, null, EventCommunicatorsRelationship.All);
                });
        }


        [Fact]
        public void DifferentTargetTypeInFilterThrows()
        {
            var actionDelegateReference = new MockDelegateReference()
            {
                Target = (Action<string>)delegate { }
            };

            var filterDelegateReference = new MockDelegateReference()
            {
                Target = (Predicate<int>)(arg =>
                {
                    return true;
                })
            };

            Assert.Throws<ArgumentException>(
                () =>
                {
                    var eventSubscription = new EventSubscription<string>(actionDelegateReference,
                        filterDelegateReference, null, EventCommunicatorsRelationship.All);
                });
        }

        [Fact]
        public void NullFilterThrows()
        {
            var actionDelegateReference = new MockDelegateReference()
            {
                Target = (Action<object>)delegate { }
            };

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    var eventSubscription = new EventSubscription<object>(actionDelegateReference, null, null, EventCommunicatorsRelationship.All); 
                    
                });
        }

        [Fact]
        public void CanInitEventSubscription()
        {
            var actionDelegateReference = new MockDelegateReference((Action<object>)delegate { });
            var filterDelegateReference = new MockDelegateReference((Predicate<object>)delegate { return true; });
            var eventSubscription = new EventSubscription<object>(actionDelegateReference, filterDelegateReference, null, EventCommunicatorsRelationship.All);

            var subscriptionToken = new SubscriptionToken(t => { });

            eventSubscription.SubscriptionToken = subscriptionToken;

            Assert.Same(actionDelegateReference.Target, eventSubscription.Action);
            Assert.Same(filterDelegateReference.Target, eventSubscription.Filter);
            Assert.Same(subscriptionToken, eventSubscription.SubscriptionToken);
        }

        [Fact]
        public void GetPublishActionReturnsDelegateThatExecutesTheFilterAndThenTheAction()
        {
            var executedDelegates = new List<string>();
            var actionDelegateReference =
                new MockDelegateReference((Action<object>)delegate { executedDelegates.Add("Action"); });

            var filterDelegateReference = new MockDelegateReference((Predicate<object>)delegate
                                                {
                                                    executedDelegates.Add(
                                                        "Filter");
                                                    return true;

                                                });

            var eventSubscription = new EventSubscription<object>(actionDelegateReference, filterDelegateReference, null, EventCommunicatorsRelationship.All);


            var publishAction = eventSubscription.GetExecutionStrategy();

            Assert.NotNull(publishAction);

            publishAction.Invoke(null);

            Assert.Equal(2, executedDelegates.Count);
            Assert.Equal("Filter", executedDelegates[0]);
            Assert.Equal("Action", executedDelegates[1]);
        }

        [Fact]
        public void GetPublishActionReturnsNullIfActionIsNull()
        {
            var actionDelegateReference = new MockDelegateReference((Action<object>)delegate { });
            var filterDelegateReference = new MockDelegateReference((Predicate<object>)delegate { return true; });

            var eventSubscription = new EventSubscription<object>(actionDelegateReference, filterDelegateReference, null, EventCommunicatorsRelationship.All);

            var publishAction = eventSubscription.GetExecutionStrategy();

            Assert.NotNull(publishAction);

            actionDelegateReference.Target = null;

            publishAction = eventSubscription.GetExecutionStrategy();

            Assert.Null(publishAction);
        }

        [Fact]
        public void GetPublishActionReturnsNullIfFilterIsNull()
        {
            var actionDelegateReference = new MockDelegateReference((Action<object>)delegate { });
            var filterDelegateReference = new MockDelegateReference((Predicate<object>)delegate { return true; });

            var eventSubscription = new EventSubscription<object>(actionDelegateReference, filterDelegateReference, null, EventCommunicatorsRelationship.All);

            var publishAction = eventSubscription.GetExecutionStrategy();

            Assert.NotNull(publishAction);

            filterDelegateReference.Target = null;

            publishAction = eventSubscription.GetExecutionStrategy();

            Assert.Null(publishAction);
        }

        [Fact]
        public void GetPublishActionDoesNotExecuteActionIfFilterReturnsFalse()
        {
            bool actionExecuted = false;
            var actionDelegateReference = new MockDelegateReference()
            {
                Target = (Action<int>)delegate { actionExecuted = true; }
            };
            var filterDelegateReference = new MockDelegateReference((Predicate<int>)delegate
                                                                                            {
                                                                                                return false;
                                                                                            });

            var eventSubscription = new EventSubscription<int>(actionDelegateReference, filterDelegateReference, null, EventCommunicatorsRelationship.All);


            var publishAction = eventSubscription.GetExecutionStrategy();

            publishAction.Invoke(new object[] { null });

            Assert.False(actionExecuted);
        }

        [Fact]
        public void StrategyPassesArgumentToDelegates()
        {
            string passedArgumentToAction = null;
            string passedArgumentToFilter = null;

            var actionDelegateReference = new MockDelegateReference((Action<string>)(obj => passedArgumentToAction = obj));
            var filterDelegateReference = new MockDelegateReference((Predicate<string>)(obj =>
                                                                                            {
                                                                                                passedArgumentToFilter = obj;
                                                                                                return true;
                                                                                            }));

            var eventSubscription = new EventSubscription<string>(actionDelegateReference, filterDelegateReference, null, EventCommunicatorsRelationship.All);
            var publishAction = eventSubscription.GetExecutionStrategy();

            publishAction.Invoke(new[] { "TestString" });

            Assert.Equal("TestString", passedArgumentToAction);
            Assert.Equal("TestString", passedArgumentToFilter);
        }



    }
}
