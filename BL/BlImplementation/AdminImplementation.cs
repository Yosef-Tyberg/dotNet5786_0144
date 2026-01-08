using System;
using BlApi;
using BL.Helpers;

namespace BlImplementation;

/// <summary>
/// Internal implementation of the IAdmin interface.
/// This class provides administrative functionalities by delegating calls to the AdminManager.
/// </summary>
internal sealed class AdminImplementation : IAdmin
{
    /// <inheritdoc />
    public void ResetDB() => AdminManager.ResetDB();

    /// <inheritdoc />
    public void InitializeDB() => AdminManager.InitializeDB();

    /// <inheritdoc />
    public DateTime GetClock() => AdminManager.Now;

    /// <inheritdoc />
    public void ForwardClock(TimeSpan timeSpan)
    {
        if (timeSpan.Ticks < 0)
            throw new BO.BlInvalidInputException("Time travel to the past is not supported.");
        DateTime newClock = AdminManager.Now.Add(timeSpan);
        AdminManager.UpdateClock(newClock);
    }

    /// <inheritdoc />
    public BO.Config GetConfig() => AdminManager.GetConfig();

    /// <inheritdoc />
    public void SetConfig(BO.Config config) => AdminManager.SetConfig(config);
}
