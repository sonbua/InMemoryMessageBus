using System.Threading;
using Machine.Specifications;

namespace MessageBus.Specs.Context;

[Subject(typeof(Bus))]
class bus_context
{
    Establish context = () => bus = new Bus();

    Cleanup after = () => bus.Dispose();

    protected static Bus bus;

    /// <summary>
    /// Waits for the message to be consumed on other thread.
    /// </summary>
    protected static void WaitForMessageToBeConsumed(int delayInMilliseconds = pub_sub_max_delay)
    {
        Thread.Sleep(delayInMilliseconds);
    }

    /// <summary>
    /// Specifies maximal delay of publishing and subscription operation in milliseconds.
    /// </summary>
    protected const int pub_sub_max_delay = 1;
}
