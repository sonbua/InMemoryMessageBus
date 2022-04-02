using System;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel.PublishMethod;

[Subject("Publishing: Validation")]
class publishing_validation_context : bus_context
{
}

[Tags(tag.validation)]
class given_null_message_when_calling_publish : publishing_validation_context
{
    Because of =
        // ReSharper disable once AssignNullToNotNullAttribute
        () => publishing = () => bus.Publish(null);

    It should_throw_argument_null_exception = () => publishing.Should().Throw<ArgumentNullException>();

    static Action publishing;
}
