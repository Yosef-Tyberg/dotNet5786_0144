using Dal;
using DalApi;
using DO;

namespace DalTest;

/*this class was generated using chatgpt on the following prompts (continued after initialization prompts):
 * 1)
 * now i would like to focus on the program class. pages 25-28 of the attached pdf 
 * (starting from The main program for testing the Data Access Layer, the DalTest project ) (*this is the stage1 pdf)
 * contain more precise instructions for completing it. the provided examples are from a similar project, 
 * but make sure to use the correct names from my project (courier delivery etc.) and not the example's (student etc.) 
 * in the code you write for me. also, as indicated in the document. 
 * utilize tryparse when getting input from the user.
 * 2)
 * when using tryparse i would like an error thrown thrown if the tryparse returns false like so: 
 * Console.WriteLine("enter the BirthDate of the Student (in format dd/mm/yy hh:mm:ss): "); 
 * if (!DateTime.TryParse(Console.ReadLine(), out DateTime bdt)) throw new FormatException("BirthDate is invalid!"); 
 * make sure tryparse is used for all applicable values.
 * for update methods, empty input should always cause no change - it means the current value should remain the same after the update.
 */
internal static class Program
{

    static readonly IDal s_dal = new DalList();
    private static void Main()
    {
        while (true)
        {
            PrintMainMenu();
            Console.Write("Choose option: ");
            var mainRaw = Console.ReadLine();
            if (!int.TryParse(mainRaw, out int mainChoice))
            {
                Console.WriteLine("Invalid number; try again.");
                continue;
            }

            switch (mainChoice)
            {
                case 0:
                    Console.WriteLine("Exiting.");
                    return;
                case 1:
                    CourierMenu();
                    break;
                case 2:
                    OrderMenu();
                    break;
                case 3:
                    DeliveryMenu();
                    break;
                case 4:
                    try
                    {
                        DalTest.Initialization.Do(s_dal);
                        Console.WriteLine("Initialization complete.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error during re-initialization:");
                        Console.WriteLine(ex.ToString());
                    }
                    break;
                case 5:
                    DisplayAllData();
                    break;
                case 6:
                    ConfigMenu();
                    break;
                case 7:
                    ResetAllDataAndConfig();
                    break;
                default:
                    Console.WriteLine("Unknown option.");
                    break;
            }
        }
    }

    private static void PrintMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== Main Menu ===");
        Console.WriteLine("0. Exit");
        Console.WriteLine("1. Couriers");
        Console.WriteLine("2. Orders");
        Console.WriteLine("3. Deliveries");
        Console.WriteLine("4. Initialize data source");
        Console.WriteLine("5. Display all data");
        Console.WriteLine("6. Configuration");
        Console.WriteLine("7. Reset all");
        Console.WriteLine();
    }


    // -------------------------
    // Courier submenu
    // -------------------------
    private static void CourierMenu()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== Courier Menu ===");
        Console.WriteLine("0. Back");
        Console.WriteLine("1. Create courier ");
        Console.WriteLine("2. Read courier by Id");
        Console.WriteLine("3. Read all couriers");
        Console.WriteLine("4. Update courier");
        Console.WriteLine("5. Delete courier by Id");
        Console.WriteLine("6. Delete all couriers");
        Console.Write("Choose option: ");

        var raw = Console.ReadLine();
        if (!int.TryParse(raw, out int choice))
        {
            Console.WriteLine("Invalid number.");
            continue;
        }

        try
        {
            switch (choice)
            {
                case 0: return;
                case 1: CreateCourier(); break;
                case 2: ReadCourier(); break;
                case 3: ReadAllCouriers(); break;
                case 4: UpdateCourier(); break;
                case 5: DeleteCourier(); break;
                case 6:
                    s_dal.Courier!.DeleteAll();
                    Console.WriteLine("All couriers deleted.");
                    break;
                default:
                    Console.WriteLine("Unknown choice.");
                    break;
            }
        }
        catch (Exception ex)
        {
            // print exception and return to main menu
            Console.WriteLine("Exception:");
            Console.WriteLine(ex.ToString());
            return;
        }
    }
}

