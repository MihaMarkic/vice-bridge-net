using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Services.Abstract
{
    /// <summary>
    /// Allows message history tracking.
    /// </summary>
    public interface IMessagesHistory
    {
        /// <summary>
        /// Starts timer and resets history.
        /// </summary>
        void Start();
        /// <summary>
        /// Adds issued command.
        /// </summary>
        /// <param name="sequence">VICE sequence id</param>
        /// <param name="command">Issued VICE command</param>
        /// <returns></returns>
        ValueTask<int> AddCommandAsync(uint sequence, IViceCommand? command);
        /// <summary>
        /// Updates existing issued command with response.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="response"></param>
        void UpdateWithResponse(int id, ViceResponse response);
        /// <summary>
        /// Adds unbound responses.
        /// </summary>
        /// <param name="response"></param>
        void AddsResponseOnly(ViceResponse response);
        /// <summary>
        /// Add linked response. Some commands might have child responses.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="response"></param>
        void UpdateWithLinkedResponse(int id, ViceResponse response);
    }
}

/// <summary>
/// Contains message history related data including command and response.
/// </summary>
/// <param name="Sequence">VICE sequence value.. Null when response is unbound.</param>
/// <param name="Command">VICE command. Null when response is unbound.</param>
/// <param name="Response">VICE response</param>
/// <param name="StartTime">Ticks when command was issued.</param>
/// <param name="Elapsed">Ticks when response was received. Null when response is unbound.</param>
/// <param name="LinkedResponses">A list of linked responses (i.e. for <see cref="CheckpointInfoResponse"/>)</param>
public record CommunicationData(uint? Sequence, IViceCommand? Command, ViceResponse? Response, long StartTime, long? Elapsed,
    ImmutableArray<ViceResponse> LinkedResponses);
