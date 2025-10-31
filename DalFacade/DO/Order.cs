namespace DO;

public record Order
{
    public required int Id { get; init; }//ini
    public required TypeOfOrder TypeOfOrder { get; set; }
    public string? Details { set; get; }
    public required string addres { get; set; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required int Weight { get; set; }
    public required DateTime OrderData { get; init;}




}
