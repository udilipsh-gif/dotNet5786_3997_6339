using DO;

namespace DalApi;

/// <summary>
/// Data access interface for managing <see cref="DO.Courier"/> entities in the DAL.
/// </summary>
/// <remarks>
/// This interface extends <see cref="ICrud{T}"/> to provide standard CRUD operations
/// for courier entities. It inherits methods for creating, reading, updating, and deleting
/// courier records from the data store without adding any courier-specific operations.
/// </remarks>
public interface ICourier : ICrud<Courier>
{
    /// <summary>
    /// Creates a new courier entity in the data store.
    /// </summary>
    /// <param name="item">The <see cref="DO.Courier"/> instance to create.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
    /// <remarks>
    /// Inherited from <see cref="ICrud{T}.Create"/>. Adds a new courier to the data store
    /// with all required fields including personal details, contact information, and delivery preferences.
    /// </remarks>

    /// <summary>
    /// Reads a courier by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier.</param>
    /// <returns>The <see cref="DO.Courier"/> if found; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// Inherited from <see cref="ICrud{T}.Read"/>. Retrieves a single courier entity
    /// based on the provided unique identifier.
    /// </remarks>

    /// <summary>
    /// Reads all courier entities.
    /// </summary>
    /// <returns>A list containing all couriers in the data store.</returns>
    /// <remarks>
    /// Inherited from <see cref="ICrud{T}.ReadAll"/>. Returns a complete collection
    /// of all courier records, including both active and inactive couriers.
    /// </remarks>

    /// <summary>
    /// Updates an existing courier entity in the data store.
    /// </summary>
    /// <param name="item">The courier instance with updated values. The instance's Id identifies the entity to update.</param>
    /// <remarks>
    /// Inherited from <see cref="ICrud{T}.Update"/>. Modifies an existing courier's information
    /// such as contact details, active status, maximum delivery distance, or shipment type.
    /// The courier is identified by the Id property of the provided instance.
    /// </remarks>

    /// <summary>
    /// Deletes a courier by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to delete.</param>
    /// <remarks>
    /// Inherited from <see cref="ICrud{T}.Delete"/>. Removes a specific courier entity
    /// from the data store based on the provided identifier.
    /// </remarks>

    /// <summary>
    /// Deletes all courier entities from the data store.
    /// </summary>
    /// <remarks>
    /// Inherited from <see cref="ICrud{T}.DeleteAll"/>. Removes all courier records
    /// from the data store. This operation should be used with caution as it is irreversible.
    /// </remarks>
}
