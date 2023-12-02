using System.Text;
using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Shared;

namespace Righthand.ViceMonitor.Bridge.Responses
{
    /// <summary>
    /// Builds response objects from data arrays.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "IoC")]
    public class ResponseBuilder
    {
        readonly ILogger<ResponseBuilder> logger;
        /// <summary>
        /// Creates an instance of <see cref="ResponseBuilder"/>.
        /// </summary>
        /// <param name="logger"></param>
        public ResponseBuilder(ILogger<ResponseBuilder> logger)
        {
            this.logger = logger;
        }

        internal uint GetResponseBodyLength(ReadOnlySpan<byte> header) => BitConverter.ToUInt32(header[2..]);
        internal (ViceResponse Response, uint RequestId) Build(ReadOnlySpan<byte> header, byte expectedApiVersion, ReadOnlySpan<byte> buffer)
        {
            byte stx = header[0]; // should be STX
            if (stx != Constants.STX)
            {
                throw new Exception("Not starting with STX");
            }
            byte apiVersion = header[1];
            if (apiVersion != expectedApiVersion)
            {
                throw new Exception($"Unknown API version {apiVersion}");
            }
            uint length = GetResponseBodyLength(header);
            var responseType = (ResponseType)header[6];
            var errorCode = (ErrorCode)header[7];
            uint requestId = BitConverter.ToUInt32(header[8..]);
            logger.LogDebug($"Decoding {responseType}({(byte)responseType:x2}) with error code {errorCode} and request id {requestId:x4}");
            ViceResponse result = responseType switch
            {
                ResponseType.MemoryGet          => BuildMemoryGetResponse(apiVersion, errorCode, buffer),
                ResponseType.MemorySet          => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.CheckpointInfo     => BuildCheckpointInfoResponse(apiVersion, errorCode, buffer),
                ResponseType.CheckpointList     => BuildCheckpointListResponse(apiVersion, errorCode, buffer),
                ResponseType.CheckpointToggle   => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.ConditionSet       => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.RegisterInfo       => BuildRegistersResponse(apiVersion, errorCode, buffer),
                ResponseType.Dump               => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.Undump             => BuildUndumpResponse(apiVersion, errorCode, buffer),
                ResponseType.ResourceGet        => BuildResourceGetResponse(apiVersion, errorCode, buffer),
                ResponseType.ResourceSet        => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.Jam                => BuildJamResponse(apiVersion, errorCode, buffer),
                ResponseType.Stopped            => BuildStoppedResponse(apiVersion, errorCode, buffer),
                ResponseType.Resumed            => BuildResumedResponse(apiVersion, errorCode, buffer),
                ResponseType.AdvanceInstruction => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.KeyboardFeed       => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.ExecuteUntilReturn => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.Ping               => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.BanksAvailable     => BuildBanksAvailableResponse(apiVersion, errorCode, buffer),
                ResponseType.RegistersAvailable => BuildRegistersAvailableResponse(apiVersion, errorCode, buffer),
                ResponseType.DisplayGet         => BuildDisplayGetResponse(apiVersion, errorCode, buffer),
                ResponseType.Info               => BuildInfoResponse(apiVersion, errorCode, buffer),
                ResponseType.Exit               => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.Quit               => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.Reset              => BuildEmptyResponse(apiVersion, errorCode),
                ResponseType.AutoStart          => BuildAutoStartResponse(apiVersion, errorCode),
                //_ => throw new Exception($"Unknown response type {responseType}"),
                _ => new EmptyViceResponse(apiVersion, errorCode),
            };
            return (result, requestId);
        }

