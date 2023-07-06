using System.Diagnostics.CodeAnalysis;

namespace Vectron.Library.Ethernet;

/// <summary>
/// A default implementation of <see cref="IConnected{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the connection object.</typeparam>
public sealed class Connected<T> : IConnected<T>
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
    [MemberNotNullWhen(true, nameof(Value))]
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
