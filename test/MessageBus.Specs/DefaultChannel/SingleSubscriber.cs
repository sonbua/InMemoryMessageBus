using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel;

class single_subscriber_context : bus_context
{
    Establish context = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }));

    [Subject("Single Subscriber")]
    class when_publishing_a_message
    {
        Because of = () =>
        {
            bus.Publish("a message");
            WaitForMessageToBeConsumed();
        };

        It should_the_message_be_consumed = () => bus.CountPending().Should().Be(0);
    }

    [Subject("Single Subscriber")]
    class given_the_subscriber_unsubscribed_when_publishing_a_message
    {
        Establish context = () => bus.Unsubscribe("a subscriber");

        Because of = () =>
        {
            bus.Publish("another message");
            WaitForMessageToBeConsumed();
        };

        It should_there_be_a_pending_message = () => bus.CountPending().Should().Be(1);
    }
}
