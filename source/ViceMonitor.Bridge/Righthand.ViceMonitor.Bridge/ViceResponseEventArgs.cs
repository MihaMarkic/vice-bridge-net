using System;
using Righthand.ViceMonitor.Bridge.Commands;

namespace Righthand.ViceMonitor.Bridge
{
    public class ViceResponseEventArgs: EventArgs
    {
        ViceResponse Response { get; }
        public ViceResponseEventArgs(ViceResponse response)
        {
            Response = response;
        }
    }
}
