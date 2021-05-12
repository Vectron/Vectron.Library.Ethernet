namespace VectronsLibrary.Ethernet
{
    /// <summary>
    /// Express the current state of a connection.
    /// </summary>
    /// <typeparam name="T">The type of the connection object.</typeparam>
    public interface IConnected<out T>
    {
        /// <summary>
        /// Gets a value indicating whether the connection is connected.
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Gets the object used for this connection.
        /// </summary>
        T? Value
        {
            get;
        }
    }
}