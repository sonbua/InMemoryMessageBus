﻿using System;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs;

[Subject("Unsubscription: Validation")]
class unsubscription_validation_context : bus_context
{
    Establish context = () => bus.Subscribe(new Subscriber("first", _ => { }));
}

[Tags(tag.validation)]
class given_subscriber_name_is_null_when_calling_unsubscribe : unsubscription_validation_context
{
    Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: null);

    It should_throw_argument_null_exception = () => unsubscription.Should().Throw<ArgumentNullException>();

    static Action unsubscription;
}

[Tags(tag.validation)]
class given_subscriber_name_is_empty_when_calling_unsubscribe : unsubscription_validation_context
{
    Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: "");

    It should_throw_argument_exception_with_expected_message =
        () => unsubscription.Should().Throw<ArgumentException>()
            .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

    static Action unsubscription;
}

[Tags(tag.validation)]
class given_subscriber_name_is_whitespaces_when_calling_unsubscribe : unsubscription_validation_context
{
    Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: "  ");

    It should_throw_argument_exception_with_expected_message =
        () => unsubscription.Should().Throw<ArgumentException>()
            .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

    static Action unsubscription;
}

[Tags(tag.validation)]
class given_subscriber_name_is_whitespace_and_newline_when_calling_unsubscribe : unsubscription_validation_context
{
    Because of = () => unsubscription = () => bus.Unsubscribe("  \r\n  ");

    It should_throw_argument_exception_with_expected_message =
        () => unsubscription.Should().Throw<ArgumentException>()
            .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

    static Action unsubscription;
}

[Tags(tag.validation)]
class given_a_subscriber_whose_name_does_not_exist_when_calling_unsubscribe : unsubscription_validation_context
{
    Because of = () => unsubscription = () => bus.Unsubscribe("unknown");

    It should_not_throw = () => unsubscription.Should().NotThrow();

    static Action unsubscription;
}
