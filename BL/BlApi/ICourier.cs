
namespace BlApi;

public interface ICourier//כרגע מכיל צפייה עדכון יצירה ומחיקה של שליחים
{
    /// <summary>
    /// Creates a new courier entry in the system.
    /// </summary>
    /// <param name="boCourier">The courier object containing the details to be added. Cannot be null.</param>
    void Create(BO.Courier boCourier);
    /// <summary>
    /// Retrieves the courier details for the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to retrieve.</param>
    /// <returns>A <see cref="BO.Courier"/> object containing the details of the courier if found; otherwise, <see
    /// langword="null"/>.</returns>
    BO.Courier? Read(int id);
    /// <summary>
    /// Retrieves a collection of all couriers, optionally sorted and filtered based on specified criteria.
    /// </summary>
    /// <param name="sort">The field by which to sort the couriers. If <see langword="null"/>, no sorting is applied.</param>
    /// <param name="filter">The field by which to filter the couriers. If <see langword="null"/>, no filtering is applied.</param>
    /// <param name="value">The value to use for filtering. This parameter is used only if <paramref name="filter"/> is specified.</param>
    /// <returns>An enumerable collection of <see cref="BO.CourierInList"/> representing the couriers. The collection may be
    /// empty if no couriers match the criteria.</returns>
    IEnumerable<BO.CourierInList> ReadAll(//מתודה של צפייה בכל השליחים עם אפשרות למיין ולסנן על פי קריטריונים שנבחרו
        BO.CourierFieldSort? sort = null,
        BO.CourierFieldFilter? filter = null,
        object? value = null);
    /// <summary>
    /// Updates the specified courier's information in the system.
    /// </summary>
    /// <param name="boCourier">The courier object containing updated information. Cannot be null.</param>
    void Update(BO.Courier boCourier);
    /// <summary>
    /// Deletes the entity with the specified identifier.
    /// </summary>
    /// <remarks>This method removes the entity from the data store. Ensure that the entity exists before
    /// calling this method to avoid exceptions.</remarks>
    /// <param name="id">The unique identifier of the entity to be deleted. Must be a positive integer.</param>
    void Delete(int id);


}
