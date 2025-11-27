namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// XML-based implementation of <see cref="DalApi.ICourier"/> using XmlSerializer for persistence.
/// Null fields are automatically excluded from serialization by XmlSerializer's default behavior
/// when using the XmlIgnoreAttribute or by not serializing properties with null values.
/// </summary>
public class CourierImplementation : ICourier
{
    /// <summary>
    /// Creates a new courier and persists it to the XML file.
    /// </summary>
    /// <param name="item">Courier to create.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown when a courier with the same Id already exists.</exception>
    public void Create(Courier item)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (couriers.Any(c => c.Id == item.Id))
            throw new DalAlreadyExistsException($"Courier with Id={item.Id} already exists");
        couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }

    /// <summary>
    /// Reads a courier by Id from the XML file.
    /// </summary>
    /// <param name="id">Id of the courier to read.</param>
    /// <returns>The courier with the specified Id or null if not found.</returns>
    public Courier? Read(int id)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return couriers.FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Reads a courier by first matching filter from the XML file.
    /// </summary>
    /// <param name="filter">Predicate to filter by.</param>
    /// <returns>The first courier matching the filter, or null if not found.</returns>
    public Courier? Read(Func<Courier, bool> filter)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return couriers.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all couriers from the XML file with optional filtering.
    /// </summary>
    /// <param name="filter">Optional predicate to filter results.</param>
    /// <returns>Enumerable of couriers matching the filter or all couriers if no filter provided.</returns>
    public IEnumerable<Courier?> ReadAll(Func<Courier, bool>? filter = null)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return filter == null ? couriers : couriers.Where(filter);
    }

    /// <summary>
    /// Updates an existing courier in the XML file.
    /// </summary>
    /// <param name="item">Courier with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the courier doesn't exist.</exception>
    public void Update(Courier item)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (couriers.RemoveAll(c => c.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Courier with Id={item.Id} doesn't exist");
        couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }

    /// <summary>
    /// Deletes a courier by Id from the XML file.
    /// </summary>
    /// <param name="id">Id of the courier to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the courier doesn't exist.</exception>
    public void Delete(int id)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (couriers.RemoveAll(c => c.Id == id) == 0)
            throw new DalDoesNotExistException($"Courier with Id={id} doesn't exist");
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }

    /// <summary>
    /// Deletes all couriers from the XML file.
    /// </summary>
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), Config.s_couriers_xml);
    }
}

