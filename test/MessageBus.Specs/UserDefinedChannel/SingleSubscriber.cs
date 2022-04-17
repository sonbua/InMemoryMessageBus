using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

class single_subscriber_context : user_defined_channel_context
{
    Establish context = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }), user_defined_channel);

    [Subject("User-defined Channel: Single Subscriber")]
    class when_publishing_a_message
    {
        Because of = () => bus.Publish("a message", user_defined_channel);

        It should_the_message_be_consumed = () => bus.CountPending(user_defined_channel).Should().Be(0);
    }

    [Subject("User-defined Channel: Single Subscriber")]
    class given_the_subscriber_unsubscribes_when_publishing_a_message
    {
        Establish context = () => bus.Unsubscribe("a subscriber", user_defined_channel);

        Because of = () => bus.Publish("a message", user_defined_channel);

        It should_there_be_a_pending_message = () => bus.CountPending(user_defined_channel).Should().Be(1);
    }
}
