using System;
using System.Globalization;
using MKEventAggregator.Plugins;
using MKEventAggregator.Properties;

namespace MKEventAggregator
{
    /// <summary>
    /// Provides a way to retrieve a <see cref="Delegate"/> to execute an action depending
    /// on the value of a second filter predicate that returns true if the action should execute.
    /// </summary>
    /// <typeparam name="TPayload">The type to use for the generic <see cref="System.Action{TPayload}"/> and <see cref="Predicate{TPayload}"/> types.</typeparam>
    public class EventSubscription<TPayload> : IEventSubscription
    {
        private readonly IDelegateReference _actionReference;
        private readonly IDelegateReference _filterReference;

        private readonly EventCommunicatorAddress _subscriberAddress;
        private readonly EventCommunicatorsRelationship _acceptFrom;

        /// <summary>
        ///  Creates a new instance of <see cref="eventSubscription{TPayload}"/>.
        /// </summary>
        /// <param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
        /// <param name="filterReference">A reference to a delegate of type <see cref="Predicate{TPayload}"/>.</param>
        /// <param name="subscriberAddress">Specifies subscriber address (it is needed for correct recognizing relationship to publisher by EventCommunicatorsRelationship)</param>
        /// <param name="acceptFrom">Specifies relationship to publisher (from what publishers you wanna get events, this subscribier will receive only correct events). Default is All.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>,
        /// or the target of <paramref name="filterReference"/> is not of type <see cref="Predicate{TPayload}"/>.</exception>
        public EventSubscription(IDelegateReference actionReference, IDelegateReference filterReference, EventCommunicatorAddress subscriberAddress, EventCommunicatorsRelationship acceptFrom)
        {
            if (actionReference == null)
                throw new ArgumentNullException("actionReference");
            if (!(actionReference.Target is Action<TPayload>))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidDelegateRerefenceTypeException, typeof(Action<TPayload>).FullName), "actionReference");

