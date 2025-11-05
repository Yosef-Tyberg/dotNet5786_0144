namespace DO;
/// <summary>
/// Courier Entity represents a courier working for the company.
/// </summary>
/// <param name="Id">Unique identifier of the courier</param>
/// <param name="FullName">Full name of the courier</param>
/// <param name="MobilePhone">Mobile phone number of the courier</param>
/// <param name="Email">Email address of the courier</param>
/// <param name="Password">Login password of the courier</param>
/// <param name="Active">Indicates if the courier is currently active</param>
/// <param name="DeliveryType">Type of delivery vehicle/method used by the courier</param>
/// <param name="EmploymentStartTime">Date and time the courier began employment</param>
/// <param name="PersonalMaxDeliveryDistance">Maximum distance (in km) the courier is willing to deliver, if limited</param>
public record Courier
(
    int Id,
    string FullName,
    string MobilePhone,
    string Email,
    string Password,
    bool Active,
    DeliveryTypes DeliveryType,
    DateTime EmploymentStartTime,
    double? PersonalMaxDeliveryDistance = null
)
{
    public Courier() : this (0,"","","","",false, DeliveryTypes.OnFoot,DateTime.Now) { }
}