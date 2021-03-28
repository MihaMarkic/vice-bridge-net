using System;
using System.Collections.Immutable;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public abstract record ViceResponse(byte ApiVersion, ErrorCode ErrorCode);

    public record MemoryGetResponse(byte ApiVersion, ErrorCode ErrorCode, ManagedBuffer? Memory) : ViceResponse(ApiVersion, ErrorCode), IDisposable
    {
        public void Dispose()
        {
            Memory?.Dispose();
        }
    }

    public record CheckpointResponse(byte ApiVersion, ErrorCode ErrorCode, uint CheckpointNumber, bool CurrentlyHit, ushort StartAddress, ushort EndAddress,
        bool StopWhenHit, bool Enabled, CpuOperation CpuOperation, bool Temporary, uint HitCount, uint IgnoreCount, bool HasCondition) 
        : ViceResponse(ApiVersion, ErrorCode);

    public record RegistersResponse(byte ApiVersion, ErrorCode ErrorCode, ImmutableArray<RegisterItem> Items) 
        : ViceResponse(ApiVersion, ErrorCode)
    {
    }
    public record UndumpResponse(byte ApiVersion, ErrorCode ErrorCode, ushort ProgramCounterPosition) : ViceResponse(ApiVersion, ErrorCode);
    public record ResourceGetResponse(byte ApiVersion, ErrorCode ErrorCode, Resource? Resource) : ViceResponse(ApiVersion, ErrorCode);
    public record JamResponse(byte ApiVersion, ErrorCode ErrorCode, ushort ProgramCounterPosition): ViceResponse(ApiVersion, ErrorCode);
    public record StoppedResponse(byte ApiVersion, ErrorCode ErrorCode, ushort ProgramCounterPosition) : ViceResponse(ApiVersion, ErrorCode);
    public record ResumedResponse(byte ApiVersion, ErrorCode ErrorCode, ushort ProgramCounterPosition) : ViceResponse(ApiVersion, ErrorCode);

    public record BanksAvailableResponse(byte ApiVersion, ErrorCode ErrorCode, ImmutableArray<BankItem> Banks) : ViceResponse(ApiVersion, ErrorCode);

    public record RegistersAvailableResponse(byte ApiVersion, ErrorCode ErrorCode, ImmutableArray<FullRegisterItem> Banks) : ViceResponse(ApiVersion, ErrorCode);

    public record DisplayGetResponse(byte ApiVersion, ErrorCode ErrorCode, 
        ushort DebugWidth, ushort DebugHeight, ushort DebugOffsetX, ushort DebugOffsetY, ushort InnerWidth, ushort InnerHeight, ManagedBuffer? Image)
        : ViceResponse(ApiVersion, ErrorCode), IDisposable
    {
        public void Dispose()
        {
            Image?.Dispose();
        }
    }

    public record EmptyViceResponse(byte ApiVersion, ErrorCode ErrorCode) : ViceResponse(ApiVersion, ErrorCode);
}
