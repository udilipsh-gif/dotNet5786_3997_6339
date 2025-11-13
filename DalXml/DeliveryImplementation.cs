namespace Dal;
using DalApi;
using DO;
using System.Xml.Linq;

internal class DeliveryImplementation : IDelivery
{
    private static Delivery getDelivery(XElement delivery)
    {
        return new Delivery()
        {
            Id = delivery.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            OrderId = delivery.ToIntNullable("OrderId") ?? throw new FormatException("can't convert order id"),
            CourierId = delivery.ToIntNullable("CourierId") ?? throw new FormatException("can't convert courier id"),
            TypeOfOrder = delivery.ToEnumNullable<TypeOfOrder>("TypeOfOrder") ?? TypeOfOrder.STANDART,
            OrderDate = (DateTime?)delivery.Element("OrderDate") ?? throw new FormatException("can't convert order date"),
            ActualDistance = delivery.ToDoubleNullable("ActualDistance"),
            EndDelivery = delivery.ToEnumNullable<EndDelivery>("EndDelivery") ?? null,
            TimeEndDelivery = (DateTime?)delivery.Element("TimeEndDelivery") ?? null,
        };
    }

    private static XElement createDeliveryElement(Delivery delivery)
    {
        return new XElement("Delivery",
            new XElement("Id", delivery.Id),
            new XElement("OrderId", delivery.OrderId),
            new XElement("CourierId", delivery.CourierId),
            new XElement("TypeOfOrder", delivery.TypeOfOrder.ToString()),
            new XElement("OrderDate", delivery.OrderDate.ToString("o")),
            new XElement("ActualDistance", delivery.ActualDistance),
            new XElement("EndDelivery", delivery.EndDelivery?.ToString() ?? ""),
            new XElement("TimeEndDelivery", delivery.TimeEndDelivery?.ToString("o") ?? "")
        );
    }

    public void Create(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        if (deliveriesRootElem.Elements().Any(d => (int?)d.Element("Id") == item.Id))
            throw new DalAlreadyExistsException($"Delivery with ID={item.Id} already exists");
        item = item with { Id = Config.NextDeliveryId };

        deliveriesRootElem.Add(createDeliveryElement(item));

        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    public Delivery? Read(int id)
    {
        XElement? deliveryElem =
            XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().
            FirstOrDefault(d => (int?)d.Element("Id") == id);
        return deliveryElem is null ? null : getDelivery(deliveryElem);
    }

    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements()
            .Select(d => getDelivery(d))
            .FirstOrDefault(filter);
    }

    public void Update(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        (deliveriesRootElem.Elements().Elements().FirstOrDefault(d => (int?)d.Element("Id") == item.Id) ??
            throw new DO.DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist"))
            .Remove();
        deliveriesRootElem.Add(createDeliveryElement(item));
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        if (filter is null)
            return deliveriesRootElem.Elements().Select(o => getDelivery(o));
        else
            return deliveriesRootElem.Elements().Select(o => getDelivery(o)).Where(filter);
    }

    public void Delete(int id)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        (deliveriesRootElem.Elements().FirstOrDefault(d => (int?)d.Element("Id") == id) ??
            throw new DO.DalDoesNotExistException($"Delivery with ID={id} does Not exist"))
            .Remove();
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    public void DeleteAll()
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);

        deliveriesRootElem.RemoveAll();

        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }
}