private static void CreateCourier()
{
    // All applicable parses use TryParse and throw FormatException if they fail.
    Console.Write("Id (int): ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Id is invalid!");

    Console.Write("FullName: ");
    var fullName = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(fullName))
        throw new FormatException("FullName is required!");

    Console.Write("MobilePhone: ");
    var phone = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(phone))
        throw new FormatException("MobilePhone is required!");

    Console.Write("Email: ");
    var email = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(email))
        throw new FormatException("Email is required!");

    Console.Write("Password: ");
    var password = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(password))
        throw new FormatException("Password is required!");

    Console.Write("Active (y/n): ");
    var activeRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(activeRaw))
        throw new FormatException("Active (y/n) is required!");
    bool active = activeRaw.Trim().ToLowerInvariant() == "y";

    Console.Write("DeliveryType (Car/Motorcycle/Bicycle/OnFoot): ");
    var dtypeRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(dtypeRaw) || !Enum.TryParse<DeliveryTypes>(dtypeRaw, true, out var dtype))
        throw new FormatException("DeliveryType is invalid!");

    Console.Write($"EmploymentStartTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dal.Config!.Clock}): ");
    var startRaw = Console.ReadLine();
    DateTime start;
    if (string.IsNullOrWhiteSpace(startRaw))
    {
        start = s_dal.Config!.Clock;
    }
    else if (!DateTime.TryParse(startRaw, out start))
    {
        throw new FormatException("EmploymentStartTime is invalid!");
    }

    Console.Write("PersonalMaxDeliveryDistance (km) (empty => null): ");
    var pmaxRaw = Console.ReadLine();
    double? pmax = null;
    if (!string.IsNullOrWhiteSpace(pmaxRaw))
    {
        if (!double.TryParse(pmaxRaw, out double parsedMax))
            throw new FormatException("PersonalMaxDeliveryDistance is invalid!");
        pmax = parsedMax;
    }

    var courier = new Courier(id, fullName!, phone!, email!, password!, active, dtype, start, pmax);
    s_dal.Courier!.Create(courier);
    Console.WriteLine("Courier created.");
}

private static void ReadCourier()
{
    Console.Write("Enter courier Id: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Courier Id is invalid!");

    var c = s_dal.Courier!.Read(id);
    Console.WriteLine(c == null ? $"Courier Id={id} not found." : c.ToString());
}

private static void ReadAllCouriers()
{
    var list = s_dal.Courier!.ReadAll();
    Console.WriteLine($"Couriers ({list.Count()}):");
    foreach (var c in list) Console.WriteLine(c);
}

private static void UpdateCourier()
{
    Console.Write("Enter courier Id to update: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Courier Id is invalid!");

    var existing = s_dal.Courier!.Read(id);
    if (existing == null)
    {
        Console.WriteLine("Courier not found.");
        return;
    }

    Console.WriteLine("Leave empty to keep current value (nullable fields: empty => null when creating only).");

    Console.Write($"FullName (current: {existing.FullName}): ");
    var fullNameRaw = Console.ReadLine();
    var fullName = string.IsNullOrEmpty(fullNameRaw) ? existing.FullName : fullNameRaw;

    Console.Write($"MobilePhone (current: {existing.MobilePhone}): ");
    var phoneRaw = Console.ReadLine();
    var phone = string.IsNullOrEmpty(phoneRaw) ? existing.MobilePhone : phoneRaw;

    Console.Write($"Email (current: {existing.Email}): ");
    var emailRaw = Console.ReadLine();
    var email = string.IsNullOrEmpty(emailRaw) ? existing.Email : emailRaw;

    Console.Write($"Password (current: {existing.Password}): ");
    var pwdRaw = Console.ReadLine();
    var password = string.IsNullOrEmpty(pwdRaw) ? existing.Password : pwdRaw;

    Console.Write($"Active (y/n) (current: {existing.Active}): ");
    var activeRaw = Console.ReadLine();
    var active = existing.Active;
    if (!string.IsNullOrWhiteSpace(activeRaw))
    {
        active = activeRaw.Trim().ToLowerInvariant() == "y";
    }

    Console.Write($"DeliveryType (Car/Motorcycle/Bicycle/OnFoot) (current: {existing.DeliveryType}): ");
    var dtypeRaw = Console.ReadLine();
    var dtype = existing.DeliveryType;
    if (!string.IsNullOrWhiteSpace(dtypeRaw))
    {
        if (!Enum.TryParse<DeliveryTypes>(dtypeRaw, true, out var parsedType))
            throw new FormatException("DeliveryType is invalid!");
        dtype = parsedType;
    }

    Console.Write($"EmploymentStartTime (yyyy-MM-dd HH:mm) (current: {existing.EmploymentStartTime}) leave empty to keep: ");
    var startRaw = Console.ReadLine();
    var start = existing.EmploymentStartTime;
    if (!string.IsNullOrWhiteSpace(startRaw))
    {
        if (!DateTime.TryParse(startRaw, out start))
            throw new FormatException("EmploymentStartTime is invalid!");
    }

    Console.Write($"PersonalMaxDeliveryDistance (km) (current: {existing.PersonalMaxDeliveryDistance?.ToString() ?? "null"}) (leave empty to keep): ");
    var pmaxRaw = Console.ReadLine();
    double? pmax = existing.PersonalMaxDeliveryDistance;
    if (!string.IsNullOrWhiteSpace(pmaxRaw))
    {
        if (!double.TryParse(pmaxRaw, out double parsed))
            throw new FormatException("PersonalMaxDeliveryDistance is invalid!");
        pmax = parsed;
    }

    var updated = existing with
    {
        FullName = fullName,
        MobilePhone = phone,
        Email = email,
        Password = password,
        Active = active,
        DeliveryType = dtype,
        EmploymentStartTime = start,
        PersonalMaxDeliveryDistance = pmax
    };

    s_dal.Courier.Update(updated);
    Console.WriteLine("Courier updated.");
}

