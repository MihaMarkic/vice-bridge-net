using System;
using System.Buffers;

namespace Righthand.ViceMonitor.Bridge
{
    /// <summary>
    /// Manages byte array buffers from pool.
    /// </summary>
    /// <threadsafe>Class is thread safe.</threadsafe>
    public static class BufferManager
    {
        /// <summary>
        /// Gets a buffer with given size or larger.
        /// </summary>
        /// <param name="minLength">A minimum size requested.</param>
        /// <returns>An instance of <see cref="ManagedBuffer"/> with a byte buffer of given minimal size or larger.</returns>
        public static ManagedBuffer GetBuffer(uint minLength)
        {
            return GetBuffer(ArrayPool<byte>.Shared, minLength);
        }
        /// <summary>
        /// Gets a buffer with given size or larger.
        /// </summary>
        /// <param name="pool">Pool where the buffer is retrieved from.</param>
        /// <param name="minLength">A minimum size requested.</param>
        /// <returns>An instance of <see cref="ManagedBuffer"/> with a byte buffer of given minimal size or larger.</returns>
        public static ManagedBuffer GetBuffer(this ArrayPool<byte> pool, uint minLength)
        {
            return new ManagedBuffer(pool, pool.Rent((int)minLength), minLength);
        }
    }
    /// <summary>
    /// Holds byte array retrieved form a pool.
    /// </summary>
    /// <remarks>
    /// It should be disposed once it is not need anymore to return data to the pool. Otherwise memory leaks will happen.
    /// </remarks>
    public readonly struct ManagedBuffer: IDisposable
    {
        /// <summary>
        /// An empty buffer.
        /// </summary>
        public static readonly ManagedBuffer Empty = new (0);
        /// <summary>
        /// Byte array of minimal size of <see cref="Size"/>.
        /// </summary>
        public byte[] Data { get; }
        readonly ArrayPool<byte>? pool;
        /// <summary>
        /// The requested size of data.
        /// </summary>
        /// <remarks>Depending on the pool, the actual <see cref="Data"/> length can be larger.</remarks>
        public uint Size { get; }
        ManagedBuffer(uint size)
        {
            pool = null;
            Data = Array.Empty<byte>();
            Size = 0;
        }
        internal ManagedBuffer(ArrayPool<byte> pool, byte[] data, uint size)
        {
            this.pool = pool;
            Data = data;
            Size = size;
        }
        /// <summary>
        /// Releases all resources used by the <see cref="ManagedBuffer"/>.
        /// </summary>
        public void Dispose()
        {
            pool?.Return(Data);
        }
    }
}
