using System;
using System.Collections.Generic;
using System.Linq;
using DalApi;
using DO;

namespace DalTest
{
    /// <summary>
    /// Test data initializer that fills the DAL with legal, consistent, and referentially-valid data.
    /// Call InitializeDatabase(...) from the DO test harness or unit tests.
    /// </summary>
    public static class Initialization
    {
        private static readonly Random Rnd = new Random(12345); // fixed seed for reproducibility

        /// <summary>
        /// Small address data model used only by the initializer.
        /// </summary>
        private record TestAddress(string Name, double Lat, double Lon, double AerialKm, double WalkingKmEst, double DrivingKmEst);

        /// <summary>
        /// Initialize the configuration first, then couriers, orders and deliveries.
        /// Uses only the DAL interfaces provided (Create/Reset).
        /// </summary>
        public static void InitializeDatabase(
            ICourier courierDal,
            IOrder orderDal,
            IDelivery deliveryDal,
            IConfig configDal)
        {
            if (courierDal == null) throw new ArgumentNullException(nameof(courierDal));
            if (orderDal == null) throw new ArgumentNullException(nameof(orderDal));
            if (deliveryDal == null) throw new ArgumentNullException(nameof(deliveryDal));
            if (configDal == null) throw new ArgumentNullException(nameof(configDal));

            // 1) Reset and initialize config (company location, speeds, limits)
            configDal.Reset();
            InitializeConfig(configDal);

            // 2) Prepare address list (real coordinates + precomputed distances)
            var addresses = PrepareAddresses();

            // 3) Create couriers
            var couriers = CreateCouriers(courierDal, configDal, count: 20);

            // 4) Create orders (pass Id = 0 so DAL auto-generates)
            var orders = CreateOrders(orderDal, addresses, count: 50, clock: configDal.Clock);

            // 5) Create deliveries (assign couriers -> orders respecting radii and no overlaps)
            CreateDeliveries(deliveryDal, courierDal, orderDal, configDal, couriers, orders, addresses);
        }

        // -------------------------
        // Configuration initialization
        // -------------------------
        private static void InitializeConfig(IConfig config)
        {
            // Company placed in Jerusalem center (example).
            config.CompanyFullAddress = "Downtown Jerusalem, Israel";
            config.Latitude = 31.771959;
            config.Longitude = 35.217018;

            // Average speeds (urban assumptions)
            config.AvgCarSpeedKmh = 40.0;
            config.AvgMotorcycleSpeedKmh = 35.0;
            config.AvgBicycleSpeedKmh = 15.0;
            config.AvgWalkingSpeedKmh = 5.0;

            // Delivery limits
            config.MaxGeneralDeliveryDistanceKm = 10.0; // allow deliveries up to 10 km aerially
            config.MaxDeliveryTimeSpan = TimeSpan.FromHours(2); // e.g. considered late if > 2 hours
            config.RiskRange = TimeSpan.FromMinutes(10); // near-late threshold
            config.InactivityRange = TimeSpan.FromMinutes(30); // inactivity threshold

            // Clock for reproducible initialization
            config.Clock = DateTime.Now;
        }

        // -------------------------
        // Address list (real coords + distances)
        // -------------------------
        private static List<TestAddress> PrepareAddresses()
        {
            // The coordinates below were collected from public sources (Jerusalem center and nearby places).
            // Aerial distance is computed w.r.t. company coords (31.771959, 35.217018).
            // Walking and driving are estimated multipliers of aerial distance (walking ≈ 1.20×, driving ≈ 1.25×).
            // See TestInitializer documentation for source citations.

            var list = new List<TestAddress>
            {
                new TestAddress(
                    "Ben Yehuda Street",
                    31.7815, 35.2176,         // lat, lon
                    1.062, 1.062 * 1.20, 1.062 * 1.25),

                new TestAddress(
                    "Jaffa Road (central)",
                    31.784637, 35.215046,
                    1.444, 1.444 * 1.20, 1.444 * 1.25),

                new TestAddress(
                    "Mahane Yehuda Market",
                    31.7847, 35.2073,
                    1.129, 1.129 * 1.20, 1.129 * 1.25),

                new TestAddress(
                    "Emek Refaim (German Colony)",
                    31.757919, 35.218139,
                    1.541, 1.541 * 1.20, 1.541 * 1.25),

                new TestAddress(
                    "Givat Shaul (Pressburg Yeshiva)",
                    31.7871278, 35.1901083,
                    2.538, 2.538 * 1.20, 2.538 * 1.25),

                new TestAddress(
                    "Hebron Road (near center)",
                    31.766509, 35.2259378,
                    1.016, 1.016 * 1.20, 1.016 * 1.25),

                new TestAddress(
                    "Ein Kerem (neighborhood)",
                    31.759164, 35.143000,
                    7.141, 7.141 * 1.20, 7.141 * 1.25),

                new TestAddress(
                    "Knesset building",
                    31.77667, 35.20528,
                    1.227, 1.227 * 1.20, 1.227 * 1.25)
            };

            // Round estimated values a bit for readability
            return list.Select(a => new TestAddress(a.Name, a.Lat, a.Lon, Math.Round(a.AerialKm, 3), Math.Round(a.WalkingKmEst, 3), Math.Round(a.DrivingKmEst, 3))).ToList();
        }

