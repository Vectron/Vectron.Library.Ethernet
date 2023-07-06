using System.Buffers;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// An disposable <see cref="ArrayPool{T}"/> array.
/// </summary>
/// <typeparam name="T">The type to store.</typeparam>
internal sealed class DisposableArrayPool<T> : IDisposable
{
    private T[] data = Array.Empty<T>();
    private bool disposed;

    /// <summary>
    /// Gets the currently stored data.
    /// </summary>
    public ReadOnlyMemory<T> Data
    {
        get;
        private set;
    }

    /// <summary>
    /// Add data to this array pool.
    /// </summary>
    /// <param name="newData">The data to add.</param>
    public void Add(ReadOnlySpan<T> newData)
    {
        ThrowIfDisposed();
        var newSize = Data.Length + newData.Length;
        var staging = ArrayPool<T>.Shared.Rent(newSize);
        Data.CopyTo(staging);
        newData.CopyTo(staging.AsSpan(Data.Length));

        Clear();
        data = staging;
        Data = data.AsMemory(0, newSize);
    }

    /// <summary>
    /// Clears the data.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        Data = default;
        ArrayPool<T>.Shared.Return(data, clearArray: false);
        data = Array.Empty<T>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        Clear();
        disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
