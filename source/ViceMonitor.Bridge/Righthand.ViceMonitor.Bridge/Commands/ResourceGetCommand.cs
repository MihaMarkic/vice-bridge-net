using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Get a resource value from the emulator. See section 6.1 Format of resource files. 
    /// </summary>
    public record ResourceGetCommand : ViceCommand<ResourceGetResponse>
    {
        /// <summary></summary>
        public string ResourceName { get; init; }
        /// <summary>
        /// Creates an instance of <see cref="ResourceGetCommand"/>.
        /// </summary>
        /// <param name="resourceName"></param>
        public ResourceGetCommand(string resourceName) : base(CommandType.ResourceGet)
        {
            if (resourceName.Length > 256)
            {
                throw new ArgumentException($"Maximum filename length is 256 chars", nameof(resourceName));
            }
            ResourceName = resourceName;
        }
        /// <inheritdoc />
        public override uint ContentLength => sizeof(ushort) + (uint)ResourceName.Length;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)ResourceName.Length;
            WriteString(ResourceName, buffer[1..]);
        }
    }
}
