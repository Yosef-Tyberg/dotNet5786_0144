namespace DO;
/// <summary>
/// Order Entity represents a customer's order request, including delivery details.
/// </summary>
/// <param name="Id">Unique identifier for the order</param>
/// <param name="OrderType">Type/category of the order</param>
/// <param name="VerbalDescription">Verbal/short description of the order contents</param>
/// <param name="FullOrderAccess">Full access string/code for order validation</param>
/// <param name="Latitude">Latitude coordinate of the delivery destination</param>
/// <param name="Longitude">Longitude coordinate of the delivery destination</param>
/// <param name="CustomerFullName">Full name of the customer making the order</param>
/// <param name="CustomerMobile">Customer's mobile phone number</param>
/// <param name="Volume">Volume of the package being delivered</param>
/// <param name="Weight">Weight of the package being delivered</param>
/// <param name="Fragile">Indicates if the contents are fragile</param>
/// <param name="Height">Package height dimension</param>
/// <param name="Width">Package width dimension</param>
/// <param name="OrderOpenTime">Timestamp when the order was created and opened</param>
public record Order
(
    int Id,
    OrderTypes OrderType,
    string VerbalDescription,
    string FullOrderAccess,
    double Latitude,
    double Longitude,
    string CustomerFullName,
    string CustomerMobile,
    double Volume,
    double Weight,
    bool Fragile,
    double Height,
    double Width,
    DateTime OrderOpenTime

)
{
    public Order() : this(0, OrderTypes.Pizza, "", "", 0, 0, "", "",
        0, 0, false, 0, 0, DateTime.Now) { }
}
