namespace Dal;
using DalApi;
using DO;
using System.Xml.Linq;

internal class OrderImplementation : IOrder
{
    private static Order getOrder(XElement order)
    {
        return new DO.Order()
        {
            Id = order.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            TypeOfOrder = order.ToEnumNullable<TypeOfOrder>("TypeOfOrder") ?? TypeOfOrder.STANDART,
            Details = (string?)order.Element("Details") ?? "",
            Addres = (string?)order.Element("Addres") ?? "",
            Latitude = order.ToDoubleNullable("Latitude") ?? throw new FormatException("can't convert latitude"),
            Longitude = order.ToDoubleNullable("Longitude") ?? throw new FormatException("can't convert longitude"),
            Name = (string?)order.Element("Name") ?? "",
            Phone = (string?)order.Element("Phone") ?? "",
            Weight = order.ToIntNullable("Weight") ?? throw new FormatException("can't convert weight"),
            OrderDate = (DateTime?)order.Element("OrderDate") ?? throw new FormatException("can't convert date"),
            OrderStatus = order.ToEnumNullable<OrderStatus>("OrderStatus") ?? OrderStatus.OPEN,
            DistanceKm = order.ToDoubleNullable("DistanceKm"),
            DistanceKmRoad = order.ToDoubleNullable("DistanceKmRoad") ?? null,
            DistanceKmWalk = order.ToDoubleNullable("DistanceKmWalk") ?? null,
        };
    }

    private static XElement createOrderElement(Order order)
    {
        return new XElement("Order",
            new XElement("Id", order.Id),
            new XElement("TypeOfOrder", order.TypeOfOrder.ToString()),
            new XElement("Details", order.Details),
            new XElement("Addres", order.Addres),
            new XElement("Latitude", order.Latitude),
            new XElement("Longitude", order.Longitude),
            new XElement("Name", order.Name),
            new XElement("Phone", order.Phone),
            new XElement("Weight", order.Weight),
            new XElement("OrderDate", order.OrderDate.ToString("o")),
            new XElement("OrderStatus", order.OrderStatus.ToString()),
            new XElement("DistanceKm", order.DistanceKm),
            new XElement("DistanceKmRoad", order.DistanceKmRoad),
            new XElement("DistanceKmWalk", order.DistanceKmWalk)
        );
    }

    public void Create(Order item)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);

        item = item with { Id = Config.NextOrderId }; 

        ordersRootElem.Add(createOrderElement(item));

        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

    public Order? Read(int id)
    {
        XElement? orderElem =
            XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements()
            .FirstOrDefault(o => (int?)o.Element("Id") == id);

        return orderElem is null ? null : getOrder(orderElem);
    }

    public Order? Read(Func<Order, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements()
            .Select(o => getOrder(o))
            .FirstOrDefault(filter);
    }

    public void Update(Order item)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);

        var orderElem = ordersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == item.Id);
        if (ordersRootElem == null)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does Not exist");
        orderElem?.Remove();

        ordersRootElem.Add(createOrderElement(item));

        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);

    }

    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);
        if (filter == null)
        {
            return ordersRootElem.Elements().Select(o => getOrder(o));
        }
        else
        {
            return ordersRootElem.Elements().Select(o => getOrder(o)).Where(filter);
        }
    }

    public void Delete(int id)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);
        var orderElem = ordersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == id);
        if (ordersRootElem == null)
            throw new DalDoesNotExistException($"Order with ID={id} does Not exist");
        orderElem?.Remove();
        
        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

    public void DeleteAll()
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);

        ordersRootElem.RemoveAll();

        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

}

