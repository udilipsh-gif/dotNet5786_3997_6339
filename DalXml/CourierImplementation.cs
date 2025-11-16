namespace Dal;
using DalApi;
using DO;
using System.Xml.Linq;

public class CourierImplementation : ICourier
{
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

    public Courier? Read(int id)
    {
        XElement? studentElem =
    XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return studentElem is null ? null : getCourier(studentElem);
    }

    public Courier? Read(Func<Courier, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements()
            .Select(s => getCourier(s))
            .FirstOrDefault(filter);
    }

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

    public void Create(Courier item)
    {
        XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        var courierElem = couriersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == item.Id);
        if (courierElem != null)
            throw new DalDoesNotExistException($"Courier with ID={item.Id} does Not exist");

        couriersRootElem.Add(createCourierElement(item));

        XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
    }

    public void Delete(int id)
    {
        XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        var courierElem = couriersRootElem.Elements().FirstOrDefault(c => (int?)c.Element("Id") == id);
        if (courierElem == null)
            throw new DalDoesNotExistException($"Courier with ID={id} does Not exist");

        courierElem.Remove();
        XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
    }

    public void DeleteAll()
    {
        XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        couriersRootElem.RemoveAll();

        XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
    }

    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        // 1. טען את כל האלמנטים
        var allCouriers = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml)
            .Elements()                    // קבל IEnumerable<XElement>
            .Select(c => getCourier(c));   // המר כל אחד ל-Courier

        // 2. בדוק אם יש פילטר
        if (filter == null)
            return allCouriers;            // החזר הכל

        // 3. החזר רק את המסוננים
        return allCouriers.Where(filter);
    }
}
