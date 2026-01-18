namespace BlImplementation;
using BlApi;

using Helpers;
using System;
using System.Threading.Tasks;

/// <summary>
/// Implements the <see cref="ICourier"/> interface and provides business logic operations for courier management
/// with authorization and access control.
/// </summary>
/// <remarks>
/// This class enforces security by validating user permissions (manager vs. courier self-service)
/// before delegating to the underlying manager layer.
/// </remarks>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Creates a new courier in the system.
    /// </summary>
    /// <param name="requesterId">The ID of the user attempting to create the courier (must be a manager).</param>
    /// <param name="boCourier">The courier business object containing details to create.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <exception cref="BO.BlInvalidValueException">Thrown when provided courier data is invalid.</exception>
    /// <exception cref="BO.BlAlreadyExistsException">Thrown when a courier with the same ID already exists.</exception>
    public void Create(int requesterId, BO.Courier boCourier)
    {
        if (!Tools.CheckManger(requesterId))
            throw new BO.BlNoAccessException();
        CourierManager.Create(boCourier);    
    }

    /// <summary>
    /// Retrieves a courier by its unique identifier.
    /// </summary>
    /// <param name="requesterId">The ID of the user attempting to read the courier (must be a manager).</param>
    /// <param name="courierId">The unique identifier of the courier to retrieve.</param>
    /// <returns>A <see cref="BO.Courier"/> instance if found.</returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    public async Task<BO.Courier?> Read(int requesterId, int courierId)
    {
        if (!Tools.CheckManger(requesterId) && requesterId != courierId)
            throw new BO.BlNoAccessException();

        var result = await CourierManager.Read(courierId);
        return result;
    }

    /// <summary>
    /// Updates an existing courier.
    /// </summary>
    /// <param name="requesterId">
    /// The ID of the user requesting the update. Must be a manager or the courier being updated.
    /// </param>
    /// <param name="boCourier">The courier business object with updated fields.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the requester is neither manager nor the courier itself.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    /// <exception cref="BO.BlInvalidValueException">Thrown when provided courier data is invalid.</exception>
    public void Update(int requesterId, BO.Courier boCourier)
    {
        if (!Tools.CheckManger(requesterId) && requesterId != boCourier.Id)
            throw new BO.BlNoAccessException();

        CourierManager.Update(requesterId, boCourier);
    }

    /// <summary>
    /// Deletes a courier by its unique identifier.
    /// </summary>
    /// <param name="requesterId">The ID of the user attempting to delete the courier (must be a manager).</param>
    /// <param name="courierId">The unique identifier of the courier to delete.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when the courier has active deliveries.</exception>
    public void Delete(int requesterId, int courierId)
    {
        if (!Tools.CheckManger(requesterId))
            throw new BO.BlNoAccessException();
        CourierManager.Delete(courierId);
    }

    /// <summary>
    /// Authenticates a user (manager or courier) using credentials.
    /// </summary>
    /// <param name="id">The unique identifier of the user attempting to log in.</param>
    /// <param name="password">The password provided for authentication.</param>
    /// <returns>
    /// Returns a role indicator string (e.g., "Manager" or "Courier") if authentication succeeded.
    /// </returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    /// <exception cref="BO.BlIncorrectPasswordException">Thrown when the password is incorrect.</exception>
    public string? Login(int id, string password)
    {
        return CourierManager.Login(id, password);
    }

    /// <summary>
    /// Retrieves all couriers with optional filtering by active status and sorting.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the list (must be a manager).</param>
    /// <param name="isActive">
    /// Optional filter:
    /// <list type="bullet">
    /// <item><description><c>true</c>: active couriers only</description></item>
    /// <item><description><c>false</c>: inactive couriers only</description></item>
    /// <item><description><c>null</c>: all couriers</description></item>
    /// </list>
    /// </param>
    /// <param name="sort">Optional sort field.</param>
    /// <returns>An enumerable of <see cref="BO.CourierInList"/> items.</returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    public async Task<IEnumerable<BO.CourierInList>> ReadAll(
        int requesterId,
        bool? isActive,
        BO.CourierFieldSort? sort)
    {
        if (!Tools.CheckManger(requesterId))
            throw new BO.BlNoAccessException("Only manager can access the list of couriers.");
        var result = await CourierManager.ReadAll(requesterId, isActive, sort);
        return result;
    }

    public void AddObserver(Action listObserver) =>
        CourierManager.Observer.AddListObserver(listObserver);

    public void RemoveObserver(Action listObserver) =>
        CourierManager.Observer.RemoveListObserver(listObserver);

    public void AddObserver(int id, Action observer) =>
        CourierManager.Observer.AddObserver(id, observer);

    public void RemoveObserver(int id, Action observer) =>
        CourierManager.Observer.RemoveObserver(id, observer);
}
