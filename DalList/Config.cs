namespace Dal;

internal class Config
{
    internal const int startorderid = 100001;
    private static int _orderid = startorderid;
    internal static int OrderId { get => _orderid++; }

    internal static void Reset()
    {
        _orderid = startorderid;
    }
}
