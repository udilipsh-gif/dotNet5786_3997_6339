namespace Dal;
using DalApi;
using DO;
using System.Xml.Linq;

/// <summary>
/// Implementation of IOrder interface for XML-based data storage.
/// Provides CRUD operations for Order entities using XML files.
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Converts an XElement to an Order object.
    /// </summary>
    /// <param name="order">The XElement containing order data.</param>
    /// <returns>An Order object populated with data from the XElement.</returns>
    /// <exception cref="FormatException">Thrown when required fields cannot be converted.</exception>
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

    /// <summary>
    /// Creates an XElement from an Order object for XML storage.
    /// </summary>
    /// <param name="order">The Order object to convert.</param>
    /// <returns>An XElement containing the order data.</returns>
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

    /// <summary>
    /// Creates a new order in the XML data store with an auto-generated ID.
    /// </summary>
    /// <param name="item">The order item to create.</param>
    public void Create(Order item)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);

        item = item with { Id = Config.NextOrderId };

        ordersRootElem.Add(createOrderElement(item));

        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

    /// <summary>
    /// Reads an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The Order object if found; otherwise, null.</returns>
    public Order? Read(int id)
    {
        XElement? orderElem =
            XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements()
            .FirstOrDefault(o => (int?)o.Element("Id") == id);

        return orderElem is null ? null : getOrder(orderElem);
    }

    /// <summary>
    /// Reads the first order that matches the specified filter condition.
    /// </summary>
    /// <param name="filter">A function to test each order for a condition.</param>
    /// <returns>The first Order that matches the filter, or null if none found.</returns>
    public Order? Read(Func<Order, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements()
            .Select(o => getOrder(o))
            .FirstOrDefault(filter);
    }

    /// <summary>
    /// Updates an existing order in the XML data store.
    /// </summary>
    /// <param name="item">The order item with updated information.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the order with the specified ID does not exist.</exception>
    public void Update(Order item)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);

        var orderElem = ordersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == item.Id);
        if (orderElem == null)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does Not exist");
        
        orderElem.Remove();
        ordersRootElem.Add(createOrderElement(item));

        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

    /// <summary>
    /// Retrieves all orders, optionally filtered by a specified predicate.
    /// </summary>
    /// <param name="filter">An optional function to filter orders. If null, all orders are returned.</param>
    /// <returns>An IEnumerable of Order objects matching the filter criteria.</returns>
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

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the order with the specified ID does not exist.</exception>
    public void Delete(int id)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);
        
        var orderElem = ordersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == id);
        if (orderElem == null)
            throw new DalDoesNotExistException($"Order with ID={id} does Not exist");
        
        orderElem.Remove();
        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

    /// <summary>
    /// Deletes all orders from the XML data store.
    /// </summary>
    public void DeleteAll()
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);

        ordersRootElem.RemoveAll();

        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }
}

