using System.Diagnostics.CodeAnalysis;
using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Represents a response to given command.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public readonly struct CommandResponse<TResponse>
        where TResponse : ViceResponse
    {
        /// <summary>
        /// <see cref="ErrorCode"/> returned by VICE
        /// </summary>
        public ErrorCode ErrorCode { get; }
        /// <summary>
        /// True when ErrorCode is <see cref="ErrorCode.OK"/>, false otherwise.
        /// When false, the <see cref="Response"/> is null.
        /// </summary>
        public bool IsSuccess => ErrorCode == ErrorCode.OK;
        /// <summary>
        /// VICE response to given command when <see cref="IsSuccess"/> is true.
        /// </summary>
        public TResponse? Response { get; }
        /// <summary>
        /// Creates a new instance of <see cref="CommandResponse{TResponse}"/> where successful.
        /// </summary>
        /// <param name="response"></param>
        public CommandResponse(TResponse response)
        {
            ErrorCode = response.ErrorCode;
            Response = response;
        }
        /// <summary>
        /// Creates a new instance of <see cref="CommandResponse{TResponse}"/> where failure.
        /// </summary>
        /// <param name="errorCode"></param>
        public CommandResponse(ErrorCode errorCode)
        {
            ErrorCode = errorCode;
            Response = default;
        }
    }
}
