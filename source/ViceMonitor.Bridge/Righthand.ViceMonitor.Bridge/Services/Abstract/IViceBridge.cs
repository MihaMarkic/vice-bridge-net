using System;
using System.Net;
using System.Threading.Tasks;
using Righthand.ViceMonitor.Bridge.Commands;

namespace Righthand.ViceMonitor.Bridge.Services.Abstract
{
    public interface IViceBridge: IAsyncDisposable, IDisposable
    {
        Task? RunnerTask { get; }
        bool IsRunning { get; }
        bool IsConnected { get; }
        void Start(IPAddress address, int port = 6502);
        void EnqueCommand(IViceCommand command);
        event EventHandler<ViceResponseEventArgs>? ViceResponse;
        event EventHandler<ConnectedChangedEventArgs>? ConnectedChanged;
    }
}
