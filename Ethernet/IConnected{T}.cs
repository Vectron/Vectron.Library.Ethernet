namespace VectronsLibrary.Ethernet
{
    public interface IConnected<out T>
    {
        bool IsConnected
        {
            get;
        }

        T Value
        {
            get;
        }
    }
}