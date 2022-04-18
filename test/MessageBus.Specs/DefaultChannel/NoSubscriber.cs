using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel;

class no_subscriber_context : bus_context
{
    [Subject("No Subscriber")]
    class when_calling_publish
    {
        Because of = () =>
        {
            bus.Publish("a message");
            WaitForMessageToBeConsumed();
        };

        It should_have_one_pending_message = () => bus.CountPending().Should().Be(1);
    }

    [Subject("No Subscriber")]
    class given_a_pending_message_when_a_subscriber_subscribes
    {
        Establish context = () =>
        {
            bus.Publish("a message");
            WaitForMessageToBeConsumed();
            bus.CountPending().Should().Be(1);
        };

        Because of = () =>
        {
            bus.Subscribe(new Subscriber("a subscriber", _ => { }));
            WaitForMessageToBeConsumed();
        };

        It should_the_message_be_consumed = () => bus.CountPending().Should().Be(0);
    }
}
