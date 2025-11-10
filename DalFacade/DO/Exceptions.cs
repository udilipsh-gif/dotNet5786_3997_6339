using System.Runtime.Serialization;

namespace DO;

[Serializable]
public class DalDoesNotExistException : Exception
{
    public int EntityId { get; }

    public DalDoesNotExistException() : base() { }

    public DalDoesNotExistException(string message) : base(message) { }


    public DalDoesNotExistException(int entityId) : base()
    {
        EntityId = entityId;
    }

    public DalDoesNotExistException(string message, Exception inner) : base(message, inner) { }

    public DalDoesNotExistException(int entityId, string message) : base(message)
    {
        EntityId = entityId;
    }
    public override string ToString() =>
        $"DalDoesNotExistException: Entity with ID {EntityId} does not exist.\n";
}

[Serializable]
public class DalAlreadyExistsException : Exception
{
    public int EntityId { get; }

    public DalAlreadyExistsException() : base() { }

    public DalAlreadyExistsException(string message) : base(message) { }


    public DalAlreadyExistsException(int entityId) : base()
    {
        EntityId = entityId;
    }

    public DalAlreadyExistsException(string message, Exception inner) : base(message, inner) { }

    public DalAlreadyExistsException(int entityId, string message) : base(message)
    {
        EntityId = entityId;
    }

    public override string ToString() =>
        $"DalAlreadyExistsException: Entity with ID {EntityId} already exists.\n";
}

[Serializable]
public class DalValueIsNotValid : Exception
{
    public string? EntityValue { get; }

    public DalValueIsNotValid() : base() { }

    public DalValueIsNotValid(string entityValue) : base() {
        EntityValue = entityValue;
    }
    public DalValueIsNotValid(string message, Exception inner) : base(message, inner) { }

    public override string ToString() =>
        $"DalValueIsNotValid: the {EntityValue} is not valid\n";
}

[Serializable]
public class DalisNotAvailable : Exception
{
    public string? TypeValue { get; }

    public DalisNotAvailable() : base() { }

    public DalisNotAvailable(string typeValue) : base()
    {
        TypeValue = typeValue;
    }
    public DalisNotAvailable(string message, Exception inner) : base(message, inner) { }

    public override string ToString() =>
        $"DalisNotAvailable: No {TypeValue} available\n";
}

[Serializable]
public class DalErrorConfig : Exception
{
    public DalErrorConfig() : base() { }

    public DalErrorConfig(string message) : base(message) { }
    
    public DalErrorConfig(string message, Exception inner) : base(message, inner) { }

    public override string ToString() =>
        $"DalErrorConfig: {Message}\n";
}