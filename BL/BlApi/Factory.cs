namespace BlApi;

/// <summary>
/// Provides a factory for creating instances of the business logic facade.
/// </summary>
public static class Factory
{
    /// <summary>
    /// Creates and returns an <see cref="IBl"/> instance.
    /// </summary>
    /// <returns>A new instance of the business logic facade.</returns>
    public static IBl Get() => new BlImplementation.Bl();
}