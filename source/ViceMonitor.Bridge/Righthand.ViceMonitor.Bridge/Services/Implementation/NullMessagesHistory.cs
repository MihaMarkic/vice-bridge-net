﻿using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Abstract;

namespace Righthand.ViceMonitor.Bridge.Services.Implementation;

/// <summary>
/// Provides a no-op service for message history.
/// </summary>
public class NullMessagesHistory : IMessagesHistory
{
    ///<inheritdoc/>
    ValueTask<int> IMessagesHistory.AddCommandAsync(uint sequence, IViceCommand? command) => new ValueTask<int>(0);
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
