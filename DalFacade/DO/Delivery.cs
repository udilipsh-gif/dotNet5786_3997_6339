namespace DO;

internal class Delivery
{
    public required int Id { get; init; }
    public required int OrderId { get; init; }

    public required int CourierId { get; init; }

    public required TypeOfOrder TypeOfOrder { get; init; }
    public required DateTime OrderData { get; init; }
    public double ActualDistance { get; init; }
    public EndDelivery EndDelivery { get; init; }

    public DateTime TimeEndDelivery { get; init; }


}
    
