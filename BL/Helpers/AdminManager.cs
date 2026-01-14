﻿//using BO;
using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

    //private static Task? _periodicTask = null; //stage 7

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4

        //Add calls here to any logic method that should be called periodically,
        //after each clock update
        //for example, Periodic students' updates:
        // - Go through all students to update properties that are affected by the clock update
        // - (couriers become not active after a certain time of inactivity etc.)

        DeliveryManager.PeriodicDeliveriesUpdate(oldClock, newClock);
        CourierManager.PeriodicCouriersUpdate(oldClock, newClock);
        OrderManager.PeriodicOrdersUpdate(oldClock, newClock);
        
        //TO_DO: //stage 7
        //if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        //    _periodicTask = Task.Run(() => {
        //          DeliveryManager.PeriodicDeliveriesUpdate(oldClock, newClock);
        //          CourierManager.PeriodicCouriersUpdate(oldClock, newClock);
        //      });

        //Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage 5
    }

    /// <summary>
    /// Method for providing current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static BO.Config GetConfig() //stage 4
    => new BO.Config()
    {
        AdminId = s_dal.Config.AdminId,
        AdminPassword = s_dal.Config.AdminPassword,
        AvgCarSpeedKmh = s_dal.Config.AvgCarSpeedKmh,
        AvgMotorcycleSpeedKmh = s_dal.Config.AvgMotorcycleSpeedKmh,
        AvgBicycleSpeedKmh = s_dal.Config.AvgBicycleSpeedKmh,
        AvgWalkingSpeedKmh = s_dal.Config.AvgWalkingSpeedKmh,
        MaxGeneralDeliveryDistanceKm = s_dal.Config.MaxGeneralDeliveryDistanceKm,
        MaxDeliveryTimeSpan = s_dal.Config.MaxDeliveryTimeSpan,
        RiskRange = s_dal.Config.RiskRange,
        InactivityRange = s_dal.Config.InactivityRange,
        CompanyFullAddress = s_dal.Config.CompanyFullAddress,
        Latitude = s_dal.Config.Latitude,
        Longitude = s_dal.Config.Longitude
    };

    /// <summary>
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void SetConfig(BO.Config configuration) //stage 4
    {
        if (configuration is null) throw new BO.BlInvalidInputException("configuration can't be null");

        if (configuration.AdminId <= 0)
            throw new BO.BlInvalidInputException("Admin ID must be positive.");
        if (string.IsNullOrWhiteSpace(configuration.AdminPassword))
            throw new BO.BlInvalidInputException("Admin password cannot be empty.");
        if (configuration.AvgCarSpeedKmh <= 0 || configuration.AvgMotorcycleSpeedKmh <= 0 || 
            configuration.AvgBicycleSpeedKmh <= 0 || configuration.AvgWalkingSpeedKmh <= 0)
            throw new BO.BlInvalidInputException("Average speeds must be positive.");
        if (configuration.MaxGeneralDeliveryDistanceKm.HasValue && configuration.MaxGeneralDeliveryDistanceKm <= 0)
            throw new BO.BlInvalidInputException("Max general delivery distance must be positive.");
        if (configuration.MaxDeliveryTimeSpan <= TimeSpan.Zero)
            throw new BO.BlInvalidInputException("Max delivery time span must be positive.");
        if (configuration.RiskRange <= TimeSpan.Zero)
            throw new BO.BlInvalidInputException("Risk range must be positive.");
        if (configuration.InactivityRange <= TimeSpan.Zero)
            throw new BO.BlInvalidInputException("Inactivity range must be positive.");

        double? newLat = null, newLon = null;
        if (s_dal.Config.CompanyFullAddress != configuration.CompanyFullAddress)
        {
             if (string.IsNullOrWhiteSpace(configuration.CompanyFullAddress))
                 throw new BO.BlInvalidAddressException("Company address cannot be empty.");
             (newLat, newLon) = Tools.GetCoordinates(configuration.CompanyFullAddress);
        }

        bool configChanged = false; // stage 5

        if (s_dal.Config.AdminId != configuration.AdminId)
        {
            s_dal.Config.AdminId = configuration.AdminId;
            configChanged = true;
        }
        if (s_dal.Config.AdminPassword != configuration.AdminPassword)
        {
            s_dal.Config.AdminPassword = configuration.AdminPassword;
            configChanged = true;
        }
        if (s_dal.Config.AvgCarSpeedKmh != configuration.AvgCarSpeedKmh)
        {
            s_dal.Config.AvgCarSpeedKmh = configuration.AvgCarSpeedKmh;
            configChanged = true;
        }
        if (s_dal.Config.AvgMotorcycleSpeedKmh != configuration.AvgMotorcycleSpeedKmh)
        {
            s_dal.Config.AvgMotorcycleSpeedKmh = configuration.AvgMotorcycleSpeedKmh;
            configChanged = true;
        }
        if (s_dal.Config.AvgBicycleSpeedKmh != configuration.AvgBicycleSpeedKmh)
        {
            s_dal.Config.AvgBicycleSpeedKmh = configuration.AvgBicycleSpeedKmh;
            configChanged = true;
        }
        if (s_dal.Config.AvgWalkingSpeedKmh != configuration.AvgWalkingSpeedKmh)
        {
            s_dal.Config.AvgWalkingSpeedKmh = configuration.AvgWalkingSpeedKmh;
            configChanged = true;
        }
        if (s_dal.Config.MaxGeneralDeliveryDistanceKm != configuration.MaxGeneralDeliveryDistanceKm)
        {
            s_dal.Config.MaxGeneralDeliveryDistanceKm = configuration.MaxGeneralDeliveryDistanceKm;
            configChanged = true;
        }
        if (s_dal.Config.MaxDeliveryTimeSpan != configuration.MaxDeliveryTimeSpan)
        {
            s_dal.Config.MaxDeliveryTimeSpan = configuration.MaxDeliveryTimeSpan;
            configChanged = true;
        }
        if (s_dal.Config.RiskRange != configuration.RiskRange)
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            configChanged = true;
        }
        if (s_dal.Config.InactivityRange != configuration.InactivityRange)
        {
            s_dal.Config.InactivityRange = configuration.InactivityRange;
            configChanged = true;
        }
        if (s_dal.Config.CompanyFullAddress != configuration.CompanyFullAddress)
        {
            s_dal.Config.CompanyFullAddress = configuration.CompanyFullAddress;
            s_dal.Config.Latitude = newLat;
            s_dal.Config.Longitude = newLon;
            configChanged = true;
        }

        //Calling all the observers of configuration update
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    internal static void ResetDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            ConfigUpdatedObservers?.Invoke(); //stage 5 - needed for update the PL 
        }
    }

    internal static void InitializeDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            DalTest.Initialization.Do(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated           
            ConfigUpdatedObservers?.Invoke(); //stage 5 - needed for update the PL
        }
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;
    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())    
    /// </summary>
    //private static int s_interval = 1;
    /// <summary>
    /// The flag that signs whether simulator is running
    /// 
    //private static volatile bool s_stop = false;
    /*

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BLTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    /*
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }
    */
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            //s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    //private static Task? _simulateTask = null;

    /*private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation
            if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
                _simulateTask = Task.Run(() => StudentManager.SimulateCourseRegistrationAndGrade());

            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }
    */
    #endregion Stage 7 base
}
