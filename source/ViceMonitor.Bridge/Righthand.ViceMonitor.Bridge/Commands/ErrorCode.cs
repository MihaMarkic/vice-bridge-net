using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public enum ErrorCode : byte
    {
        OK = 0x00,
        ObjectDoesNotExist = 0x01,
        InvalidMemSpace = 0x02,
        IncorrectCommandLength = 0x80,
        InvalidParameterValue = 0x81,
        UnknownApiVersion = 0x82,
        UnknownCommandType = 0x83,
        GeneralFailure = 0x8f,

    }
}
