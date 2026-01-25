namespace BlApi;

/// <summary>
/// Defines courier-related operations available in the business logic layer.
/// </summary>
/// <remarks>
/// This interface includes authentication and CRUD operations for couriers, as well as list retrieval
/// with optional filtering and sorting.
/// </remarks>
public interface ICourier : IObservable
{
    /// <summary>
    /// Authenticates a user (manager or courier) using an ID and password.
    /// </summary>
    /// <param name="id">The unique identifier of the user attempting to log in.</param>
    /// <param name="password">The password provided for authentication.</param>
    /// <returns>
    /// Returns a role indicator string (e.g., "Manager" or "Courier") if authentication succeeds;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    /// <exception cref="BO.BlIncorrectPasswordException">Thrown when the password is incorrect.</exception>
    string? Login(int id, string password);

    /// <summary>
    /// Creates a new courier in the system.
    /// </summary>
    /// <param name="id">The ID of the user performing the operation (must be a manager).</param>
    /// <param name="boCourier">The courier object containing the details to create.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the requester is not a manager.</exception>
    /// <exception cref="BO.BlInvalidValueException">Thrown when provided courier data is invalid.</exception>
    /// <exception cref="BO.BlAlreadyExistsException">Thrown when a courier with the same ID already exists.</exception>
    void Create(int id, BO.Courier boCourier);

    /// <summary>
    /// Retrieves a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the user performing the operation (must be a manager).</param>
    /// <param name="courierId">The unique identifier of the courier to retrieve.</param>
    /// <returns>A <see cref="BO.Courier"/> instance if found.</returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the requester is not a manager.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    IAsyncEnumerable<BO.Courier?> Read(int id, int courierId);

    //BO.Courier? Read(int id, int courierId, string light);

    /// <summary>
    /// Updates an existing courier.
    /// </summary>
    /// <param name="id">
    /// The ID of the user performing the operation. Must be a manager or the courier itself.
    /// </param>
    /// <param name="boCourier">The courier object containing updated information.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the requester is neither manager nor the courier itself.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    /// <exception cref="BO.BlInvalidValueException">Thrown when provided courier data is invalid.</exception>
    void Update(int id, BO.Courier boCourier);

    /// <summary>
    /// Deletes a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the user performing the operation (must be a manager).</param>
    /// <param name="courierId">The unique identifier of the courier to delete.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the requester is not a manager.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when the courier has active deliveries.</exception>
    void Delete(int id, int courierId);

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
    /// <exception cref="BO.BlNoAccessException">Thrown when the requester is not a manager.</exception>
    Task<IEnumerable<BO.CourierInList>> ReadAll(
        int requesterId,
        bool? isActive,
        BO.CourierFieldSort? sort);
}
