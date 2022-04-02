using System;
using System.Collections.Generic;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel;

[Subject("Two Subscribers")]
class two_subscribers_context : bus_context
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

    protected static List<string> spy;
}

class given_a_faulted_subscriber_when_exception_thrown_for_one_subscriber : two_subscribers_context
{
    Because of = () => bus.Publish("a message");

    It should_both_subscribers_receive_message = () => spy.Should().HaveCount(2);

    It should_the_message_be_handled_in_the_correct_order = () => spy.Should().BeEquivalentTo("faulted", "success");
}
