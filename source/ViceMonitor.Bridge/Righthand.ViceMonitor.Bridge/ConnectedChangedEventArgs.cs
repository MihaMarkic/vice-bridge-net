using System;

namespace Righthand.ViceMonitor.Bridge
{
    /// <summary>
    /// Occurs when connection status changes.
    /// </summary>
    public class ConnectedChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets whether the connection with VICE binary bridge is active.
        /// </summary>
        public bool IsConnected { get; }
        internal ConnectedChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
