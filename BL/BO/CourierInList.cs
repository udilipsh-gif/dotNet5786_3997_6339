

namespace BO
{
    public class CourierInList
    {
        public required int Id { get; init; }
        public required string Name { get; init; }

        public required bool Active { get; init; }
        public required TheTypeShipment TypeShipment { get; init; }
        
        public DateTime WorkingSince { get; init; }

        public int DeliveryOnTime { get; init; }

        public int DeliveryLate { get; init; }

        public int? DeliveryId { get; init; }

    }
}
