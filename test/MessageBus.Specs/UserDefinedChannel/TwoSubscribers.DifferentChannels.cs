using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

class two_subscribers_different_channels_context : user_defined_channel_context
{
    Establish context = () =>
    {
        bus.Subscribe(new Subscriber("first", _ => { }));
        bus.Subscribe(new Subscriber("second", _ => { }), user_defined_channel);
    };

    [Subject("User-defined Channel: Two Subscribers: Different Channels")]
    class when_publishing_a_message_onto_one_channel
    {
        Because of = () =>
        {
            bus.Publish("a message");
            WaitForMessageToBeConsumed();
        };

        It should_the_message_be_consumed = () => bus.CountPending().Should().Be(0);
    }

    [Subject("User-defined Channel: Two Subscribers: Different Channels")]
    class when_publishing_a_message_onto_the_other_channel
    {
        Because of = () =>
        {
            bus.Publish("another message", user_defined_channel);
            WaitForMessageToBeConsumed();
        };

        It should_the_message_also_be_consumed = () => bus.CountPending(user_defined_channel).Should().Be(0);
    }
}
