﻿//used chatgpt prompt to make this class lazy and threadsafe
using DalApi;
namespace Dal;

using System.Diagnostics;

/// <summary>
/// XML-based implementation of the Data Access Layer (DAL).
/// </summary>
sealed internal class DalXml : IDal
{
    //The use of Lazy<T> ensures threadsafe lazy implementation. lazy is desirable to avoid unneeded allocation and slower startups
    //thread safe is desirable to avoid separate threads seeing the instance as null and creating multiple instances, and avoid a second thread
    // seeing a partially constructed instance.
    private static readonly Lazy<DalXml> s_instance =
           new Lazy<DalXml>(() => new DalXml(), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Singleton instance of DalXml.
    /// </summary>
    public static DalXml Instance => s_instance.Value;
    /// <summary>
    /// Private constructor for singleton pattern.
    /// </summary>
    private DalXml() { }
    /// <summary>
    /// Courier data access object.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Order data access object.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Delivery data access object.
    /// </summary>
    public IDelivery Delivery { get; } = new DeliveryImplementation();

    /// <summary>
    /// Configuration data access object.
    /// </summary>
    public IConfig Config { get; } = new ConfigImplementation();

    /// <summary>
    /// Resets the database by clearing all XML files and resetting configuration.
    /// </summary>
    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}