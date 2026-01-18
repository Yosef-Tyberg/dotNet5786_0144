using System;

namespace BlApi;

/// <summary>
/// Administrator-level actions for database management, clock control, and system configuration.
/// </summary>
public interface IAdmin
{
    /// <summary>
    /// Resets the database to its initial empty state.
    /// </summary>
    void ResetDB();

    /// <summary>
    /// Initializes the database with a default set of data for testing or demonstration.
    /// </summary>
    void InitializeDB();

    /// <summary>
    /// Retrieves the current system clock time.
    /// </summary>
    /// <returns>The current DateTime of the system clock.</returns>
    DateTime GetClock();

    /// <summary>
    /// Advances the system clock by a specified amount of time.
    /// </summary>
    /// <param name="timeSpan">The duration to add to the current clock.</param>
    void ForwardClock(TimeSpan timeSpan);

    /// <summary>
    /// Retrieves the current system configuration settings relevant to the presentation layer.
    /// </summary>
    /// <returns>A BO.Config object with the current settings.</returns>
    BO.Config GetConfig();

    /// <summary>
    /// Updates the system configuration with new values.
    /// </summary>
    /// <param name="config">A BO.Config object containing the new settings.</param>
    void SetConfig(BO.Config config);

    #region Stage 5
    void AddConfigObserver(Action configObserver);
    void RemoveConfigObserver(Action configObserver);
    void AddClockObserver(Action clockObserver);
    void RemoveClockObserver(Action clockObserver);
    #endregion Stage 5
}
