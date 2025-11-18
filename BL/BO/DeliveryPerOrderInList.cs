

namespace BO
{
    public class DeliveryPerOrderInList
    {
        public required int DeliveryId { get; init; }
        public int? CourierId { get; init; }
        public required string CourierName { get; init; } 
        public required TypeOfOrder TypeOfOrder { get; init; }
        public required DateTime OrderDate { get; init; }
        public EndDelivery? EndDelivery { get; init; } 

        public DateTime? TimeEndDelivery { get; init; }

    }
}