        // -------------------------
        // Generate couriers
        // -------------------------
        private static List<Courier> CreateCouriers(ICourier courierDal, IConfig config, int count)
        {
            var result = new List<Courier>();
            var types = Enum.GetValues(typeof(DeliveryTypes)).Cast<DeliveryTypes>().ToArray();

            for (int i = 1; i <= count; i++)
            {
                // uniform distribution of delivery types
                var deliveryType = types[Rnd.Next(types.Length)];

                // personal max radius (random between 1.0 km and company max)
                double? personalMax = null;
                if (config.MaxGeneralDeliveryDistanceKm.HasValue && Rnd.NextDouble() < 0.8) // most couriers set a personal max
                {
                    var max = config.MaxGeneralDeliveryDistanceKm.Value;
                    personalMax = Math.Round(1.0 + Rnd.NextDouble() * (max - 1.0), 2);
                }

                // active or inactive: most active, some inactive
                bool active = Rnd.NextDouble() < 0.85;

                // employment start time earlier than clock
                var start = config.Clock.AddDays(-Rnd.Next(1, 365 * 3)).AddMinutes(-Rnd.Next(0, 60 * 24)); // up to 3 years ago

                var courier = new Courier
                (
                    Id: i, // DAL courier implementation expects caller to set ID
                    FullName: $"Courier {i:00}",
                    MobilePhone: RandomPhone(),
                    Email: $"courier{i:00}@example.local",
                    Password: $"pwd{i:0000}",
                    Active: active,
                    DeliveryType: deliveryType,
                    EmploymentStartTime: start,
                    PersonalMaxDeliveryDistance: personalMax
                );

                courierDal.Create(courier);
                result.Add(courier);
            }

            // ensure at least some inactive with delivery history
            for (int j = 0; j < Math.Max(1, count / 6); j++)
            {
                var idx = Rnd.Next(result.Count);
                var c = result[idx];
                var replaced = c with { Active = false, EmploymentStartTime = c.EmploymentStartTime.AddYears(-1) };
                // update in DAL (we assume Update works by remove-create pattern in your DAL)
                courierDal.Update(replaced);
                result[idx] = replaced;
            }

            return result;
        }

        // -------------------------
        // Create orders
        // -------------------------
        private static List<Order> CreateOrders(IOrder orderDal, List<TestAddress> addresses, int count, DateTime clock)
        {
            var list = new List<Order>();
            var orderTypes = Enum.GetValues(typeof(OrderTypes)).Cast<OrderTypes>().ToArray();

            for (int i = 0; i < count; i++)
            {
                // pick random address
                var addr = addresses[Rnd.Next(addresses.Count)];

                var orderType = orderTypes[Rnd.Next(orderTypes.Length)];
                var verbal = orderType == OrderTypes.Pizza ? "Pizza order with toppings" : "Falafel order";

                var openTime = clock.AddMinutes(-Rnd.Next(1, 60 * 24 * 14)); // opened in last 14 days

                var order = new Order
                (
                    Id: 0, // let DAL auto-generate
                    OrderType: orderType,
                    VerbalDescription: $"{verbal} #{i + 1}",
                    FullOrderAccess: Guid.NewGuid().ToString("N"),
                    Latitude: addr.Lat,
                    Longitude: addr.Lon,
                    CustomerFullName: $"Customer {i + 1:00}",
                    CustomerMobile: RandomPhone(),
                    Volume: Math.Round(0.1 + Rnd.NextDouble() * 5.0, 3),
                    Weight: Math.Round(0.1 + Rnd.NextDouble() * 15.0, 3),
                    Fragile: Rnd.NextDouble() < 0.15,
                    Height: Math.Round(5 + Rnd.NextDouble() * 50, 2),
                    Width: Math.Round(5 + Rnd.NextDouble() * 50, 2),
                    OrderOpenTime: openTime
                );

                orderDal.Create(order);
                list.Add(order);
            }

            // After creation, orders in DAL have real IDs (if DAL assigns them). We need to read them back
            // so that later delivery assignment can reference correct IDs.
            var allOrders = orderDal.ReadAll();
            // sort by open time to make predictable ordering
            allOrders = allOrders.OrderBy(o => o.OrderOpenTime).ToList();

            // We'll return the in-DAL objects to the caller to work with the real IDs.
            return allOrders;
        }

