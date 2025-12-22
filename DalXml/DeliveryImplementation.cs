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
    /// <exception cref="FormatException">Thrown when required fields cannot be converted to their proper types.</exception>
    /// <remarks>
    /// This method parses XML elements and converts them to strongly-typed Delivery properties.
    /// The OrderDate and TimeEndDelivery are expected to be in ISO 8601 format.
    /// Empty TimeEndDelivery values are treated as null.
    /// </remarks>
    private static Delivery getDelivery(XElement delivery)
    {
        return new Delivery()
        {
            Id = delivery.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            OrderId = delivery.ToIntNullable("OrderId") ?? throw new FormatException("can't convert order id"),
            CourierId = delivery.ToIntNullable("CourierId") ?? throw new FormatException("can't convert courier id"),
            TypeShipment = delivery.ToEnumNullable<TheTypeShipment>("TypeShipment") ?? throw new FormatException("can't convert type shipment"),
            OrderDate = (DateTime?)delivery.Element("OrderDate") ?? throw new FormatException("can't convert order date"),
            ActualDistance = delivery.ToDoubleNullable("ActualDistance"),
            EndDelivery = delivery.ToEnumNullable<EndDelivery>("EndDelivery") ?? null,
            TimeEndDelivery = (string?)delivery.Element("TimeEndDelivery") == "" ? 
                null : (DateTime?)delivery.Element("TimeEndDelivery"),
        };
    }

    /// <summary>
    /// Creates an XElement from a Delivery object for XML storage.
    /// </summary>
    /// <param name="delivery">The Delivery object to convert.</param>
    /// <returns>An XElement containing the delivery data in XML format.</returns>
    /// <remarks>
    /// This method serializes a Delivery object to XML format.
    /// DateTime values are formatted using ISO 8601 format (using "o" format specifier).
    /// Null enum and DateTime values are stored as empty strings.
    /// </remarks>
    private static XElement createDeliveryElement(Delivery delivery)
    {
        return new XElement("Delivery",
            new XElement("Id", delivery.Id),
            new XElement("OrderId", delivery.OrderId),
            new XElement("CourierId", delivery.CourierId),
            new XElement("TypeShipment", delivery.TypeShipment.ToString()),
            new XElement("OrderDate", delivery.OrderDate.ToString("o")),
            new XElement("ActualDistance", delivery.ActualDistance),
            new XElement("EndDelivery", delivery.EndDelivery?.ToString() ?? ""),
            new XElement("TimeEndDelivery", delivery.TimeEndDelivery?.ToString("o") ?? "")
        );
    }

    /// <summary>
    /// Creates a new delivery in the XML data store with an auto-generated ID.
    /// </summary>
    /// <param name="item">The delivery item to create. The Id property will be overwritten with an auto-generated value.</param>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item><description>Loads the existing deliveries XML document</description></item>
    /// <item><description>Assigns a new auto-generated ID to the delivery</description></item>
    /// <item><description>Adds the delivery to the XML document</description></item>
    /// <item><description>Saves the updated document back to the file</description></item>
    /// </list>
    /// The ID is generated using the Config.NextDeliveryId property.
    /// </remarks>
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
    /// <remarks>
    /// This method loads the deliveries XML document and searches for a delivery with the matching ID.
    /// Returns null if no delivery with the specified ID exists.
    /// </remarks>
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
    /// <remarks>
    /// This method loads all deliveries from the XML file, converts them to Delivery objects,
    /// and returns the first one that satisfies the filter condition.
    /// If no delivery matches the filter, returns null.
    /// </remarks>
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
    /// <remarks>
    /// This method loads all deliveries from the XML file and converts them to Delivery objects.
    /// If a filter is provided, only deliveries that satisfy the filter condition are returned.
    /// If the filter is null, all deliveries are returned.
    /// </remarks>
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
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item><description>Loads the existing deliveries XML document</description></item>
    /// <item><description>Locates the delivery element with the matching ID</description></item>
    /// <item><description>Removes the delivery element from the document</description></item>
    /// <item><description>Saves the updated document back to the file</description></item>
    /// </list>
    /// If the delivery ID doesn't exist, throws a DalDoesNotExistException.
    /// </remarks>
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
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item><description>Loads the deliveries XML document</description></item>
    /// <item><description>Removes all delivery elements from the root</description></item>
    /// <item><description>Saves the empty document back to the file</description></item>
    /// </list>
    /// This operation is irreversible and removes all delivery data.
    /// Use with caution.
    /// </remarks>
    public void DeleteAll()
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliverys_xml);

        deliveriesRootElem.RemoveAll();

        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliverys_xml);
    }
}
