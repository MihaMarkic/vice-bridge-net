using System;
using Microsoft.Extensions.Logging;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public class ResponseBuilder
    {
        readonly ILogger<ResponseBuilder> logger;
        public ResponseBuilder(ILogger<ResponseBuilder> logger)
        {
            this.logger = logger;
        }
        public uint GetReponseBodyLength(ReadOnlySpan<byte> header) => BitConverter.ToUInt32(header[2..]);
        public (ViceResponse Response, uint RequestId) Build(ReadOnlySpan<byte> header, ReadOnlySpan<byte> buffer)
        {
            byte stx = header[0]; // should be STX
            if (stx != Constants.STX)
            {
                throw new Exception("Not starting with STX");
            }
            byte apiVersion = header[1];
            if (apiVersion != 0x01)
            {
                throw new Exception($"Unknown API version {apiVersion}");
            }
            uint length = GetReponseBodyLength(header);
            var responseType = (ResponseType)header[6];
            var errorCode = (ErrorCode)header[7];
            uint requestId = BitConverter.ToUInt32(header[8..]);
            logger.LogDebug($"Decoding {responseType}({(byte)responseType:x2}) with error code {errorCode} and request id {requestId:x4}");
            ViceResponse result = responseType switch
            {
                ResponseType.CheckpointSet => BuildCheckpointSetResponse(apiVersion, errorCode, buffer),
                //_ => throw new Exception($"Unknown response type {responseType}"),
                _ => new TempViceResponse(apiVersion, errorCode),
            };
            return (result, requestId);
        }

        internal CheckpointSetResponse BuildCheckpointSetResponse(byte apiVersion, ErrorCode errorCode, ReadOnlySpan<byte> buffer)
        {
            if (errorCode == ErrorCode.OK)
            {
                return new CheckpointSetResponse(apiVersion, errorCode,
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
                return new CheckpointSetResponse(apiVersion, errorCode, default, default, default, default,
                    default, default, default, default, default, default, default);
            }
        }
    }
}
