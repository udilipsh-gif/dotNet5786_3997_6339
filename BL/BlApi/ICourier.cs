
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

    string? Login(int id, string password);

    /// <summary>
    /// Reads all couriers, with optional filtering by active status and sorting.
    /// </summary>
    /// <param name="requesterId"></param>
    /// <param name="isActive"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    IEnumerable<BO.CourierInList> ReadAll(
    int requesterId,
    bool? isActive,
    BO.CourierFieldSort? sort);



}
