using System;
using System.Collections.Generic;
using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

class two_subscribers_same_channel_context : user_defined_channel_context
{
    Establish context = () =>
    {
        spy = new List<string>();
        bus.Subscribe(
            new Subscriber(
                "faulted",
                _ =>
                {
                    spy.Add("faulted");
                    throw new InvalidOperationException("exception thrown from faulted subscriber");
                }),
            user_defined_channel);
        bus.Subscribe(new Subscriber("success", _ => spy.Add("success")), user_defined_channel);
    };

    [Subject("User-defined Channel: Two Subscribers: Same Channel")]
    class given_a_faulted_subscriber_when_exception_thrown_for_one_subscriber
    {
        Because of = () =>
        {
            bus.Publish("a message", user_defined_channel);
            WaitForMessageToBeConsumed();
        };

        It should_both_subscribers_receive_message = () => spy.Should().HaveCount(2);

        It should_the_message_be_handled_in_the_correct_order = () => spy.Should().BeEquivalentTo("faulted", "success");
    }

    static List<string> spy;
}
