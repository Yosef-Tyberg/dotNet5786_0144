namespace Dal;

using DO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

static class XMLTools
{
    const string s_xmlDir = @"..\xml\";
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer
    private const int MAX_SAVE_TRIES = 5;
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        saveListToXMLSerializer(list, xmlFileName, MAX_SAVE_TRIES);
    }
    private static void saveListToXMLSerializer<T>(List<T> list, string xmlFileName, int tries) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;
        try
        {
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);
        }
        catch (Exception ex)
        {
            if (tries == 1)
                throw new DalXMLFileLoadCreateException($"fail to create xml file: {xmlFilePath}, {ex.Message}", ex);
            else
                saveListToXMLSerializer(list, xmlFileName, tries - 1);
        }
    }

    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;
        XmlSerializer x = new(typeof(List<T>));
        try
        {
            if (!File.Exists(xmlFilePath)) return [];
            using FileStream file = new(xmlFilePath, FileMode.Open);
            return x.Deserialize(file) as List<T> ?? [];
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}", ex);
        }
    }
    #endregion

    #region SaveLoadWithXElement
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;
        try
        {
            rootElem.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {xmlFilePath}, {ex.Message}", ex);
        }
    }
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;
        try
        {
            if (File.Exists(xmlFilePath))
                return XElement.Load(xmlFilePath);
            XElement root = new(xmlFileName);
            root.Save(xmlFilePath);
            return root;
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {s_xmlDir + xmlFilePath}, {ex.Message}", ex);
        }
    }
    #endregion

    #region XmlConfig
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int nextId = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
        return nextId;
    }
    public static int GetConfigIntVal(string xmlFileName, string elemName) =>
        LoadListFromXMLElement(xmlFileName)
            .ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");

    public static DateTime GetConfigDateVal(string xmlFileName, string elemName) =>
        LoadListFromXMLElement(xmlFileName)
            .ToDateTimeNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");

    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        SaveListToXMLElement(root, xmlFileName);
    }

    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        SaveListToXMLElement(root, xmlFileName);
    }
    #endregion

    #region ExtensionMethods
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;

    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;

    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;

    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;
    #endregion
}