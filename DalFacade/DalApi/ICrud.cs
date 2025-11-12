namespace DalApi;

/// <summary>
/// Interface for CRUD operations on entities in the DAL.
/// </summary>
public interface ICrud<T> where T : class
{
    /// <summary>
    /// Creates a new  entity in the DAL.
    /// </summary>
    /// <param name="item">entity to create.</param>
    void Create(T item); 

    /// <summary>
    /// Reads an entity by its identifier.
    /// </summary>
    /// <param name="id">entity Id to read.</param>
    /// <returns>The entity with the given Id, or null if not found.</returns>
    T? Read(int id);



    /// <summary>
    /// Reads an entity by first matching filter.
    /// </summary>
    /// <param name="filter">boolean to filter by.</param>
    /// <returns>The first entity matching the filter, or null if not found.</returns>
    T? Read(Func<T, bool> filter);


    /// <summary>
    /// Reads all entities stored in the DAL, with optional filtering
    /// </summary>
    /// <returns>List of all entities.</returns>

    IEnumerable<T?> ReadAll(Func<T, bool>? filter = null); 

    /// <summary>
    /// Updates an existing  entity.
    /// </summary>
    /// <param name="item"> entity with updated values.</param>
    void Update(T item);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">Id of the entity to delete.</param>
    void Delete(int id);

    /// <summary>
    /// Deletes all entities in the DAL.
    /// </summary>
    void DeleteAll();
}
