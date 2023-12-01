using System.Collections.Immutable;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Abstract;

namespace Righthand.ViceMonitor.Bridge.Services.Implementation;

/// <summary>
/// Provides a no-op service for message history.
/// </summary>
public class NullMessagesHistory : IMessagesHistory
{
    ///<inheritdoc/>
    int IMessagesHistory.AddCommand(uint sequence, IViceCommand? command) => 0;
    ///<inheritdoc/>
    void IMessagesHistory.AddsResponseOnly(ViceResponse response)
    { }
    ///<inheritdoc/>
    void IMessagesHistory.Start()
    { }
    ///<inheritdoc/>
    void IMessagesHistory.UpdateWithLinkedResponse(int id, ViceResponse response)
    { }
    ///<inheritdoc/>
    void IMessagesHistory.UpdateWithResponse(int id, ViceResponse response)
    { }
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
