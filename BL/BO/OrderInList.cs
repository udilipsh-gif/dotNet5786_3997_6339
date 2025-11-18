
namespace BO
{
    public class OrderInList
    {
        public int? DeliveryId { get; init; } = null;

        public required int OrderId { get; init; }

        public required TypeOfOrder TypeOfOrder { get; init; }

        public required double DistanceKm { get; init; } 

        public required OrderStatus OrderStatus { get; init; }

        public required ScheduleStatus ScheduleStatus { get; init; }

        public required TimeSpan TimeLeftForDelivery { get; init; }

        public required TimeSpan TotalTimeOfDelivery { get; init; }

        public required int NumberOfDeliveryAttempts { get; init; }

    }
}
