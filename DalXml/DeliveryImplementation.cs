namespace Dal;
using DalApi;
using DO;
using System.Xml.Linq;

/// <summary>
/// Implementation of IDelivery interface for XML-based data storage.
/// Provides CRUD operations for Delivery entities using XML files.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Converts an XElement to a Delivery object.
    /// </summary>
    /// <param name="delivery">The XElement containing delivery data.</param>
    /// <returns>A Delivery object populated with data from the XElement.</returns>
    /// <exception cref="FormatException">Thrown when required fields cannot be converted.</exception>
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

    /// <summary>
    /// Creates an XElement from a Delivery object for XML storage.
    /// </summary>
    /// <param name="delivery">The Delivery object to convert.</param>
    /// <returns>An XElement containing the delivery data.</returns>
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

    /// <summary>
    /// Creates a new delivery in the XML data store with an auto-generated ID.
    /// </summary>
    /// <param name="item">The delivery item to create.</param>
    public void Create(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliverys_xml);

        item = item with { Id = Config.NextDeliveryId };

        deliveriesRootElem.Add(createDeliveryElement(item));

        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliverys_xml);
    }

    /// <summary>
    /// Reads a delivery by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery.</param>
    /// <returns>The Delivery object if found; otherwise, null.</returns>
    public Delivery? Read(int id)
    {
        XElement? deliveryElem =
            XMLTools.LoadListFromXMLElement(Config.s_deliverys_xml).Elements()
            .FirstOrDefault(d => (int?)d.Element("Id") == id);
        return deliveryElem is null ? null : getDelivery(deliveryElem);
    }

    /// <summary>
    /// Reads the first delivery that matches the specified filter condition.
    /// </summary>
    /// <param name="filter">A function to test each delivery for a condition.</param>
    /// <returns>The first Delivery that matches the filter, or null if none found.</returns>
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_deliverys_xml).Elements()
            .Select(d => getDelivery(d))
            .FirstOrDefault(filter);
    }

    /// <summary>
    /// Updates an existing delivery in the XML data store.
    /// </summary>
    /// <param name="item">The delivery item with updated information.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the delivery with the specified ID does not exist.</exception>
    public void Update(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliverys_xml);
        
        var deliveryElem = deliveriesRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == item.Id);
        if (deliveryElem == null)
            throw new DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist");
        
        deliveryElem.Remove();
        deliveriesRootElem.Add(createDeliveryElement(item));
        
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliverys_xml);
    }

    /// <summary>
    /// Retrieves all deliveries, optionally filtered by a specified predicate.
    /// </summary>
    /// <param name="filter">An optional function to filter deliveries. If null, all deliveries are returned.</param>
    /// <returns>An IEnumerable of Delivery objects matching the filter criteria.</returns>
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliverys_xml);
        
        if (filter is null)
            return deliveriesRootElem.Elements().Select(o => getDelivery(o));
        else
            return deliveriesRootElem.Elements().Select(o => getDelivery(o)).Where(filter);
    }

    /// <summary>
    /// Deletes a delivery by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the delivery with the specified ID does not exist.</exception>
    public void Delete(int id)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliverys_xml);
        
        var deliveryElem = deliveriesRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == id);
        if (deliveryElem == default)
            throw new DalDoesNotExistException($"Delivery with ID={id} does Not exist");
        
        deliveryElem.Remove();
        
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliverys_xml);
    }

    /// <summary>
    /// Deletes all deliveries from the XML data store.
    /// </summary>
    public void DeleteAll()
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliverys_xml);

        deliveriesRootElem.RemoveAll();

        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliverys_xml);
    }
}
