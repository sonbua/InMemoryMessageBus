using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel;

[Subject("Single Subscriber")]
class single_subscriber_context : bus_context
{
    Establish context = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }));
}

class when_publishing_a_message : single_subscriber_context
{
    Because of = () => bus.Publish("a message");

    It should_the_message_be_consumed = () => bus.PendingCount.Should().Be(0);
}

class given_the_subscriber_unsubscribed_when_publishing_a_message : single_subscriber_context
{
    Establish context = () => bus.Unsubscribe("a subscriber");

    Because of = () => bus.Publish("another message");

    It should_there_be_a_pending_message = () => bus.PendingCount.Should().Be(1);
}
