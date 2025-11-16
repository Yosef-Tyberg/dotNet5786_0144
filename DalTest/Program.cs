using Dal;
using DalList;
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
 * 3)
* using github copilot with claude:
* i would like to modify the config menu in Program.cs. first, there should be clock increments for days and months. 
* second, Set MaxGeneralDeliveryDistanceKm"); should be removed. instead i want an option for set configuration variables
* and another for display configuration variables. these should each open a submenu containing return (as 0) 
* and options for all configuration variables found in the ConfigImplementation class 
* (except for clock, which has its options in the main config menu already). 
* the set config menu should allow setting the variables (in the case of an exception return to the set config menu. 
* remember to use try parse where appropriate when reading input). the display config menu should give allow 
* displaying each variable, plus an additional option to display all variables.
*/
internal static class Program
{
    // Program-owned DAL instances (we pass these to Initialization so both use the same store)
    // These are interfaces; concrete implementations live in other project files.
    private static ICourier? s_dalCourier = new CourierImplementation();
    private static IOrder? s_dalOrder = new OrderImplementation();
    private static IDelivery? s_dalDelivery = new DeliveryImplementation();
    private static IConfig? s_dalConfig = new ConfigImplementation();

    // The application's main loop.
    // Shows the main menu and dispatches to submenus or utilities based on the user's choice.
    private static void Main()
    {
        while (true)
        {
            PrintMainMenu();
            Console.Write("Choose option: ");
            var mainRaw = Console.ReadLine();
            if (!int.TryParse(mainRaw, out int mainChoice))
            {
                // Invalid numeric input — inform the user and show menu again.
                Console.WriteLine("Invalid number; try again.");
                continue;
            }

            // Switch based on main menu choice.
            switch (mainChoice)
            {
                case 0:
                    // Exit the program.
                    Console.WriteLine("Exiting.");
                    return;
                case 1:
                    // Enter Couriers submenu.
                    CourierMenu();
                    break;
                case 2:
                    // Enter Orders submenu.
                    OrderMenu();
                    break;
                case 3:
                    // Enter Deliveries submenu.
                    DeliveryMenu();
                    break;
                case 4:
                    // Re-initialize the DAL data source using Initialization provided elsewhere.
                    try
                    {
                        DalTest.Initialization.Do(s_dalCourier!, s_dalOrder!, s_dalDelivery!, s_dalConfig!);
                        Console.WriteLine("Initialization complete.");
                    }
                    catch (Exception ex)
                    {
                        // Print initialization exceptions to the console.
                        Console.WriteLine("Error during re-initialization:");
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case 5:
                    // Display all data from all stores.
                    DisplayAllData();
                    break;
                case 6:
                    // Enter configuration menu.
                    ConfigMenu();
                    break;
                case 7:
                    // Reset all data and configuration to defaults.
                    ResetAllDataAndConfig();
                    break;
                default:
                    // Unknown selection (out of range).
                    Console.WriteLine("Unknown option.");
                    break;
            }
        }
    }

    // Prints the main menu to the console.
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
    // This submenu provides full CRUD for Courier entities:
    //  - CreateCourier: reads user input for all required courier fields and calls DAL Create.
    //  - ReadCourier: read by Id.
    //  - ReadAllCouriers: list all couriers.
    //  - UpdateCourier: interactive update preserving existing values on empty input.
    //  - DeleteCourier: delete by Id.
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
                // Input validation for menu selection.
                Console.WriteLine("Invalid number.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 0: return; // Back to main menu.
                    case 1: CreateCourier(); break;
                    case 2: ReadCourier(); break;
                    case 3: ReadAllCouriers(); break;
                    case 4: UpdateCourier(); break;
                    case 5: DeleteCourier(); break;
                    case 6:
                        // Delete all couriers from DAL store.
                        s_dalCourier!.DeleteAll();
                        Console.WriteLine("All couriers deleted.");
                        break;
                    default:
                        Console.WriteLine("Unknown choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Any exception thrown by DAL or parsing shows message and returns to courier menu.
                Console.WriteLine(ex.Message);
                continue;
            }
        }
    }

