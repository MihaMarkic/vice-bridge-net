using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Exceptions
{
    internal class ResumeOnStoppedTimeoutException: TimeoutException
    {
        public ViceResponse Response { get; }
        /// <summary>Initializes a new instance of the <see cref="ResumeOnStoppedTimeoutException" /> class.
        /// </summary>
        /// <param name="response"></param>
        public ResumeOnStoppedTimeoutException(ViceResponse response)
        {
            Response = response;
        }

        /// <summary>Initializes a new instance of the <see cref="ResumeOnStoppedTimeoutException" /> class with the specified error message.</summary>
        /// <param name="response"></param>
        /// <param name="message">The message that describes the error.</param>
        public ResumeOnStoppedTimeoutException(ViceResponse response, string message) : base(message)
        {
            Response = response;
        }

        /// <summary>Initializes a new instance of the <see cref="ResumeOnStoppedTimeoutException" /> class with the specified error message and inner exception.</summary>
        /// <param name="response"></param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not <see langword="null" />, the current exception is raised in a <see langword="catch" /> block that handles the inner exception.</param>
        public ResumeOnStoppedTimeoutException(ViceResponse response, string message, Exception innerException) : base(message, innerException)
        {
            Response = response;
        }
    }
}
