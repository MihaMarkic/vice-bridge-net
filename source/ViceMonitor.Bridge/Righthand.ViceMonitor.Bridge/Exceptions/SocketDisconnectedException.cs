using System;
using System.Runtime.Serialization;

namespace Righthand.ViceMonitor.Bridge.Exceptions
{
    public class SocketDisconnectedException : Exception
    {
        public SocketDisconnectedException()
        {
        }

        public SocketDisconnectedException(string? message) : base(message)
        {
        }

        public SocketDisconnectedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SocketDisconnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
