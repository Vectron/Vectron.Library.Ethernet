namespace VectronsLibrary.Ethernet
{
    public class Connected<T> : IConnected<T>
    {
        public Connected(T value, bool state)
        {
            Value = value;
            IsConnected = state;
        }

        public bool IsConnected
        {
            get;
        }

        public T Value
        {
            get;
        }
    }
}