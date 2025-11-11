using DO;
namespace DalApi;

/// <summary>
/// Data access interface for managing courier entities in the DAL layer.
/// </summary>
/// <remarks>
/// Extends <see cref="ICrud{T}"/> with type parameter <see cref="DO.Courier"/>.
/// Does not add any courier-specific methods.
/// </remarks>
public interface ICourier : ICrud<Courier>{}
