using System;
using System.Collections.Generic;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel;

class two_subscribers_context : bus_context
{
    [Subject("Two Subscribers")]
    class given_a_faulted_subscriber_when_exception_thrown_for_one_subscriber
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
                    }));
            bus.Subscribe(new Subscriber("success", _ => spy.Add("success")));
        };

        Because of = () =>
        {
            bus.Publish("a message");
            WaitForMessageToBeConsumed();
        };

        It should_both_subscribers_receive_message = () => spy.Should().HaveCount(2);

        It should_the_message_be_handled_in_the_correct_order = () => spy.Should().BeEquivalentTo("faulted", "success");
    }

    protected static List<string> spy;
}
