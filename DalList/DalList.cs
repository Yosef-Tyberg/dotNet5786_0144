//used chatgpt prompt to make this class lazy and threadsafe

namespace Dal;
using DalApi;
sealed internal class DalList : IDal
{
    //The use of Lazy<T> ensures threadsafe lazy implementation. lazy is desirable to avoid unneeded allocation and slower startups
    //thread safe is desirable to avoid separate threads seeing the instance as null and creating multiple instances, and avoid a second thread
    // seeing a partially constructed instance.
    private static readonly Lazy<DalList> s_instance =
           new Lazy<DalList>(() => new DalList(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static DalList Instance => s_instance.Value;
    private DalList() { }
    public ICourier Courier { get; } = new CourierImplementation();

    public IOrder Order { get; }  = new OrderImplementation();

    public IDelivery Delivery { get; } =  new DeliveryImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}
