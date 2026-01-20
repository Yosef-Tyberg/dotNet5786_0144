﻿﻿﻿using Dal;
using DalApi;
using DO;

namespace DalTest;

/*stage 1: this class was generated using chatgpt on the following prompts (continued after initialization prompts):
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
 * 
 * stage 2:
 * 1)
 * I would like to modify the exceptions used. instead of calling any of the currently used exceptions, 
 * only my exceptions defined in Exceptions.cs should be used.
 * 2)
 * I have added an a Read method which accepts a function well as an option to pass a filtering method to readall methods. 
 * (pasted the new methods). Please add options to each of the Entity menus to make use of these new possibilities (directly after the options
 * for the current versions of these methods. for read use the example method of finding the entity with the highest Id and for readall allow
 * the user to filter by an appropriate enum field.
 */ 


/// <summary>
/// Main program class for testing the Data Access Layer.
/// </summary>
internal static class Program
{
    // The DAL instance used by the test program. It is created once and reused.
    //static readonly IDal s_dal = new DalList(); // stage2
    //static readonly IDal s_dal = new DalXml(); //stage 3 
    static readonly IDal s_dal = Factory.Get; // stage4
    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    // Main entry point: repeatedly show main menu and handle user's top-level choice.
    private static void Main()
    {
        while (true)
        {
            PrintMainMenu();
            Console.Write("Choose option: ");
            var mainRaw = Console.ReadLine();
            if (!int.TryParse(mainRaw, out int mainChoice))
            {
                // If parsing of the main menu choice fails, inform the user and redisplay menu.
                Console.WriteLine("invalid number; try again.");
                continue;
            }

            // Switch based on main menu choice.
            switch (mainChoice)
            {
                case 0:
                    // Exit program gracefully.
                    Console.WriteLine("Exiting.");
                    return;
                case 1:
                    // Enter courier submenu.
                    CourierMenu();
                    break;
                case 2:
                    // Enter order submenu.
                    OrderMenu();
                    break;
                case 3:
                    // Enter delivery submenu.
                    DeliveryMenu();
                    break;
                case 4:
                    // Initialize (populate) the data source using an Initialization helper.
                    try
                    {
                        Initialization.Do();
                        Console.WriteLine("Initialization complete.");
                    }
                    catch (Exception ex)
                    {
                        // Show initialization error message.
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case 5:
                    // Display all stored data (couriers, orders, deliveries).
                    DisplayAllData();
                    break;
                case 6:
                    // Configuration menu for manipulating app configuration and simulated clock.
                    ConfigMenu();
                    break;
                case 7:
                    // Reset all data and configuration to defaults.
                    ResetAllDataAndConfig();
                    break;
                default:
                    Console.WriteLine("unknown option.");
                    break;
            }
        }
    }

    /// <summary>
    /// Prints the main menu options to the console.
    /// </summary>
    // Print the top-level main menu options.
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
    /// <summary>
    /// Displays the courier management menu and handles user input.
    /// </summary>
    // This method shows the courier submenu and routes user choices to specific courier operations.
    private static void CourierMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Courier Menu ===");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Create courier");
            Console.WriteLine("2. Read courier by Id");
            Console.WriteLine("3. Read all couriers");
            Console.WriteLine("4. Read courier by function (currently: courier with highest Id)");//stage2
            Console.WriteLine("5. Read all couriers filtered by DeliveryType");//stage2
            Console.WriteLine("6. Update courier");
            Console.WriteLine("7. Delete courier by Id");
            Console.WriteLine("8. Delete all couriers");
            Console.Write("Choose option: ");

