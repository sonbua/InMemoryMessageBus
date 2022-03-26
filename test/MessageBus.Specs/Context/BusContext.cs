using Machine.Specifications;

namespace MessageBus.Specs.Context;

[Subject(typeof(Bus))]
class bus_context
{
    Establish context = () => bus = new Bus();

    protected static Bus bus;
}
