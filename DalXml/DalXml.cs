//used chatgpt prompt to make this class lazy and threadsafe
using DalApi;
namespace Dal;

using System.Diagnostics;

sealed internal class DalXml : IDal
{
    //The use of Lazy<T> ensures threadsafe lazy implementation. lazy is desirable to avoid unneeded allocation and slower startups
    //thread safe is desirable to avoid separate threads seeing the instance as null and creating multiple instances, and avoid a second thread
    // seeing a partially constructed instance.
    private static readonly Lazy<DalXml> s_instance =
           new Lazy<DalXml>(() => new DalXml(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static DalXml Instance => s_instance.Value;
    private DalXml() { }
    public ICourier Courier { get; } = new CourierImplementation();

    public IOrder Order { get; } = new OrderImplementation();

    public IDelivery Delivery { get; } = new DeliveryImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}