private static void DeleteCourier()
{
    Console.Write("Enter courier Id to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Courier Id is invalid!");

    s_dal.Courier!.Delete(id);
    Console.WriteLine("Courier deleted.");
}

// -------------------------
// Order submenu
// -------------------------
private static void OrderMenu()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== Order Menu ===");
        Console.WriteLine("0. Back");
        Console.WriteLine("1. Create order (all fields)");
        Console.WriteLine("2. Read order by Id");
        Console.WriteLine("3. Read all orders");
        Console.WriteLine("4. Update order (all fields)");
        Console.WriteLine("5. Delete order by Id");
        Console.WriteLine("6. Delete all orders");
        Console.Write("Choose option: ");

        var raw = Console.ReadLine();
        if (!int.TryParse(raw, out int choice))
        {
            Console.WriteLine("Invalid number.");
            continue;
        }

        try
        {
            switch (choice)
            {
                case 0: return;
                case 1: CreateOrder(); break;
                case 2: ReadOrder(); break;
                case 3: ReadAllOrders(); break;
                case 4: UpdateOrder(); break;
                case 5: DeleteOrder(); break;
                case 6:
                    s_dal.Order!.DeleteAll();
                    Console.WriteLine("All orders deleted.");
                    break;
                default:
                    Console.WriteLine("Unknown choice.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception:");
            Console.WriteLine(ex.ToString());
            return;
        }
    }
}

private static void CreateOrder()
{
    Console.Write("OrderType (Pizza/Falafel): ");
    var otRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(otRaw) || !Enum.TryParse<OrderTypes>(otRaw, true, out var otype))
        throw new FormatException("OrderType is invalid!");

    Console.Write("VerbalDescription: ");
    var verbal = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(verbal))
        throw new FormatException("VerbalDescription is required!");

    Console.Write("FullOrderAccess (empty to auto-generate): ");
    var access = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(access)) access = Guid.NewGuid().ToString("N");

    Console.Write("Latitude (double): ");
    var latRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(latRaw) || !double.TryParse(latRaw, out double lat))
        throw new FormatException("Latitude is invalid!");

    Console.Write("Longitude (double): ");
    var lonRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(lonRaw) || !double.TryParse(lonRaw, out double lon))
        throw new FormatException("Longitude is invalid!");

    Console.Write("CustomerFullName: ");
    var cust = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(cust))
        throw new FormatException("CustomerFullName is required!");

    Console.Write("CustomerMobile: ");
    var custPhone = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(custPhone))
        throw new FormatException("CustomerMobile is required!");

    Console.Write("Volume (double): ");
    var volRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(volRaw) || !double.TryParse(volRaw, out double volume))
        throw new FormatException("Volume is invalid!");

    Console.Write("Weight (double): ");
    var wRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(wRaw) || !double.TryParse(wRaw, out double weight))
        throw new FormatException("Weight is invalid!");

    Console.Write("Fragile (y/n): ");
    var fragRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(fragRaw))
        throw new FormatException("Fragile (y/n) is required!");
    bool fragile = fragRaw.Trim().ToLowerInvariant() == "y";

    Console.Write("Height (double): ");
    var hRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(hRaw) || !double.TryParse(hRaw, out double height))
        throw new FormatException("Height is invalid!");

    Console.Write("Width (double): ");
    var widRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(widRaw) || !double.TryParse(widRaw, out double width))
        throw new FormatException("Width is invalid!");

    Console.Write($"OrderOpenTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dal.Config!.Clock}): ");
    var openRaw = Console.ReadLine();
    DateTime open;
    if (string.IsNullOrWhiteSpace(openRaw))
    {
        open = s_dal.Config!.Clock;
    }
    else if (!DateTime.TryParse(openRaw, out open))
    {
        throw new FormatException("OrderOpenTime is invalid!");
    }

    var order = new Order(0, otype, verbal!, access!, lat, lon, cust!, custPhone!, volume, weight, fragile, height, width, open);
    s_dal.Order!.Create(order);
    Console.WriteLine("Order created (Id assigned by DAL).");
}

