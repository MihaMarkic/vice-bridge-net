using System;
using System.Runtime.Serialization;

namespace Righthand.ViceMonitor.Bridge.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class SocketDisconnectedException : Exception
    {
        /// <summary>
        /// Creates an instance of <see cref="SocketDisconnectedException"/>
        /// </summary>
        public SocketDisconnectedException()
        {
        }
        /// <summary>
        /// Creates an instance of <see cref="SocketDisconnectedException"/>
        /// </summary>
        /// <param name="message"></param>
        public SocketDisconnectedException(string? message) : base(message)
        {
        }
        /// <summary>
        /// Creates an instance of <see cref="SocketDisconnectedException"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public SocketDisconnectedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// Creates an instance of <see cref="SocketDisconnectedException"/>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SocketDisconnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
