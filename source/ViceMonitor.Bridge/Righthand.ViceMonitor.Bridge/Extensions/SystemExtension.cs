using System.Net.Sockets;

namespace System
{
    internal static class SystemExtension
    {
        internal static byte AsByte(this bool value) => value ? (byte)1 : (byte)0;
        internal static Task WaitForDataAsync(this Socket socket, CancellationToken ct = default)
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            socket.BeginReceive([], 0, 0, SocketFlags.Peek, _ =>
            {
                completion.TrySetResult();
            }, null);
            return completion.Task;
        }
    }
}
