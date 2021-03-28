using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record ResourceSetCommand(Resource Resource) : ViceCommand<EmptyViceResponse>(CommandType.ResourceSet)
    {
        public override uint ContentLength => Resource.Length;
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
