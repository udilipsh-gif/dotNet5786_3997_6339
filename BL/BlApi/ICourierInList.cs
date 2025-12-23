namespace BlApi;

/// <summary>
/// Defines operations for retrieving courier summary/list information.
/// </summary>
public interface ICourierInList
{
    /// <summary>
    /// Retrieves a courier list item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier.</param>
    /// <returns>A <see cref="BO.CourierInList"/> instance.</returns>
    BO.CourierInList Read(int id);
}
