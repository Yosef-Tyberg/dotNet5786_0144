namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Xml.Linq;

/// <summary>
/// XML-based implementation of <see cref="DalApi.IOrder"/> using XElement for persistence.
/// Null fields are not included in the XML output.
/// </summary>
public class OrderImplementation : IOrder
{
    /// <summary>
    /// Helper method to convert XElement to Order object.
    /// </summary>
    private static Order GetOrder(XElement orderElem)
    {
        return new Order()
        {
            Id = orderElem.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            OrderType = orderElem.ToEnumNullable<OrderTypes>("OrderType") ?? OrderTypes.Pizza,
            VerbalDescription = (string?)orderElem.Element("VerbalDescription") ?? "",
            Latitude = orderElem.ToDoubleNullable("Latitude") ?? 0,
            Longitude = orderElem.ToDoubleNullable("Longitude") ?? 0,
            CustomerFullName = (string?)orderElem.Element("CustomerFullName") ?? "",
            CustomerMobile = (string?)orderElem.Element("CustomerMobile") ?? "",
            Volume = orderElem.ToDoubleNullable("Volume") ?? 0,
            Weight = orderElem.ToDoubleNullable("Weight") ?? 0,
            Fragile = (bool?)orderElem.Element("Fragile") ?? false,
            Height = orderElem.ToDoubleNullable("Height") ?? 0,
            Width = orderElem.ToDoubleNullable("Width") ?? 0,
            OrderOpenTime = orderElem.ToDateTimeNullable("OrderOpenTime") ?? DateTime.Now,
            FullOrderAddress = (string?)orderElem.Element("FullOrderAddress") ?? ""
        };
    }

    /// <summary>
    /// Helper method to create XElement from Order object.
    /// Only includes fields with non-null/non-default values.
    /// </summary>
    private static IEnumerable<XElement> CreateOrderElement(Order item)
    {
        yield return new XElement("Id", item.Id);
        yield return new XElement("OrderType", item.OrderType);
        if (!string.IsNullOrEmpty(item.VerbalDescription))
            yield return new XElement("VerbalDescription", item.VerbalDescription);
        yield return new XElement("Latitude", item.Latitude);
        yield return new XElement("Longitude", item.Longitude);
        if (!string.IsNullOrEmpty(item.CustomerFullName))
            yield return new XElement("CustomerFullName", item.CustomerFullName);
        if (!string.IsNullOrEmpty(item.CustomerMobile))
            yield return new XElement("CustomerMobile", item.CustomerMobile);
        yield return new XElement("Volume", item.Volume);
        yield return new XElement("Weight", item.Weight);
        if (item.Fragile)
            yield return new XElement("Fragile", item.Fragile);
        yield return new XElement("Height", item.Height);
        yield return new XElement("Width", item.Width);
        yield return new XElement("OrderOpenTime", item.OrderOpenTime);
        if (!string.IsNullOrEmpty(item.FullOrderAddress))
            yield return new XElement("FullOrderAddress", item.FullOrderAddress);
    }

    /// <summary>
    /// Creates a new order and persists it to the XML file.
    /// Assigns a new ID from Config.NextOrderId.
    /// </summary>
    /// <param name="item">Order to create.</param>
    public void Create(Order item)
    {
        int id = Config.NextOrderId;
        Order orderWithId = item with { Id = id };
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);
        ordersRootElem.Add(new XElement("Order", CreateOrderElement(orderWithId)));
        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

    /// <summary>
    /// Reads an order by Id from the XML file.
    /// </summary>
    /// <param name="id">Id of the order to read.</param>
    /// <returns>The order with the specified Id or null if not found.</returns>
    public Order? Read(int id)
    {
        XElement? orderElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements().FirstOrDefault(o => (int?)o.Element("Id") == id);
        return orderElem is null ? null : GetOrder(orderElem);
    }

    /// <summary>
    /// Reads an order by first matching filter from the XML file.
    /// </summary>
    /// <param name="filter">Predicate to filter by.</param>
    /// <returns>The first order matching the filter, or null if not found.</returns>
    public Order? Read(Func<Order, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements().Select(o => GetOrder(o)).FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all orders from the XML file with optional filtering.
    /// </summary>
    /// <param name="filter">Optional predicate to filter results.</param>
    /// <returns>Enumerable of orders matching the filter or all orders if no filter provided.</returns>
    public IEnumerable<Order?> ReadAll(Func<Order, bool>? filter = null)
    {
        var allOrders = XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements().Select(o => GetOrder(o));
        return filter == null ? allOrders : allOrders.Where(filter);
    }

    /// <summary>
    /// Updates an existing order in the XML file.
    /// </summary>
    /// <param name="item">Order with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the order doesn't exist.</exception>
    public void Update(Order item)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);

        (ordersRootElem.Elements().FirstOrDefault(o => (int?)o.Element("Id") == item.Id)
            ?? throw new DalDoesNotExistException($"Order with Id={item.Id} doesn't exist"))
            .Remove();

        ordersRootElem.Add(new XElement("Order", CreateOrderElement(item)));
        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

    /// <summary>
    /// Deletes an order by Id from the XML file.
    /// </summary>
    /// <param name="id">Id of the order to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the order doesn't exist.</exception>
    public void Delete(int id)
    {
        XElement ordersRootElem = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);
        XElement? orderElem = ordersRootElem.Elements().FirstOrDefault(o => (int?)o.Element("Id") == id);
        
        if (orderElem is null)
            throw new DalDoesNotExistException($"Order with Id={id} doesn't exist");
        
        orderElem.Remove();
        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }

    /// <summary>
    /// Deletes all orders from the XML file.
    /// </summary>
    public void DeleteAll()
    {
        XElement ordersRootElem = new("Orders");
        XMLTools.SaveListToXMLElement(ordersRootElem, Config.s_orders_xml);
    }
}