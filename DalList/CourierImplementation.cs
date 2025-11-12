namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// List-backed implementation of <see cref="DalApi.ICrud"/> for in-memory storage.
/// </summary>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Creates a new courier in the in-memory data source.
    /// </summary>
    /// <param name="item">Courier to add.</param>
    /// <exception cref="Exception">Thrown when a courier with the same Id already exists.</exception>
    public void Create(Courier item)
    {
        if (Read(item.Id) is not null)
            throw new Exception($"Courier with Id={item.Id} already exists");
        DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Deletes a courier by Id.
    /// </summary>
    /// <param name="id">Id of the courier to delete.</param>
    /// <exception cref="Exception">Thrown when the courier doesn't exist.</exception>
    public void Delete(int id)
    {
        Courier? temp = Read(id);
        if (temp != null)
        {
            DataSource.Couriers.Remove(temp);
        }
        else
        {
            throw new Exception($"Courier with Id=" + id + " doesn't exist");
        }
    }

    /// <summary>
    /// Removes all couriers from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
    }

    /// <summary>
    /// Reads a courier by Id.
    /// </summary>
    /// <param name="id">Id of the courier to read.</param>
    /// <returns>The courier with the specified Id or null if not found.</returns>
    public Courier? Read(int id)
    {
        return DataSource.Couriers.FirstOrDefault(item => item.Id == id);
    }


    /// <summary>
    /// Reads an entity by first matching filter.
    /// </summary>
    /// <param name="filter">boolean to filter by.</param>
    /// <returns>The first entity matching the filter, or null if not found.</returns>
    public Courier? Read(Func<Courier, bool> filter)
    {
        return DataSource.Couriers.FirstOrDefault(item => filter(item));
    }

    /// <summary>
    /// Returns all couriers currently in the data source.
    /// </summary>
    /// <returns>List of all couriers.</returns>
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null) //stage 2 
       => filter == null
           ? DataSource.Couriers.Select(item => item)
            : DataSource.Couriers.Where(filter);

    /// <summary>
    /// Updates an existing courier by replacing the stored instance.
    /// </summary>
    /// <param name="item">Courier with updated data. Must have an existing Id.</param>
    public void Update(Courier item)
    {
        Delete(item.Id);
        Create(item);
    }
}
