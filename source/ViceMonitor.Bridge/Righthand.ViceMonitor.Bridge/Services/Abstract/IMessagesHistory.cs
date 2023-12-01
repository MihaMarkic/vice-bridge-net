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
        int AddCommand(uint sequence, IViceCommand? command);
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