private static void ReadOrder()
{
    Console.Write("Enter order Id: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Order Id is invalid!");

    var o = s_dal.Order!.Read(id);
    Console.WriteLine(o == null ? $"Order Id={id} not found." : o.ToString());
}

private static void ReadAllOrders()
{
    var list = s_dal.Order!.ReadAll();
    Console.WriteLine($"Orders ({list.Count()}):");
    foreach (var o in list) Console.WriteLine(o);
}

private static void UpdateOrder()
{
    Console.Write("Enter order Id to update: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Order Id is invalid!");

    var existing = s_dal.Order!.Read(id);
    if (existing == null)
    {
        Console.WriteLine("Order not found.");
        return;
    }

    Console.WriteLine("Leave empty to keep current value (nullable fields: empty => null when creating only).");

    Console.Write($"OrderType (current: {existing.OrderType}): ");
    var otRaw = Console.ReadLine();
    var otype = existing.OrderType;
    if (!string.IsNullOrWhiteSpace(otRaw))
    {
        if (!Enum.TryParse<OrderTypes>(otRaw, true, out var parsedOt))
            throw new FormatException("OrderType is invalid!");
        otype = parsedOt;
    }

    Console.Write($"VerbalDescription (current: {existing.VerbalDescription}): ");
    var verbalRaw = Console.ReadLine();
    var verbal = string.IsNullOrEmpty(verbalRaw) ? existing.VerbalDescription : verbalRaw;

    Console.Write($"FullOrderAccess (current: {existing.FullOrderAccess}): ");
    var accessRaw = Console.ReadLine();
    var access = string.IsNullOrWhiteSpace(accessRaw) ? existing.FullOrderAccess : accessRaw;

    Console.Write($"Latitude (current: {existing.Latitude}): ");
    var latRaw = Console.ReadLine();
    var lat = existing.Latitude;
    if (!string.IsNullOrWhiteSpace(latRaw))
    {
        if (!double.TryParse(latRaw, out lat))
            throw new FormatException("Latitude is invalid!");
    }

    Console.Write($"Longitude (current: {existing.Longitude}): ");
    var lonRaw = Console.ReadLine();
    var lon = existing.Longitude;
    if (!string.IsNullOrWhiteSpace(lonRaw))
    {
        if (!double.TryParse(lonRaw, out lon))
            throw new FormatException("Longitude is invalid!");
    }

    Console.Write($"CustomerFullName (current: {existing.CustomerFullName}): ");
    var custRaw = Console.ReadLine();
    var cust = string.IsNullOrEmpty(custRaw) ? existing.CustomerFullName : custRaw;

    Console.Write($"CustomerMobile (current: {existing.CustomerMobile}): ");
    var custPhoneRaw = Console.ReadLine();
    var custPhone = string.IsNullOrEmpty(custPhoneRaw) ? existing.CustomerMobile : custPhoneRaw;

    Console.Write($"Volume (current: {existing.Volume}): ");
    var volRaw = Console.ReadLine();
    var volume = existing.Volume;
    if (!string.IsNullOrWhiteSpace(volRaw))
    {
        if (!double.TryParse(volRaw, out volume))
            throw new FormatException("Volume is invalid!");
    }

    Console.Write($"Weight (current: {existing.Weight}): ");
    var wRaw = Console.ReadLine();
    var weight = existing.Weight;
    if (!string.IsNullOrWhiteSpace(wRaw))
    {
        if (!double.TryParse(wRaw, out weight))
            throw new FormatException("Weight is invalid!");
    }

    Console.Write($"Fragile (y/n) (current: {existing.Fragile}): ");
    var fragRaw = Console.ReadLine();
    var fragile = existing.Fragile;
    if (!string.IsNullOrWhiteSpace(fragRaw))
    {
        fragile = fragRaw.Trim().ToLowerInvariant() == "y";
    }

    Console.Write($"Height (current: {existing.Height}): ");
    var hRaw = Console.ReadLine();
    var height = existing.Height;
    if (!string.IsNullOrWhiteSpace(hRaw))
    {
        if (!double.TryParse(hRaw, out height))
            throw new FormatException("Height is invalid!");
    }

    Console.Write($"Width (current: {existing.Width}): ");
    var widRaw = Console.ReadLine();
    var width = existing.Width;
    if (!string.IsNullOrWhiteSpace(widRaw))
    {
        if (!double.TryParse(widRaw, out width))
            throw new FormatException("Width is invalid!");
    }

    Console.Write($"OrderOpenTime (yyyy-MM-dd HH:mm) (current: {existing.OrderOpenTime}): ");
    var openRaw = Console.ReadLine();
    var open = existing.OrderOpenTime;
    if (!string.IsNullOrWhiteSpace(openRaw))
    {
        if (!DateTime.TryParse(openRaw, out open))
            throw new FormatException("OrderOpenTime is invalid!");
    }

    var updated = existing with
    {
        OrderType = otype,
        VerbalDescription = verbal,
        FullOrderAccess = access,
        Latitude = lat,
        Longitude = lon,
        CustomerFullName = cust,
        CustomerMobile = custPhone,
        Volume = volume,
        Weight = weight,
        Fragile = fragile,
        Height = height,
        Width = width,
        OrderOpenTime = open
    };

    s_dal.Order.Update(updated);
    Console.WriteLine("Order updated.");
}

