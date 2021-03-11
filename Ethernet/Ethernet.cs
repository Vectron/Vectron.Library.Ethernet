using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace VectronsLibrary.Ethernet
{
    public abstract class Ethernet : IDisposable, IEthernet
    {
        protected readonly BehaviorSubject<IConnected<IEthernetConnection>> connectionState = new BehaviorSubject<IConnected<IEthernetConnection>>(Connected.No<IEthernetConnection>(null));
        protected readonly ILogger logger;
        protected bool disposedValue;

        public Ethernet(ILogger logger)
        {
            this.logger = logger;
        }

        public IObservable<IConnected<IEthernetConnection>> SessionStream => connectionState.AsObservable();

        public Socket Socket
        {
            get;
            internal set;
        }

        public void Close()
        {
            Shutdown();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    connectionState.Dispose();
                }

                disposedValue = true;
            }
        }

        protected abstract void Shutdown();
    }
}