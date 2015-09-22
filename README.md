MKEventAggregator
===============

Event aggregator / Message bus for .NET with plugins system support. Based on PRISM 5.0, Prism.PubSubEvents library. 

How to use:

**Creating custom event:**
```C#
class EmailData
{
    public string From { get; private set; }
    public string To { get; private set; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public DateTime Created { get; private set; }

    public EmailData(string @from, string to, string subject, string body)
    {
        From = @from;
        To = to;
        Subject = subject;
        Body = body;
        Created = DateTime.Now;
    }
}

class SendEmailEvent : Event<EmailData>
{
}
```

**Plugins:**
```C#
abstract class PluginBase : IEventCommunicator
{
    protected PluginBase(IEventAggregator eventAggregator, IEventCommunicator parentCommunicator)
    {
        CommunicatorId = Guid.NewGuid();
        EvAggregator = eventAggregator;
        ParentCommunicator = parentCommunicator;
    }

    protected IEventAggregator EvAggregator { get; private set; }

    public Guid CommunicatorId { get; private set; }
    public IEventCommunicator ParentCommunicator { get; private set; }

    public void SetEventAggregator(IEventAggregator eventAggregator)
    {
        EvAggregator = eventAggregator;
    }
}
```

**Subscribing:**
```C#
class PluginSub : PluginBase
{
    public PluginSub(IEventAggregator eventAggregator, IEventCommunicator parentCommunicator) 
      : base(eventAggregator, parentCommunicator)
    {
        subscribeToEvents();
    }

    private void subscribeToEvents()
    {
        EvAggregator.GetEvent<SendEmailEvent>()
            .Subscribe(customMethod, this, EventCommunicatorsRelationship.All, ThreadOption.UIThread);
    }

    private void customMethod(EmailData email)
    {
        // Send email
    }
}
```

**Publishing:**
```C#
class PluginPub : PluginBase
{
    public PluginPub(IEventAggregator eventAggregator, IEventCommunicator parentCommunicator)
      : base(eventAggregator, parentCommunicator)
    {
    }

    public void PublishSendEmail()
    {
        var email = new EmailData("me@test.com", "other@test.com", "Subject", "Body");
        EvAggregator.GetEvent<SendEmailEvent>()
            .Publish(this, email, 
            EventCommunicatorsRelationship.ClosestChild | EventCommunicatorsRelationship.Parent);
    }
}
```

**All together:**
```C#
public void Main()
{
    var evAggregator = new EventAggregator();

    var parent = new PluginSub(evAggregator, null);

    var child1 = new PluginPub(evAggregator, parent); // Publisher
    var child2 = new PluginSub(evAggregator, parent);

    var child1_1 = new PluginSub(evAggregator, child1);
    var child2_1 = new PluginSub(evAggregator, child2);

    
    
    child1.PublishSendEmail();
    /*
     * The send email event will be sent only to plugins 'parent' and 'child1_1' 
     * Due to selected EventCommunicatorsRelationship in 'PublishSendEmail' method
     */
}
```

___

**Possible relationships:**
```C#
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
```
