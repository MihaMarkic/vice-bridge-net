using System.Net.Sockets;

namespace System
{
    internal static class SystemExtension
    {
        internal static byte AsByte(this bool value) => value ? (byte)1 : (byte)0;
        internal static Task WaitForDataAsync(this Socket socket, CancellationToken ct = default)
        {
            var completition = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            socket.BeginReceive(Array.Empty<byte>(), 0, 0, SocketFlags.Peek, ar =>
            {
                completition.TrySetResult();
            }, null);
            return completition.Task;
        }
    }
}
