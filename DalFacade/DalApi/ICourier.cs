using DO;

namespace DalApi;

/// <summary>
/// Data access interface for managing <see cref="DO.Courier"/> entities in the DAL.
/// </summary>
public interface ICourier : ICrud<Courier>
{
    /// <summary>
    /// Creates a new courier entity in the data store.
    /// </summary>
    /// <param name="item">The <see cref="DO.Courier"/> instance to create.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>

    /// <summary>
    /// Reads a courier by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier.</param>
    /// <returns>The <see cref="DO.Courier"/> if found; otherwise <c>null</c>.</returns>


    /// <summary>
    /// Reads all courier entities.
    /// </summary>
    /// <returns>A list containing all couriers in the data store.</returns>


    /// <summary>
    /// Updates an existing courier entity in the data store.
    /// </summary>
    /// <param name="item">The courier instance with updated values. The instance's Id identifies the entity to update.</param>


    /// <summary>
    /// Deletes a courier by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to delete.</param>


    /// <summary>
    /// Deletes all courier entities from the data store.
    /// </summary>

}
