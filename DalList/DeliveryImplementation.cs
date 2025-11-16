namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// DAL implementation of <see cref="DalApi.ICrud"/> using the in-memory DataSource.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Creates a new Delivery record and assigns it a new ID from <see cref="Config.NextDeliveryId"/>.
    /// Note: this method intentionally assigns a new id for new deliveries.
    /// </summary>
    /// <param name="item">Delivery item to create.</param>
    public void Create(Delivery item)
    {
        int id = Config.NextDeliveryId;
        Delivery copy = item with { Id = id };
        DataSource.Deliveries.Add(copy);
    }

    /// <summary>
    /// Deletes a delivery by Id.
    /// </summary>
    /// <param name="id">Id of the delivery to delete.</param>
    /// <exception cref="Exception">Thrown when the delivery doesn't exist.</exception>
    public void Delete(int id)
    {
        Delivery? temp = Read(id);
        if (temp != null)
        {
            DataSource.Deliveries.Remove(temp);
        }
        else
        {
            throw new DalDoesNotExistException($"Delivery with Id=" + id + " doesn't exist");
        }
    }

    /// <summary>
    /// Removes all deliveries from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Deliveries.Clear();
    }

    /// <summary>
    /// Reads a delivery by Id.
    /// </summary>
    /// <param name="id">Id of the delivery to read.</param>
    /// <returns>The delivery with the specified Id or null if not found.</returns>
    public Delivery? Read(int id)
    {
        return DataSource.Deliveries.FirstOrDefault(item => item.Id == id);
    }


    /// <summary>
    /// Reads an entity by first matching filter.
    /// </summary>
    /// <param name="filter">boolean to filter by.</param>
    /// <returns>The first entity matching the filter, or null if not found.</returns>
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return DataSource.Deliveries.FirstOrDefault(item => filter(item));
    }

    /// <summary>
    /// Returns all deliveries currently in the data source.
    /// </summary>
    /// <returns>List of all deliveries.</returns>
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null) //stage 2 
       => filter == null
           ? DataSource.Deliveries.Select(item => item)
            : DataSource.Deliveries.Where(filter);

    /// <summary>
    /// Updates an existing delivery entry by replacing the stored instance.
    /// </summary>
    /// <param name="item">Delivery item containing updated values.</param>
    public void Update(Delivery item)
    {
        Delete(item.Id);
        // calling create here would give item a different id
        DataSource.Deliveries.Add(item);
    }
}
