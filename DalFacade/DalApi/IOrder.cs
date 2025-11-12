using DO;

namespace DalApi;

/// <summary>
/// Interface for managing Order entities in the data access layer.
/// Provides CRUD operations for Order objects.
/// </summary>
/// <remarks>
/// This interface extends <see cref="ICrud{T}"/> to provide standard
/// Create, Read, Update, and Delete operations for <see cref="Order"/> entities.
/// Implementations of this interface handle the persistence and retrieval
/// of order data in the underlying data store.
/// </remarks>
public interface IOrder : ICrud<Order>{}
