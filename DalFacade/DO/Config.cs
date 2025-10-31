namespace DO;

internal class Config
{

    public required int NextOrderId { get; init; }
    public required int NextDeliveryId { get; init; }
    public required DateTime Clock { get; set; }

    public required int me
}
