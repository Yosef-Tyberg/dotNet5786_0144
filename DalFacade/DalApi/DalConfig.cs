namespace DalApi;
using System.Xml.Linq;

//I named this DalConfig to match the name used in the Factory class, and by the static constructor (though I had to capitalize it).
//between the two I preferred this one for clarity's sake

static class DalConfig
{
    /// <summary> 
    /// internal PDS class 
    /// </summary> 
    internal record DalImplementation
    (string Package,   // package/dll name 
     string Namespace, // namespace where DAL implementation class is contained in 
     string Class   // DAL implementation class name 
    );

    internal static string s_dalName;
    internal static Dictionary<string, DalImplementation> s_dalPackages;

    static DalConfig()
    {
        // Try multiple path strategies
        string configPath = null!;
        
        // Strategy 1: Check relative to BaseDirectory with up-navigation
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(basePath, @"..\..\..\..\xml\dal-config.xml"),
            Path.Combine(basePath, @"..\..\..\xml\dal-config.xml"),
            Path.Combine(basePath, @"..\..\xml\dal-config.xml"),
            Path.Combine(basePath, @"..\xml\dal-config.xml"),
            Path.Combine(basePath, "xml", "dal-config.xml"),
            Path.Combine(Directory.GetCurrentDirectory(), "xml", "dal-config.xml"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "xml", "dal-config.xml"),
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                configPath = fullPath;
                break;
            }
        }

        if (configPath == null)
        {
            throw new DalConfigException(
                $"dal-config.xml file not found. Searched in: {string.Join(", ", candidates.Select(c => Path.GetFullPath(c)))}");
        }

        XElement dalConfig = XElement.Load(configPath) ??
            throw new DalConfigException("dal-config.xml file is not found");

        s_dalName =
            dalConfig.Element("dal")?.Value ??
            throw new DalConfigException("<dal> element is missing");

        var packages = dalConfig.Element("dal-packages")?.Elements() ??
            throw new DalConfigException("<dal-packages> element is missing");
        s_dalPackages = (from item in packages
                         let pkg = item.Value
                         let ns = item.Attribute("namespace")?.Value ?? "Dal"
                         let cls = item.Attribute("class")?.Value ?? pkg
                         select (item.Name, new DalImplementation(pkg, ns, cls))
                     ).ToDictionary(p => "" + p.Name, p => p.Item2);
    }
}

[Serializable]
public class DalConfigException : Exception
{
    public DalConfigException(string msg) : base(msg) { }
    public DalConfigException(string msg, Exception ex) : base(msg, ex) { }
}