    // CreateCourier: prompts the user for courier properties and creates the courier in the DAL.
    // Uses TryParse or required field checks and throws FormatException for invalid input.
    private static void CreateCourier()
    {
        // Id is required and must parse to int.
        Console.Write("Id (int): ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Id is invalid!");

        // FullName (required).
        Console.Write("FullName: ");
        var fullName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fullName))
            throw new FormatException("FullName is required!");

        // MobilePhone (required).
        Console.Write("MobilePhone: ");
        var phone = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(phone))
            throw new FormatException("MobilePhone is required!");

        // Email (required).
        Console.Write("Email: ");
        var email = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(email))
            throw new FormatException("Email is required!");

        // Password (required).
        Console.Write("Password: ");
        var password = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(password))
            throw new FormatException("Password is required!");

        // Active flag: expects 'y' or 'n' (case-insensitive). Required.
        Console.Write("Active (y/n): ");
        var activeRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(activeRaw))
            throw new DalInvalidInputException("invalid active flag");
        activeRaw = activeRaw.Trim().ToLowerInvariant();
        if (!(activeRaw == "y") && !(activeRaw == "n"))
            throw new DalInvalidInputException("invalid active flag");
        // Interpret 'y' (case-insensitive) as true
        bool active = activeRaw == "y";

        // DeliveryType enum parsing (Car/Motorcycle/Bicycle/OnFoot). Required.
        Console.Write("DeliveryType (Car/Motorcycle/Bicycle/OnFoot): ");
        var dtypeRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(dtypeRaw) || !Enum.TryParse<DeliveryTypes>(dtypeRaw, true, out var dtype)
            || !Enum.IsDefined(typeof(DeliveryTypes), dtype))
        {
            throw new DalInvalidInputException("invalid delivery type");
        }
        // EmploymentStartTime: optional; if empty, default to config clock.
        Console.Write($"EmploymentStartTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dalConfig!.Clock}): ");
        var startRaw = Console.ReadLine();
        DateTime start;
        if (string.IsNullOrWhiteSpace(startRaw))
        {
            // Use the current configured clock as default when empty.
            start = s_dalConfig!.Clock;
        }
        else if (!DateTime.TryParse(startRaw, out start))
        {
            // Invalid datetime format.
            throw new FormatException("EmploymentStartTime is invalid!");
        }

        // PersonalMaxDeliveryDistance: optional double; empty => null.
        Console.Write("PersonalMaxDeliveryDistance (km) (empty => null): ");
        var pmaxRaw = Console.ReadLine();
        double? pmax = null;
        if (!string.IsNullOrWhiteSpace(pmaxRaw))
        {
            if (!double.TryParse(pmaxRaw, out double parsedMax))
                throw new FormatException("PersonalMaxDeliveryDistance is invalid!");
            pmax = parsedMax;
        }

        // Create courier record and persist through DAL.
        var courier = new Courier(id, fullName!, phone!, email!, password!, active, dtype, start, pmax);
        s_dalCourier!.Create(courier);
        Console.WriteLine("Courier created.");
    }

    // ReadCourier: prompts for an Id and displays the courier if found.
    private static void ReadCourier()
    {
        Console.Write("Enter courier Id: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Courier Id is invalid!");

        var c = s_dalCourier!.Read(id);
        Console.WriteLine(c == null ? $"Courier Id={id} not found." : c.ToString());
    }

    // ReadAllCouriers: lists all couriers returned by the DAL.
    private static void ReadAllCouriers()
    {
        var list = s_dalCourier!.ReadAll();
        Console.WriteLine($"Couriers ({list.Count}):");
        foreach (var c in list) Console.WriteLine(c);
    }

    // UpdateCourier: interactive update flow that preserves existing values on empty input.
    private static void UpdateCourier()
    {
        Console.Write("Enter courier Id to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Courier Id is invalid!");

        var existing = s_dalCourier!.Read(id);
        if (existing == null)
        {
            Console.WriteLine("Courier not found.");
            return;
        }

        // Instruction: leave empty to keep current values.
        Console.WriteLine("Leave empty to keep current value (nullable fields: empty => null when creating only).");

        // Each input may be empty to keep existing values (except when creating).
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

        // Active: keep existing unless input provided.
        Console.Write($"Active (y/n) (current: {existing.Active}): ");
        var activeRaw = Console.ReadLine();
        var active = existing.Active;
        if (!string.IsNullOrWhiteSpace(activeRaw))
            //if value provided, parse and validate
            activeRaw = activeRaw.Trim().ToLowerInvariant();
            if (!(activeRaw == "y") && !(activeRaw == "n"))
                throw new DalInvalidInputException("invalid active flag");
            active = activeRaw.Trim().ToLowerInvariant() == "y";

        // DeliveryType enum update.
        Console.Write($"DeliveryType (Car/Motorcycle/Bicycle/OnFoot) (current: {existing.DeliveryType}): ");
        var dtypeRaw = Console.ReadLine();
        var dtype = existing.DeliveryType;
        if (!string.IsNullOrWhiteSpace(dtypeRaw))
        {
            if (!Enum.TryParse<DeliveryTypes>(dtypeRaw, true, out var parsedType)
                || !Enum.IsDefined(typeof(DeliveryTypes), parsedType))
            {
                throw new DalInvalidInputException("invalid delivery type");
            }
            dtype = parsedType;
        }

        // EmploymentStartTime: parse when provided; otherwise, keep current.
        Console.Write($"EmploymentStartTime (yyyy-MM-dd HH:mm) (current: {existing.EmploymentStartTime}) leave empty to keep: ");
        var startRaw = Console.ReadLine();
        var start = existing.EmploymentStartTime;
        if (!string.IsNullOrWhiteSpace(startRaw))
        {
            if (!DateTime.TryParse(startRaw, out start))
                throw new FormatException("EmploymentStartTime is invalid!");
        }

        // PersonalMaxDeliveryDistance: numeric optional field.
        Console.Write($"PersonalMaxDeliveryDistance (km) (current: {existing.PersonalMaxDeliveryDistance?.ToString() ?? \"null\"}) (leave empty to keep): ");
        var pmaxRaw = Console.ReadLine();
        double? pmax = existing.PersonalMaxDeliveryDistance;
        if (!string.IsNullOrWhiteSpace(pmaxRaw))
        {
            if (!double.TryParse(pmaxRaw, out double parsed))
                throw new FormatException("PersonalMaxDeliveryDistance is invalid!");
            pmax = parsed;
        }

        // Create updated record using record 'with' expression and persist update.
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

        s_dalCourier.Update(updated);
        Console.WriteLine("Courier updated.");
    }

    // DeleteCourier: deletes a courier by Id after parsing.
    private static void DeleteCourier()
    {
        Console.Write("Enter courier Id to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Courier Id is invalid!");

        s_dalCourier!.Delete(id);
        Console.WriteLine("Courier deleted.");
    }

    // -------------------------
    // Order submenu
    // -------------------------
    // The Order submenu offers similar CRUD flows as the Courier submenu.
    // Inputs are validated with TryParse where needed and exceptions are thrown for invalid formats.
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
                        s_dalOrder!.DeleteAll();
                        Console.WriteLine("All orders deleted.");
                        break;
                    default:
                        Console.WriteLine("Unknown choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                continue;
            }
        }
    }

    // CreateOrder: prompts the user for all order fields and creates an order in the DAL.
    private static void CreateOrder()
    {
        // OrderType enum parsing (Pizza/Falafel) required.
        Console.Write("OrderType (Pizza/Falafel): ");
        var otRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(otRaw) || !Enum.TryParse<OrderTypes>(otRaw, true, out var otype)
            || !Enum.IsDefined(typeof(OrderTypes), otype))
        {
            throw new DalInvalidInputException("invalid order type");
        }
        // VerbalDescription (required).
        Console.Write("VerbalDescription: ");
        var verbal = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(verbal))
            throw new FormatException("VerbalDescription is required!");

        // FullOrderAccess: optional free-text; auto-generate Guid if empty.
        Console.Write("FullOrderAccess (empty to auto-generate): ");
        var access = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(access)) access = Guid.NewGuid().ToString("N");

        // Latitude and Longitude: required numeric doubles.
        Console.Write("Latitude (double): ");
        var latRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(latRaw) || !double.TryParse(latRaw, out double lat))
            throw new FormatException("Latitude is invalid!");

        Console.Write("Longitude (double): ");
        var lonRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(lonRaw) || !double.TryParse(lonRaw, out double lon))
            throw new FormatException("Longitude is invalid!");

        // Customer details required.
        Console.Write("CustomerFullName: ");
        var cust = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(cust))
            throw new FormatException("CustomerFullName is required!");

        Console.Write("CustomerMobile: ");
        var custPhone = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(custPhone))
            throw new FormatException("CustomerMobile is required!");

        // Volume and Weight: numeric required doubles.
        Console.Write("Volume (double): ");
        var volRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(volRaw) || !double.TryParse(volRaw, out double volume))
            throw new FormatException("Volume is invalid!");

        Console.Write("Weight (double): ");
        var wRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(wRaw) || !double.TryParse(wRaw, out double weight))
            throw new FormatException("Weight is invalid!");

        // Fragile flag required (y/n).
        Console.Write("Fragile (y/n): ");
        var fragRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fragRaw))
            throw new FormatException("Fragile (y/n) is required!");
        bool fragile = fragRaw.Trim().ToLowerInvariant() == "y";

        // Dimensions.
        Console.Write("Height (double): ");
        var hRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(hRaw) || !double.TryParse(hRaw, out double height))
            throw new FormatException("Height is invalid!");

        Console.Write("Width (double): ");
        var widRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(widRaw) || !double.TryParse(widRaw, out double width))
            throw new FormatException("Width is invalid!");

        // OrderOpenTime: optional; defaults to config clock when empty.
        Console.Write($"OrderOpenTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dalConfig!.Clock}): ");
        var openRaw = Console.ReadLine();
        DateTime open;
        if (string.IsNullOrWhiteSpace(openRaw))
        {
            open = s_dalConfig!.Clock;
        }
        else if (!DateTime.TryParse(openRaw, out open))
        {
            throw new FormatException("OrderOpenTime is invalid!");
        }

        // Create order and persist through DAL. Id passed as 0 to allow DAL to assign Id.
        var order = new Order(0, otype, verbal!, access!, lat, lon, cust!, custPhone!, volume, weight, fragile, height, width, open);
        s_dalOrder!.Create(order);
        Console.WriteLine("Order created (Id assigned by DAL).");
    }

    // ReadOrder: prompt for Id and display order or not-found message.
    private static void ReadOrder()
    {
        Console.Write("Enter order Id: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Order Id is invalid!");

        var o = s_dalOrder!.Read(id);
        Console.WriteLine(o == null ? $"Order Id={id} not found." : o.ToString());
    }

    // ReadAllOrders: list all orders in DAL.
    private static void ReadAllOrders()
    {
        var list = s_dalOrder!.ReadAll();
        Console.WriteLine($"Orders ({list.Count}):");
        foreach (var o in list) Console.WriteLine(o);
    }

    // UpdateOrder: interactive update preserving existing values on empty input.
    private static void UpdateOrder()
    {
        Console.Write("Enter order Id to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Order Id is invalid!");

        var existing = s_dalOrder!.Read(id);
        if (existing == null)
        {
            Console.WriteLine("Order not found.");
            return;
        }

        Console.WriteLine("Leave empty to keep current value (nullable fields: empty => null when creating only).");

        // OrderType: optional — parse if provided.
        Console.Write($"OrderType (current: {existing.OrderType}): ");
        var otRaw = Console.ReadLine();
        var otype = existing.OrderType;
        if (!string.IsNullOrWhiteSpace(otRaw))
        {
            if (!Enum.TryParse<OrderTypes>(otRaw, true, out var parsedOt) 
                || !Enum.IsDefined(typeof(OrderTypes), otype))
            {
                throw new DalInvalidInputException("invalid order type");
            }
            otype = parsedOt;
        }

        // VerbalDescription: optional string update.
        Console.Write($"VerbalDescription (current: {existing.VerbalDescription}): ");
        var verbalRaw = Console.ReadLine();
        var verbal = string.IsNullOrEmpty(verbalRaw) ? existing.VerbalDescription : verbalRaw;

        // FullOrderAccess: optional update.
        Console.Write($"FullOrderAccess (current: {existing.FullOrderAccess}): ");
        var accessRaw = Console.ReadLine();
        var access = string.IsNullOrWhiteSpace(accessRaw) ? existing.FullOrderAccess : accessRaw;

        // Latitude and Longitude: numeric optional updates.
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

        // Customer details optional updates.
        Console.Write($"CustomerFullName (current: {existing.CustomerFullName}): ");
        var custRaw = Console.ReadLine();
        var cust = string.IsNullOrEmpty(custRaw) ? existing.CustomerFullName : custRaw;

        Console.Write($"CustomerMobile (current: {existing.CustomerMobile}): ");
        var custPhoneRaw = Console.ReadLine();
        var custPhone = string.IsNullOrEmpty(custPhoneRaw) ? existing.CustomerMobile : custPhoneRaw;

        // Volume and Weight optional numerical updates.
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

        // Fragile flag.
        Console.Write($"Fragile (y/n) (current: {existing.Fragile}): ");
        var fragRaw = Console.ReadLine();
        var fragile = existing.Fragile;
        if (!string.IsNullOrWhiteSpace(fragRaw))
        {
            fragile = fragRaw.Trim().ToLowerInvariant() == "y";
        }

        // Dimensions optional updates.
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

        // OrderOpenTime optional update.
        Console.Write($"OrderOpenTime (yyyy-MM-dd HH:mm) (current: {existing.OrderOpenTime}): ");
        var openRaw = Console.ReadLine();
        var open = existing.OrderOpenTime;
        if (!string.IsNullOrWhiteSpace(openRaw))
        {
            if (!DateTime.TryParse(openRaw, out open))
                throw new FormatException("OrderOpenTime is invalid!");
        }

        // Persist updates.
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

        s_dalOrder.Update(updated);
        Console.WriteLine("Order updated.");
    }

    // DeleteOrder: prompts for Id and deletes the order.
    private static void DeleteOrder()
    {
        Console.Write("Enter order Id to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Order Id is invalid!");

        s_dalOrder!.Delete(id);
        Console.WriteLine("Order deleted.");
    }

    // -------------------------
    // Delivery submenu
    // -------------------------
    // Provides create/read/update/delete and list operations for Delivery entities.
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
                        s_dalDelivery!.DeleteAll();
                        Console.WriteLine("All deliveries deleted.");
                        break;
                    default:
                        Console.WriteLine("Unknown choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                continue;
            }
        }
    }

    // CreateDelivery: reads required fields, handles optional fields and dependent fields (end time required if end type provided).
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
        if (string.IsNullOrWhiteSpace(dtRaw) || !Enum.TryParse<DeliveryTypes>(dtRaw, true, out var dtype)
            || !Enum.IsDefined(typeof(DeliveryTypes), dtype))
        {
            throw new DalInvalidInputException("invalid delivery type");
        
        }

        Console.Write($"DeliveryStartTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dalConfig!.Clock}): ");
        var stRaw = Console.ReadLine();
        DateTime start;
        if (string.IsNullOrWhiteSpace(stRaw)) start = s_dalConfig!.Clock;
        else if (!DateTime.TryParse(stRaw, out start)) throw new FormatException("DeliveryStartTime is invalid!");

        // ActualDistance: optional double.
        Console.Write("ActualDistance (km) (empty => null): ");
        var adRaw = Console.ReadLine();
        double? actual = null;
        if (!string.IsNullOrWhiteSpace(adRaw))
        {
            if (!double.TryParse(adRaw, out double parsed)) throw new FormatException("ActualDistance is invalid!");
            actual = parsed;
        }

        // DeliveryEndType: optional enum; if provided, DeliveryEndTime becomes required.
        Console.Write("DeliveryEndType (Delivered/Failed/Cancelled/CustomerRefused/RecipientNotFound) (empty => null): ");
        var etRaw = Console.ReadLine();
        DeliveryEndTypes? endType = null;
        if (!string.IsNullOrWhiteSpace(etRaw))
        {
            if (!Enum.TryParse<DeliveryEndTypes>(etRaw, true, out var parsedEt)
                || !Enum.IsDefined(typeof(DeliveryEndTypes), parsedEt))
            {
                throw new DalInvalidInputException("invalid delivery end type");
            }
            endType = parsedEt;
        }

        DateTime? endTime = null;
        if (endType != null)
        {
            // When an end type is provided, an end time must be provided too.
            Console.Write("DeliveryEndTime (yyyy-MM-dd HH:mm) (required since end type provided): ");
            var eRaw = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(eRaw) || !DateTime.TryParse(eRaw, out DateTime parsedEnd))
                throw new FormatException("DeliveryEndTime is invalid or missing!");
            endTime = parsedEnd;
        }

        // Create delivery and persist via DAL (Id=0 to let DAL assign Id).
        var d = new Delivery(0, orderId, courierId, dtype, start, actual, endType, endTime);
        s_dalDelivery!.Create(d);
        Console.WriteLine("Delivery created.");
    }

    // ReadDelivery: read by Id and print to console.
    private static void ReadDelivery()
    {
        Console.Write("Enter delivery Id: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Delivery Id is invalid!");

        var d = s_dalDelivery!.Read(id);
        Console.WriteLine(d == null ? $"Delivery Id={id} not found." : d.ToString());
    }

    // ReadAllDeliveries: list all deliveries from DAL.
    private static void ReadAllDeliveries()
    {
        var list = s_dalDelivery!.ReadAll();
        Console.WriteLine($"Deliveries ({list.Count}):");
        foreach (var d in list) Console.WriteLine(d);
    }

    // UpdateDelivery: interactive update preserving existing values; validates numeric and enum fields.
    private static void UpdateDelivery()
    {
        Console.Write("Enter delivery Id to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Delivery Id is invalid!");

        var existing = s_dalDelivery!.Read(id);
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
            if (!Enum.TryParse<DeliveryTypes>(dtRaw, true, out var parsedDt)
                || !Enum.IsDefined(typeof(DeliveryTypes), parsedDt))
            {
                throw new DalInvalidInputException("invalid delivery type");
            }
                dtype = parsedDt;
        }

        Console.Write($"DeliveryStartTime (current: {existing.DeliveryStartTime}) leave empty to keep: ");
        var stRaw2 = Console.ReadLine();
        var start = existing.DeliveryStartTime;
        if (!string.IsNullOrWhiteSpace(stRaw2))
        {
            if (!DateTime.TryParse(stRaw2, out start)) throw new FormatException("DeliveryStartTime is invalid!");
        }

        Console.Write($"ActualDistance (current: {existing.ActualDistance?.ToString() ?? \"null\"}) leave empty to keep: ");
        var aRaw = Console.ReadLine();
        double? actual = existing.ActualDistance;
        if (!string.IsNullOrWhiteSpace(aRaw))
        {
            if (!double.TryParse(aRaw, out double parsedA)) throw new FormatException("ActualDistance is invalid!");
            actual = parsedA;
        }

        Console.Write($"DeliveryEndType (current: {existing.DeliveryEndType?.ToString() ?? \"null\"}) (empty to keep): ");
        var etRaw2 = Console.ReadLine();
        DeliveryEndTypes? endType = existing.DeliveryEndType;
        if (!string.IsNullOrWhiteSpace(etRaw2))
        {
            if (!Enum.TryParse<DeliveryEndTypes>(etRaw2, true, out var parsedEt)
                || !Enum.IsDefined(typeof(DeliveryEndTypes), parsedEt))
            {
                throw new DalInvalidInputException("invalid delivery end type");
            }
            endType = parsedEt;
        }

        Console.Write($"DeliveryEndTime (current: {existing.DeliveryEndTime?.ToString() ?? \"null\"}) (empty to keep): ");
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

        s_dalDelivery.Update(updated);
        Console.WriteLine("Delivery updated.");
    }

    // DeleteDelivery: delete a delivery by Id after parsing.
    private static void DeleteDelivery()
    {
        Console.Write("Enter delivery Id to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new FormatException("Delivery Id is invalid!");

        s_dalDelivery!.Delete(id);
        Console.WriteLine("Delivery deleted.");
    }

    // -------------------------
    // Configuration menu
    // -------------------------
    // Allows manipulating the Config clock (minute/hour/day/month), resetting, and setting/displaying config variables.
    private static void ConfigMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Configuration Menu ===");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Increment time by one minute");
            Console.WriteLine("2. Increment time by one hour");
            Console.WriteLine("3. Increment time by one day");
            Console.WriteLine("4. Increment time by one month");
            Console.WriteLine("5. Display current time");
            Console.WriteLine("6. Set configuration variables");
            Console.WriteLine("7. Display configuration variables");
            Console.WriteLine("8. Reset configuration");
            Console.Write("Choose option: ");

            var raw = Console.ReadLine();
            if (!int.TryParse(raw, out int choice))
            {
                Console.WriteLine("invalid number.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 0: return;
                    case 1:
                        // Increment clock by one minute using Config API.
                        s_dalConfig!.Clock = s_dalConfig.Clock.AddMinutes(1);
                        Console.WriteLine($"Clock: {s_dalConfig.Clock}");
                        break;
                    case 2:
                        // Increment clock by one hour.
                        s_dalConfig!.Clock = s_dalConfig.Clock.AddHours(1);
                        Console.WriteLine($"Clock: {s_dalConfig.Clock}");
                        break;
                    case 3:
                        // Increment clock by one day.
                        s_dalConfig!.Clock = s_dalConfig.Clock.AddDays(1);
                        Console.WriteLine($"Clock: {s_dalConfig.Clock}");
                        break;
                    case 4:
                        // Increment clock by one month.
                        s_dalConfig!.Clock = s_dalConfig.Clock.AddMonths(1);
                        Console.WriteLine($"Clock: {s_dalConfig.Clock}");
                        break;
                    case 5:
                        // Display current configured clock.
                        Console.WriteLine($"Clock: {s_dalConfig!.Clock}");
                        break;
                    case 6:
                        // Enter submenu to set config variables.
                        SetConfigMenu();
                        break;
                    case 7:
                        // Enter submenu to display config variables.
                        DisplayConfigMenu();
                        break;
                    case 8:
                        // Reset configuration to defaults using Config API.
                        s_dalConfig!.Reset();
                        Console.WriteLine("Configuration reset.");
                        break;
                    default:
                        Console.WriteLine("unknown option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                continue;
            }
        }
    }

    // SetConfigMenu: interactive submenu allowing setting of individual config variables.
    // Input parsing uses TryParse where suitable; invalid inputs throw DalInvalidInputException.
    private static void SetConfigMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Set Configuration Variables ===");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Set AdminId");
            Console.WriteLine("2. Set AdminPassword");
            Console.WriteLine("3. Set AvgCarSpeedKmh");
            Console.WriteLine("4. Set AvgMotorcycleSpeedKmh");
            Console.WriteLine("5. Set AvgBicycleSpeedKmh");
            Console.WriteLine("6. Set AvgWalkingSpeedKmh");
            Console.WriteLine("7. Set MaxGeneralDeliveryDistanceKm");
            Console.WriteLine("8. Set MaxDeliveryTimeSpan");
            Console.WriteLine("9. Set RiskRange");
            Console.WriteLine("10. Set InactivityRange");
            Console.WriteLine("11. Set CompanyFullAddress");
            Console.WriteLine("12. Set Latitude");
            Console.WriteLine("13. Set Longitude");
            Console.Write("Choose option: ");

            var raw = Console.ReadLine();
            if (!int.TryParse(raw, out int choice))
            {
                Console.WriteLine("invalid number.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 0: return;
                    case 1:
                        Console.Write("Enter AdminId (int): ");
                        if (!int.TryParse(Console.ReadLine(), out int adminId))
                            throw new DalInvalidInputException("invalid admin id");
                        s_dalConfig!.AdminId = adminId;
                        Console.WriteLine("AdminId set.");
                        break;
                    case 2:
                        Console.Write("Enter AdminPassword: ");
                        var adminPwd = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(adminPwd))
                            throw new DalInvalidInputException("admin password cannot be empty");
                        s_dalConfig!.AdminPassword = adminPwd;
                        Console.WriteLine("AdminPassword set.");
                        break;
                    case 3:
                        Console.Write("Enter AvgCarSpeedKmh (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double carSpeed))
                            throw new DalInvalidInputException("invalid car speed");
                        s_dalConfig!.AvgCarSpeedKmh = carSpeed;
                        Console.WriteLine("AvgCarSpeedKmh set.");
                        break;
                    case 4:
                        Console.Write("Enter AvgMotorcycleSpeedKmh (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double motoSpeed))
                            throw new DalInvalidInputException("invalid motorcycle speed");
                        s_dalConfig!.AvgMotorcycleSpeedKmh = motoSpeed;
                        Console.WriteLine("AvgMotorcycleSpeedKmh set.");
                        break;
                    case 5:
                        Console.Write("Enter AvgBicycleSpeedKmh (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double bikeSpeed))
                            throw new DalInvalidInputException("invalid bicycle speed");
                        s_dalConfig!.AvgBicycleSpeedKmh = bikeSpeed;
                        Console.WriteLine("AvgBicycleSpeedKmh set.");
                        break;
                    case 6:
                        Console.Write("Enter AvgWalkingSpeedKmh (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double walkSpeed))
                            throw new DalInvalidInputException("invalid walking speed");
                        s_dalConfig!.AvgWalkingSpeedKmh = walkSpeed;
                        Console.WriteLine("AvgWalkingSpeedKmh set.");
                        break;
                    case 7:
                        Console.Write("Enter MaxGeneralDeliveryDistanceKm (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double maxDist))
                            throw new DalInvalidInputException("invalid max delivery distance");
                        s_dalConfig!.MaxGeneralDeliveryDistanceKm = maxDist;
                        Console.WriteLine("MaxGeneralDeliveryDistanceKm set.");
                        break;
                    case 8:
                        Console.Write("Enter MaxDeliveryTimeSpan (format: d.HH:mm:ss): ");
                        if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan maxTime))
                            throw new DalInvalidInputException("invalid max delivery time span");
                        s_dalConfig!.MaxDeliveryTimeSpan = maxTime;
                        Console.WriteLine("MaxDeliveryTimeSpan set.");
                        break;
                    case 9:
                        Console.Write("Enter RiskRange (format: d.HH:mm:ss): ");
                        if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan riskRange))
                            throw new DalInvalidInputException("invalid risk range");
                        s_dalConfig!.RiskRange = riskRange;
                        Console.WriteLine("RiskRange set.");
                        break;
                    case 10:
                        Console.Write("Enter InactivityRange (format: d.HH:mm:ss): ");
                        if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan inactRange))
                            throw new DalInvalidInputException("invalid inactivity range");
                        s_dalConfig!.InactivityRange = inactRange;
                        Console.WriteLine("InactivityRange set.");
                        break;
                    case 11:
                        Console.Write("Enter CompanyFullAddress: ");
                        var address = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(address))
                            throw new DalInvalidInputException("company address cannot be empty");
                        s_dalConfig!.CompanyFullAddress = address;
                        Console.WriteLine("CompanyFullAddress set.");
                        break;
                    case 12:
                        Console.Write("Enter Latitude (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double lat))
                            throw new DalInvalidInputException("invalid latitude");
                        s_dalConfig!.Latitude = lat;
                        Console.WriteLine("Latitude set.");
                        break;
                    case 13:
                        Console.Write("Enter Longitude (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double lon))
                            throw new DalInvalidInputException("invalid longitude");
                        s_dalConfig!.Longitude = lon;
                        Console.WriteLine("Longitude set.");
                        break;
                    default:
                        Console.WriteLine("unknown option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                // If any DAL-specific or parsing exception occurs, print message and stay in set-config menu.
                Console.WriteLine(ex.Message);
                continue;
            }
        }
    }

    // DisplayConfigMenu: allows viewing individual config variables or all at once.
    private static void DisplayConfigMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Display Configuration Variables ===");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Display AdminId");
            Console.WriteLine("2. Display AdminPassword");
            Console.WriteLine("3. Display AvgCarSpeedKmh");
            Console.WriteLine("4. Display AvgMotorcycleSpeedKmh");
            Console.WriteLine("5. Display AvgBicycleSpeedKmh");
            Console.WriteLine("6. Display AvgWalkingSpeedKmh");
            Console.WriteLine("7. Display MaxGeneralDeliveryDistanceKm");
            Console.WriteLine("8. Display MaxDeliveryTimeSpan");
            Console.WriteLine("9. Display RiskRange");
            Console.WriteLine("10. Display InactivityRange");
            Console.WriteLine("11. Display CompanyFullAddress");
            Console.WriteLine("12. Display Latitude");
            Console.WriteLine("13. Display Longitude");
            Console.WriteLine("14. Display All Variables");
            Console.Write("Choose option: ");

            var raw = Console.ReadLine();
            if (!int.TryParse(raw, out int choice))
            {
                Console.WriteLine("invalid number.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 0: return;
                    case 1:
                        Console.WriteLine($"AdminId: {s_dalConfig!.AdminId}");
                        break;
                    case 2:
                        Console.WriteLine($"AdminPassword: {s_dalConfig!.AdminPassword}");
                        break;
                    case 3:
                        Console.WriteLine($"AvgCarSpeedKmh: {s_dalConfig!.AvgCarSpeedKmh}");
                        break;
                    case 4:
                        Console.WriteLine($"AvgMotorcycleSpeedKmh: {s_dalConfig!.AvgMotorcycleSpeedKmh}");
                        break;
                    case 5:
                        Console.WriteLine($"AvgBicycleSpeedKmh: {s_dalConfig!.AvgBicycleSpeedKmh}");
                        break;
                    case 6:
                        Console.WriteLine($"AvgWalkingSpeedKmh: {s_dalConfig!.AvgWalkingSpeedKmh}");
                        break;
                    case 7:
                        Console.WriteLine($"MaxGeneralDeliveryDistanceKm: {s_dalConfig!.MaxGeneralDeliveryDistanceKm}");
                        break;
                    case 8:
                        Console.WriteLine($"MaxDeliveryTimeSpan: {s_dalConfig!.MaxDeliveryTimeSpan}");
                        break;
                    case 9:
                        Console.WriteLine($"RiskRange: {s_dalConfig!.RiskRange}");
                        break;
                    case 10:
                        Console.WriteLine($"InactivityRange: {s_dalConfig!.InactivityRange}");
                        break;
                    case 11:
                        Console.WriteLine($"CompanyFullAddress: {s_dalConfig!.CompanyFullAddress}");
                        break;
                    case 12:
                        Console.WriteLine($"Latitude: {s_dalConfig!.Latitude}");
                        break;
                    case 13:
                        Console.WriteLine($"Longitude: {s_dalConfig!.Longitude}");
                        break;
                    case 14:
                        // Display all config variables with labels for clarity.
                        Console.WriteLine();
                        Console.WriteLine("=== All Configuration Variables ===");
                        Console.WriteLine($"AdminId: {s_dalConfig!.AdminId}");
                        Console.WriteLine($"AdminPassword: {s_dalConfig!.AdminPassword}");
                        Console.WriteLine($"AvgCarSpeedKmh: {s_dalConfig!.AvgCarSpeedKmh}");
                        Console.WriteLine($"AvgMotorcycleSpeedKmh: {s_dalConfig!.AvgMotorcycleSpeedKmh}");
                        Console.WriteLine($"AvgBicycleSpeedKmh: {s_dalConfig!.AvgBicycleSpeedKmh}");
                        Console.WriteLine($"AvgWalkingSpeedKmh: {s_dalConfig!.AvgWalkingSpeedKmh}");
                        Console.WriteLine($"MaxGeneralDeliveryDistanceKm: {s_dalConfig!.MaxGeneralDeliveryDistanceKm}");
                        Console.WriteLine($"MaxDeliveryTimeSpan: {s_dalConfig!.MaxDeliveryTimeSpan}");
                        Console.WriteLine($"RiskRange: {s_dalConfig!.RiskRange}");
                        Console.WriteLine($"InactivityRange: {s_dalConfig!.InactivityRange}");
                        Console.WriteLine($"CompanyFullAddress: {s_dalConfig!.CompanyFullAddress}");
                        Console.WriteLine($"Latitude: {s_dalConfig!.Latitude}");
                        Console.WriteLine($"Longitude: {s_dalConfig!.Longitude}");
                        break;
                    default:
                        Console.WriteLine("unknown option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                continue;
            }
        }
    }

    // -------------------------
    // Utilities
    // -------------------------
    // DisplayAllData: attempts to read and print all Couriers, Orders and Deliveries.
    private static void DisplayAllData()
    {
        try
        {
            var couriers = s_dalCourier!.ReadAll();
            var orders = s_dalOrder!.ReadAll();
            var deliveries = s_dalDelivery!.ReadAll();

            Console.WriteLine($"\nCouriers ({couriers.Count}):");
            foreach (var c in couriers) Console.WriteLine(c);

            Console.WriteLine($"\nOrders ({orders.Count}):");
            foreach (var o in orders) Console.WriteLine(o);

            Console.WriteLine($"\nDeliveries ({deliveries.Count}):");
            foreach (var d in deliveries) Console.WriteLine(d);
        }
        catch (Exception ex)
        {
            // Catch-all to avoid crashing the UI if any DAL read fails.
            Console.WriteLine(ex.Message);
        }
    }

    // ResetAllDataAndConfig: clears all entities from DAL and resets configuration using Config API.
    private static void ResetAllDataAndConfig()
    {
        try
        {
            s_dalCourier!.DeleteAll();
            s_dalOrder!.DeleteAll();
            s_dalDelivery!.DeleteAll();
            s_dalConfig!.Reset();
            Console.WriteLine("Reset complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
