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
    /// </summary>
    internal static TimeSpan MaxDeliveryTimeSpan
    {
        get => TimeSpan.Parse(XMLTools.LoadListFromXMLElement(s_data_config_xml).Element("MaxDeliveryTimeSpan")?.Value ?? "00:00:00");
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("MaxDeliveryTimeSpan")?.SetValue(value);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the risk range time span from/to XML configuration.
    /// </summary>
    internal static TimeSpan RiskRange
    {
        get => TimeSpan.Parse(XMLTools.LoadListFromXMLElement(s_data_config_xml).Element("RiskRange")?.Value ?? "00:00:00");
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("RiskRange")?.SetValue(value);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }

    /// <summary>
    /// Gets or sets the inactivity range time span from/to XML configuration.
    /// </summary>
    internal static TimeSpan InactivityRange
    {
        get => TimeSpan.Parse(XMLTools.LoadListFromXMLElement(s_data_config_xml).Element("InactivityRange")?.Value ?? "00:00:00");
        set
        {
            var root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("InactivityRange")?.SetValue(value);
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
    /// </summary>
    internal static void Reset()
    {
        var root = new System.Xml.Linq.XElement("config");
        root.Add(new System.Xml.Linq.XElement("AdminId", 0));
        root.Add(new System.Xml.Linq.XElement("AdminPassword", "Admin"));
        root.Add(new System.Xml.Linq.XElement("AvgCarSpeedKmh", 0));
        root.Add(new System.Xml.Linq.XElement("AvgMotorcycleSpeedKmh", 0));
        root.Add(new System.Xml.Linq.XElement("AvgBicycleSpeedKmh", 0));
        root.Add(new System.Xml.Linq.XElement("AvgWalkingSpeedKmh", 0));
        root.Add(new System.Xml.Linq.XElement("MaxDeliveryTimeSpan", TimeSpan.Zero));
        root.Add(new System.Xml.Linq.XElement("RiskRange", TimeSpan.Zero));
        root.Add(new System.Xml.Linq.XElement("InactivityRange", TimeSpan.Zero));
        root.Add(new System.Xml.Linq.XElement("Clock", DateTime.Now));
        XMLTools.SaveListToXMLElement(root, s_data_config_xml);
    }
}
