/*This and all the entity implementation classes + the data-config file, were generated based on 
 * the following prompt to github copilot using claude (due to its length I will paste it in the Readme, but not every class):
Up until now, all implementations have been through the DalList project, 
with data saved in lists in Datasource.cs.
I would now like to create the option to use persistent storage instead, via XML files.
To that end, I’ve created the "xml" folder to store the XML files to be used for persistent storage,
and the DalXml project for the implementation using those files.

The XmlTools class contains methods to help with those implementations.
The data-config XML file is to be used to store all the variables found in the (DalList) Config class.
The other XML files will store lists of the appropriate entities.

Your first task is to implement DalXml.Config.
This class is a mirror of the DalList Config class — it needs getters and setters for all the appropriate variables,
but it must load and save the XML file instead of just storing values in the class.
Make sure to use the methods from XmlTools.

Example (though it uses different entity names):

internal static int NextCourseId
{
    get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextCourseId");
    private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextCourseId", value);
}

internal static DateTime Clock
{
    get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
    set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
}

Include the ResetConfig method as well.

Your second task is to set up the data-config XML file for all those variables.
Partial example:

<?xml version="1.0" encoding="utf-8"?>
<config>
  <NextOrderId>1008</NextOrderId>
  <NextDeliveryId>41</NextDeliveryId>
  <MaxRange>41</MaxRange>
</config>

Your third task is to implement the implementation classes in the DalXml project (except ConfigImplementation, which I already completed).
I created the classes, but right now all the methods just throw exceptions.
Each of those methods should do the exact same thing as their counterparts in DalList — 
but load/save from/to the appropriate XML file instead of using Datasource’s lists.
Once again, please use the methods from XmlTools.

There are two options provided by XmlTools.

Option 1: XmlSerializer
Example (different entities):

public void Update(Course item)
{
    List<Course> Courses = XMLTools.LoadListFromXMLSerializer<Course>(Config.s_courses_xml);
    if (Courses.RemoveAll(it => it.Id == item.Id) == 0)
        throw new DalDoesNotExistException($"Course with ID={item.Id} does Not exist");
    Courses.Add(item);
    XMLTools.SaveListToXMLSerializer(Courses, Config.s_courses_xml);
}

public void Delete(int id)
{
    List<Course> Courses = XMLTools.LoadListFromXMLSerializer<Course>(Config.s_courses_xml);
    if (Courses.RemoveAll(it => it.Id == id) == 0)
        throw new DalDoesNotExistException($"Course with ID={id} does Not exist");
    XMLTools.SaveListToXMLSerializer(Courses, Config.s_courses_xml);
}

public void DeleteAll()
{
    XMLTools.SaveListToXMLSerializer(new List<Course>(), Config.s_courses_xml);
}

Option 2: XElement
Example (also different entities):

static Student getStudent(XElement s)
{
    return new DO.Student()
    {
        Id = s.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
        Name = (string?)s.Element("Name") ?? "",
        Alias = (string?)s.Element("Alias") ?? null,
        IsActive = (bool?)s.Element("IsActive") ?? false,
        // CurrentYear = s.ToEnumNullable<Year>("CurrentYear") ?? Year.FirstYear,
        BirthDate = s.ToDateTimeNullable("BirthDate"),
        RegistrationDate = s.ToDateTimeNullable("RegistrationDate")
    };
}

public Student? Read(int id)
{
    XElement? studentElem = XMLTools.LoadListFromXMLElement(Config.s_students_xml)
                                  .Elements()
                                  .FirstOrDefault(st => (int?)st.Element("Id") == id);
    return studentElem is null ? null : getStudent(studentElem);
}

public Student? Read(Func<Student, bool> filter)
{
    return XMLTools.LoadListFromXMLElement(Config.s_students_xml)
                   .Elements()
                   .Select(s => getStudent(s))
                   .FirstOrDefault(filter);
}

public void Update(Student item)
{
    XElement studentsRootElem = XMLTools.LoadListFromXMLElement(Config.s_students_xml);

    (studentsRootElem.Elements()
        .FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
        ?? throw new DO.DalDoesNotExistException($"Student with ID={item.Id} does Not exist"))
        .Remove();

    studentsRootElem.Add(new XElement("Student", createStudentElement(item)));

    XMLTools.SaveListToXMLElement(studentsRootElem, Config.s_students_xml);
}

I would like CourierImplementation to use the first option,
while OrderImplementation and DeliveryImplementation should use the second.

Reference for how an entity’s XML file should look once it has been filled (different entities).
Fields which are null do not appear:

<?xml version="1.0" encoding="utf-8"?>
<ArrayOfStudent>
  <Student>
    <Id>344165165</Id>
    <Name>Dani Levi</Name>
    <IsActive>false</IsActive>
    <BirthDate>2011-04-04T00:00:00</BirthDate>
  </Student>
  <Student>
    <Id>239894668</Id>
    <Name>Eli Amar</Name>
    <Alias>Eli AmarALIAS</Alias>
    <IsActive>true</IsActive>
    <BirthDate>2009-11-03T00:00:00</BirthDate>
  </Student>
  <Student>
    <Id>384822544</Id>
    <Name>Yair Cohen</Name>
    <Alias>Yair CohenALIAS</Alias>
    <IsActive>true</IsActive>
    <BirthDate>2019-10-27T00:00:00</BirthDate>
  </Student>
  <Student>
    <Id>200315134</Id>
    <Name>Ariela Levin</Name>
    <IsActive>true</IsActive>
    <BirthDate>2015-07-25T00:00:00</BirthDate>
  </Student>
</ArrayOfStudent>
*/


