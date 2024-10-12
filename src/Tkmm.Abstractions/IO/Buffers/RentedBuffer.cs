using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tkmm.Abstractions.IO.Buffers;

public readonly struct RentedBuffer<T> : IDisposable where T : unmanaged
{
    private readonly T[] _buffer;
    private readonly int _size;

    public static readonly RentedBuffer<T> Empty = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RentedBuffer<T> Allocate(int size)
    {
        return new RentedBuffer<T>(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RentedBuffer<byte> Allocate(Stream stream)
    {
        return Allocate(stream, Convert.ToInt32(stream.Length));
    }

    public static RentedBuffer<byte> Allocate(Stream stream, int size)
    {
        RentedBuffer<byte> result = RentedBuffer<byte>.Allocate(size);
        int read = stream.Read(result._buffer, 0, size);
        Debug.Assert(read == size);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<RentedBuffer<byte>> AllocateAsync(Stream stream, CancellationToken ct = default)
    {
        return AllocateAsync(stream, Convert.ToInt32(stream.Length), ct);
    }
    
    public static async ValueTask<RentedBuffer<byte>> AllocateAsync(Stream stream, int size, CancellationToken ct = default)
    {
        RentedBuffer<byte> result = RentedBuffer<byte>.Allocate(size);
        int read = await stream.ReadAsync(result.Memory, ct);
        Debug.Assert(read == size);
        return result;
    }

    public Span<T> Span {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.AsSpan(.._size);
    }

    public Memory<T> Memory {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.AsMemory(.._size);
    }

    public ArraySegment<T> Segment {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    public RentedBuffer()
    {
        _buffer = [];
        Segment = [];
    }
    
    private RentedBuffer(int size)
    {
        _buffer = ArrayPool<T>.Shared.Rent(size);
        _size = size;
        Segment = new ArraySegment<T>(_buffer, 0, _size);
    }

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(_buffer);
    }
}