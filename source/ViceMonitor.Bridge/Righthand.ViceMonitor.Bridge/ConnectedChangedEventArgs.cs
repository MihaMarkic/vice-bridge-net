using System;

namespace Righthand.ViceMonitor.Bridge
{
    public class ConnectedChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; }
        public ConnectedChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
