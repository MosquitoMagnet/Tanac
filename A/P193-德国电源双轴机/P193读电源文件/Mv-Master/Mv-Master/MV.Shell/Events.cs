using Mv.Core.Interfaces;
using Prism.Events;

namespace Mv.Shell
{
    internal class MainWindowLoadingEvent : PubSubEvent<bool> { }

    internal class SignUpSuccessEvent : PubSubEvent<SignUpArgs> { }
}