        // -------------------------
        // Create deliveries
        // -------------------------
        private static void CreateDeliveries(
            IDelivery deliveryDal,
            ICourier courierDal,
            IOrder orderDal,
            IConfig config,
            List<Courier> couriersCreated,
            List<Order> ordersFromDal,
            List<TestAddress> addresses)
        {
            // retrieve current state from DAL
            var couriers = courierDal.ReadAll();
            var orders = ordersFromDal.ToList();

            // prepare order queues: 20 open, 10 in-progress, 20 closed
            // open orders: just keep them without any delivery
            var openOrders = orders.Take(20).ToList();

            // to pick in-progress and closed, start from next orders
            var remaining = orders.Skip(20).ToList();
            var inProgressOrders = remaining.Take(10).ToList();
            var closedOrders = remaining.Skip(10).Take(20).ToList();

            // Keep track of courier schedules (list of tuples start,end). end can be null for in-progress
            var courierSchedules = couriers.ToDictionary(c => c.Id, c => new List<(DateTime Start, DateTime? End)>());

            // Helper to determine aerial distance between two coords
            double AerialDistanceKm(double lat1, double lon1, double lat2, double lon2)
            {
                double R = 6371.0;
                double deg2rad(double deg) => deg * Math.PI / 180.0;
                var dLat = deg2rad(lat2 - lat1);
                var dLon = deg2rad(lon2 - lon1);
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                return R * c;
            }

            // Helper: find an available courier for given order (distance check + no overlapping)
            Courier? FindAvailableCourierForOrder(Order order, DateTime desiredStart)
            {
                // compute aerial distance from company to order
                var distToOrder = AerialDistanceKm(config.Latitude.Value, config.Longitude.Value, order.Latitude, order.Longitude);

                // shuffle couriers for randomness
                var shuffled = couriers.OrderBy(_ => Rnd.Next()).ToList();

                foreach (var courier in shuffled)
                {
                    // courier must be active or possibly inactive but previously employed (we can use either)
                    if (!courier.Active) continue;

                    // check personal max radius
                    if (courier.PersonalMaxDeliveryDistance.HasValue && distToOrder > courier.PersonalMaxDeliveryDistance.Value) continue;

                    // also check global max
                    if (config.MaxGeneralDeliveryDistanceKm.HasValue && distToOrder > config.MaxGeneralDeliveryDistanceKm.Value) continue;

                    // check for overlapping deliveries for this courier
                    var schedule = courierSchedules[courier.Id];
                    var candidateStart = desiredStart;
                    var candidateEnd = desiredStart.AddMinutes(30 + Rnd.Next(0, 61)); // assume 30-90 mins delivery duration estimate

                    var overlaps = schedule.Any(slot =>
                        // overlapping if candidate overlaps existing slot (open-ended or timed)
                        (slot.End == null && slot.Start <= candidateEnd) ||
                        (slot.End != null && slot.Start < candidateEnd && candidateStart < slot.End.Value)
                    );

                    if (overlaps) continue;

                    // if all checks pass, choose this courier
                    return courier;
                }

                return null;
            }

            // Utility to create a delivery record and persist it + update schedule
            void PersistDelivery(Order order, Courier courier, DateTime start, DeliveryEndTypes? endType, DateTime? endTime, double? actualDistance)
            {
                var delivery = new Delivery
                (
                    Id: 0, // DAL will assign ID
                    OrderId: order.Id,
                    CourierId: courier.Id,
                    DeliveryType: courier.DeliveryType,
                    DeliveryStartTime: start,
                    ActualDistance: actualDistance,
                    DeliveryEndType: endType,
                    DeliveryEndTime: endTime
                );

                deliveryDal.Create(delivery);

                // update courier schedule
                courierSchedules[courier.Id].Add((start, endTime));
            }

            // 1) Create in-progress deliveries (no end time)
            foreach (var order in inProgressOrders)
            {
                var desiredStart = order.OrderOpenTime.AddMinutes(Rnd.Next(5, 60 * 6)); // within 6 hours after opening
                if (desiredStart > config.Clock) desiredStart = config.Clock.AddMinutes(-Rnd.Next(1, 120)); // ensure it's earlier than clock

                var courier = FindAvailableCourierForOrder(order, desiredStart);
                if (courier == null)
                {
                    // if none found, skip (order remains open)
                    continue;
                }

                // estimate distance: aerial distance from company to order
                var dist = AerialDistanceKm(config.Latitude.Value, config.Longitude.Value, order.Latitude, order.Longitude);
                PersistDelivery(order, courier, desiredStart, endType: null, endTime: null, actualDistance: Math.Round(dist * (1.1 + Rnd.NextDouble() * 0.4), 3));
            }

            // 2) Create closed deliveries (ensure a mix of end types and some reopened flows)
            var endTypes = Enum.GetValues(typeof(DeliveryEndTypes)).Cast<DeliveryEndTypes>().ToArray();
            for (int idx = 0; idx < closedOrders.Count; idx++)
            {
                var order = closedOrders[idx];
                var start = order.OrderOpenTime.AddMinutes(Rnd.Next(10, 60 * 24)); // within 24 hours after opening
                if (start > config.Clock) start = config.Clock.AddMinutes(-Rnd.Next(1, 180));

                var courier = FindAvailableCourierForOrder(order, start);
                if (courier == null) continue;

                // choose end type with weighted probabilities
                // Delivered: 70%, Failed: 5%, Cancelled: 5%, CustomerRefused: 10%, RecipientNotFound: 10%
                double p = Rnd.NextDouble();
                DeliveryEndTypes chosen;
                if (p < 0.70) chosen = DeliveryEndTypes.Delivered;
                else if (p < 0.75) chosen = DeliveryEndTypes.Failed;
                else if (p < 0.80) chosen = DeliveryEndTypes.Cancelled;
                else if (p < 0.90) chosen = DeliveryEndTypes.CustomerRefused;
                else chosen = DeliveryEndTypes.RecipientNotFound;

                var durationMins = 10 + Rnd.Next(0, 60); // 10-70 minutes
                var endTime = start.AddMinutes(durationMins);

                var dist = AerialDistanceKm(config.Latitude.Value, config.Longitude.Value, order.Latitude, order.Longitude);
                PersistDelivery(order, courier, start, endType: chosen, endTime: endTime, actualDistance: Math.Round(dist * (1.1 + Rnd.NextDouble() * 0.4), 3));

                // simulate some reopened cases: if CustomerRefused or RecipientNotFound, sometimes create a second delivery that eventually delivers
                if ((chosen == DeliveryEndTypes.CustomerRefused || chosen == DeliveryEndTypes.RecipientNotFound) && Rnd.NextDouble() < 0.5)
                {
                    // find another courier a bit later
                    var reopenedStart = endTime.AddMinutes(30 + Rnd.Next(0, 180));
                    var courier2 = FindAvailableCourierForOrder(order, reopenedStart);
                    if (courier2 != null)
                    {
                        var reopenedEnd = reopenedStart.AddMinutes(10 + Rnd.Next(0, 60));
                        PersistDelivery(order, courier2, reopenedStart, endType: DeliveryEndTypes.Delivered, endTime: reopenedEnd, actualDistance: Math.Round(dist * (1.0 + Rnd.NextDouble() * 0.5), 3));
                    }
                }
            }

            // 3) Ensure that orders used in deliveries are not left in selection pools (we already selected specific subsets)
            // All done - deliveries persisted.
        }

        // -------------------------
        // Helpers (random phone, etc.)
        // -------------------------
        private static string RandomPhone()
        {
            // Israeli-like local number e.g. 050-1234567 (for tests only)
            var prefix = new[] { "050", "052", "054", "058" }[Rnd.Next(4)];
            var num = Rnd.Next(1000000, 9999999);
            return $"{prefix}-{num}";
        }
    }
}

