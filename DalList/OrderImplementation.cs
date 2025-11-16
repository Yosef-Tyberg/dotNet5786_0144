namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// DAL implementation of <see cref="DalApi.ICrud"/> using the in-memory DataSource.
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new Order record and assigns it a new ID from <see cref="Config.NextOrderId"/>.
    /// </summary>
    /// <param name="item">Order item to create.</param>
    public void Create(Order item)
    {
        int id = Config.NextOrderId;
        Order copy = item with { Id = id };
        DataSource.Orders.Add(copy);
    }

    /// <summary>
    /// Deletes an order by Id.
    /// </summary>
    /// <param name="id">Id of the order to delete.</param>
    /// <exception cref="Exception">Thrown when the order doesn't exist.</exception>
    public void Delete(int id)
    {
        Order? temp = Read(id);
        if (temp != null)
        {
            DataSource.Orders.Remove(temp);
        }
        else
        {
            throw new DalDoesNotExistException($"Order with Id=" + id + " doesn't exist");
        }
    }

    /// <summary>
    /// Removes all orders from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    /// <summary>
    /// Reads an order by Id.
    /// </summary>
    /// <param name="id">Id of the order to read.</param>
    /// <returns>The order with the specified Id or null if not found.</returns>
    public Order? Read(int id)
    {
        return DataSource.Orders.FirstOrDefault(item => item.Id == id);
    }


    /// <summary>
    /// Reads an entity by first matching filter.
    /// </summary>
    /// <param name="filter">boolean to filter by.</param>
    /// <returns>The first entity matching the filter, or null if not found.</returns>
    public Order? Read(Func<Order, bool> filter)
    {
        return DataSource.Orders.FirstOrDefault(item => filter(item));
    }

    /// <summary>
    /// Returns all orders currently in the data source.
    /// </summary>
    /// <returns>List of all orders.</returns>
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null) //stage 2 
       => filter == null
           ? DataSource.Orders.Select(item => item) 
            : DataSource.Orders.Where(filter);

    /// <summary>
    /// Updates an existing order entry by replacing the stored instance.
    /// </summary>
    /// <param name="item">Order item containing updated values.</param>
    public void Update(Order item)
    {
        Delete(item.Id);
        // calling create here would give item a different id
        DataSource.Orders.Add(item);
    }
}