            var raw = Console.ReadLine();
            if (!int.TryParse(raw, out int choice))
            {
                // If user input cannot be parsed to an integer, show error and repeat menu.
                Console.WriteLine("invalid number.");
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
                    case 4: ReadCourierByFunction(); break;
                    case 5: ReadAllCouriersByDeliveryType(); break;
                    case 6: UpdateCourier(); break;
                    case 7: DeleteCourier(); break;
                    case 8:
                        // Delete all couriers in the DAL. Rely on DAL implementation for internal behavior.
                        s_dal.Courier!.DeleteAll();
                        Console.WriteLine("All couriers deleted.");
                        break;
                    default:
                        Console.WriteLine("unknown choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                // print exception message and stay in current menu
                Console.WriteLine(ex.Message);
                continue;
            }
        }
    }


    /// <summary>
    /// Prompts user for input to create a new courier.
    /// </summary>
    // Create a new courier by prompting user for all required fields.
    // Input is validated using TryParse / string checks and exceptions are thrown for invalid inputs.
    private static void CreateCourier()
    {
        // parsing & validation -> DalInvalidInputException
        Console.Write("Id (int): ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid courier id");

        Console.Write("FullName: ");
        var fullName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DalInvalidInputException("invalid full name");

        Console.Write("MobilePhone: ");
        var phone = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(phone))
            throw new DalInvalidInputException("invalid mobile phone");

        Console.Write("Email: ");
        var email = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(email))
            throw new DalInvalidInputException("invalid email");

        Console.Write("Password: ");
        var password = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(password))
            throw new DalInvalidInputException("invalid password");

        Console.Write("Active (y/n): ");
        var activeRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(activeRaw))
            throw new DalInvalidInputException("invalid active flag");
        activeRaw = activeRaw.Trim().ToLowerInvariant();
        if (!(activeRaw == "y") && !(activeRaw == "n"))
            throw new DalInvalidInputException("invalid active flag");
        // Interpret 'y' (case-insensitive) as true
        bool active = activeRaw == "y";

        Console.Write("DeliveryType (Car/Motorcycle/Bicycle/OnFoot): ");
        var dtypeRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(dtypeRaw) || !Enum.TryParse<DeliveryTypes>(dtypeRaw, true, out var dtype)
            || !Enum.IsDefined(typeof(DeliveryTypes), dtype))
        {
            throw new DalInvalidInputException("invalid delivery type");
        }

        Console.Write($"EmploymentStartTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dal.Config!.Clock}): ");
        var startRaw = Console.ReadLine();
        DateTime start;
        if (string.IsNullOrWhiteSpace(startRaw))
        {
            // If user leaves the field empty, use the current simulated clock from configuration.
            start = s_dal.Config!.Clock;
        }
        else if (!DateTime.TryParse(startRaw, out start))
        {
            throw new DalInvalidInputException("invalid employment start time");
        }

        Console.Write("PersonalMaxDeliveryDistance (km) (empty => null): ");
        var pmaxRaw = Console.ReadLine();
        double? pmax = null;
        if (!string.IsNullOrWhiteSpace(pmaxRaw))
        {
            if (!double.TryParse(pmaxRaw, out double parsedMax))
                throw new DalInvalidInputException("invalid personal max delivery distance");
            pmax = parsedMax;
        }

        // Create Courier record. Note: DAL assigns existence / uniqueness checks and may throw DalAlreadyExistsException.
        var courier = new Courier(id, fullName!, phone!, email!, password!, active, dtype, start, pmax);

        // DON'T pre-check existence; let DAL throw DalAlreadyExistsException if needed
        s_dal.Courier!.Create(courier);
        Console.WriteLine("Courier created.");
    }

    /// <summary>
    /// Prompts user for ID and displays the corresponding courier.
    /// </summary>
    // Read and display a single courier identified by Id.
    private static void ReadCourier()
    {
        Console.Write("Enter courier Id: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid courier id");

        // Read from DAL
        var c = s_dal.Courier!.Read(id);
        if (c is null)
        {
            Console.WriteLine($"courier id={id} not found.");
            return;
        }

        // Display courier information using its ToString implementation.
        Console.WriteLine(c.ToString());
    }

    /// <summary>
    /// Displays all couriers in the system.
    /// </summary>
    // Read and display all couriers stored in the DAL.
    private static void ReadAllCouriers()
    {
        var list = s_dal.Courier!.ReadAll();
        Console.WriteLine($"Couriers ({list.Count()}):");
        foreach (var c in list) Console.WriteLine(c);
    }

    /// <summary>
    /// Read a courier using the DAL Read(Func<T,bool>) overload.
    /// The predicate computes the highest Id internally (by calling DAL.ReadAll()) and returns true
    /// for the courier whose Id equals that max.
    /// </summary>
    private static void ReadCourierByFunction()
    {
        // quick existence check (not a selection) so we can notify the user if there are no items
        if (!s_dal.Courier!.ReadAll().Any())
        {
            Console.WriteLine("no couriers available.");
            return;
        }

        // predicate that performs the necessary computation internally and returns whether the given item matches
        Func<Courier, bool> predicate = courier =>
        {
            // compute the max id inside the predicate (so selection/filtering happens "inside" the function)
            var maxId = s_dal.Courier.ReadAll().Max(x => x!.Id);
            return courier.Id == maxId;
        };

        var c = s_dal.Courier.Read(predicate);
        if (c is null)
        {
            Console.WriteLine("courier not found by function.");
            return;
        }

        Console.WriteLine("Courier selected by function (highest Id):");
        Console.WriteLine(c.ToString());
    }

    /// <summary>
    /// ReadAll couriers filtered by DeliveryType. The predicate is built here and passed to DAL.ReadAll.
    /// </summary>
    private static void ReadAllCouriersByDeliveryType()
    {
        Console.Write("Enter DeliveryType to filter by (Car/Motorcycle/Bicycle/OnFoot): ");
        var raw = Console.ReadLine();

        // parse (case-insensitive)
        if (!Enum.TryParse<DeliveryTypes>(raw.Trim(), true, out var dtype)
            || !Enum.IsDefined(typeof(DeliveryTypes), dtype))
        {
            // Enum.TryParse may succeed for numeric strings; IsDefined prevents undefined integer values.
            throw new DalInvalidInputException("invalid delivery type");
        }

        // build predicate and pass it into ReadAll
        Func<Courier, bool> predicate = c => c.DeliveryType == dtype;

        var list = s_dal.Courier!.ReadAll(predicate).ToList();
        Console.WriteLine($"Couriers (filtered by {dtype}) ({list.Count}):");
        foreach (var c in list) Console.WriteLine(c);
    }

    /// <summary>
    /// Prompts user for ID and updates the corresponding courier.
    /// </summary>
    // Update existing courier - empty inputs keep current values (no change).
    private static void UpdateCourier()
    {
        Console.Write("Enter courier Id to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid courier id");

        var existing = s_dal.Courier!.Read(id);
        if (existing == null)
        {
            Console.WriteLine($"courier id={id} not found.");
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
            //if value provided, parse and validate
            activeRaw = activeRaw.Trim().ToLowerInvariant();
            if (!(activeRaw == "y") && !(activeRaw == "n"))
                throw new DalInvalidInputException("invalid active flag");
            active = activeRaw.Trim().ToLowerInvariant() == "y";
        }

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

        Console.Write($"EmploymentStartTime (yyyy-MM-dd HH:mm) (current: {existing.EmploymentStartTime}) leave empty to keep: ");
        var startRaw = Console.ReadLine();
        var start = existing.EmploymentStartTime;
        if (!string.IsNullOrWhiteSpace(startRaw))
        {
            if (!DateTime.TryParse(startRaw, out start))
                throw new DalInvalidInputException("invalid employment start time");
        }

        Console.Write($"PersonalMaxDeliveryDistance (km) (current: {existing.PersonalMaxDeliveryDistance?.ToString() ?? "null"}) (leave empty to keep, 'null' to clear): ");
        var pmaxRaw = Console.ReadLine();
        double? pmax = existing.PersonalMaxDeliveryDistance;
        if (!string.IsNullOrWhiteSpace(pmaxRaw))
        {
            if (pmaxRaw.Trim().ToLowerInvariant() == "null") pmax = null;
            else if (!double.TryParse(pmaxRaw, out double parsed))
                throw new DalInvalidInputException("invalid personal max delivery distance");
            else pmax = parsed;
        }

        // Create an updated copy using record 'with' expression and write it back to DAL.
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

    /// <summary>
    /// Prompts user for ID and deletes the corresponding courier.
    /// </summary>
    // Delete a courier identified by Id. DAL is expected to throw if the id does not exist.
    private static void DeleteCourier()
    {
        Console.Write("Enter courier Id to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid courier id");

        // Let DAL throw if not exists (DalDoesNotExistException)
        s_dal.Courier!.Delete(id);
        Console.WriteLine("Courier deleted.");
    }

    // -------------------------
    // Order submenu
    // -------------------------
    /// <summary>
    /// Displays the order management menu and handles user input.
    /// </summary>
    // Shows order submenu and routes user choices to order-specific operations.
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
            // new options placed immediately after Read / ReadAll
            Console.WriteLine("4. Read order by function (example: order with highest Id)");
            Console.WriteLine("5. Read all orders filtered by OrderType");
            Console.WriteLine("6. Update order (all fields)");
            Console.WriteLine("7. Delete order by Id");
            Console.WriteLine("8. Delete all orders");
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
                    case 1: CreateOrder(); break;
                    case 2: ReadOrder(); break;
                    case 3: ReadAllOrders(); break;
                    case 4: ReadOrderByFunction(); break; //stage2
                    case 5: ReadAllOrdersByOrderType(); break; //stage2
                    case 6: UpdateOrder(); break;
                    case 7: DeleteOrder(); break;
                    case 8:
                        s_dal.Order!.DeleteAll();
                        Console.WriteLine("All orders deleted.");
                        break;
                    default:
                        Console.WriteLine("unknown choice.");
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
   


    /// <summary>
    /// Prompts user for input to create a new order.
    /// </summary>
    // Create a new order. Empty FullOrderAccess auto-generates a GUID string.
    private static void CreateOrder()
    {
        Console.Write("OrderType (Pizza/Falafel): ");
        var otRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(otRaw) || !Enum.TryParse<OrderTypes>(otRaw, true, out var otype)
            || !Enum.IsDefined(typeof(OrderTypes), otype))
        {
            throw new DalInvalidInputException("invalid order type");
        }

        Console.Write("VerbalDescription: ");
        var verbal = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(verbal))
            throw new DalInvalidInputException("invalid verbal description");

        Console.Write("FullOrderAddress: ");
        var address = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(address))
            throw new DalInvalidInputException("invalid full order address");

        Console.Write("Latitude (double): ");
        var latRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(latRaw) || !double.TryParse(latRaw, out double lat))
            throw new DalInvalidInputException("invalid latitude");

        Console.Write("Longitude (double): ");
        var lonRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(lonRaw) || !double.TryParse(lonRaw, out double lon))
            throw new DalInvalidInputException("invalid longitude");

        Console.Write("CustomerFullName: ");
        var cust = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(cust))
            throw new DalInvalidInputException("invalid customer full name");

        Console.Write("CustomerMobile: ");
        var custPhone = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(custPhone))
            throw new DalInvalidInputException("invalid customer mobile");

        Console.Write("Volume (double): ");
        var volRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(volRaw) || !double.TryParse(volRaw, out double volume))
            throw new DalInvalidInputException("invalid volume");

        Console.Write("Weight (double): ");
        var wRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(wRaw) || !double.TryParse(wRaw, out double weight))
            throw new DalInvalidInputException("invalid weight");

        Console.Write("Fragile (y/n): ");
        var fragRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fragRaw))
            throw new DalInvalidInputException("invalid fragile flag");
        bool fragile = fragRaw.Trim().ToLowerInvariant() == "y";

        Console.Write("Height (double): ");
        var hRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(hRaw) || !double.TryParse(hRaw, out double height))
            throw new DalInvalidInputException("invalid height");

        Console.Write("Width (double): ");
        var widRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(widRaw) || !double.TryParse(widRaw, out double width))
            throw new DalInvalidInputException("invalid width");

        Console.Write($"OrderOpenTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dal.Config!.Clock}): ");
        var openRaw = Console.ReadLine();
        DateTime open;
        if (string.IsNullOrWhiteSpace(openRaw))
        {
            // When omitted, default to current simulated clock.
            open = s_dal.Config!.Clock;
        }
        else if (!DateTime.TryParse(openRaw, out open))
            throw new DalInvalidInputException("invalid order open time");

        var order = new Order(0, otype, verbal!, lat, lon, cust!, custPhone!, volume, weight, fragile, height, width, open, address);
        s_dal.Order!.Create(order);
        Console.WriteLine("Order created (Id assigned by DAL).");
    }

    /// <summary>
    /// Prompts user for ID and displays the corresponding order.
    /// </summary>
    // Read and display a single order by its Id.
    private static void ReadOrder()
    {
        Console.Write("Enter order Id: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid order id");

        var o = s_dal.Order!.Read(id);
        if (o is null)
        {
            Console.WriteLine($"order id={id} not found.");
            return;
        }

        Console.WriteLine(o.ToString());
    }

    /// <summary>
    /// Displays all orders in the system.
    /// </summary>
    // Read and display all orders.
    private static void ReadAllOrders()
    {
        var list = s_dal.Order!.ReadAll();
        Console.WriteLine($"Orders ({list.Count()}):");
        foreach (var o in list) Console.WriteLine(o);
    }

    /// <summary>
    /// Read an order using the DAL Read(Func<T,bool>) overload.
    /// Predicate computes highest Id internally and returns true for the order with that Id.
    /// </summary>
    private static void ReadOrderByFunction()
    {
        if (!s_dal.Order!.ReadAll().Any())
        {
            Console.WriteLine("no orders available.");
            return;
        }

        Func<Order, bool> predicate = order =>
        {
            var maxId = s_dal.Order.ReadAll().Max(x => x!.Id);
            return order.Id == maxId;
        };

        var o = s_dal.Order.Read(predicate);
        if (o is null)
        {
            Console.WriteLine("order not found by function.");
            return;
        }

        Console.WriteLine("Order selected by function (highest Id):");
        Console.WriteLine(o.ToString());
    }

    /// <summary>
    /// ReadAll orders filtered by OrderType. Predicate is passed directly into ReadAll.
    /// </summary>
    private static void ReadAllOrdersByOrderType()
    {
        Console.Write("Enter OrderType to filter by (Pizza/Falafel): ");
        var raw = Console.ReadLine();
        if (!Enum.TryParse<OrderTypes>(raw.Trim(), true, out var otype)
            || !Enum.IsDefined(typeof(OrderTypes), otype))
        {
            throw new DalInvalidInputException("invalid order type");
        }

        Func<Order, bool> predicate = o => o.OrderType == otype;

        var list = s_dal.Order!.ReadAll(predicate).ToList();
        Console.WriteLine($"Orders (filtered by {otype}) ({list.Count}):");
        foreach (var o in list) Console.WriteLine(o);
    }

    
    /// <summary>
    /// Prompts user for ID and updates the corresponding order.
    /// </summary>
    // Update an existing order; empty inputs keep current values.
    private static void UpdateOrder()
    {
        Console.Write("Enter order Id to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid order id");

        var existing = s_dal.Order!.Read(id);
        if (existing == null)
        {
            Console.WriteLine($"order id={id} not found.");
            return;
        }

        Console.WriteLine("Leave empty to keep current value (nullable fields: empty => null when creating only).");

        Console.Write($"OrderType (current: {existing.OrderType}): ");
        var otRaw = Console.ReadLine();
        var otype = existing.OrderType;
        if (!string.IsNullOrWhiteSpace(otRaw))
        {
            if (!Enum.TryParse<OrderTypes>(otRaw, true, out var parsedOt) 
                || !Enum.IsDefined(typeof(OrderTypes), parsedOt))
            {
                throw new DalInvalidInputException("invalid order type");
            }
            otype = parsedOt;
        }

        Console.Write($"VerbalDescription (current: {existing.VerbalDescription}): ");
        var verbalRaw = Console.ReadLine();
        var verbal = string.IsNullOrEmpty(verbalRaw) ? existing.VerbalDescription : verbalRaw;

        Console.Write($"Latitude (current: {existing.Latitude}): ");
        var latRaw = Console.ReadLine();
        var lat = existing.Latitude;
        if (!string.IsNullOrWhiteSpace(latRaw))
        {
            if (!double.TryParse(latRaw, out lat))
                throw new DalInvalidInputException("invalid latitude");
        }

        Console.Write($"Longitude (current: {existing.Longitude}): ");
        var lonRaw = Console.ReadLine();
        var lon = existing.Longitude;
        if (!string.IsNullOrWhiteSpace(lonRaw))
        {
            if (!double.TryParse(lonRaw, out lon))
                throw new DalInvalidInputException("invalid longitude");
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
                throw new DalInvalidInputException("invalid volume");
        }

        Console.Write($"Weight (current: {existing.Weight}): ");
        var wRaw = Console.ReadLine();
        var weight = existing.Weight;
        if (!string.IsNullOrWhiteSpace(wRaw))
        {
            if (!double.TryParse(wRaw, out weight))
                throw new DalInvalidInputException("invalid weight");
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
                throw new DalInvalidInputException("invalid height");
        }

        Console.Write($"Width (current: {existing.Width}): ");
        var widRaw = Console.ReadLine();
        var width = existing.Width;
        if (!string.IsNullOrWhiteSpace(widRaw))
        {
            if (!double.TryParse(widRaw, out width))
                throw new DalInvalidInputException("invalid width");
        }

        Console.Write($"FullOrderAddress (current: {existing.FullOrderAddress}): ");
        var addressRaw = Console.ReadLine();
        var address = string.IsNullOrEmpty(addressRaw) ? existing.FullOrderAddress : addressRaw;

        Console.Write($"OrderOpenTime (yyyy-MM-dd HH:mm) (current: {existing.OrderOpenTime}): ");
        var openRaw = Console.ReadLine();
        var open = existing.OrderOpenTime;
        if (!string.IsNullOrWhiteSpace(openRaw))
        {
            if (!DateTime.TryParse(openRaw, out open))
                throw new DalInvalidInputException("invalid order open time");
        }

        var updated = existing with
        {
            OrderType = otype,
            VerbalDescription = verbal,
            Latitude = lat,
            Longitude = lon,
            CustomerFullName = cust,
            CustomerMobile = custPhone,
            Volume = volume,
            Weight = weight,
            Fragile = fragile,
            Height = height,
            Width = width,
            FullOrderAddress = address,
            OrderOpenTime = open
        };

        s_dal.Order.Update(updated);
        Console.WriteLine("Order updated.");
    }

    /// <summary>
    /// Prompts user for ID and deletes the corresponding order.
    /// </summary>
    // Delete an order by Id.
    private static void DeleteOrder()
    {
        Console.Write("Enter order Id to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid order id");

        s_dal.Order!.Delete(id);
        Console.WriteLine("Order deleted.");
    }

    // -------------------------
    // Delivery submenu
    // -------------------------
    /// <summary>
    /// Displays the delivery management menu and handles user input.
    /// </summary>
    // Shows delivery submenu and routes user choices to delivery-specific operations.
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
            Console.WriteLine("4. Read delivery by function (example: delivery with highest Id)");//stage2
            Console.WriteLine("5. Read all deliveries filtered by DeliveryEndType");//stage2
            Console.WriteLine("6. Update delivery (all fields)");
            Console.WriteLine("7. Delete delivery by Id");
            Console.WriteLine("8. Delete all deliveries");
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
                    case 1: CreateDelivery(); break;
                    case 2: ReadDelivery(); break;
                    case 3: ReadAllDeliveries(); break;
                    case 4: ReadDeliveryByFunction(); break; //stage2
                    case 5: ReadAllDeliveriesByEndType(); break; //stage2
                    case 6: UpdateDelivery(); break;
                    case 7: DeleteDelivery(); break;
                    case 8:
                        s_dal.Delivery!.DeleteAll();
                        Console.WriteLine("All deliveries deleted.");
                        break;
                    default:
                        Console.WriteLine("unknown choice.");
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

    /// <summary>
    /// Prompts user for input to create a new delivery.
    /// </summary>
    // Create a delivery record and write it to DAL.
    private static void CreateDelivery()
    {
        Console.Write("OrderId (int): ");
        var oRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(oRaw) || !int.TryParse(oRaw, out int orderId))
            throw new DalInvalidInputException("invalid order id");

        Console.Write("CourierId (int): ");
        var cRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(cRaw) || !int.TryParse(cRaw, out int courierId))
            throw new DalInvalidInputException("invalid courier id");

        Console.Write("DeliveryType (Car/Motorcycle/Bicycle/OnFoot): ");
        var dtRaw = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(dtRaw) || !Enum.TryParse<DeliveryTypes>(dtRaw, true, out var dtype)
            || !Enum.IsDefined(typeof(DeliveryTypes), dtype))
        {
            throw new DalInvalidInputException("invalid delivery type");

        }

        Console.Write($"DeliveryStartTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dal.Config!.Clock}): ");
        var stRaw = Console.ReadLine();
        DateTime start;
        if (string.IsNullOrWhiteSpace(stRaw)) start = s_dal.Config!.Clock;
        else if (!DateTime.TryParse(stRaw, out start)) throw new DalInvalidInputException("invalid delivery start time");

        Console.Write("ActualDistance (km) (empty => null): ");
        var adRaw = Console.ReadLine();
        double? actual = null;
        if (!string.IsNullOrWhiteSpace(adRaw))
        {
            if (!double.TryParse(adRaw, out double parsed)) throw new DalInvalidInputException("invalid actual distance");
            actual = parsed;
        }

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
            Console.Write("DeliveryEndTime (yyyy-MM-dd HH:mm) (required since end type provided): ");
            var eRaw = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(eRaw) || !DateTime.TryParse(eRaw, out DateTime parsedEnd))
                throw new DalInvalidInputException("invalid delivery end time");
            endTime = parsedEnd;
        }

        var d = new Delivery(0, orderId, courierId, dtype, start, actual, endType, endTime);
        s_dal.Delivery!.Create(d);
        Console.WriteLine("Delivery created.");
    }

    /// <summary>
    /// Prompts user for ID and displays the corresponding delivery.
    /// </summary>
    // Read and display a single delivery by Id.
    private static void ReadDelivery()
    {
        Console.Write("Enter delivery Id: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid delivery id");

        var d = s_dal.Delivery!.Read(id);
        if (d is null)
        {
            Console.WriteLine($"delivery id={id} not found.");
            return;
        }

        Console.WriteLine(d.ToString());
    }

    /// <summary>
    /// Displays all deliveries in the system.
    /// </summary>
    // Read and display all deliveries.
    private static void ReadAllDeliveries()
    {
        var list = s_dal.Delivery!.ReadAll();
        Console.WriteLine($"Deliveries ({list.Count()}):");
        foreach (var d in list) Console.WriteLine(d);
    }
    
    /// <summary>
    /// Read a delivery using the DAL Read(Func<T,bool>) overload.
    /// Predicate computes highest Id internally and returns true for the delivery with that Id.
    /// </summary>
    private static void ReadDeliveryByFunction()
    {
        if (!s_dal.Delivery!.ReadAll().Any())
        {
            Console.WriteLine("no deliveries available.");
            return;
        }

        Func<Delivery, bool> predicate = del =>
        {
            var maxId = s_dal.Delivery.ReadAll().Max(x => x!.Id);
            return del.Id == maxId;
        };

        var d = s_dal.Delivery.Read(predicate);
        if (d is null)
        {
            Console.WriteLine("delivery not found by function.");
            return;
        }

        Console.WriteLine("Delivery selected by function (highest Id):");
        Console.WriteLine(d.ToString());
    }

    /// <summary>
    /// ReadAll deliveries filtered by DeliveryEndType. Predicate passed directly into ReadAll.
    /// Note: DeliveryEndType is nullable on the record; the predicate compares nullable to enum value.
    /// </summary>
    private static void ReadAllDeliveriesByEndType()
    {
        Console.Write("Enter DeliveryEndType to filter by (Delivered/Failed/Cancelled/CustomerRefused/RecipientNotFound): ");
        var raw = Console.ReadLine();
        if (!Enum.TryParse<DeliveryEndTypes>(raw.Trim(), true, out var endType)
            || !Enum.IsDefined(typeof(DeliveryEndTypes), endType))
        {
            throw new DalInvalidInputException("invalid delivery end type");
        }

        Func<Delivery, bool> predicate = d => d.DeliveryEndType == endType;

        var list = s_dal.Delivery!.ReadAll(predicate).ToList();
        Console.WriteLine($"Deliveries (filtered by {endType}) ({list.Count}):");
        foreach (var d in list) Console.WriteLine(d);
    }


    /// <summary>
    /// Prompts user for ID and updates the corresponding delivery.
    /// </summary>
    // Update existing delivery. Empty fields keep current values.
    private static void UpdateDelivery()
    {
        Console.Write("Enter delivery Id to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid delivery id");

        var existing = s_dal.Delivery!.Read(id);
        if (existing == null)
        {
            Console.WriteLine($"delivery id={id} not found.");
            return;
        }

        Console.WriteLine("Leave empty to keep current value (nullable fields: empty => null when creating only).");

        Console.Write($"OrderId (current: {existing.OrderId}): ");
        var oRaw = Console.ReadLine();
        var orderId = existing.OrderId;
        if (!string.IsNullOrWhiteSpace(oRaw))
        {
            if (!int.TryParse(oRaw, out orderId)) throw new DalInvalidInputException("invalid order id");
        }

        Console.Write($"CourierId (current: {existing.CourierId}): ");
        var cRaw = Console.ReadLine();
        var courierId = existing.CourierId;
        if (!string.IsNullOrWhiteSpace(cRaw))
        {
            if (!int.TryParse(cRaw, out courierId)) throw new DalInvalidInputException("invalid courier id");
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
            if (!DateTime.TryParse(stRaw2, out start)) throw new DalInvalidInputException("invalid delivery start time");
        }

        Console.Write($"ActualDistance (current: {existing.ActualDistance?.ToString() ?? "null"}) leave empty to keep, 'null' to clear: ");
        var aRaw = Console.ReadLine();
        double? actual = existing.ActualDistance;
        if (!string.IsNullOrWhiteSpace(aRaw))
        {
            if (aRaw.Trim().ToLowerInvariant() == "null") actual = null;
            else if (!double.TryParse(aRaw, out double parsedA)) throw new DalInvalidInputException("invalid actual distance");
            else actual = parsedA;
        }

        Console.Write($"DeliveryEndType (current: {existing.DeliveryEndType?.ToString() ?? "null"}) (empty to keep, 'null' to clear): ");
        var etRaw2 = Console.ReadLine();
        DeliveryEndTypes? endType = existing.DeliveryEndType;
        if (!string.IsNullOrWhiteSpace(etRaw2))
        {
            if (etRaw2.Trim().ToLowerInvariant() == "null") endType = null;
            else if (!Enum.TryParse<DeliveryEndTypes>(etRaw2, true, out var parsedEt)
                || !Enum.IsDefined(typeof(DeliveryEndTypes), parsedEt))
            {
                throw new DalInvalidInputException("invalid delivery end type");
            }
            else endType = parsedEt;
        }

        Console.Write($"DeliveryEndTime (current: {existing.DeliveryEndTime?.ToString() ?? "null"}) (empty to keep, 'null' to clear): ");
        var eRaw2 = Console.ReadLine();
        DateTime? endTime = existing.DeliveryEndTime;
        if (!string.IsNullOrWhiteSpace(eRaw2))
        {
            if (eRaw2.Trim().ToLowerInvariant() == "null") endTime = null;
            else if (!DateTime.TryParse(eRaw2, out DateTime parsedE)) throw new DalInvalidInputException("invalid delivery end time");
            else endTime = parsedE;
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

    /// <summary>
    /// Prompts user for ID and deletes the corresponding delivery.
    /// </summary>
    // Delete a delivery by Id.
    private static void DeleteDelivery()
    {
        Console.Write("Enter delivery Id to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            throw new DalInvalidInputException("invalid delivery id");

        s_dal.Delivery!.Delete(id);
        Console.WriteLine("Delivery deleted.");
    }

    // -------------------------
    // Configuration menu
    // -------------------------
    /// <summary>
    /// Displays the configuration menu and handles user input.
    /// </summary>
    // Configuration and simulated clock control. The menu allows incrementing the clock by minutes/hours/days/months,
    // setting and displaying various configuration variables, and resetting configuration to defaults.
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
                        s_dal.Config!.Clock = s_dal.Config.Clock.AddMinutes(1);
                        Console.WriteLine($"Clock: {s_dal.Config.Clock}");
                        break;
                    case 2:
                        s_dal.Config!.Clock = s_dal.Config.Clock.AddHours(1);
                        Console.WriteLine($"Clock: {s_dal.Config.Clock}");
                        break;
                    case 3:
                        s_dal.Config!.Clock = s_dal.Config.Clock.AddDays(1);
                        Console.WriteLine($"Clock: {s_dal.Config.Clock}");
                        break;
                    case 4:
                        s_dal.Config!.Clock = s_dal.Config.Clock.AddMonths(1);
                        Console.WriteLine($"Clock: {s_dal.Config.Clock}");
                        break;
                    case 5:
                        Console.WriteLine($"Clock: {s_dal.Config!.Clock}");
                        break;
                    case 6:
                        SetConfigMenu();
                        break;
                    case 7:
                        DisplayConfigMenu();
                        break;
                    case 8:
                        s_dal.Config!.Reset();
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

    /// <summary>
    /// Displays the menu for setting configuration variables.
    /// </summary>
    // Set configuration variables submenu. Uses input parsing and validation for each variable.
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
                        s_dal.Config!.AdminId = adminId;
                        Console.WriteLine("AdminId set.");
                        break;
                    case 2:
                        Console.Write("Enter AdminPassword: ");
                        var adminPwd = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(adminPwd))
                            throw new DalInvalidInputException("admin password cannot be empty");
                        s_dal.Config!.AdminPassword = adminPwd;
                        Console.WriteLine("AdminPassword set.");
                        break;
                    case 3:
                        Console.Write("Enter AvgCarSpeedKmh (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double carSpeed))
                            throw new DalInvalidInputException("invalid car speed");
                        s_dal.Config!.AvgCarSpeedKmh = carSpeed;
                        Console.WriteLine("AvgCarSpeedKmh set.");
                        break;
                    case 4:
                        Console.Write("Enter AvgMotorcycleSpeedKmh (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double motoSpeed))
                            throw new DalInvalidInputException("invalid motorcycle speed");
                        s_dal.Config!.AvgMotorcycleSpeedKmh = motoSpeed;
                        Console.WriteLine("AvgMotorcycleSpeedKmh set.");
                        break;
                    case 5:
                        Console.Write("Enter AvgBicycleSpeedKmh (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double bikeSpeed))
                            throw new DalInvalidInputException("invalid bicycle speed");
                        s_dal.Config!.AvgBicycleSpeedKmh = bikeSpeed;
                        Console.WriteLine("AvgBicycleSpeedKmh set.");
                        break;
                    case 6:
                        Console.Write("Enter AvgWalkingSpeedKmh (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double walkSpeed))
                            throw new DalInvalidInputException("invalid walking speed");
                        s_dal.Config!.AvgWalkingSpeedKmh = walkSpeed;
                        Console.WriteLine("AvgWalkingSpeedKmh set.");
                        break;
                    case 7:
                        Console.Write("Enter MaxGeneralDeliveryDistanceKm (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double maxDist))
                            throw new DalInvalidInputException("invalid max delivery distance");
                        s_dal.Config!.MaxGeneralDeliveryDistanceKm = maxDist;
                        Console.WriteLine("MaxGeneralDeliveryDistanceKm set.");
                        break;
                    case 8:
                        Console.Write("Enter MaxDeliveryTimeSpan (format: d.HH:mm:ss): ");
                        if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan maxTime))
                            throw new DalInvalidInputException("invalid max delivery time span");
                        s_dal.Config!.MaxDeliveryTimeSpan = maxTime;
                        Console.WriteLine("MaxDeliveryTimeSpan set.");
                        break;
                    case 9:
                        Console.Write("Enter RiskRange (format: d.HH:mm:ss): ");
                        if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan riskRange))
                            throw new DalInvalidInputException("invalid risk range");
                        s_dal.Config!.RiskRange = riskRange;
                        Console.WriteLine("RiskRange set.");
                        break;
                    case 10:
                        Console.Write("Enter InactivityRange (format: d.HH:mm:ss): ");
                        if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan inactRange))
                            throw new DalInvalidInputException("invalid inactivity range");
                        s_dal.Config!.InactivityRange = inactRange;
                        Console.WriteLine("InactivityRange set.");
                        break;
                    case 11:
                        Console.Write("Enter CompanyFullAddress: ");
                        var address = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(address))
                            throw new DalInvalidInputException("company address cannot be empty");
                        s_dal.Config!.CompanyFullAddress = address;
                        Console.WriteLine("CompanyFullAddress set.");
                        break;
                    case 12:
                        Console.Write("Enter Latitude (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double lat))
                            throw new DalInvalidInputException("invalid latitude");
                        s_dal.Config!.Latitude = lat;
                        Console.WriteLine("Latitude set.");
                        break;
                    case 13:
                        Console.Write("Enter Longitude (double): ");
                        if (!double.TryParse(Console.ReadLine(), out double lon))
                            throw new DalInvalidInputException("invalid longitude");
                        s_dal.Config!.Longitude = lon;
                        Console.WriteLine("Longitude set.");
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

    /// <summary>
    /// Displays the menu for viewing configuration variables.
    /// </summary>
    // Display configuration variables individually or all at once.
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
                        Console.WriteLine($"AdminId: {s_dal.Config!.AdminId}");
                        break;
                    case 2:
                        Console.WriteLine($"AdminPassword: {s_dal.Config!.AdminPassword}");
                        break;
                    case 3:
                        Console.WriteLine($"AvgCarSpeedKmh: {s_dal.Config!.AvgCarSpeedKmh}");
                        break;
                    case 4:
                        Console.WriteLine($"AvgMotorcycleSpeedKmh: {s_dal.Config!.AvgMotorcycleSpeedKmh}");
                        break;
                    case 5:
                        Console.WriteLine($"AvgBicycleSpeedKmh: {s_dal.Config!.AvgBicycleSpeedKmh}");
                        break;
                    case 6:
                        Console.WriteLine($"AvgWalkingSpeedKmh: {s_dal.Config!.AvgWalkingSpeedKmh}");
                        break;
                    case 7:
                        Console.WriteLine($"MaxGeneralDeliveryDistanceKm: {s_dal.Config!.MaxGeneralDeliveryDistanceKm}");
                        break;
                    case 8:
                        Console.WriteLine($"MaxDeliveryTimeSpan: {s_dal.Config!.MaxDeliveryTimeSpan}");
                        break;
                    case 9:
                        Console.WriteLine($"RiskRange: {s_dal.Config!.RiskRange}");
                        break;
                    case 10:
                        Console.WriteLine($"InactivityRange: {s_dal.Config!.InactivityRange}");
                        break;
                    case 11:
                        Console.WriteLine($"CompanyFullAddress: {s_dal.Config!.CompanyFullAddress}");
                        break;
                    case 12:
                        Console.WriteLine($"Latitude: {s_dal.Config!.Latitude}");
                        break;
                    case 13:
                        Console.WriteLine($"Longitude: {s_dal.Config!.Longitude}");
                        break;
                    case 14:
                        Console.WriteLine();
                        Console.WriteLine("=== All Configuration Variables ===");
                        Console.WriteLine($"AdminId: {s_dal.Config!.AdminId}");
                        Console.WriteLine($"AdminPassword: {s_dal.Config!.AdminPassword}");
                        Console.WriteLine($"AvgCarSpeedKmh: {s_dal.Config!.AvgCarSpeedKmh}");
                        Console.WriteLine($"AvgMotorcycleSpeedKmh: {s_dal.Config!.AvgMotorcycleSpeedKmh}");
                        Console.WriteLine($"AvgBicycleSpeedKmh: {s_dal.Config!.AvgBicycleSpeedKmh}");
                        Console.WriteLine($"AvgWalkingSpeedKmh: {s_dal.Config!.AvgWalkingSpeedKmh}");
                        Console.WriteLine($"MaxGeneralDeliveryDistanceKm: {s_dal.Config!.MaxGeneralDeliveryDistanceKm}");
                        Console.WriteLine($"MaxDeliveryTimeSpan: {s_dal.Config!.MaxDeliveryTimeSpan}");
                        Console.WriteLine($"RiskRange: {s_dal.Config!.RiskRange}");
                        Console.WriteLine($"InactivityRange: {s_dal.Config!.InactivityRange}");
                        Console.WriteLine($"CompanyFullAddress: {s_dal.Config!.CompanyFullAddress}");
                        Console.WriteLine($"Latitude: {s_dal.Config!.Latitude}");
                        Console.WriteLine($"Longitude: {s_dal.Config!.Longitude}");
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
    /// <summary>
    /// Displays all data (couriers, orders, deliveries) currently in the system.
    /// </summary>
    // Read and display all data categories (couriers, orders, deliveries) with counts.
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
            // Print DAL-related error messages (for instance, if DAL is not initialized).
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Resets all data and configuration to default values.
    /// </summary>
    // Reset databases and configuration to defaults via DAL implementation.
    private static void ResetAllDataAndConfig()
    {
        try
        {
            s_dal.ResetDB();
            Console.WriteLine("Reset complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