        internal MemoryGetResponse BuildMemoryGetResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            ManagedBuffer segmentBuffer;
            if (errorCode == ErrorCode.OK)
            {
                ushort memorySegmentLength = BitConverter.ToUInt16(buffer);
                segmentBuffer = BufferManager.GetBuffer(memorySegmentLength);
                buffer.Slice(2, memorySegmentLength).CopyTo(segmentBuffer.Data);
            }
            else
            {
                segmentBuffer = ManagedBuffer.Empty;
            }
            return new MemoryGetResponse(apiVersion, errorCode, segmentBuffer);
        }
        internal CheckpointInfoResponse BuildCheckpointInfoResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                return new CheckpointInfoResponse(apiVersion, errorCode,
                    CheckpointNumber: BitConverter.ToUInt32(buffer),
                    CurrentlyHit: BitConverter.ToBoolean(buffer[4..]),
                    StartAddress: BitConverter.ToUInt16(buffer[5..]),
                    EndAddress: BitConverter.ToUInt16(buffer[7..]),
                    StopWhenHit: BitConverter.ToBoolean(buffer[9..]),
                    Enabled: BitConverter.ToBoolean(buffer[10..]),
                    CpuOperation: (CpuOperation)buffer[11],
                    Temporary: BitConverter.ToBoolean(buffer[12..]),
                    HitCount: BitConverter.ToUInt32(buffer[13..]),
                    IgnoreCount: BitConverter.ToUInt32(buffer[17..]),
                    HasCondition: BitConverter.ToBoolean(buffer[21..]));
            }
            else
            {
                return new CheckpointInfoResponse(apiVersion, errorCode, default, default, default, default,
                    default, default, default, default, default, default, default);
            }
        }
        internal CheckpointListResponse BuildCheckpointListResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                return new CheckpointListResponse(apiVersion, errorCode, 
                    TotalNumberOfCheckpoints: BitConverter.ToUInt32(buffer),
                    Info: ImmutableArray<CheckpointInfoResponse>.Empty);
            }
            else
            {
                return new CheckpointListResponse(apiVersion, errorCode, default, default);
            }
        }
        internal RegistersResponse BuildRegistersResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            var items = ImmutableArray<RegisterItem>.Empty;
            if (errorCode == ErrorCode.OK)
            {
                ushort itemsCount = BitConverter.ToUInt16(buffer);
                for (ushort i = 0; i < itemsCount; i++)
                {
                    var itemBuffer = buffer[(int)(2 + i * RegisterItem.ContentLength)..];
                    System.Diagnostics.Debug.Assert(itemBuffer[0] == 3);
                    var item = new RegisterItem(
                        //Size: itemBuffer[0], should be 3
                        RegisterId: itemBuffer[1],
                        RegisterValue: BitConverter.ToUInt16(itemBuffer[2..])
                    );
                    items = items.Add(item);
                }
            }
            return new RegistersResponse(apiVersion, errorCode, items);
        }
        internal UndumpResponse BuildUndumpResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                return new UndumpResponse(apiVersion, errorCode, ProgramCounterPosition: BitConverter.ToUInt16(buffer));
            }
            else
            {
                return new UndumpResponse(apiVersion, errorCode, ProgramCounterPosition: default);
            }
        }
        internal ResourceGetResponse BuildResourceGetResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                ResourceType resourceType = (ResourceType)buffer[0];
                byte length = buffer[1];
                var data = buffer.Slice(2, length);
                Resource resource = resourceType switch
                {
                    ResourceType.String => new StringResource(Encoding.ASCII.GetString(data)),
                    ResourceType.Integer => new IntegerResource(BitConverter.ToInt32(data)),
                    _ => throw new Exception($"Unknown resource type {resourceType}"),
                };
                return new ResourceGetResponse(apiVersion, errorCode, resource);
            }
            else
            {
                return new ResourceGetResponse(apiVersion, errorCode, Resource: default);
            }
        }
        internal JamResponse BuildJamResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                return new JamResponse(apiVersion, errorCode, ProgramCounterPosition: BitConverter.ToUInt16(buffer));
            }
            else
            {
                return new JamResponse(apiVersion, errorCode, ProgramCounterPosition: default);
            }
        }
        internal StoppedResponse BuildStoppedResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                return new StoppedResponse(apiVersion, errorCode, ProgramCounterPosition: BitConverter.ToUInt16(buffer));
            }
            else
            {
                return new StoppedResponse(apiVersion, errorCode, ProgramCounterPosition: default);
            }
        }
        internal ResumedResponse BuildResumedResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                return new ResumedResponse(apiVersion, errorCode, ProgramCounterPosition: BitConverter.ToUInt16(buffer));
            }
            else
            {
                return new ResumedResponse(apiVersion, errorCode, ProgramCounterPosition: default);
            }
        }
        internal BanksAvailableResponse BuildBanksAvailableResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            var items = ImmutableArray<BankItem>.Empty;
            if (errorCode == ErrorCode.OK)
            {
                ushort itemsCount = BitConverter.ToUInt16(buffer);
                var itemBuffer = buffer[2..];
                int offset = 0;
                for (ushort i = 0; i < itemsCount; i++)
                {
                    itemBuffer = itemBuffer[offset..];
                    byte itemSize = itemBuffer[0];
                    ushort bankId = BitConverter.ToUInt16(itemBuffer[1..]);
                    byte nameLength = itemBuffer[3];
                    string name = Encoding.ASCII.GetString(buffer.Slice(4, nameLength));

                    var item = new BankItem(bankId, name);
                    items = items.Add(item);
                    offset += itemSize + 1;
                }
            }
            return new BanksAvailableResponse(apiVersion, errorCode, items);
        }
        internal RegistersAvailableResponse BuildRegistersAvailableResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            var items = ImmutableArray<FullRegisterItem>.Empty;
            if (errorCode == ErrorCode.OK)
            {
                ushort itemsCount = BitConverter.ToUInt16(buffer);
                var itemBuffer = buffer[2..];
                int offset = 0;
                for (ushort i = 0; i < itemsCount; i++)
                {
                    itemBuffer = itemBuffer[offset..];
                    byte itemSize = itemBuffer[0];
                    byte registerId = itemBuffer[1];
                    byte registerSize = itemBuffer[2];
                    byte nameLength = itemBuffer[3];
                    string name = Encoding.ASCII.GetString(itemBuffer.Slice(4, nameLength));

                    var item = new FullRegisterItem(registerId, registerSize, name);
                    items = items.Add(item);
                    offset = itemSize + 1;
                }
            }
            return new RegistersAvailableResponse(apiVersion, errorCode, items);
        }
        internal DisplayGetResponse BuildDisplayGetResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                uint infoLength = BitConverter.ToUInt32(buffer);
                ushort debugWidth = BitConverter.ToUInt16(buffer[4..]);
                ushort debugHeight = BitConverter.ToUInt16(buffer[6..]);
                ushort debugOffsetX = BitConverter.ToUInt16(buffer[8..]);
                ushort debugOffsetY = BitConverter.ToUInt16(buffer[10..]);
                ushort innerWidth = BitConverter.ToUInt16(buffer[12..]);
                ushort innerHeight = BitConverter.ToUInt16(buffer[14..]);
                byte bitsPerPixel = buffer[16];
                uint bufferLength = BitConverter.ToUInt32(buffer[17..]);
                var image = BufferManager.GetBuffer(bufferLength);
                buffer.Slice((int)infoLength+4, (int)bufferLength).CopyTo(image.Data);
                return new DisplayGetResponse(apiVersion, errorCode, debugWidth, debugHeight, debugOffsetX, debugOffsetY, 
                    innerWidth, innerHeight, bitsPerPixel, image);
            }
            else
            {
                return new DisplayGetResponse(apiVersion, errorCode, default, default, default, default, default, default,
                    default, ManagedBuffer.Empty);
            }
        }
        internal InfoResponse BuildInfoResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                byte versionSize = buffer[0];
                if (versionSize == 4)
                {
                    byte svnVersionSize = buffer[5];
                    if (svnVersionSize == 4)
                    {
                        uint svnVersion = BitConverter.ToUInt32(buffer[6..]);
                        return new InfoResponse(apiVersion, errorCode,
                            Major: buffer[1], Minor: buffer[2], Build: buffer[3], Revision: buffer[4], svnVersion);
                    }
                }
            }
            return new InfoResponse(apiVersion, errorCode, default, default, default, default, default);
        }
        internal EmptyViceResponse BuildEmptyResponse(byte apiVersion, ErrorCode errorCode) => new(apiVersion, errorCode);
        internal AutoStartResponse BuildAutoStartResponse(byte apiVersion, ErrorCode errorCode) => new(apiVersion, errorCode);
    }
}
