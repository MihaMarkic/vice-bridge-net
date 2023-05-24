using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// VICE defaults.
    /// </summary>
    public static class ViceCommand
    {
        /// <summary>
        /// Default API version supported.
        /// </summary>
        public const int DefaultApiVersion = 0x02;
    }
    /// <inheritdoc cref="IViceCommand"/>
    /// <summary>
    /// Base class for VICE commands.
    /// </summary>
    /// <typeparam name="TResponse">Type of the response to this command.</typeparam>
    public abstract record ViceCommand<TResponse> : IViceCommand
        where TResponse : ViceResponse
    {
        /// <inheritdoc cref="IViceCommand.ApiVersion"/>
        public byte ApiVersion { get; }
        /// <inheritdoc cref="IViceCommand.CommandType"/>
        public CommandType CommandType { get; }
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="apiVersion"></param>
        protected ViceCommand(CommandType commandType, byte apiVersion = ViceCommand.DefaultApiVersion)
        {
            CommandType = commandType;
            ApiVersion = apiVersion;
        }
        /// <summary>
        /// Task that returns the result.
        /// </summary>
        public Task<CommandResponse<TResponse>> Response => tcs.Task;
        readonly TaskCompletionSource<CommandResponse<TResponse>> tcs = new (TaskCreationOptions.RunContinuationsAsynchronously);
        /// <summary>
        /// Length of the command's body expressed in bytes.
        /// </summary>
        public abstract uint ContentLength { get; }
        /// <inheritdoc cref="IViceCommand.WriteContent(Span{byte})"/>
        public abstract void WriteContent(Span<byte> buffer);
        /// <inheritdoc cref="IViceCommand.SetResult(ViceResponse)"/>
        void IViceCommand.SetResult(ViceResponse response)
        {
            if (response is TResponse && response.ErrorCode == ErrorCode.OK)
            {
                tcs.SetResult(new CommandResponse<TResponse>((TResponse)response));
            }
            else
            {
                tcs.SetResult(new CommandResponse<TResponse>(response.ErrorCode));
            }
        }
        /// <inheritdoc />
        public void SetException(Exception ex)
        {
            tcs.SetException(ex);
        }
        /// <inheritdoc cref="IViceCommand.GetBinaryData(uint)"/>
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
            if (contentLength > 0)
            {
                WriteContent(bufferSpan[11..]);
            }
            return (buffer, totalLength);
        }
        /// <inheritdoc cref="IViceCommand.ApiVersion"/>
        protected int WriteString(string text, Span<byte> buffer)
        {
            var encoder = Encoding.ASCII.GetEncoder();
            encoder.Convert(text, buffer, flush: true, out _, out int bytesUsed, out _);
            return bytesUsed;
        }
        ///<inheritdoc />
        public virtual ImmutableArray<string> CollectErrors() => ImmutableArray<string>.Empty;
    }
    /// <summary>
    /// Defines a command without parameters.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public record ParameterlessCommand<TResponse> : ViceCommand<TResponse>
        where TResponse : ViceResponse
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ParameterlessCommand{TResponse}"/> command.
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="apiVersion"></param>
        protected ParameterlessCommand(CommandType commandType, byte apiVersion = ViceCommand.DefaultApiVersion) 
            : base(commandType, apiVersion)
        {

        }
        /// <inheritdoc />
        public override uint ContentLength => 0;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        { }
    }
}
