﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel.PublishMethod;

[Subject("Publishing: Multithreading")]
class multithreading_publishing_context : bus_context
{
}

[Tags(tag.async)]
[Tags(tag.concurrency)]
class when_multiple_messages_are_published_concurrently : multithreading_publishing_context
{
    Establish context = () =>
    {
        message_count = 10_000;
        publishings = Enumerable.Range(1, message_count)
            .Select(_ => Guid.NewGuid().ToString("N"))
            .Select(message => Task.Factory.StartNew(() => bus.Publish(message)));
    };

    Because of = () => aggregated_publishing = () => Task.WhenAll(publishings);

    It should_succeed =
        // ReSharper disable once AsyncVoidLambda
        async () => await aggregated_publishing.Should().NotThrowAsync();

    It should_all_messages_be_pending_since_there_is_no_subscriber =
        () => bus.CountPending().Should().Be(message_count);

    static int message_count;
    static IEnumerable<Task> publishings;
    static Func<Task> aggregated_publishing;
}