private static void DeleteOrder()
{
    Console.Write("Enter order Id to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Order Id is invalid!");

    s_dal.Order!.Delete(id);
    Console.WriteLine("Order deleted.");
}

// -------------------------
// Delivery submenu
// -------------------------
private static void DeliveryMenu()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== Delivery Menu ===");
        Console.WriteLine("0. Back");
        Console.WriteLine("1. Create delivery (all fields)");
        Console.WriteLine("2. Read delivery by Id");
        Console.WriteLine("3. Read all deliveries");
        Console.WriteLine("4. Update delivery (all fields)");
        Console.WriteLine("5. Delete delivery by Id");
        Console.WriteLine("6. Delete all deliveries");
        Console.Write("Choose option: ");

        var raw = Console.ReadLine();
        if (!int.TryParse(raw, out int choice))
        {
            Console.WriteLine("Invalid number.");
            continue;
        }

        try
        {
            switch (choice)
            {
                case 0: return;
                case 1: CreateDelivery(); break;
                case 2: ReadDelivery(); break;
                case 3: ReadAllDeliveries(); break;
                case 4: UpdateDelivery(); break;
                case 5: DeleteDelivery(); break;
                case 6:
                    s_dal.Delivery!.DeleteAll();
                    Console.WriteLine("All deliveries deleted.");
                    break;
                default:
                    Console.WriteLine("Unknown choice.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception:");
            Console.WriteLine(ex.ToString());
            return;
        }
    }
}

