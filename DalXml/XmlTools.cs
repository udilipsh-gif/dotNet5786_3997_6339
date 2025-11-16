namespace Dal;

using DO;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

/// <summary>
/// Provides utility methods for XML file operations including serialization, deserialization, and configuration management.
/// </summary>
/// <remarks>
/// This static class handles all XML file I/O operations for the Data Access Layer.
/// All XML files are stored in a dedicated directory specified by s_xmlDir.
/// </remarks>
static class XMLTools
{
    const string s_xmlDir = @"..\xml\";

    /// <summary>
    /// Static constructor that ensures the XML directory exists.
    /// Creates the directory if it doesn't already exist.
    /// </summary>
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer

    /// <summary>
    /// Saves a list of objects to an XML file using XmlSerializer.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list. Must be a reference type.</typeparam>
    /// <param name="list">The list of objects to serialize and save.</param>
    /// <param name="xmlFileName">The name of the XML file (without path).</param>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown when the file cannot be created or written to.</exception>
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }

    /// <summary>
    /// Loads a list of objects from an XML file using XmlSerializer.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list. Must be a reference type.</typeparam>
    /// <param name="xmlFileName">The name of the XML file to load (without path).</param>
    /// <returns>A list of deserialized objects, or an empty list if the file doesn't exist.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown when the file cannot be loaded or deserialized.</exception>
    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (!File.Exists(xmlFilePath)) return new();
            using FileStream file = new(xmlFilePath, FileMode.Open);
            XmlSerializer x = new(typeof(List<T>));
            return x.Deserialize(file) as List<T> ?? new();
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}");
        }
    }

    #endregion

    #region SaveLoadWithXElement

    /// <summary>
    /// Saves an XElement tree to an XML file.
    /// </summary>
    /// <param name="rootElem">The root XElement to save.</param>
    /// <param name="xmlFileName">The name of the XML file (without path).</param>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown when the file cannot be created or written to.</exception>
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            rootElem.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }

    /// <summary>
    /// Loads an XElement tree from an XML file.
    /// If the file doesn't exist, creates a new empty root element and saves it.
    /// </summary>
    /// <param name="xmlFileName">The name of the XML file to load (without path).</param>
    /// <returns>The root XElement of the loaded or newly created XML document.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown when the file cannot be loaded or created.</exception>
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (File.Exists(xmlFilePath))
                return XElement.Load(xmlFilePath);
            XElement rootElem = new(xmlFileName);
            rootElem.Save(xmlFilePath);
            return rootElem;
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }

    #endregion

    #region XmlConfig

    /// <summary>
    /// Gets the current integer value from a configuration element and increments it by 1.
    /// Useful for generating auto-incrementing IDs.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the integer value.</param>
    /// <returns>The current integer value before incrementing.</returns>
    /// <exception cref="FormatException">Thrown when the element value cannot be converted to an integer.</exception>
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int nextId = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
        return nextId;
    }

    /// <summary>
    /// Gets an integer value from a configuration element.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the integer value.</param>
    /// <returns>The integer value from the configuration element.</returns>
    /// <exception cref="FormatException">Thrown when the element value cannot be converted to an integer.</exception>
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Gets a DateTime value from a configuration element.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the DateTime value.</param>
    /// <returns>The DateTime value from the configuration element.</returns>
    /// <exception cref="FormatException">Thrown when the element value cannot be converted to a DateTime.</exception>
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// Sets an integer value in a configuration element.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element to update.</param>
    /// <param name="elemVal">The integer value to set.</param>
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Sets a DateTime value in a configuration element.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element to update.</param>
    /// <param name="elemVal">The DateTime value to set.</param>
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Gets a generic parsable value from a configuration element.
    /// Supports any type that implements IParsable&lt;T&gt; interface.
    /// </summary>
    /// <typeparam name="T">The type to parse. Must implement IParsable&lt;T&gt;.</typeparam>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the value.</param>
    /// <returns>The parsed value of type T.</returns>
    /// <exception cref="DalDoesNotExistException">Thrown when the element is not found.</exception>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to type T.</exception>
    public static T GetConfigGenericVal<T>(string xmlFileName, string elemName) where T : IParsable<T>
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        XElement elementValue = root.Element(elemName) ?? throw new DalDoesNotExistException($"Element {elemName} not found");
        string val = elementValue.Value;
        if (T.TryParse(val, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        else
        {
            throw new FormatException($"Cannot convert '{val}' to {typeof(T)} in {xmlFileName}, element {elemName}");
        }
    }

    /// <summary>
    /// Sets a generic value in a configuration element.
    /// Creates the element if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element to update or create.</param>
    /// <param name="elemVal">The value to set.</param>
    public static void SetConfigGenericVal<T>(string xmlFileName, string elemName, T elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        XElement? elementToSet = root.Element(elemName);
        if (elementToSet == null)
        {
            elementToSet = new XElement(elemName);
            root.Add(elementToSet);
        }

        string valueToSet = elemVal switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => elemVal.ToString() ?? string.Empty
        };

        elementToSet.SetValue(valueToSet);
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    #endregion

    #region ExtensionFuctions

    /// <summary>
    /// Extension method to safely parse an enum value from an XElement's child element.
    /// </summary>
    /// <typeparam name="T">The enum type to parse. Must be a struct and enum.</typeparam>
    /// <param name="element">The parent XElement containing the child element.</param>
    /// <param name="name">The name of the child element containing the enum value.</param>
    /// <returns>The parsed enum value, or null if parsing fails.</returns>
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;

    /// <summary>
    /// Extension method to safely parse a DateTime value from an XElement's child element.
    /// </summary>
    /// <param name="element">The parent XElement containing the child element.</param>
    /// <param name="name">The name of the child element containing the DateTime value.</param>
    /// <returns>The parsed DateTime value, or null if parsing fails.</returns>
    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Extension method to safely parse a double value from an XElement's child element.
    /// </summary>
    /// <param name="element">The parent XElement containing the child element.</param>
    /// <param name="name">The name of the child element containing the double value.</param>
    /// <returns>The parsed double value, or null if parsing fails.</returns>
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;

    /// <summary>
    /// Extension method to safely parse an integer value from an XElement's child element.
    /// </summary>
    /// <param name="element">The parent XElement containing the child element.</param>
    /// <param name="name">The name of the child element containing the integer value.</param>
    /// <returns>The parsed integer value, or null if parsing fails.</returns>
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;

    #endregion
}
