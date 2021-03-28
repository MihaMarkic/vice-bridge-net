using System;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public abstract record ViceCommand<TResponse> : IViceCommand
        where TResponse : ViceResponse
    {
        public byte ApiVersion { get; }
        public CommandType CommandType { get; }
        public ViceCommand(CommandType commandType, byte apiVersion = 0x01)
        {
            CommandType = commandType;
            ApiVersion = apiVersion;
        }
        public Task<TResponse> Result => tcs.Task;
        readonly TaskCompletionSource<TResponse> tcs = new TaskCompletionSource<TResponse>();
        public abstract uint ContentLength { get; }
        public abstract void WriteContent(Span<byte> buffer);
        public void SetResult(ViceResponse response)
        {
            tcs.SetResult((TResponse)response);
        }
        public (ManagedBuffer Buffer, uint Length) GetBinaryData(uint requestId)
        {
            const uint HeaderLength = 11;
            uint contentLength = ContentLength;
            uint totalLength = HeaderLength + contentLength;
            var buffer = BufferManager.GetBuffer(totalLength);
            buffer.Data[0] = Constants.STX;
            buffer.Data[1] = ApiVersion;
            uint commandLength = contentLength;
            var bufferSpan = buffer.Data.AsSpan();
            BitConverter.TryWriteBytes(bufferSpan.Slice(2,4), commandLength);
            BitConverter.TryWriteBytes(bufferSpan.Slice(6, 4), requestId);
            bufferSpan[10] = (byte)CommandType;
            WriteContent(bufferSpan[11..]);
            return (buffer, totalLength);
        }
        protected int WriteString(string text, Span<byte> buffer)
        {
            var encoder = Encoding.ASCII.GetEncoder();
            encoder.Convert(text, buffer, flush: true, out _, out int bytesUsed, out _);
            return bytesUsed;
        }
    }

    public record ParameterlessCommand<TResponse> : ViceCommand<TResponse>
        where TResponse : ViceResponse
    {
        public ParameterlessCommand(CommandType commandType, byte apiVersion = 0x01): base(commandType, apiVersion)
        {

        }
        public override uint ContentLength => 0;
        public override void WriteContent(Span<byte> buffer)
        { }
    }
}
