using BlApi;
using BO;

namespace BlTest;

/// <summary>
/// Test program for the Business Logic layer.
/// Provides a console-based menu to interact with all BL functionalities.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The single entry point to the Business Logic layer.
    /// </summary>
    private static readonly IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// The main entry point for the test application.
    /// </summary>
    static void Main(string[] args)
    {
        try
        {
            Console.Write("Would you like to create Initial data? (Y/N) ");
            string? ans = Console.ReadLine()?.Trim().ToUpper();
            if (ans == "Y")
            {
                DalTest.Initialization.Do();
                Console.WriteLine("Initial data created successfully.");
            }

            MainMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"A top-level error occurred: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Displays the main menu and handles user navigation.
    /// </summary>
    private static void MainMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Order Management (Admin)");
            Console.WriteLine("2. Courier Management (Admin)");
            Console.WriteLine("3. Delivery Management (Admin)");
            Console.WriteLine("4. Courier Actions");
            Console.WriteLine("5. System Settings");
            Console.WriteLine("0. Exit");
            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 1: OrderMenu(); break;
                    case 2: CourierMenu(); break;
                    case 3: DeliveryMenu(); break;
                    case 4: CourierActionsMenu(); break;
                    case 5: SystemMenu(); break;
                    case 0: Console.WriteLine("Exiting program."); return;
                    default: Console.WriteLine("Unknown choice. Please try again."); break;
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine("\n--- An Error Occurred ---");
                 Console.WriteLine($"Type: {ex.GetType().Name}");
                 Console.WriteLine($"Message: {ex.Message}");
                 if(ex.InnerException != null)
                 {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                 }
                 Console.WriteLine("--------------------------");
            }
        }
    }

    #region Menu Implementations

    private static void OrderMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Order Management Menu ---");
            Console.WriteLine("1. List all Orders");
            Console.WriteLine("2. Get Order Details");
            Console.WriteLine("3. Add a new Order");
            Console.WriteLine("4. Update an Order");
            Console.WriteLine("5. Delete an Order");
            Console.WriteLine("6. Get Order Tracking Info");
            Console.WriteLine("7. Cancel an Order");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) { Console.WriteLine("Invalid choice."); continue; }

            switch (choice)
            {
                case 1: GetOrderList(); break;
                case 2: GetOrderDetails(); break;
                case 3: AddNewOrder(); break;
                case 4: UpdateOrder(); break;
                case 5: DeleteOrder(); break;
                case 6: GetOrderTracking(); break;
                case 7: CancelOrder(); break;
                case 0: return;
                default: Console.WriteLine("Unknown choice."); break;
            }
        }
    }

    private static void CourierMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Courier Management Menu ---");
            Console.WriteLine("1. List all Couriers");
            Console.WriteLine("2. Get Courier Details");
            Console.WriteLine("3. Add a new Courier");
            Console.WriteLine("4. Update a Courier");
            Console.WriteLine("5. Delete a Courier");
            Console.WriteLine("6. Get Courier's Delivery History");
            Console.WriteLine("7. Get Courier's Statistics");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) { Console.WriteLine("Invalid choice."); continue; }
            
            switch (choice)
            {
                case 1: GetCourierList(); break;
                case 2: GetCourierDetails(); break;
                case 3: AddNewCourier(); break;
                case 4: UpdateCourier(); break;
                case 5: DeleteCourier(); break;
                case 6: GetCourierDeliveryHistory(); break;
                case 7: GetCourierStatistics(); break;
                case 0: return;
                default: Console.WriteLine("Unknown choice."); break;
            }
        }
    }

    private static void DeliveryMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Delivery Management Menu ---");
            Console.WriteLine("1. List all Deliveries");
            Console.WriteLine("2. Get Delivery Details");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) { Console.WriteLine("Invalid choice."); continue; }

            switch (choice)
            {
                case 1: GetDeliveryList(); break;
                case 2: GetDeliveryDetails(); break;
                case 0: return;
                default: Console.WriteLine("Unknown choice."); break;
            }
        }
    }

    private static void CourierActionsMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Courier Actions Menu ---");
            Console.WriteLine("1. Update My Details");
            Console.WriteLine("2. List Available Orders (matching my profile)");
            Console.WriteLine("3. List Open Orders (from Courier perspective)");
            Console.WriteLine("4. Pick Up an Order");
            Console.WriteLine("5. Finalize a Delivery");
            Console.WriteLine("6. View My Current Delivery");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) { Console.WriteLine("Invalid choice."); continue; }
            
            switch (choice)
            {
                case 1: UpdateMyDetails(); break;
                case 2: GetAvailableOrdersForCourier(); break;
                case 3: GetOpenOrdersForCourier(); break;
                case 4: PickUpOrder(); break;
                case 5: FinalizeDelivery(); break;
                case 6: GetMyCurrentDelivery(); break;
                case 0: return;
                default: Console.WriteLine("Unknown choice."); break;
            }
        }
    }

    private static void SystemMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- System Settings Menu ---");
            Console.WriteLine("1. Get System Clock");
            Console.WriteLine("2. Advance System Clock");
            Console.WriteLine("3. Get System Configuration");
            Console.WriteLine("4. Set System Configuration");
            Console.WriteLine("5. Initialize Database");
            Console.WriteLine("6. Reset Database");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) { Console.WriteLine("Invalid choice."); continue; }
            
            switch (choice)
            {
                case 1: Console.WriteLine($"Current system time: {s_bl.Admin.GetClock()}"); break;
                case 2: AdvanceClock(); break;
                case 3: Console.WriteLine(s_bl.Admin.GetConfig()); break;
                case 4: SetConfig(); break;
                case 5: s_bl.Admin.InitializeDB(); Console.WriteLine("Database initialized."); break;
                case 6: s_bl.Admin.ResetDB(); Console.WriteLine("Database reset."); break;
                case 0: return;
                default: Console.WriteLine("Unknown choice."); break;
            }
        }
    }

    #endregion

    #region Action Methods

    // --- Order Actions ---
    private static void GetOrderList()
    {
        var orders = s_bl.Order.ReadAll();
        if (!orders.Any()) { Console.WriteLine("No orders found."); return; }
        foreach (var order in orders) Console.WriteLine(order);
    }

    private static void GetOrderDetails()
    {
        Console.Write("Enter Order ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Order ID.");
        var order = s_bl.Order.Read(id);
        Console.WriteLine(order);
    }

    private static void AddNewOrder()
    {
        Console.Write("Enter Order Type (e.g., Pizza, Falafel): ");
        if (!Enum.TryParse<OrderTypes>(Console.ReadLine(), true, out var orderType) || !Enum.IsDefined(orderType)) throw new BlInvalidInputException("Invalid Order Type.");

        Console.Write("Enter Verbal Description: ");
        string? verbalDescription = Console.ReadLine();

        Console.Write("Enter Latitude: ");
        if (!double.TryParse(Console.ReadLine(), out double latitude)) throw new BlInvalidInputException("Invalid latitude.");

        Console.Write("Enter Longitude: ");
        if (!double.TryParse(Console.ReadLine(), out double longitude)) throw new BlInvalidInputException("Invalid longitude.");
        
        Console.Write("Enter Customer Full Name: ");
        string? customerFullName = Console.ReadLine();
        
        Console.Write("Enter Customer Mobile: ");
        string? customerMobile = Console.ReadLine();
        
        Console.Write("Enter Customer Address: ");
        string? fullOrderAddress = Console.ReadLine();

        Console.Write("Enter Package Volume: ");
        if (!double.TryParse(Console.ReadLine(), out double volume)) throw new BlInvalidInputException("Invalid volume.");

        Console.Write("Enter Package Weight: ");
        if (!double.TryParse(Console.ReadLine(), out double weight)) throw new BlInvalidInputException("Invalid weight.");

        Console.Write("Is the package fragile? (y/n): ");
        string? fragileStr = Console.ReadLine();
        if (string.IsNullOrEmpty(fragileStr) || (fragileStr.ToLower() != "y" && fragileStr.ToLower() != "n")) throw new BlInvalidInputException("Invalid input for fragile.");
        bool isFragile = fragileStr.ToLower() == "y";

        Console.Write("Enter Package Height: ");
        if (!double.TryParse(Console.ReadLine(), out double height)) throw new BlInvalidInputException("Invalid height.");

        Console.Write("Enter Package Width: ");
        if (!double.TryParse(Console.ReadLine(), out double width)) throw new BlInvalidInputException("Invalid width.");

        var newOrder = new Order
        {
            OrderType = orderType,
            VerbalDescription = verbalDescription,
            Latitude = latitude,
            Longitude = longitude,
            CustomerFullName = customerFullName,
            CustomerMobile = customerMobile,
            FullOrderAddress = fullOrderAddress,
            Volume = volume,
            Weight = weight,
            Fragile = isFragile,
            Height = height,
            Width = width,
        };
        int newId = s_bl.Order.Create(newOrder);
        Console.WriteLine($"Order added successfully. New ID: {newId}");
    }

    private static void UpdateOrder()
    {
        Console.Write("Enter Order ID to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Order ID.");
        var orderToUpdate = s_bl.Order.Read(id);

        Console.WriteLine("Enter new details (leave blank to keep current value):");

        Console.Write($"Order Type ({orderToUpdate.OrderType}): ");
        string? orderTypeStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(orderTypeStr))
        {
            if (!Enum.TryParse<OrderTypes>(orderTypeStr, true, out var newOrderType) || !Enum.IsDefined(newOrderType)) throw new BlInvalidInputException("Invalid Order Type.");
            orderToUpdate.OrderType = newOrderType;
        }

        Console.Write($"Verbal Description ({orderToUpdate.VerbalDescription}): ");
        string? newVerbal = Console.ReadLine();
        if (!string.IsNullOrEmpty(newVerbal)) orderToUpdate.VerbalDescription = newVerbal;
        
        Console.Write($"Latitude ({orderToUpdate.Latitude}): ");
        string? latStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(latStr))
        {
            if (!double.TryParse(latStr, out double newLat)) throw new BlInvalidInputException("Invalid latitude.");
            orderToUpdate.Latitude = newLat;
        }

        Console.Write($"Longitude ({orderToUpdate.Longitude}): ");
        string? lonStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(lonStr))
        {
            if (!double.TryParse(lonStr, out double newLon)) throw new BlInvalidInputException("Invalid longitude.");
            orderToUpdate.Longitude = newLon;
        }

        Console.Write($"Customer Full Name ({orderToUpdate.CustomerFullName}): ");
        string? newCustomerFullName = Console.ReadLine();
        if (!string.IsNullOrEmpty(newCustomerFullName)) orderToUpdate.CustomerFullName = newCustomerFullName;

        Console.Write($"Customer Mobile ({orderToUpdate.CustomerMobile}): ");
        string? newCustomerMobile = Console.ReadLine();
        if (!string.IsNullOrEmpty(newCustomerMobile)) orderToUpdate.CustomerMobile = newCustomerMobile;

        Console.Write($"Full Order Address ({orderToUpdate.FullOrderAddress}): ");
        string? newFullOrderAddress = Console.ReadLine();
        if (!string.IsNullOrEmpty(newFullOrderAddress)) orderToUpdate.FullOrderAddress = newFullOrderAddress;

        Console.Write($"Volume ({orderToUpdate.Volume}): ");
        string? volStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(volStr))
        {
            if (!double.TryParse(volStr, out double newVol)) throw new BlInvalidInputException("Invalid volume.");
            orderToUpdate.Volume = newVol;
        }

        Console.Write($"Weight ({orderToUpdate.Weight}): ");
        string? weightStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(weightStr))
        {
            if (!double.TryParse(weightStr, out double newWeight)) throw new BlInvalidInputException("Invalid weight.");
            orderToUpdate.Weight = newWeight;
        }

        Console.Write($"Fragile ({orderToUpdate.Fragile}): ");
        string? fragileStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(fragileStr))
        {
             if (fragileStr.ToLower() != "y" && fragileStr.ToLower() != "n") throw new BlInvalidInputException("Invalid input for fragile.");
             orderToUpdate.Fragile = fragileStr.ToLower() == "y";
        }

        Console.Write($"Height ({orderToUpdate.Height}): ");
        string? heightStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(heightStr))
        {
            if (!double.TryParse(heightStr, out double newHeight)) throw new BlInvalidInputException("Invalid height.");
            orderToUpdate.Height = newHeight;
        }

        Console.Write($"Width ({orderToUpdate.Width}): ");
        string? widthStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(widthStr))
        {
            if (!double.TryParse(widthStr, out double newWidth)) throw new BlInvalidInputException("Invalid width.");
            orderToUpdate.Width = newWidth;
        }

        s_bl.Order.Update(orderToUpdate);
        Console.WriteLine("Order updated successfully.");
    }

    private static void DeleteOrder()
    {
        Console.Write("Enter Order ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Order ID.");
        s_bl.Order.Delete(id);
        Console.WriteLine("Order deleted successfully.");
    }
    
    private static void CancelOrder()
    {
        Console.Write("Enter Order ID to cancel: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Order ID.");
        s_bl.Order.Cancel(id);
        Console.WriteLine("Order cancelled successfully.");
    }

    private static void GetOrderTracking()
    {
        Console.Write("Enter Order ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Order ID.");
        var tracking = s_bl.Order.GetOrderTracking(id);
        Console.WriteLine(tracking);
    }

    // --- Courier Actions ---
    private static void GetCourierList()
    {
        var couriers = s_bl.Courier.ReadAll();
        if (!couriers.Any()) { Console.WriteLine("No couriers found."); return; }
        foreach (var courier in couriers) Console.WriteLine(courier);
    }

    private static void GetCourierDetails()
    {
        Console.Write("Enter Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");
        var courier = s_bl.Courier.Read(id);
        Console.WriteLine(courier);
    }

    private static void AddNewCourier()
    {
        Console.Write("Enter Courier Full Name: ");
        string? fullName = Console.ReadLine();
        
        Console.Write("Enter Courier Email: ");
        string? email = Console.ReadLine();
        
        Console.Write("Enter Courier Mobile Phone: ");
        string? mobilePhone = Console.ReadLine();
        
        Console.Write("Enter Courier Password: ");
        string? password = Console.ReadLine();
        
        Console.Write("Enter Max Delivery Distance: ");
        string? maxDistStr = Console.ReadLine();
        if (!double.TryParse(maxDistStr, out double maxDistance)) throw new BlInvalidInputException("Invalid distance.");
        
        Console.Write("Enter Delivery Type (Standard, Fast, Blitz): ");
        if (!Enum.TryParse<DeliveryTypes>(Console.ReadLine(), true, out var deliveryType) || !Enum.IsDefined(deliveryType)) throw new BlInvalidInputException("Invalid Delivery Type.");

        var newCourier = new Courier
        {
            FullName = fullName,
            Email = email,
            MobilePhone = mobilePhone,
            Password = password,
            PersonalMaxDeliveryDistance = maxDistance,
            DeliveryType = deliveryType
        };
        s_bl.Courier.Create(newCourier);
        Console.WriteLine("Courier added successfully.");
    }

    private static void UpdateCourier()
    {
        Console.Write("Enter Courier ID to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");
        var courierToUpdate = s_bl.Courier.Read(id);

        Console.WriteLine("Enter new details (leave blank to keep current value):");

        Console.Write($"Full Name ({courierToUpdate.FullName}): ");
        string? newName = Console.ReadLine();
        if (!string.IsNullOrEmpty(newName)) courierToUpdate.FullName = newName;

        Console.Write($"Email ({courierToUpdate.Email}): ");
        string? newEmail = Console.ReadLine();
        if (!string.IsNullOrEmpty(newEmail)) courierToUpdate.Email = newEmail;

        Console.Write($"Mobile Phone ({courierToUpdate.MobilePhone}): ");
        string? newPhone = Console.ReadLine();
        if (!string.IsNullOrEmpty(newPhone)) courierToUpdate.MobilePhone = newPhone;

        Console.Write("Password: ");
        string? newPassword = Console.ReadLine();
        if (!string.IsNullOrEmpty(newPassword)) courierToUpdate.Password = newPassword;

        Console.Write($"Max Delivery Distance ({courierToUpdate.PersonalMaxDeliveryDistance}): ");
        string? maxDistStrIn = Console.ReadLine();
        if (!string.IsNullOrEmpty(maxDistStrIn))
        {
            if (!double.TryParse(maxDistStrIn, out double newMaxDist)) throw new BlInvalidInputException("Invalid distance.");
            courierToUpdate.PersonalMaxDeliveryDistance = newMaxDist;
        }

        Console.Write($"Delivery Type ({courierToUpdate.DeliveryType}): ");
        string? delTypeStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(delTypeStr))
        {
            if (!Enum.TryParse<DeliveryTypes>(delTypeStr, true, out var newDelType) || !Enum.IsDefined(newDelType)) throw new BlInvalidInputException("Invalid Delivery Type.");
            courierToUpdate.DeliveryType = newDelType;
        }

        s_bl.Courier.Update(courierToUpdate);
        Console.WriteLine("Courier updated successfully.");
    }

    private static void DeleteCourier()
    {
        Console.Write("Enter Courier ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");
        s_bl.Courier.Delete(id);
        Console.WriteLine("Courier deleted successfully.");
    }

    private static void GetCourierDeliveryHistory()
    {
        Console.Write("Enter Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");
        var history = s_bl.Courier.GetCourierDeliveryHistory(id);
        if (!history.Any()) { Console.WriteLine("No delivery history found for this courier."); return; }
        foreach (var delivery in history) Console.WriteLine(delivery);
    }

    private static void GetCourierStatistics()
    {
        Console.Write("Enter Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");
        var stats = s_bl.Courier.GetCourierStatistics(id);
        Console.WriteLine(stats);
    }
    
    // --- Delivery Actions ---
    private static void GetDeliveryList()
    {
        var deliveries = s_bl.Delivery.ReadAll();
        if (!deliveries.Any()) { Console.WriteLine("No deliveries found."); return; }
        foreach (var delivery in deliveries) Console.WriteLine(delivery);
    }

    private static void GetDeliveryDetails()
    {
        Console.Write("Enter Delivery ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Delivery ID.");
        var delivery = s_bl.Delivery.Read(id);
        Console.WriteLine(delivery);
    }

    // --- Courier-Specific Actions ---
    private static void UpdateMyDetails()
    {
        Console.Write("Enter your Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");

        Console.WriteLine("Enter new details (leave blank to keep current value):");
        Console.Write($"Full Name: ");
        string? name = Console.ReadLine();
        
        Console.Write($"Mobile Phone: ");
        string? phone = Console.ReadLine();
        
        Console.Write($"Email: ");
        string? email = Console.ReadLine();
        
        Console.Write($"Password: ");
        string? password = Console.ReadLine();

        Console.Write($"Max Delivery Distance: ");
        string? maxDistStr = Console.ReadLine();
        double? maxDistance = null;
        if (!string.IsNullOrEmpty(maxDistStr))
        {
            if (!double.TryParse(maxDistStr, out double newMaxDist)) throw new BlInvalidInputException("Invalid distance.");
            maxDistance = newMaxDist;
        }

        Console.Write($"Delivery Type: ");
        string? delTypeStr = Console.ReadLine();
        DeliveryTypes? deliveryType = null;
        if(!string.IsNullOrEmpty(delTypeStr))
        {
            if(!Enum.TryParse<DeliveryTypes>(delTypeStr, true, out var delType) || !Enum.IsDefined(delType)) throw new BlInvalidInputException("Invalid Delivery Type.");
            deliveryType = delType;
        }

        s_bl.Courier.UpdateMyDetails(id, 
            string.IsNullOrEmpty(name) ? null : name, 
            string.IsNullOrEmpty(phone) ? null : phone, 
            string.IsNullOrEmpty(email) ? null : email, 
            string.IsNullOrEmpty(password) ? null : password, 
            maxDistance, 
            deliveryType);
        Console.WriteLine("Your details have been updated.");
    }

    private static void GetOpenOrdersForCourier()
    {
        Console.Write("Enter your Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");
        var orders = s_bl.Courier.GetOpenOrders(id);
        if (!orders.Any()) { Console.WriteLine("No open orders for you."); return; }
        Console.WriteLine("--- Open Orders ---");
        foreach (var order in orders) Console.WriteLine(order);
    }

    private static void GetAvailableOrdersForCourier()
    {
        Console.Write("Enter your Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");
        var orders = s_bl.Order.GetAvailableOrders(id);
        if (!orders.Any()) { Console.WriteLine("No available orders matching your profile."); return; }
        foreach (var order in orders) Console.WriteLine(order);
    }

    private static void PickUpOrder()
    {
        Console.Write("Enter your Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int courierId)) throw new BlInvalidInputException("Invalid Courier ID.");
        
        Console.Write("Enter the Order ID to pick up: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId)) throw new BlInvalidInputException("Invalid Order ID.");
        
        s_bl.Delivery.PickUp(courierId, orderId);
        Console.WriteLine("Order picked up successfully.");
    }

    private static void FinalizeDelivery()
    {
        Console.Write("Enter your Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int courierId)) throw new BlInvalidInputException("Invalid Courier ID.");

        Console.Write("How did the delivery end? (Delivered, CustomerUnavailable, Refused, Other): ");
        if (!Enum.TryParse<DeliveryEndTypes>(Console.ReadLine(), true, out var endType) || !Enum.IsDefined(endType)) throw new BlInvalidInputException("Invalid Delivery End Type.");

        s_bl.Delivery.Deliver(courierId, endType);
        Console.WriteLine("Delivery finalized.");
    }

    private static void GetMyCurrentDelivery()
    {
        Console.Write("Enter your Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) throw new BlInvalidInputException("Invalid Courier ID.");
        var delivery = s_bl.Delivery.GetMyCurrentDelivery(id);
        if (delivery == null) { Console.WriteLine("You do not have an active delivery."); } 
        else { Console.WriteLine(delivery); }
    }

    // --- System Actions ---
    private static void AdvanceClock()
    {
        Console.Write("Enter hours to advance the clock: ");
        if (!double.TryParse(Console.ReadLine(), out double hours)) throw new BlInvalidInputException("Invalid number of hours.");
        s_bl.Admin.ForwardClock(TimeSpan.FromHours(hours));
        Console.WriteLine($"Clock advanced. New time: {s_bl.Admin.GetClock()}");
    }

    private static void SetConfig()
    {
        Console.WriteLine("Enter new configuration values:");

        Console.Write("Admin ID: ");
        if (!int.TryParse(Console.ReadLine(), out int adminId)) throw new BlInvalidInputException("Invalid Admin ID.");

        Console.Write("Admin Password: ");
        string? adminPassword = Console.ReadLine();

        Console.Write("Average Car Speed (km/h): ");
        if (!double.TryParse(Console.ReadLine(), out double carSpeed)) throw new BlInvalidInputException("Invalid speed.");
        
        Console.Write("Average Motorcycle Speed (km/h): ");
        if (!double.TryParse(Console.ReadLine(), out double motoSpeed)) throw new BlInvalidInputException("Invalid speed.");

        Console.Write("Average Bicycle Speed (km/h): ");
        if (!double.TryParse(Console.ReadLine(), out double bikeSpeed)) throw new BlInvalidInputException("Invalid speed.");

        Console.Write("Average Walking Speed (km/h): ");
        if (!double.TryParse(Console.ReadLine(), out double walkSpeed)) throw new BlInvalidInputException("Invalid speed.");

        Console.Write("Max General Delivery Distance (km) (optional): ");
        string? maxDistStr = Console.ReadLine();
        double? maxDist = null;
        if(!string.IsNullOrEmpty(maxDistStr))
        {
            if(!double.TryParse(maxDistStr, out double parsedMaxDist)) throw new BlInvalidInputException("Invalid distance.");
            maxDist = parsedMaxDist;
        }

        Console.Write("Max Delivery Time (hours): ");
        if (!double.TryParse(Console.ReadLine(), out double maxHours)) throw new BlInvalidInputException("Invalid hours.");
        
        Console.Write("Risk Range (hours): ");
        if (!double.TryParse(Console.ReadLine(), out double riskHours)) throw new BlInvalidInputException("Invalid hours.");

        Console.Write("Inactivity Range (hours): ");
        if (!double.TryParse(Console.ReadLine(), out double inactiveHours)) throw new BlInvalidInputException("Invalid hours.");

        Console.Write("Company Full Address (optional): ");
        string? companyAddress = Console.ReadLine();

        Console.Write("Company Latitude (optional): ");
        string? latStr = Console.ReadLine();
        double? lat = null;
        if(!string.IsNullOrEmpty(latStr))
        {
            if(!double.TryParse(latStr, out double parsedLat)) throw new BlInvalidInputException("Invalid latitude.");
            lat = parsedLat;
        }

        Console.Write("Company Longitude (optional): ");
        string? lonStr = Console.ReadLine();
        double? lon = null;
        if(!string.IsNullOrEmpty(lonStr))
        {
            if(!double.TryParse(lonStr, out double parsedLon)) throw new BlInvalidInputException("Invalid longitude.");
            lon = parsedLon;
        }

        var config = new Config
        {
            AdminId = adminId,
            AdminPassword = adminPassword,
            AvgCarSpeedKmh = carSpeed,
            AvgMotorcycleSpeedKmh = motoSpeed,
            AvgBicycleSpeedKmh = bikeSpeed,
            AvgWalkingSpeedKmh = walkSpeed,
            MaxGeneralDeliveryDistanceKm = maxDist,
            MaxDeliveryTimeSpan = TimeSpan.FromHours(maxHours),
            RiskRange = TimeSpan.FromHours(riskHours),
            InactivityRange = TimeSpan.FromHours(inactiveHours),
            CompanyFullAddress = string.IsNullOrEmpty(companyAddress) ? null : companyAddress,
            Latitude = lat,
            Longitude = lon
        };
        s_bl.Admin.SetConfig(config);
        Console.WriteLine("Configuration updated successfully.");
    }

    #endregion
}
