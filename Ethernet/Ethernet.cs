using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet
{
    /// <summary>
    /// Base class for using an ethernet connection.
    /// </summary>
    public abstract class Ethernet : IDisposable, IEthernet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ethernet"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> instance used for logging.</param>
        public Ethernet(ILogger logger)
        {
            Logger = logger;
            ConnectionState = new BehaviorSubject<IConnected<IEthernetConnection>>(Connected.No<IEthernetConnection>(null));
        }

        /// <inheritdoc/>
        public IObservable<IConnected<IEthernetConnection>> SessionStream => ConnectionState.AsObservable();

        /// <inheritdoc/>
        public Socket? Socket
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the observable stream for publishing connection changes.
        /// </summary>
        protected BehaviorSubject<IConnected<IEthernetConnection>> ConnectionState
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the object has been disposed.
        /// </summary>
        protected bool DisposedValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="ILogger"/> instance used for logging.
        /// </summary>
        protected ILogger Logger
        {
            get;
        }

        /// <inheritdoc/>
        public void Close()
            => Shutdown();

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">Value indicating if we need to cleanup managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    ConnectionState.Dispose();
                }

                DisposedValue = true;
            }
        }

        /// <summary>
        /// Shutdown the connection.
        /// </summary>
        protected abstract void Shutdown();
    }
}