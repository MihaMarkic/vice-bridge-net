﻿using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record MemoryGetCommand(byte SideEffects, ushort StartAddress, ushort EndAddress, MemSpace MemSpace, ushort BankId)
        : ViceCommand<EmptyViceResponse>(CommandType.MemoryGet)
    {
        public override uint ContentLength { get; } = sizeof(byte) + sizeof(ushort) + sizeof(ushort) + sizeof(byte) + sizeof(ushort);
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = SideEffects;
            BitConverter.TryWriteBytes(buffer[1..], StartAddress);
            BitConverter.TryWriteBytes(buffer[3..], EndAddress);
            buffer[5] = (byte)MemSpace;
            BitConverter.TryWriteBytes(buffer[6..], BankId);
        }
    }
}