using System.Diagnostics.CodeAnalysis;

namespace Vectron.Library.Ethernet;

/// <summary>
/// A default implementation of <see cref="IConnected{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the connection object.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Connected{T}"/> class.
/// </remarks>
/// <param name="value">The object used for this connection.</param>
/// <param name="state">The state of this connection.</param>
public sealed class Connected<T>(T value, bool state) : IConnected<T>
{
    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsConnected => state;

    /// <inheritdoc/>
    public T Value => value;
}
