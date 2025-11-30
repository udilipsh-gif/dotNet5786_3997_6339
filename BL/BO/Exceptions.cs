
using DO;

namespace BO;
[Serializable]


public class BlInvalidIdException  : Exception
{
    int EntityId { get; }
    public BlInvalidIdException() : base() { } 
    public BlInvalidIdException (string message) : base(message) { }

   public BlInvalidIdException (string message, Exception inner) : base(message, inner) { }

    public BlInvalidIdException (int entityId) : base()
    {
          EntityId = entityId;
    }

    public override string ToString() =>
        $"DalDoesNotExistException: Entity with ID {EntityId} does not exist.\n";
}

public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException() : base() { }
    public BlAlreadyExistsException(string message) : base(message) { }

    public BlAlreadyExistsException(string message, Exception inner) : base(message, inner) { }

    public override string ToString() =>
        $"Bl Exception: Already Exists. {Message}\n {InnerException}\n";
}

[Serializable]
public class DalException : Exception
{
    string Message { get; }
    public int EntityId { get; }
   
    public DalException(string message) : base(message) { }

    public DalException() : base() { }
    public DalException(DalAlreadyExistsException ex) { 
    Message = ex.ToString();
    }

    public DalException(Exception ex)
    {
        Message = ex.ToString();
    }

    public override string ToString() => $"DalException: {Message}\n";

}