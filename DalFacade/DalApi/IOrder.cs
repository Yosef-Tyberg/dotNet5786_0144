namespace DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Interface for CRUD operations on Order entities in the DAL.
/// </summary>
public interface IOrder
{
    /// <summary>
    /// Creates a new order entity in the DAL.
    /// </summary>
    /// <param name="item">Order entity to create.</param>
    void Create(Order item);

    /// <summary>
    /// Reads an order entity by its identifier.
    /// </summary>
    /// <param name="id">Order Id to read.</param>
    /// <returns>The order with the given Id, or null if not found.</returns>
    Order? Read(int id);

    /// <summary>
    /// Reads all order entities stored in the DAL.
    /// </summary>
    /// <returns>List of all orders.</returns>
    List<Order> ReadAll();

    /// <summary>
    /// Updates an existing order entity.
    /// </summary>
    /// <param name="item">Order entity with updated values.</param>
    void Update(Order item);

    /// <summary>
    /// Deletes an order entity by its identifier.
    /// </summary>
    /// <param name="id">Id of the order to delete.</param>
    void Delete(int id);

    /// <summary>
    /// Deletes all order entities in the DAL.
    /// </summary>
    void DeleteAll();
}
