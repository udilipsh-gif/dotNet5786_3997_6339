using System.Runtime.Serialization;
namespace DO;

/// <summary>
/// Custom exception thrown when a requested entity does not exist in the data access layer.
/// </summary>
[Serializable]
public class DalDoesNotExistException : Exception
{
    /// <summary>
    /// Gets the ID of the entity that does not exist.
    /// </summary>
    public int EntityId { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class.
    /// </summary>
    public DalDoesNotExistException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class with a specified error message.
    /// </summary>
    /// <param name="message"></param>
    public DalDoesNotExistException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class for a specific entity ID.
    /// </summary>
    /// <param name="entityId"></param>
    public DalDoesNotExistException(int entityId) : base()
    {
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class 
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public DalDoesNotExistException(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="message"></param>
    public DalDoesNotExistException(int entityId, string message) : base(message)
    {
        EntityId = entityId;
    }

    /// <summary>
    /// </summary>
    /// <returns>Returns a string that represents the current exception.</returns>
    /// </returns>
    public override string ToString() =>
        $"DalDoesNotExistException: Entity with ID {EntityId} does not exist.\n";
}

[Serializable]
public class DalAlreadyExistsException : Exception
{
    /// <summary>
    /// Gets the ID of the entity that already exists.
    /// </summary>
    public int EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class.
    /// </summary>
    public DalAlreadyExistsException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class 
    /// with a specified error message.
    /// </summary>
    /// <param name="message"></param>
    public DalAlreadyExistsException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class for a specific entity ID.
    /// </summary>
    /// <param name="entityId"></param>
    public DalAlreadyExistsException(int entityId) : base()
    {
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>

    public DalAlreadyExistsException(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="message"></param>
    public DalAlreadyExistsException(int entityId, string message) : base(message)
    {
        EntityId = entityId;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <returns>Returns a string that represents the current exception.
    /// </returns>
    public override string ToString() =>
        $"DalAlreadyExistsException: Entity with ID {EntityId} already exists.\n";
}

[Serializable]

public class DalValueIsNotValid : Exception
{
    /// <summary>
    /// Gets the value of the entity that is not valid.
    /// </summary>
    public string? EntityValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalValueIsNotValid"/> class.
    /// </summary>
    public DalValueIsNotValid() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalValueIsNotValid"/> class 
    /// for a specific entity value.
    /// </summary>
    /// <param name="entityValue"></param>
    public DalValueIsNotValid(string entityValue) : base() {
        EntityValue = entityValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalValueIsNotValid"/> class
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public DalValueIsNotValid(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns a string that represents the current exception.
    /// </returns>
    public override string ToString() =>
        $"DalValueIsNotValid: the {EntityValue} is not valid\n";
}

[Serializable]
public class DalisNotAvailable : Exception
{
    /// <summary>
    /// Gets the type value that is not available.
    /// </summary>
    public string? TypeValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalisNotAvailable"/> class.
    /// </summary>
    public DalisNotAvailable() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalisNotAvailable"/> class
    /// </summary>
    /// <param name="typeValue"></param>
    public DalisNotAvailable(string typeValue) : base()
    {
        TypeValue = typeValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalisNotAvailable"/> class
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public DalisNotAvailable(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns a string that represents the current exception.
    /// </returns>
    public override string ToString() =>
        $"DalisNotAvailable: No {TypeValue} available\n";
}

[Serializable]
public class DalErrorConfig : Exception
{
    /// <summary>
    /// Gets the configuration error message.
    /// </summary>
    public DalErrorConfig() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalErrorConfig"/> class
    /// </summary>
    /// <param name="message"></param>
    public DalErrorConfig(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalErrorConfig"/> class
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public DalErrorConfig(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns a string that represents the current exception.
    /// </returns>
    public override string ToString() =>
        $"DalErrorConfig: {Message}\n";
}