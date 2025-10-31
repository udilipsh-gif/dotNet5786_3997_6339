namespace DO;

/// <summary>
/// Represents a courier responsible for delivering packages, including their personal details,  delivery preferences,
/// and operational status.
/// </summary>
/// <remarks>This record encapsulates information about a courier, such as their contact details,  delivery
/// capabilities, and working status. It is designed to be used in systems that manage  delivery personnel and their
/// assignments.</remarks>
internal record Courier
{
    public required int Id { get; init; }
    public required string Name { get; set; } = string.Empty;
    public required string Phone { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public required bool Active { get; set; } = false;
    public double MaxDistanceDelivery { get; set; }
    public required TheTypeShipment TheTypeShipment { get; set; }
    public DateTime WorkingSince { get; init; } = DateTime.Now;
}
