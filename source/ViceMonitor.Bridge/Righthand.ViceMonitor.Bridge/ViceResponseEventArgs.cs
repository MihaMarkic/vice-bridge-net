using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge
{
    /// <summary>
    /// Occurs when unbound response is pulled.
    /// </summary>
    public class ViceResponseEventArgs: EventArgs
    {
        /// <summary>
        /// The response that is unbound to any command.
        /// </summary>
        public ViceResponse Response { get; }
        /// <summary>
        /// Initializes an instance of <see cref="ViceResponseEventArgs"/>.
        /// </summary>
        /// <param name="response"></param>
        internal ViceResponseEventArgs(ViceResponse response)
        {
            Response = response;
        }
    }
}