namespace Dal;

/// <summary>
/// Configuration constants and XML-based property accessors for DAL storage.
/// </summary>
internal static class Config
{
    /// <summary>
    /// Path to the XML file storing configuration data.
    /// </summary>
    internal static string s_data_config_xml = "data-config.xml";

    /// <summary>
    /// Path to the XML file storing courier data.
    /// </summary>
    internal static string s_couriers_xml = "couriers.xml";

    /// <summary>
    /// Path to the XML file storing order data.
    /// </summary>
    internal static string s_orders_xml = "orders.xml";

    /// <summary>
    /// Path to the XML file storing delivery data.
    /// </summary>
    internal static string s_deliveries_xml = "deliveries.xml";

    /// <summary>
    /// Gets and increments the next Order ID from XML configuration.
    /// </summary>
    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
    }

    /// <summary>
    /// Gets and increments the next Delivery ID from XML configuration.
    /// </summary>
    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
    }

    /// <summary>
    /// Gets or sets the system clock from/to XML configuration.
    /// </summary>
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    /// <summary>
    /// Gets or sets the Admin user ID from/to XML configuration.
    /// </summary>
    internal static int AdminId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "AdminId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "AdminId", value);
    }

    /// <summary>
    /// Gets or sets the Admin password from/to XML configuration.
    /// </summary>
    internal static string AdminPassword
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).Element("AdminPassword")?.Value ?? "Admin";
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("AdminPassword")?.SetValue(value);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the average car speed from/to XML configuration.
    /// </summary>
    internal static double AvgCarSpeedKmh
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).ToDoubleNullable("AvgCarSpeedKmh") ?? 0;
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("AvgCarSpeedKmh")?.SetValue(value);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the average motorcycle speed from/to XML configuration.
    /// </summary>
    internal static double AvgMotorcycleSpeedKmh
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).ToDoubleNullable("AvgMotorcycleSpeedKmh") ?? 0;
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("AvgMotorcycleSpeedKmh")?.SetValue(value);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the average bicycle speed from/to XML configuration.
    /// </summary>
    internal static double AvgBicycleSpeedKmh
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).ToDoubleNullable("AvgBicycleSpeedKmh") ?? 0;
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("AvgBicycleSpeedKmh")?.SetValue(value);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the average walking speed from/to XML configuration.
    /// </summary>
    internal static double AvgWalkingSpeedKmh
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).ToDoubleNullable("AvgWalkingSpeedKmh") ?? 0;
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("AvgWalkingSpeedKmh")?.SetValue(value);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the maximum general delivery distance from/to XML configuration.
    /// </summary>
    internal static double? MaxGeneralDeliveryDistanceKm
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).ToDoubleNullable("MaxGeneralDeliveryDistanceKm");
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("MaxGeneralDeliveryDistanceKm")?.Remove();
            if (value.HasValue)
                root.Add(new System.Xml.Linq.XElement("MaxGeneralDeliveryDistanceKm", value.Value));
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the maximum delivery time span from/to XML configuration.
    /// Stored as total seconds to ensure reliable serialization/deserialization.
    /// </summary>
    internal static TimeSpan MaxDeliveryTimeSpan
    {
        get
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            var elem = root.Element("MaxDeliveryTimeSpan");
            if (elem != null && double.TryParse(elem.Value, out double seconds))
                return TimeSpan.FromSeconds(seconds);
            return TimeSpan.Zero;
        }
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("MaxDeliveryTimeSpan")?.SetValue(value.TotalSeconds);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the risk range time span from/to XML configuration.
    /// Stored as total seconds to ensure reliable serialization/deserialization.
    /// </summary>
    internal static TimeSpan RiskRange
    {
        get
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            var elem = root.Element("RiskRange");
            if (elem != null && double.TryParse(elem.Value, out double seconds))
                return TimeSpan.FromSeconds(seconds);
            return TimeSpan.Zero;
        }
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("RiskRange")?.SetValue(value.TotalSeconds);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the inactivity range time span from/to XML configuration.
    /// Stored as total seconds to ensure reliable serialization/deserialization.
    /// </summary>
    internal static TimeSpan InactivityRange
    {
        get
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            var elem = root.Element("InactivityRange");
            if (elem != null && double.TryParse(elem.Value, out double seconds))
                return TimeSpan.FromSeconds(seconds);
            return TimeSpan.Zero;
        }
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("InactivityRange")?.SetValue(value.TotalSeconds);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the company's full address from/to XML configuration.
    /// </summary>
    internal static string? CompanyFullAddress
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).Element("CompanyFullAddress")?.Value;
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("CompanyFullAddress")?.Remove();
            if (!string.IsNullOrEmpty(value))
                root.Add(new System.Xml.Linq.XElement("CompanyFullAddress", value));
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the company's latitude from/to XML configuration.
    /// </summary>
    internal static double? Latitude
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).ToDoubleNullable("Latitude");
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("Latitude")?.Remove();
            if (value.HasValue)
                root.Add(new System.Xml.Linq.XElement("Latitude", value.Value));
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the company's longitude from/to XML configuration.
    /// </summary>
    internal static double? Longitude
    {
        get => XMLTools.LoadListFromXMLElement(s_data_config_xml).ToDoubleNullable("Longitude");
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("Longitude")?.Remove();
            if (value.HasValue)
                root.Add(new System.Xml.Linq.XElement("Longitude", value.Value));
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Resets all configuration values to their defaults.
    /// TimeSpan values are stored as total seconds.
    /// </summary>
    internal static void Reset()
    {
        var root = new System.Xml.Linq.XElement("config");
        root.Add(new System.Xml.Linq.XElement("NextOrderId", 1));
        root.Add(new System.Xml.Linq.XElement("NextDeliveryId", 1));
        root.Add(new System.Xml.Linq.XElement("AdminId", 0));
        root.Add(new System.Xml.Linq.XElement("AdminPassword", "Admin"));
        root.Add(new System.Xml.Linq.XElement("AvgCarSpeedKmh", 0));
        root.Add(new System.Xml.Linq.XElement("AvgMotorcycleSpeedKmh", 0));
        root.Add(new System.Xml.Linq.XElement("AvgBicycleSpeedKmh", 0));
        root.Add(new System.Xml.Linq.XElement("AvgWalkingSpeedKmh", 0));
        root.Add(new System.Xml.Linq.XElement("MaxDeliveryTimeSpan", TimeSpan.Zero.TotalSeconds));
        root.Add(new System.Xml.Linq.XElement("RiskRange", TimeSpan.Zero.TotalSeconds));
        root.Add(new System.Xml.Linq.XElement("InactivityRange", TimeSpan.Zero.TotalSeconds));
        root.Add(new System.Xml.Linq.XElement("Clock", DateTime.Now));
        // Note: MaxGeneralDeliveryDistanceKm, CompanyFullAddress, Latitude, and Longitude are nullable
        // and are not included in the reset to their default null state
        XMLTools.SaveListToXMLElement(root, s_data_config_xml);
    }
}
