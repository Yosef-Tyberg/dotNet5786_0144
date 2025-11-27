namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Xml.Linq;

/// <summary>
/// XML-based implementation of <see cref="DalApi.IDelivery"/> using XElement for persistence.
/// </summary>
public class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Helper method to convert XElement to Delivery object.
    /// </summary>
    private static Delivery GetDelivery(XElement deliveryElem)
    {
        return new Delivery()
        {
            Id = deliveryElem.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            OrderId = deliveryElem.ToIntNullable("OrderId") ?? 0,
            CourierId = deliveryElem.ToIntNullable("CourierId") ?? 0,
            DeliveryType = deliveryElem.ToEnumNullable<DeliveryTypes>("DeliveryType") ?? DeliveryTypes.OnFoot,
            DeliveryStartTime = deliveryElem.ToDateTimeNullable("DeliveryStartTime") ?? DateTime.Now,
            ActualDistance = deliveryElem.ToDoubleNullable("ActualDistance"),
            DeliveryEndType = deliveryElem.ToEnumNullable<DeliveryEndTypes>("DeliveryEndType"),
            DeliveryEndTime = deliveryElem.ToDateTimeNullable("DeliveryEndTime")
        };
    }

    /// <summary>
    /// Helper method to create XElement from Delivery object.
    /// </summary>
    private static IEnumerable<XElement> CreateDeliveryElement(Delivery item)
    {
        yield return new XElement("Id", item.Id);
        yield return new XElement("OrderId", item.OrderId);
        yield return new XElement("CourierId", item.CourierId);
        yield return new XElement("DeliveryType", item.DeliveryType);
        yield return new XElement("DeliveryStartTime", item.DeliveryStartTime);
        if (item.ActualDistance.HasValue)
            yield return new XElement("ActualDistance", item.ActualDistance.Value);
        if (item.DeliveryEndType.HasValue)
            yield return new XElement("DeliveryEndType", item.DeliveryEndType.Value);
        if (item.DeliveryEndTime.HasValue)
            yield return new XElement("DeliveryEndTime", item.DeliveryEndTime.Value);
    }

    /// <summary>
    /// Creates a new delivery and persists it to the XML file.
    /// </summary>
    /// <param name="item">Delivery to create.</param>
    public void Create(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        deliveriesRootElem.Add(new XElement("Delivery", CreateDeliveryElement(item)));
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    /// <summary>
    /// Reads a delivery by Id from the XML file.
    /// </summary>
    /// <param name="id">Id of the delivery to read.</param>
    /// <returns>The delivery with the specified Id or null if not found.</returns>
    public Delivery? Read(int id)
    {
        XElement? deliveryElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().FirstOrDefault(d => (int?)d.Element("Id") == id);
        return deliveryElem is null ? null : GetDelivery(deliveryElem);
    }

    /// <summary>
    /// Reads a delivery by first matching filter from the XML file.
    /// </summary>
    /// <param name="filter">Predicate to filter by.</param>
    /// <returns>The first delivery matching the filter, or null if not found.</returns>
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().Select(d => GetDelivery(d)).FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all deliveries from the XML file with optional filtering.
    /// </summary>
    /// <param name="filter">Optional predicate to filter results.</param>
    /// <returns>Enumerable of deliveries matching the filter or all deliveries if no filter provided.</returns>
    public IEnumerable<Delivery?> ReadAll(Func<Delivery, bool>? filter = null)
    {
        var allDeliveries = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().Select(d => GetDelivery(d));
        return filter == null ? allDeliveries : allDeliveries.Where(filter);
    }

    /// <summary>
    /// Updates an existing delivery in the XML file.
    /// </summary>
    /// <param name="item">Delivery with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the delivery doesn't exist.</exception>
    public void Update(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);

        (deliveriesRootElem.Elements().FirstOrDefault(d => (int?)d.Element("Id") == item.Id)
            ?? throw new DalDoesNotExistException($"Delivery with Id={item.Id} doesn't exist"))
            .Remove();

        deliveriesRootElem.Add(new XElement("Delivery", CreateDeliveryElement(item)));
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    /// <summary>
    /// Deletes a delivery by Id from the XML file.
    /// </summary>
    /// <param name="id">Id of the delivery to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the delivery doesn't exist.</exception>
    public void Delete(int id)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        XElement? deliveryElem = deliveriesRootElem.Elements().FirstOrDefault(d => (int?)d.Element("Id") == id);
        
        if (deliveryElem is null)
            throw new DalDoesNotExistException($"Delivery with Id={id} doesn't exist");
        
        deliveryElem.Remove();
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    /// <summary>
    /// Deletes all deliveries from the XML file.
    /// </summary>
    public void DeleteAll()
    {
        XElement deliveriesRootElem = new("Deliveries");
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }
}