namespace VectronsLibrary.Ethernet
{
    public static class Connected
    {
        public static IConnected<T> No<T>(T value)
        {
            return new Connected<T>(value, false);
        }

        public static IConnected<T> Yes<T>(T value)
        {
            return new Connected<T>(value, true);
        }
    }
}