namespace Dal;
using DalApi;
using DO;
using System.Xml.Linq;

/// <summary>
/// Implementation of ICourier interface for XML-based data storage.
/// Provides CRUD operations for Courier entities using XML files.
/// </summary>
public class CourierImplementation : ICourier
{
    /// <summary>
    /// Converts an XElement to a Courier object.
    /// </summary>
    /// <param name="s">The XElement containing courier data.</param>
    /// <returns>A Courier object populated with data from the XElement.</returns>
    /// <exception cref="FormatException">Thrown when required fields cannot be converted.</exception>
    static Courier getCourier(XElement s)
    {
        return new Courier()
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            Name = (string?)s.Element("Name") ?? "",
            Phone = (string?)s.Element("Phone") ?? "",
            Email = (string?)s.Element("Email") ?? "",
            Password = (string?)s.Element("Password") ?? "",
            Active = (bool?)s.Element("Active") ?? false,
            MaxDistanceDelivery = (double?)s.Element("MaxDistanceDelivery") ?? null,
            TypeShipment = s.ToEnumNullable<TheTypeShipment>("TypeShipment") ?? TheTypeShipment.FOOT,
            WorkingSince = (DateTime?)s.Element("WorkingSince") ?? throw new FormatException("can't convert date"),
        };
    }

    /// <summary>
    /// Creates an XElement from a Courier object for XML storage.
    /// </summary>
    /// <param name="courier">The Courier object to convert.</param>
    /// <returns>An XElement containing the courier data.</returns>
    static XElement createCourierElement(Courier courier)
    {
        return new XElement("Courier",
            new XElement("Id", courier.Id),
            new XElement("Name", courier.Name),
            new XElement("Phone", courier.Phone),
            new XElement("Email", courier.Email),
            new XElement("Password", courier.Password),
            new XElement("Active", courier.Active),
            new XElement("MaxDistanceDelivery", courier.MaxDistanceDelivery),
            new XElement("TypeShipment", courier.TypeShipment.ToString()),
            new XElement("WorkingSince", courier.WorkingSince.ToString("o"))
        );
    }

    /// <summary>
    /// Reads a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier.</param>
    /// <returns>The Courier object if found; otherwise, null.</returns>
    public Courier? Read(int id)
    {
        XElement? courierElem =
            XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements()
            .FirstOrDefault(st => (int?)st.Element("Id") == id);
        return courierElem is null ? null : getCourier(courierElem);
    }

    /// <summary>
    /// Reads the first courier that matches the specified filter condition.
    /// </summary>
    /// <param name="filter">A function to test each courier for a condition.</param>
    /// <returns>The first Courier that matches the filter, or null if none found.</returns>
    public Courier? Read(Func<Courier, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements()
            .Select(s => getCourier(s))
            .FirstOrDefault(filter);
    }

    /// <summary>
    /// Updates an existing courier in the XML data store.
    /// </summary>
    /// <param name="item">The courier item with updated information.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the courier with the specified ID does not exist.</exception>
    public void Update(Courier item)
    {
        XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        var courierElem = couriersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == item.Id);
        if (courierElem == null)
            throw new DalDoesNotExistException($"Courier with ID={item.Id} does Not exist");
        
        courierElem.Remove();
        couriersRootElem.Add(createCourierElement(item));

        XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
    }

    /// <summary>
    /// Creates a new courier in the XML data store.
    /// </summary>
    /// <param name="item">The courier item to create.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown when a courier with the specified ID already exists.</exception>
    public void Create(Courier item)
    {
        XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        var courierElem = couriersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == item.Id);
        if (courierElem != null)
            throw new DalAlreadyExistsException($"Courier with ID={item.Id} already exists");

        couriersRootElem.Add(createCourierElement(item));

        XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
    }

    /// <summary>
    /// Deletes a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the courier with the specified ID does not exist.</exception>
    public void Delete(int id)
    {
        XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        var courierElem = couriersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == id);
        if (courierElem == null)
            throw new DalDoesNotExistException($"Courier with ID={id} does Not exist");

        courierElem.Remove();
        XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
    }

    /// <summary>
    /// Deletes all couriers from the XML data store.
    /// </summary>
    public void DeleteAll()
    {
        XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        couriersRootElem.RemoveAll();

        XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
    }

    /// <summary>
    /// Retrieves all couriers, optionally filtered by a specified predicate.
    /// </summary>
    /// <param name="filter">An optional function to filter couriers. If null, all couriers are returned.</param>
    /// <returns>An IEnumerable of Courier objects matching the filter criteria.</returns>
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        var allCouriers = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml)
            .Elements()
            .Select(c => getCourier(c));

        if (filter == null)
            return allCouriers;

        return allCouriers.Where(filter);
    }
}
