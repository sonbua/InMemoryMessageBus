using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.UserDefinedChannel;

[Subject(typeof(Bus), "User-defined Channel")]
class user_defined_channel_context : bus_context
{
    Establish context = () => user_defined_channel = "user-defined-channel";

    protected static string user_defined_channel;
}
