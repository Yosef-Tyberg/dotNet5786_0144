using BlImplementation;

namespace BlApi;

/// <summary>
/// Factory class to create an instance of the business logic layer.
/// </summary>
public static class Factory
{
    /// <summary>
    /// Gets an instance of the business logic layer.
    /// </summary>
    /// <returns>An object that implements the IBl interface.</returns>
    public static IBl Get() => new Bl();
}