private static void CreateDelivery()
{
    Console.Write("OrderId (int): ");
    var oRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(oRaw) || !int.TryParse(oRaw, out int orderId))
        throw new FormatException("OrderId is invalid!");

    Console.Write("CourierId (int): ");
    var cRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(cRaw) || !int.TryParse(cRaw, out int courierId))
        throw new FormatException("CourierId is invalid!");

    Console.Write("DeliveryType (Car/Motorcycle/Bicycle/OnFoot): ");
    var dtRaw = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(dtRaw) || !Enum.TryParse<DeliveryTypes>(dtRaw, true, out var dtype))
        throw new FormatException("DeliveryType is invalid!");

    Console.Write($"DeliveryStartTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dal.Config!.Clock}): ");
    var stRaw = Console.ReadLine();
    DateTime start;
    if (string.IsNullOrWhiteSpace(stRaw)) start = s_dal.Config!.Clock;
    else if (!DateTime.TryParse(stRaw, out start)) throw new FormatException("DeliveryStartTime is invalid!");

    Console.Write("ActualDistance (km) (empty => null): ");
    var adRaw = Console.ReadLine();
    double? actual = null;
    if (!string.IsNullOrWhiteSpace(adRaw))
    {
        if (!double.TryParse(adRaw, out double parsed)) throw new FormatException("ActualDistance is invalid!");
        actual = parsed;
    }

    Console.Write("DeliveryEndType (Delivered/Failed/Cancelled/CustomerRefused/RecipientNotFound) (empty => null): ");
    var etRaw = Console.ReadLine();
    DeliveryEndTypes? endType = null;
    if (!string.IsNullOrWhiteSpace(etRaw))
    {
        if (!Enum.TryParse<DeliveryEndTypes>(etRaw, true, out var parsedEt)) throw new FormatException("DeliveryEndType is invalid!");
        endType = parsedEt;
    }

    DateTime? endTime = null;
    if (endType != null)
    {
        Console.Write("DeliveryEndTime (yyyy-MM-dd HH:mm) (required since end type provided): ");
        var eRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(eRaw) || !DateTime.TryParse(eRaw, out DateTime parsedEnd))
            throw new FormatException("DeliveryEndTime is invalid or missing!");
        endTime = parsedEnd;
    }

    var d = new Delivery(0, orderId, courierId, dtype, start, actual, endType, endTime);
    s_dal.Delivery!.Create(d);
    Console.WriteLine("Delivery created.");
}

private static void ReadDelivery()
{
    Console.Write("Enter delivery Id: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Delivery Id is invalid!");

    var d = s_dal.Delivery!.Read(id);
    Console.WriteLine(d == null ? $"Delivery Id={id} not found." : d.ToString());
}

private static void ReadAllDeliveries()
{
    var list = s_dal.Delivery!.ReadAll();
    Console.WriteLine($"Deliveries ({list.Count()}):");
    foreach (var d in list) Console.WriteLine(d);
}

private static void UpdateDelivery()
{
    Console.Write("Enter delivery Id to update: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Delivery Id is invalid!");

    var existing = s_dal.Delivery!.Read(id);
    if (existing == null)
    {
        Console.WriteLine("Delivery not found.");
        return;
    }

    Console.WriteLine("Leave empty to keep current value (nullable fields: empty => null when creating only).");

    Console.Write($"OrderId (current: {existing.OrderId}): ");
    var oRaw = Console.ReadLine();
    var orderId = existing.OrderId;
    if (!string.IsNullOrWhiteSpace(oRaw))
    {
        if (!int.TryParse(oRaw, out orderId)) throw new FormatException("OrderId is invalid!");
    }

    Console.Write($"CourierId (current: {existing.CourierId}): ");
    var cRaw = Console.ReadLine();
    var courierId = existing.CourierId;
    if (!string.IsNullOrWhiteSpace(cRaw))
    {
        if (!int.TryParse(cRaw, out courierId)) throw new FormatException("CourierId is invalid!");
    }

    Console.Write($"DeliveryType (current: {existing.DeliveryType}): ");
    var dtRaw = Console.ReadLine();
    var dtype = existing.DeliveryType;
    if (!string.IsNullOrWhiteSpace(dtRaw))
    {
        if (!Enum.TryParse<DeliveryTypes>(dtRaw, true, out var parsedDt)) throw new FormatException("DeliveryType is invalid!");
        dtype = parsedDt;
    }

    Console.Write($"DeliveryStartTime (current: {existing.DeliveryStartTime}) leave empty to keep: ");
    var stRaw2 = Console.ReadLine();
    var start = existing.DeliveryStartTime;
    if (!string.IsNullOrWhiteSpace(stRaw2))
    {
        if (!DateTime.TryParse(stRaw2, out start)) throw new FormatException("DeliveryStartTime is invalid!");
    }

    Console.Write($"ActualDistance (current: {existing.ActualDistance?.ToString() ?? "null"}) leave empty to keep: ");
    var aRaw = Console.ReadLine();
    double? actual = existing.ActualDistance;
    if (!string.IsNullOrWhiteSpace(aRaw))
    {
        if (!double.TryParse(aRaw, out double parsedA)) throw new FormatException("ActualDistance is invalid!");
        actual = parsedA;
    }

    Console.Write($"DeliveryEndType (current: {existing.DeliveryEndType?.ToString() ?? "null"}) (empty to keep): ");
    var etRaw2 = Console.ReadLine();
    DeliveryEndTypes? endType = existing.DeliveryEndType;
    if (!string.IsNullOrWhiteSpace(etRaw2))
    {
        if (!Enum.TryParse<DeliveryEndTypes>(etRaw2, true, out var parsedEt)) throw new FormatException("DeliveryEndType is invalid!");
        endType = parsedEt;
    }

    Console.Write($"DeliveryEndTime (current: {existing.DeliveryEndTime?.ToString() ?? "null"}) (empty to keep): ");
    var eRaw2 = Console.ReadLine();
    DateTime? endTime = existing.DeliveryEndTime;
    if (!string.IsNullOrWhiteSpace(eRaw2))
    {
        if (!DateTime.TryParse(eRaw2, out DateTime parsedE)) throw new FormatException("DeliveryEndTime is invalid!");
        endTime = parsedE;
    }

    var updated = existing with
    {
        OrderId = orderId,
        CourierId = courierId,
        DeliveryType = dtype,
        DeliveryStartTime = start,
        ActualDistance = actual,
        DeliveryEndType = endType,
        DeliveryEndTime = endTime
    };

    s_dal.Delivery.Update(updated);
    Console.WriteLine("Delivery updated.");
}

