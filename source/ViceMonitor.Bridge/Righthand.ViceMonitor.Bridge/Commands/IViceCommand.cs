using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Provides non generic interface to <see cref="ViceCommand{TResponse}"/>.
    /// </summary>
    public interface IViceCommand
    {
        /// <summary>
        /// ApiVersion that command is for.
        /// </summary>
        byte ApiVersion { get; }
        /// <summary>
        /// VICE command type specifier.
        /// </summary>
        CommandType CommandType { get; }
        /// <summary>
        /// Writes command's body to given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer"></param>
        void WriteContent(Span<byte> buffer);
        /// <summary>
        /// Sets response. When ErrorCode is OK, the response is set, otherwise only ErrorCode is set.
        /// </summary>
        /// <param name="response"></param>
        void SetResult(ViceResponse response);
        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <param name="ex"></param>
        void SetException(Exception ex);
        /// <summary>
        /// Serializes command into byte array.
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        (ManagedBuffer Buffer, uint Length) GetBinaryData(uint requestId);
        /// <summary>
        /// Checks if arguments are valid.
        /// </summary>
        /// <returns>List of errors if there are any, empty list otherwise.</returns>
        ImmutableArray<string> CollectErrors();
    }
}
