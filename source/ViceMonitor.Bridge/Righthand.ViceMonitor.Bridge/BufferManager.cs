using System;
using System.Buffers;

namespace Righthand.ViceMonitor.Bridge
{
    public static class BufferManager
    {
        public static ManagedBuffer GetBuffer(uint minLength)
        {
            return GetBuffer(ArrayPool<byte>.Shared, minLength);
        }
        public static ManagedBuffer GetBuffer(this ArrayPool<byte> pool, uint minLength)
        {
            return new ManagedBuffer(pool, pool.Rent((int)minLength), minLength);
        }
    }

    public readonly struct ManagedBuffer: IDisposable
    {
        public static readonly ManagedBuffer Empy = new ManagedBuffer(0);
        public byte[] Data { get; }
        readonly ArrayPool<byte>? pool;
        public uint Size { get; }
        ManagedBuffer(uint size)
        {
            pool = null;
            Data = new byte[0];
            Size = 0;
        }
        internal ManagedBuffer(ArrayPool<byte> pool, byte[] data, uint size)
        {
            this.pool = pool;
            Data = data;
            Size = size;
        }

        public void Dispose()
        {
            pool?.Return(Data);
        }
    }
}