private static void DeleteDelivery()
{
    Console.Write("Enter delivery Id to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        throw new FormatException("Delivery Id is invalid!");

    s_dal.Delivery!.Delete(id);
    Console.WriteLine("Delivery deleted.");
}

// -------------------------
// Configuration menu
// -------------------------
private static void ConfigMenu()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== Configuration Menu ===");
        Console.WriteLine("0. Back");
        Console.WriteLine("1. Increment time by one minute");
        Console.WriteLine("2. Increment time by one hour");
        Console.WriteLine("3. Display current time");
        Console.WriteLine("4. Set MaxGeneralDeliveryDistanceKm");
        Console.WriteLine("5. Reset configuration");
        Console.Write("Choose option: ");

        var raw = Console.ReadLine();
        if (!int.TryParse(raw, out int choice))
        {
            Console.WriteLine("Invalid number.");
            continue;
        }

        try
        {
            switch (choice)
            {
                case 0: return;
                case 1:
                    s_dal.Config!.Clock = s_dal.Config.Clock.AddMinutes(1);
                    Console.WriteLine($"Clock: {s_dal.Config.Clock}");
                    break;
                case 2:
                    s_dal.Config!.Clock = s_dal.Config.Clock.AddHours(1);
                    Console.WriteLine($"Clock: {s_dal.Config.Clock}");
                    break;
                case 3:
                    Console.WriteLine($"Clock: {s_dal.Config!.Clock}");
                    break;
                case 4:
                    Console.Write("Enter MaxGeneralDeliveryDistanceKm (double) (empty to cancel): ");
                    var vRaw = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(vRaw)) break;
                    if (!double.TryParse(vRaw, out double v)) throw new FormatException("MaxGeneralDeliveryDistanceKm is invalid!");
                    s_dal.Config!.MaxGeneralDeliveryDistanceKm = v;
                    Console.WriteLine("Saved.");
                    break;
                case 5:
                    s_dal.Config!.Reset();
                    Console.WriteLine("Configuration reset.");
                    break;
                default:
                    Console.WriteLine("Unknown option.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception:");
            Console.WriteLine(ex.ToString());
            return;
        }
    }
}

// -------------------------
// Utilities
// -------------------------
private static void DisplayAllData()
{
    try
    {
        var couriers = s_dal.Courier!.ReadAll();
        var orders = s_dal.Order!.ReadAll();
        var deliveries = s_dal.Delivery!.ReadAll();

        Console.WriteLine($"\nCouriers ({couriers.Count()}):");
        foreach (var c in couriers) Console.WriteLine(c);

        Console.WriteLine($"\nOrders ({orders.Count()}):");
        foreach (var o in orders) Console.WriteLine(o);

        Console.WriteLine($"\nDeliveries ({deliveries.Count()}):");
        foreach (var d in deliveries) Console.WriteLine(d);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception while reading data:");
        Console.WriteLine(ex.ToString());
    }
}

private static void ResetAllDataAndConfig()
{
    try
    {
        s_dal.Courier!.DeleteAll();
        s_dal.Order!.DeleteAll();
        s_dal.Delivery!.DeleteAll();
        s_dal.Config!.Reset();
        Console.WriteLine("Reset complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception while resetting:");
        Console.WriteLine(ex.ToString());
    }
}
}