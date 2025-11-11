using DO;
namespace DalApi;

/// <summary>
/// Defines a contract for basic CRUD (Create, Read, Update, Delete) operations on a data store for entities of type
/// <typeparamref name="T"/>.
/// </summary>
/// <remarks>This method updates the existing entity in the data store with the values from the provided <paramref
/// name="item"/>. Ensure that the entity exists in the data store before calling this method to avoid unexpected
/// behavior.</remarks>
/// <typeparam name="T">The type of entity the CRUD operations are performed on. Must be a reference type.</typeparam>
public interface ICrud<T> where T : class
{
    /// <summary>
    /// Adds a new entity object to the data access layer.
    /// </summary>
    /// <param name="item">The entity object to be added. Cannot be null.</param>
    void Create(T item); //Creates new entity object in DAL
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    T? Read(int id); //Reads entity object by its ID //stage 2
    /// <summary>
    /// Reads and returns the first element that satisfies the specified filter condition.
    /// </summary>
    /// <param name="filter">A function to test each element for a condition. The function should return <see langword="true"/> for the
    /// desired element.</param>
    /// <returns>The first element that matches the filter condition, or <see langword="null"/> if no such element is found.</returns>
    T? Read(Func<T, bool> filter); // stage 2
    /// <summary>
    /// Retrieves all entity objects, optionally filtered by a specified predicate.
    /// </summary>
    /// <param name="filter">An optional function to filter the entities. If provided, only entities for which the function returns <see
    /// langword="true"/> are included. If <see langword="null"/>, all entities are returned.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the filtered entity objects. If no filter is applied, returns all
    /// entities.</returns>

    //List<T> ReadAll(); //stage 1 only, Reads all entity objects //stage 1
    IEnumerable<T> ReadAll(Func<T, bool>? filter = null); // stage 2
    /// <summary>
    /// Updates the specified entity object in the data store.
    /// </summary>
    /// <remarks>This method updates the existing entity in the data store with the values from the provided
    /// <paramref name="item"/>. Ensure that the entity exists in the data store before calling this method to avoid
    /// unexpected behavior.</remarks>
    /// <param name="item">The entity object to update. Cannot be null.</param>
    void Update(T item); //Updates entity object
    /// <summary>
    /// Deletes the object with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the object to delete. Must be a positive integer.</param>
    void Delete(int id); //Deletes an object by its Id
    /// <summary>
    /// Deletes all entity objects from the data store.
    /// </summary>
    /// <remarks>This operation removes all entities without any filtering. Use with caution as it cannot be
    /// undone.</remarks>
    void DeleteAll(); //Delete all entity objects
}

