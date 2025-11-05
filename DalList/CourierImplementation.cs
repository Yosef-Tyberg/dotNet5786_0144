namespace DalList;

using Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// List-backed implementation of <see cref="DalApi.ICourier"/> for in-memory storage.
/// </summary>
public class CourierImplementation : ICourier
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
    /// Returns all couriers currently in the data source.
    /// </summary>
    /// <returns>List of all couriers.</returns>
    public List<Courier> ReadAll()
    {
        return DataSource.Couriers.ToList(); ;
    }

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
