using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Shared;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Set a resource value in the emulator. See section 6.1 Format of resource files. 
    /// </summary>
    /// <param name="Resource"></param>
    public record ResourceSetCommand(Resource Resource) : ViceCommand<EmptyViceResponse>(CommandType.ResourceSet)
    {
        /// <inheritdoc />
        public override uint ContentLength => Resource.Length;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            switch (Resource)
            {
                case StringResource stringResource:
                    buffer[0] = (byte)ResourceType.String;
                    buffer[1] = stringResource.Length;
                    WriteString(stringResource.Text, buffer[2..]);
                    break;
                case IntegerResource integerResource:
                    buffer[0] = (byte)ResourceType.Integer;
                    buffer[1] = Resource.Length;
                    BitConverter.TryWriteBytes(buffer[2..], integerResource.Value);
                    break;
                default:
                    throw new Exception($"Unknown resource type {Resource.GetType().Name}");
            }
        }
    }
}
