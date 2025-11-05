namespace DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Interface for CRUD operations on Courier entities in the DAL.
/// </summary>
public interface ICourier
{
    /// <summary>
    /// Creates a new courier entity in the DAL.
    /// </summary>
    /// <param name="item">Courier entity to create.</param>
    void Create(Courier item);

    /// <summary>
    /// Reads a courier entity by its identifier.
    /// </summary>
    /// <param name="id">Courier Id to read.</param>
    /// <returns>The courier with the given Id, or null if not found.</returns>
    Courier? Read(int id);

    /// <summary>
    /// Reads all courier entities stored in the DAL.
    /// </summary>
    /// <returns>List of all couriers.</returns>
    List<Courier> ReadAll();

    /// <summary>
    /// Updates an existing courier entity.
    /// </summary>
    /// <param name="item">Courier entity with updated values.</param>
    void Update(Courier item);

    /// <summary>
    /// Deletes a courier entity by its identifier.
    /// </summary>
    /// <param name="id">Id of the courier to delete.</param>
    void Delete(int id);

    /// <summary>
    /// Deletes all courier entities in the DAL.
    /// </summary>
    void DeleteAll();
}
