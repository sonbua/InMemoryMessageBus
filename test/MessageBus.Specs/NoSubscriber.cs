using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs;

[Subject("No Subscriber")]
class no_subscriber_context : bus_context
{
    class when_publishing_a_message
    {
        Because of = () => bus.Publish("a message");

        It should_have_one_pending_message = () => bus.PendingCount.Should().Be(1);
    }

    class given_a_pending_message_when_a_subscriber_subscribes
    {
        Establish context = () =>
        {
            bus.Publish("a message");
            bus.PendingCount.Should().Be(1);
        };

        Because of = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }));

        It the_message_should_be_consumed = () => bus.PendingCount.Should().Be(0);
    }
}
