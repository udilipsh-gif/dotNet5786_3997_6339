
using Helpers;

namespace BO
{
    /// <summary>
    /// Represents a courier in a list, including their details such as ID, name, activity status, shipment type, and
    /// delivery statistics.
    /// </summary>
    /// <remarks>This class provides information about a courier, including their performance metrics such as
    /// on-time and late deliveries. It is designed to be used in scenarios where a summary of courier details is
    /// required, such as in reporting or list displays.</remarks>
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

        public override string ToString() => this.ToStringProperty();

    }
}
