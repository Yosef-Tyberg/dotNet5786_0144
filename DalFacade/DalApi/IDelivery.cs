namespace DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Interface for CRUD operations on Delivery entities in the DAL.
/// </summary>
public interface IDelivery
{
    /// <summary>
    /// Creates a new delivery entity in the DAL.
    /// </summary>
    /// <param name="item">Delivery entity to create.</param>
    void Create(Delivery item);

    /// <summary>
    /// Reads a delivery entity by its identifier.
    /// </summary>
    /// <param name="id">Delivery Id to read.</param>
    /// <returns>The delivery with the given Id, or null if not found.</returns>
    Delivery? Read(int id);

    /// <summary>
    /// Reads all delivery entities stored in the DAL.
    /// </summary>
    /// <returns>List of all deliveries.</returns>
    List<Delivery> ReadAll();

    /// <summary>
    /// Updates an existing delivery entity.
    /// </summary>
    /// <param name="item">Delivery entity with updated values.</param>
    void Update(Delivery item);

    /// <summary>
    /// Deletes a delivery entity by its identifier.
    /// </summary>
    /// <param name="id">Id of the delivery to delete.</param>
    void Delete(int id);

    /// <summary>
    /// Deletes all delivery entities in the DAL.
    /// </summary>
    void DeleteAll();
}
