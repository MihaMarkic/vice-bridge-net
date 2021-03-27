using System.Collections.Immutable;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public abstract record ViceResponse(byte ApiVersion, ErrorCode ErrorCode);

    public record MemoryGetResponse(byte ApiVersion, ErrorCode ErrorCode, ImmutableArray<byte> Memory) : ViceResponse(ApiVersion, ErrorCode);

    public abstract record CheckpointResponse(byte ApiVersion, ErrorCode ErrorCode, uint CheckpointNumber, bool CurrentlyHit, ushort StartAddress, ushort EndAddress,
        bool StopWhenHit, bool Enabled, CpuOperation CpuOperation, bool Temporary, uint HitCount, uint IgnoreCount, bool HasCondition) 
        : ViceResponse(ApiVersion, ErrorCode);
    public record CheckpointSetResponse(byte ApiVersion, ErrorCode ErrorCode, uint CheckpointNumber, bool CurrentlyHit, ushort StartAddress, ushort EndAddress,
        bool StopWhenHit, bool Enabled, CpuOperation CpuOperation, bool Temporary, uint HitCount, uint IgnoreCount, bool HasCondition)
        : CheckpointResponse(ApiVersion, ErrorCode, CheckpointNumber, CurrentlyHit, StartAddress, EndAddress,
        StopWhenHit, Enabled, CpuOperation, Temporary, HitCount, IgnoreCount, HasCondition);
    public record CheckpointGetResponse(byte ApiVersion, ErrorCode ErrorCode, uint CheckpointNumber, bool CurrentlyHit, ushort StartAddress, ushort EndAddress,
        bool StopWhenHit, bool Enabled, CpuOperation CpuOperation, bool Temporary, uint HitCount, uint IgnoreCount, bool HasCondition)
        : CheckpointResponse(ApiVersion, ErrorCode, CheckpointNumber, CurrentlyHit, StartAddress, EndAddress,
        StopWhenHit, Enabled, CpuOperation, Temporary, HitCount, IgnoreCount, HasCondition);
    public record CheckpointListResponse(byte ApiVersion, ErrorCode ErrorCode, uint CheckpointNumber, bool CurrentlyHit, ushort StartAddress, ushort EndAddress,
        bool StopWhenHit, bool Enabled, CpuOperation CpuOperation, bool Temporary, uint HitCount, uint IgnoreCount, bool HasCondition)
        : CheckpointResponse(ApiVersion, ErrorCode, CheckpointNumber, CurrentlyHit, StartAddress, EndAddress,
        StopWhenHit, Enabled, CpuOperation, Temporary, HitCount, IgnoreCount, HasCondition);

    public record TempViceResponse(byte ApiVersion, ErrorCode ErrorCode) : ViceResponse(ApiVersion, ErrorCode);
}