            if (filterReference == null)
                throw new ArgumentNullException("filterReference");
            if (!(filterReference.Target is Predicate<TPayload>))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidDelegateRerefenceTypeException, typeof(Predicate<TPayload>).FullName), "filterReference");

            if (_acceptFrom != EventCommunicatorsRelationship.All)
                if(_subscriberAddress == null)
                    throw new EventAggregatorException(Resources.SubscribeFailedSubscriberNotSpecified);


            _actionReference = actionReference;
            _filterReference = filterReference;
            _subscriberAddress = subscriberAddress;
            _acceptFrom = acceptFrom;
        }

        /// <summary>
        /// Gets the target <see cref="System.Action{T}"/> that is referenced by the <see cref="IDelegateReference"/>.
        /// </summary>
        /// <value>An <see cref="System.Action{T}"/> or <see langword="null" /> if the referenced target is not alive.</value>
        public Action<TPayload> Action
        {
            get { return (Action<TPayload>)_actionReference.Target; }
        }

        /// <summary>
        /// Gets the target <see cref="Predicate{T}"/> that is referenced by the <see cref="IDelegateReference"/>.
        /// </summary>
        /// <value>An <see cref="Predicate{T}"/> or <see langword="null" /> if the referenced target is not alive.</value>
        public Predicate<TPayload> Filter
        {
            get { return (Predicate<TPayload>)_filterReference.Target; }
        }

        /// <summary>
        /// Gets or sets a <see cref="SubscriptionToken"/> that identifies this <see cref="IEventSubscription"/>.
        /// </summary>
        /// <value>A token that identifies this <see cref="IEventSubscription"/>.</value>
        public SubscriptionToken SubscriptionToken { get; set; }

        /// <summary>
        /// Gets the execution strategy to publish this event.
        /// </summary>
        /// <returns>An <see cref="System.Action{T}"/> with the execution strategy, or <see langword="null" /> if the <see cref="IEventSubscription"/> is no longer valid.</returns>
        /// <remarks>
        /// If <see cref="Action"/> or <see cref="Filter"/> are no longer valid because they were
        /// garbage collected, this method will return <see langword="null" />.
        /// Otherwise it will return a delegate that evaluates the <see cref="Filter"/> and if it
        /// returns <see langword="true" /> will then call <see cref="InvokeAction"/>. The returned
        /// delegate holds hard references to the <see cref="Action"/> and <see cref="Filter"/> target
        /// <see cref="Delegate">delegates</see>. As long as the returned delegate is not garbage collected,
        /// the <see cref="Action"/> and <see cref="Filter"/> references delegates won't get collected either.
        /// </remarks>
        public virtual Action<object[]> GetExecutionStrategy()
        {
            return GetExecutionStrategy(null, EventCommunicatorsRelationship.All);
        }

        /// <summary>
        /// Gets the execution strategy to publish this event. Specify publisher address.
        /// </summary>
        /// <returns>An <see cref="System.Action{T}"/> with the execution strategy, or <see langword="null" /> if the <see cref="IEventSubscription"/> is no longer valid.</returns>
        /// <remarks>
        /// If <see cref="Action"/> or <see cref="Filter"/> are no longer valid because they were
        /// garbage collected, this method will return <see langword="null" />.
        /// Otherwise it will return a delegate that evaluates the <see cref="Filter"/> and if it
        /// returns <see langword="true" /> will then call <see cref="InvokeAction"/>. The returned
        /// delegate holds hard references to the <see cref="Action"/> and <see cref="Filter"/> target
        /// <see cref="Delegate">delegates</see>. As long as the returned delegate is not garbage collected,
        /// the <see cref="Action"/> and <see cref="Filter"/> references delegates won't get collected either.
        /// </remarks>
        public virtual Action<object[]> GetExecutionStrategy(EventCommunicatorAddress publisherAddress, EventCommunicatorsRelationship publishTo)
        {
            Action<TPayload> action = this.Action;
            Predicate<TPayload> filter = this.Filter;
            if (action != null && filter != null)
            {
                return arguments =>
                {
                    TPayload argument = default(TPayload);
                    if (arguments != null && arguments.Length > 0 && arguments[0] != null)
                    {
                        argument = (TPayload)arguments[0];
                    }
                    if (filter(argument) && isAllowedByRelationship(publisherAddress, publishTo))
                    {
                        InvokeAction(action, argument);
                    }
                };
            }
            return null;
        }

        /// <summary>
        /// Invokes the specified <see cref="System.Action{TPayload}"/> synchronously when not overridden.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="argument">The payload to pass <paramref name="action"/> while invoking it.</param>
        /// <exception cref="ArgumentNullException">An <see cref="ArgumentNullException"/> is thrown if <paramref name="action"/> is null.</exception>
        public virtual void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            if (action == null) throw new System.ArgumentNullException("action");
            
            action(argument);
        }


        private bool isAllowedByRelationship(EventCommunicatorAddress publisherAddress, EventCommunicatorsRelationship publishTo)
        {
            return isPublisherAllowedByRelationship(publisherAddress) &&
                   isSubscriberAllowedByRelationship(publisherAddress, publishTo);
        }

        private bool isPublisherAllowedByRelationship(EventCommunicatorAddress publisherAddress)
        {
            // Accept from all publishers (even if publisher address is not specified)
            if (_acceptFrom == EventCommunicatorsRelationship.All)
                return true;
            // AcceptFrom relationship is specified (other than All), but publisher address doesn't, 
            // we can't figure out relationship, so we ignore this event
            if (publisherAddress == null)
                return false;

            var relSubscriberToPublisher = _subscriberAddress.RecognizeRelationship(publisherAddress);
            return (relSubscriberToPublisher & _acceptFrom) != EventCommunicatorsRelationship.All;
        }

        private bool isSubscriberAllowedByRelationship(EventCommunicatorAddress publisherAddress, EventCommunicatorsRelationship publishTo)
        {
            // PublishTo is all, so publish to this subscriber (even if publisher address is not specified)
            if (publishTo == EventCommunicatorsRelationship.All)
                return true;
            // PublishTo relationship is specified (other than All), but publisher address doesn't or subsciber address doesn't, 
            // we can't figure out relationship, so we ignore this event
            if (publisherAddress == null || _subscriberAddress == null)
                return false;

            var relPublisherToSubscriber = publisherAddress.RecognizeRelationship(_subscriberAddress);
            return (relPublisherToSubscriber & publishTo) != EventCommunicatorsRelationship.All;
        }

    }
}