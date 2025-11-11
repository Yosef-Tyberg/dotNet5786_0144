using System;
using System.Linq;
using Dal;
using DalList;
using DalApi;
using DO;

namespace DalTest
{
    internal static class Program
    {
        // Program-owned DAL instances (we pass these to Initialization so both use the same store)
        private static ICourier? s_dalCourier = new CourierImplementation();
        private static IOrder? s_dalOrder = new OrderImplementation();
        private static IDelivery? s_dalDelivery = new DeliveryImplementation();
        private static IConfig? s_dalConfig = new ConfigImplementation();

        private static void Main()
        {
            // Provide same instances to Initialization (so it populates the same data we will operate on)
            try
            {
                // If your Initialization already has static DAL fields you can remove this init call;
                // but to make sure both sides use the same instances we call InitWithDalInstances.
                DalTest.Initialization.InitWithDalInstances(s_dalCourier!, s_dalOrder!, s_dalDelivery!, s_dalConfig!);

                Console.WriteLine("Initializing database with test data...");
                DalTest.Initialization.Do();
                Console.WriteLine("Initialization completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Initialization failed:");
                Console.WriteLine(ex.ToString());
                // return to main menu - user can still operate (or exit)
            }

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
                            Console.WriteLine("Re-initializing database...");
                            DalTest.Initialization.Do();
                            Console.WriteLine("Re-initialization complete.");
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
            Console.WriteLine("=== DAL Test Main Menu ===");
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Test Couriers");
            Console.WriteLine("2. Test Orders");
            Console.WriteLine("3. Test Deliveries");
            Console.WriteLine("4. (Re)Initialize data source (Initialization.Do)");
            Console.WriteLine("5. Display all data (print everything)");
            Console.WriteLine("6. Configuration menu");
            Console.WriteLine("7. Reset all (clear lists + Reset config)");
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
                Console.WriteLine("1. Create courier (all fields prompted)");
                Console.WriteLine("2. Read courier by Id");
                Console.WriteLine("3. Read all couriers");
                Console.WriteLine("4. Update courier (all fields prompted)");
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
                    // print exception then return to main menu (exit submenu)
                    Console.WriteLine("Exception:");
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }

