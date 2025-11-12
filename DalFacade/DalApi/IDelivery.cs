namespace DalApi;
using DO;

/// <summary>
/// Defines the contract for delivery data access operations.
/// </summary>
/// <remarks>
/// This interface provides CRUD operations for managing delivery entities in the data access layer.
/// It inherits from <see cref="ICrud{T}"/> to provide standard create, read, update, and delete functionality
/// for <see cref="Delivery"/> objects.
/// </remarks>
public interface IDelivery: ICrud<Delivery>{}
