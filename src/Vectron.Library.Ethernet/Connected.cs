namespace Vectron.Library.Ethernet;

/// <summary>
/// Class for creating <see cref="IConnected{T}"/>.
/// </summary>
public static class Connected
{
    /// <summary>
    /// Create a <see cref="IConnected{T}"/> that is not connected.
    /// </summary>
    /// <typeparam name="T">The type of the connection object.</typeparam>
    /// <param name="value">The object to use for this connection can be <see langword="null"/>.</param>
    /// <returns>A <see cref="IConnected{T}"/> with the current connection state.</returns>
    public static IConnected<T> No<T>(T value)
        => new Connected<T>(value, state: false);

    /// <summary>
    /// Create a <see cref="IConnected{T}"/> that is connected.
    /// </summary>
    /// <typeparam name="T">The type of the connection object.</typeparam>
    /// <param name="value">The object to use for this connection can be <see langword="null"/>.</param>
    /// <returns>A <see cref="IConnected{T}"/> with the current connection state.</returns>
    public static IConnected<T> Yes<T>(T value)
        => new Connected<T>(value, state: true);
}
