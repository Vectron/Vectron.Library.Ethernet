using System.Buffers;

namespace Vectron.Library.Ethernet;

/// <summary>
/// An disposable <see cref="ArrayPool{T}"/> array.
/// </summary>
/// <typeparam name="T">The type to store.</typeparam>
internal sealed class DisposableArrayPool<T> : IDisposable
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1010:Opening square brackets should be spaced correctly", Justification = "Style cop hasn't caught up yet.")]
    private T[] data = [];

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1010:Opening square brackets should be spaced correctly", Justification = "Style cop hasn't caught up yet.")]
    public void Clear()
    {
        ThrowIfDisposed();
        Data = default;
        ArrayPool<T>.Shared.Return(data, clearArray: false);
        data = [];
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
#if NET7_0_OR_GREATER
        => ObjectDisposedException.ThrowIf(disposed, this);
#else
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

#endif
}
