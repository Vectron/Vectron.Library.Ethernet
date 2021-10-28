﻿namespace VectronsLibrary.Ethernet;

/// <summary>
/// A default implementation of <see cref="IConnected{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the connection object.</typeparam>
public class Connected<T> : IConnected<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Connected{T}"/> class.
    /// </summary>
    /// <param name="value">The object used for this connection.</param>
    /// <param name="state">The state of this connection.</param>
    public Connected(T? value, bool state)
    {
        Value = value;
        IsConnected = state;
    }

    /// <inheritdoc/>
    public bool IsConnected
    {
        get;
    }

    /// <inheritdoc/>
    public T? Value
    {
        get;
    }
}