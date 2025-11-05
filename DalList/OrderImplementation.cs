namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// DAL implementation of <see cref="DalApi.IOrder"/> using the in-memory DataSource.
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
            throw new Exception($"Order with Id=" + id + " doesn't exist");
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
    /// Returns all orders currently in the data source.
    /// </summary>
    /// <returns>List of all orders.</returns>
    public List<Order> ReadAll()
    {
        return DataSource.Orders.ToList(); ;
    }

    /// <summary>
    /// Updates an existing order entry by replacing the stored instance.
    /// Note: Update preserves the provided Id — it does not request a new id.
    /// </summary>
    /// <param name="item">Order item containing updated values.</param>
    public void Update(Order item)
    {
        Delete(item.Id);
        // calling create here would give item a different id
        DataSource.Orders.Add(item);
    }
}
