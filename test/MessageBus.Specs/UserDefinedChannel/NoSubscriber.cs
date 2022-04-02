using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

[Subject(typeof(Bus), "User-defined Channel: No Subscriber")]
class no_subscriber_context : user_defined_channel_context
{
    class when_calling_publish
    {
        Because of = () => bus.Publish("a message", user_defined_channel);

        It should_have_one_pending_message_on_user_defined_channel =
            () => bus.CountPending(user_defined_channel).Should().Be(1);

        It should_default_channel_be_empty =
            () => bus.CountPending().Should().Be(0);
    }

    class given_a_pending_message_when_a_subscriber_subscribes_default_channel
    {
        Establish context = () =>
        {
            bus.Publish("a message", user_defined_channel);

            bus.CountPending(user_defined_channel).Should().Be(1);
            bus.CountPending().Should().Be(0);
        };

        Because of = () => bus.Subscribe(new Subscriber("default channel subscriber", _ => { }));

        It should_the_message_still_be_pending = () => bus.CountPending(user_defined_channel).Should().Be(1);
    }
}