        private static void CreateCourier()
        {
            // Prompt every field
            Console.Write("Id (int): ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            Console.Write("FullName: ");
            var fullName = Console.ReadLine() ?? "";

            Console.Write("MobilePhone: ");
            var phone = Console.ReadLine() ?? "";

            Console.Write("Email: ");
            var email = Console.ReadLine() ?? "";

            Console.Write("Password: ");
            var password = Console.ReadLine() ?? "";

            Console.Write("Active (y/n): ");
            var activeRaw = Console.ReadLine() ?? "";
            bool active = activeRaw.Trim().ToLowerInvariant() == "y";

            Console.Write("DeliveryType (Car/Motorcycle/Bicycle/OnFoot): ");
            var dtypeRaw = Console.ReadLine() ?? "";
            if (!Enum.TryParse<DeliveryTypes>(dtypeRaw, true, out var dtype))
            {
                Console.WriteLine("Invalid DeliveryType.");
                return;
            }

            Console.Write($"EmploymentStartTime (yyyy-MM-dd HH:mm) (empty = use config clock {s_dalConfig!.Clock}): ");
            var startRaw = Console.ReadLine();
            DateTime start;
            if (string.IsNullOrWhiteSpace(startRaw))
                start = s_dalConfig!.Clock;
            else if (!DateTime.TryParse(startRaw, out start))
            {
                Console.WriteLine("Invalid date/time.");
                return;
            }

            // nullable personal max: empty => null
            Console.Write("PersonalMaxDeliveryDistance (km) (empty = null): ");
            var pmaxRaw = Console.ReadLine();
            double? pmax = null;
            if (!string.IsNullOrWhiteSpace(pmaxRaw))
            {
                if (double.TryParse(pmaxRaw, out double pval))
                    pmax = pval;
                else
                {
                    Console.WriteLine("Invalid number for personal max; setting null.");
                    pmax = null;
                }
            }

            var c = new Courier(id, fullName, phone, email, password, active, dtype, start, pmax);
            s_dalCourier!.Create(c);
            Console.WriteLine("Courier created.");
        }

        private static void ReadCourier()
        {
            Console.Write("Enter courier Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            var c = s_dalCourier!.Read(id);
            Console.WriteLine(c == null ? $"Courier with Id={id} not found." : c.ToString());
        }

        private static void ReadAllCouriers()
        {
            var list = s_dalCourier!.ReadAll();
            Console.WriteLine($"Couriers ({list.Count}):");
            foreach (var c in list) Console.WriteLine(c);
        }

        private static void UpdateCourier()
        {
            Console.Write("Enter courier Id to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            var existing = s_dalCourier!.Read(id);
            if (existing == null)
            {
                Console.WriteLine("Courier not found.");
                return;
            }

            Console.WriteLine("Leave text empty to keep current (for nullable fields empty means null).");

            // Prompt every field; empty -> keep existing (except nullable fields where empty => null)
            Console.Write($"FullName (current: {existing.FullName}): ");
            var fullName = Console.ReadLine();
            if (fullName == null) fullName = existing.FullName;

            Console.Write($"MobilePhone (current: {existing.MobilePhone}): ");
            var phone = Console.ReadLine();
            if (phone == null) phone = existing.MobilePhone;

            Console.Write($"Email (current: {existing.Email}): ");
            var email = Console.ReadLine();
            if (email == null) email = existing.Email;

            Console.Write($"Password (current: {existing.Password}): ");
            var password = Console.ReadLine();
            if (password == null) password = existing.Password;

            Console.Write($"Active (y/n) (current: {existing.Active}): ");
            var actRaw = Console.ReadLine();
            bool active = existing.Active;
            if (!string.IsNullOrWhiteSpace(actRaw))
                active = actRaw.Trim().ToLowerInvariant() == "y";

            Console.Write($"DeliveryType (Car/Motorcycle/Bicycle/OnFoot) (current: {existing.DeliveryType}): ");
            var dtypeRaw = Console.ReadLine();
            var dtype = existing.DeliveryType;
            if (!string.IsNullOrWhiteSpace(dtypeRaw) && Enum.TryParse<DeliveryTypes>(dtypeRaw, true, out var parsedType))
                dtype = parsedType;

            Console.Write($"EmploymentStartTime (yyyy-MM-dd HH:mm) (current: {existing.EmploymentStartTime}) (empty keep): ");
            var startRaw = Console.ReadLine();
            DateTime start = existing.EmploymentStartTime;
            if (!string.IsNullOrWhiteSpace(startRaw) && !DateTime.TryParse(startRaw, out start))
            {
                Console.WriteLine("Invalid date; keeping existing.");
                start = existing.EmploymentStartTime;
            }

            Console.Write($"PersonalMaxDeliveryDistance (km) (current: {existing.PersonalMaxDeliveryDistance?.ToString() ?? "null"}) (empty = null): ");
            var pmaxRaw = Console.ReadLine();
            double? pmax = existing.PersonalMaxDeliveryDistance;
            if (pmaxRaw != null)
            {
                if (string.IsNullOrWhiteSpace(pmaxRaw)) pmax = null;
                else if (double.TryParse(pmaxRaw, out double parsed)) pmax = parsed;
                else { Console.WriteLine("Invalid number; keeping existing."); pmax = existing.PersonalMaxDeliveryDistance; }
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

            s_dalCourier.Update(updated);
            Console.WriteLine("Courier updated.");
        }

        private static void DeleteCourier()
        {
            Console.Write("Enter courier Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            s_dalCourier!.Delete(id);
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
                    Console.WriteLine("Exception:");
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }

        private static void CreateOrder()
        {
            Console.Write("OrderType (Pizza/Falafel): ");
            var otRaw = Console.ReadLine() ?? "";
            if (!Enum.TryParse<OrderTypes>(otRaw, true, out var otype))
            {
                Console.WriteLine("Invalid order type.");
                return;
            }

            Console.Write("VerbalDescription: ");
            var verbal = Console.ReadLine() ?? "";

            Console.Write("FullOrderAccess (empty to auto-generate): ");
            var access = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(access)) access = Guid.NewGuid().ToString("N");

            Console.Write("Latitude (double): ");
            if (!double.TryParse(Console.ReadLine(), out double lat))
            {
                Console.WriteLine("Invalid latitude.");
                return;
            }

            Console.Write("Longitude (double): ");
            if (!double.TryParse(Console.ReadLine(), out double lon))
            {
                Console.WriteLine("Invalid longitude.");
                return;
            }

            Console.Write("CustomerFullName: ");
            var cust = Console.ReadLine() ?? "";

            Console.Write("CustomerMobile: ");
            var custPhone = Console.ReadLine() ?? "";

            Console.Write("Volume (double): ");
            if (!double.TryParse(Console.ReadLine(), out double volume))
            {
                Console.WriteLine("Invalid volume.");
                return;
            }

            Console.Write("Weight (double): ");
            if (!double.TryParse(Console.ReadLine(), out double weight))
            {
                Console.WriteLine("Invalid weight.");
                return;
            }

            Console.Write("Fragile (y/n): ");
            var fragRaw = Console.ReadLine() ?? "";
            var fragile = fragRaw.Trim().ToLowerInvariant() == "y";

            Console.Write("Height (double): ");
            if (!double.TryParse(Console.ReadLine(), out double height))
            {
                Console.WriteLine("Invalid height.");
                return;
            }

            Console.Write("Width (double): ");
            if (!double.TryParse(Console.ReadLine(), out double width))
            {
                Console.WriteLine("Invalid width.");
                return;
            }

            Console.Write("OrderOpenTime (yyyy-MM-dd HH:mm) (empty = config clock): ");
            var openRaw = Console.ReadLine();
            DateTime open;
            if (string.IsNullOrWhiteSpace(openRaw)) open = s_dalConfig!.Clock;
            else if (!DateTime.TryParse(openRaw, out open))
            {
                Console.WriteLine("Invalid date; using config clock.");
                open = s_dalConfig!.Clock;
            }

            var order = new Order(0, otype, verbal, access!, lat, lon, cust, custPhone, volume, weight, fragile, height, width, open);
            s_dalOrder!.Create(order);
            Console.WriteLine("Order created (Id assigned by DAL).");
        }

        private static void ReadOrder()
        {
            Console.Write("Enter order Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            var o = s_dalOrder!.Read(id);
            Console.WriteLine(o == null ? $"Order Id={id} not found." : o.ToString());
        }

        private static void ReadAllOrders()
        {
            var list = s_dalOrder!.ReadAll();
            Console.WriteLine($"Orders ({list.Count}):");
            foreach (var o in list) Console.WriteLine(o);
        }

        private static void UpdateOrder()
        {
            Console.Write("Enter order Id to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            var existing = s_dalOrder!.Read(id);
            if (existing == null)
            {
                Console.WriteLine("Order not found.");
                return;
            }

            Console.WriteLine("Leave empty to keep current value (nullable fields: empty => null).");

            Console.Write($"OrderType (current: {existing.OrderType}): ");
            var otRaw = Console.ReadLine();
            var otype = existing.OrderType;
            if (!string.IsNullOrWhiteSpace(otRaw) && Enum.TryParse<OrderTypes>(otRaw, true, out var parsedOt)) otype = parsedOt;

            Console.Write($"VerbalDescription (current: {existing.VerbalDescription}): ");
            var verbal = Console.ReadLine();
            if (verbal == null) verbal = existing.VerbalDescription;

            Console.Write($"FullOrderAccess (current: {existing.FullOrderAccess}) (empty auto-generate): ");
            var access = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(access)) access = existing.FullOrderAccess;

            Console.Write($"Latitude (current: {existing.Latitude}): ");
            var latRaw = Console.ReadLine();
            double lat = existing.Latitude;
            if (!string.IsNullOrWhiteSpace(latRaw) && !double.TryParse(latRaw, out lat))
            {
                Console.WriteLine("Invalid latitude; keeping existing.");
                lat = existing.Latitude;
            }

            Console.Write($"Longitude (current: {existing.Longitude}): ");
            var lonRaw = Console.ReadLine();
            double lon = existing.Longitude;
            if (!string.IsNullOrWhiteSpace(lonRaw) && !double.TryParse(lonRaw, out lon))
            {
                Console.WriteLine("Invalid longitude; keeping existing.");
                lon = existing.Longitude;
            }

            Console.Write($"CustomerFullName (current: {existing.CustomerFullName}): ");
            var cust = Console.ReadLine();
            if (cust == null) cust = existing.CustomerFullName;

            Console.Write($"CustomerMobile (current: {existing.CustomerMobile}): ");
            var custPhone = Console.ReadLine();
            if (custPhone == null) custPhone = existing.CustomerMobile;

            Console.Write($"Volume (current: {existing.Volume}): ");
            var volRaw = Console.ReadLine();
            double volume = existing.Volume;
            if (!string.IsNullOrWhiteSpace(volRaw) && !double.TryParse(volRaw, out volume))
            {
                Console.WriteLine("Invalid volume; keeping existing.");
                volume = existing.Volume;
            }

            Console.Write($"Weight (current: {existing.Weight}): ");
            var wRaw = Console.ReadLine();
            double weight = existing.Weight;
            if (!string.IsNullOrWhiteSpace(wRaw) && !double.TryParse(wRaw, out weight))
            {
                Console.WriteLine("Invalid weight; keeping existing.");
                weight = existing.Weight;
            }

            Console.Write($"Fragile (y/n) (current: {existing.Fragile}): ");
            var fragRaw = Console.ReadLine();
            bool fragile = existing.Fragile;
            if (!string.IsNullOrWhiteSpace(fragRaw))
                fragile = fragRaw.Trim().ToLowerInvariant() == "y";

            Console.Write($"Height (current: {existing.Height}): ");
            var hRaw = Console.ReadLine();
            double height = existing.Height;
            if (!string.IsNullOrWhiteSpace(hRaw) && !double.TryParse(hRaw, out height))
            {
                Console.WriteLine("Invalid height; keeping existing.");
                height = existing.Height;
            }

            Console.Write($"Width (current: {existing.Width}): ");
            var widRaw = Console.ReadLine();
            double width = existing.Width;
            if (!string.IsNullOrWhiteSpace(widRaw) && !double.TryParse(widRaw, out width))
            {
                Console.WriteLine("Invalid width; keeping existing.");
                width = existing.Width;
            }

            Console.Write($"OrderOpenTime (yyyy-MM-dd HH:mm) (current: {existing.OrderOpenTime}): ");
            var openRaw = Console.ReadLine();
            DateTime open = existing.OrderOpenTime;
            if (!string.IsNullOrWhiteSpace(openRaw) && !DateTime.TryParse(openRaw, out open))
            {
                Console.WriteLine("Invalid date; keeping existing.");
                open = existing.OrderOpenTime;
            }

            var updated = existing with
            {
                OrderType = otype,
                VerbalDescription = verbal,
                FullOrderAccess = access!,
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

        private static void DeleteOrder()
        {
            Console.Write("Enter order Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            s_dalOrder!.Delete(id);
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
                    Console.WriteLine("Exception:");
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }

        private static void CreateDelivery()
        {
            Console.Write("OrderId (int): ");
            if (!int.TryParse(Console.ReadLine(), out int orderId))
            {
                Console.WriteLine("Invalid order Id.");
                return;
            }

            Console.Write("CourierId (int): ");
            if (!int.TryParse(Console.ReadLine(), out int courierId))
            {
                Console.WriteLine("Invalid courier Id.");
                return;
            }

            Console.Write("DeliveryType (Car/Motorcycle/Bicycle/OnFoot): ");
            var dtypeRaw = Console.ReadLine() ?? "";
            if (!Enum.TryParse<DeliveryTypes>(dtypeRaw, true, out var dtype))
            {
                Console.WriteLine("Invalid delivery type.");
                return;
            }

            Console.Write($"DeliveryStartTime (yyyy-MM-dd HH:mm) (empty = config clock {s_dalConfig!.Clock}): ");
            var startRaw = Console.ReadLine();
            DateTime start = s_dalConfig!.Clock;
            if (!string.IsNullOrWhiteSpace(startRaw) && !DateTime.TryParse(startRaw, out start))
            {
                Console.WriteLine("Invalid date; using config clock.");
                start = s_dalConfig.Clock;
            }

            // ActualDistance is nullable: empty => null
            Console.Write("ActualDistance (km) (empty = null): ");
            var adRaw = Console.ReadLine();
            double? actual = null;
            if (!string.IsNullOrWhiteSpace(adRaw))
            {
                if (double.TryParse(adRaw, out double a)) actual = a;
                else { Console.WriteLine("Invalid number; setting actual distance to null."); actual = null; }
            }

            // DeliveryEndType nullable
            Console.Write("DeliveryEndType (Delivered/Failed/Cancelled/CustomerRefused/RecipientNotFound) (empty = null): ");
            var etRaw = Console.ReadLine();
            DeliveryEndTypes? endType = null;
            if (!string.IsNullOrWhiteSpace(etRaw) && Enum.TryParse<DeliveryEndTypes>(etRaw, true, out var et)) endType = et;

            DateTime? endTime = null;
            if (endType != null)
            {
                Console.Write("DeliveryEndTime (yyyy-MM-dd HH:mm) (empty = null): ");
                var eRaw = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(eRaw))
                {
                    if (DateTime.TryParse(eRaw, out DateTime eVal)) endTime = eVal;
                    else { Console.WriteLine("Invalid date; leaving end time null."); endTime = null; }
                }
            }

            var d = new Delivery(0, orderId, courierId, dtype, start, actual, endType, endTime);
            s_dalDelivery!.Create(d);
            Console.WriteLine("Delivery created.");
        }

        private static void ReadDelivery()
        {
            Console.Write("Enter delivery Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            var d = s_dalDelivery!.Read(id);
            Console.WriteLine(d == null ? $"Delivery Id={id} not found." : d.ToString());
        }

        private static void ReadAllDeliveries()
        {
            var list = s_dalDelivery!.ReadAll();
            Console.WriteLine($"Deliveries ({list.Count}):");
            foreach (var d in list) Console.WriteLine(d);
        }

        private static void UpdateDelivery()
        {
            Console.Write("Enter delivery Id to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            var existing = s_dalDelivery!.Read(id);
            if (existing == null)
            {
                Console.WriteLine("Delivery not found.");
                return;
            }

            Console.WriteLine("Leave empty to keep current (nullable fields: empty => null)");

            Console.Write($"OrderId (current: {existing.OrderId}): ");
            var oRaw = Console.ReadLine();
            int orderId = existing.OrderId;
            if (!string.IsNullOrWhiteSpace(oRaw) && !int.TryParse(oRaw, out orderId)) { Console.WriteLine("Invalid Id; keeping existing."); orderId = existing.OrderId; }

            Console.Write($"CourierId (current: {existing.CourierId}): ");
            var cRaw = Console.ReadLine();
            int courierId = existing.CourierId;
            if (!string.IsNullOrWhiteSpace(cRaw) && !int.TryParse(cRaw, out courierId)) { Console.WriteLine("Invalid Id; keeping existing."); courierId = existing.CourierId; }

            Console.Write($"DeliveryType (current: {existing.DeliveryType}): ");
            var dtRaw = Console.ReadLine();
            var dtype = existing.DeliveryType;
            if (!string.IsNullOrWhiteSpace(dtRaw) && Enum.TryParse<DeliveryTypes>(dtRaw, true, out var parsedDt)) dtype = parsedDt;

            Console.Write($"DeliveryStartTime (current: {existing.DeliveryStartTime}) (empty keep): ");
            var stRaw = Console.ReadLine();
            DateTime start = existing.DeliveryStartTime;
            if (!string.IsNullOrWhiteSpace(stRaw) && !DateTime.TryParse(stRaw, out start)) { Console.WriteLine("Invalid date; keeping existing."); start = existing.DeliveryStartTime; }

            Console.Write($"ActualDistance (current: {existing.ActualDistance?.ToString() ?? "null"}) (empty => null): ");
            var aRaw = Console.ReadLine();
            double? actual = existing.ActualDistance;
            if (aRaw != null)
            {
                if (string.IsNullOrWhiteSpace(aRaw)) actual = null;
                else if (double.TryParse(aRaw, out double parsedA)) actual = parsedA;
                else { Console.WriteLine("Invalid number; keeping existing."); actual = existing.ActualDistance; }
            }

            Console.Write($"DeliveryEndType (current: {existing.DeliveryEndType?.ToString() ?? "null"}) (empty => null): ");
            var etRaw = Console.ReadLine();
            DeliveryEndTypes? endType = existing.DeliveryEndType;
            if (etRaw != null)
            {
                if (string.IsNullOrWhiteSpace(etRaw)) endType = null;
                else if (Enum.TryParse<DeliveryEndTypes>(etRaw, true, out var parsedEt)) endType = parsedEt;
                else { Console.WriteLine("Invalid end type; keeping existing."); endType = existing.DeliveryEndType; }
            }

            Console.Write($"DeliveryEndTime (current: {existing.DeliveryEndTime?.ToString() ?? "null"}) (empty => null): ");
            var eTimeRaw = Console.ReadLine();
            DateTime? endTime = existing.DeliveryEndTime;
            if (eTimeRaw != null)
            {
                if (string.IsNullOrWhiteSpace(eTimeRaw)) endTime = null;
                else if (DateTime.TryParse(eTimeRaw, out DateTime parsed)) endTime = parsed;
                else { Console.WriteLine("Invalid date; keeping existing."); endTime = existing.DeliveryEndTime; }
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

        private static void DeleteDelivery()
        {
            Console.Write("Enter delivery Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }

            s_dalDelivery!.Delete(id);
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
                Console.WriteLine("3. Display current system time");
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
                            s_dalConfig!.Clock = s_dalConfig.Clock.AddMinutes(1);
                            Console.WriteLine($"Clock: {s_dalConfig.Clock}");
                            break;
                        case 2:
                            s_dalConfig!.Clock = s_dalConfig.Clock.AddHours(1);
                            Console.WriteLine($"Clock: {s_dalConfig.Clock}");
                            break;
                        case 3:
                            Console.WriteLine($"Clock: {s_dalConfig!.Clock}");
                            break;
                        case 4:
                            Console.Write("Enter MaxGeneralDeliveryDistanceKm (double) (empty to cancel): ");
                            var vRaw = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(vRaw)) break;
                            if (double.TryParse(vRaw, out double v)) { s_dalConfig!.MaxGeneralDeliveryDistanceKm = v; Console.WriteLine("Saved."); }
                            else Console.WriteLine("Invalid number.");
                            break;
                        case 5:
                            s_dalConfig!.Reset();
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
        // Utility
        // -------------------------
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
                Console.WriteLine("Exception while reading data:");
                Console.WriteLine(ex.ToString());
            }
        }

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
                Console.WriteLine("Exception while resetting:");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
