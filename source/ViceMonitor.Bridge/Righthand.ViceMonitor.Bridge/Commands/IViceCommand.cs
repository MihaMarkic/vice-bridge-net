using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public interface IViceCommand
    {
        byte ApiVersion { get; }
        CommandType CommandType { get; }
        string ToString();
        void WriteContent(Span<byte> buffer);
        void SetResult(ViceResponse response);
        (ManagedBuffer Buffer, uint Length) GetBinaryData(uint requestId);
    }
}
