namespace DO;

/// <summary>
/// Represents a courier responsible for delivering packages, including their personal details,  delivery preferences,
/// and operational status.
/// </summary>
/// <remarks>This record encapsulates information about a courier, such as their contact details,  delivery
/// capabilities, and working status. It is designed to be used in systems that manage  delivery personnel and their
/// assignments.</remarks>
/// <param name="Id"> The unique identifier for the courier.</param>
/// <param name="Name"> The full name of the courier.</param>
/// <param name="Phone"> The contact phone number of the courier.</param>
/// <param name="Email"> The email address of the courier.</param>
/// <param name="Password"> The password for the courier's account.</param>
/// <param name="Active"> Indicates whether the courier is currently active and available for deliveries.</param>
/// <param name="MaxDistanceDelivery"> The maximum distance the courier is willing to travel for deliveries.</param>
/// <param name="TheTypeShipment"> The type of shipment the courier is equipped to handle (e.g., car, motorcycle, bike, foot).</param>
/// <param name="WorkingSince"> The date and time when the courier started working.</param>
public record Courier
{
    private object workingSince;

    public Courier(int id, string name, string phone, string email, string password, bool active, double maxDistanceDelivery , TheTypeShipment typeShipment, object workingSince)
    {
        Id = id;
        Name = name;
        Phone = phone;
        Email = email;
        Password = password;
        Active = active;
        MaxDistanceDelivery = maxDistanceDelivery;
        TypeShipment = typeShipment;
        this.workingSince = workingSince;
    }

    public int Id { get; init; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public bool Active { get; set; }
    public double? MaxDistanceDelivery { get; set; } = null;
    public TheTypeShipment TypeShipment { get; set; }
    public DateTime WorkingSince { get; init; }
}

