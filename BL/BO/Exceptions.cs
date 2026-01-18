
using DO;

namespace BO;
//BlIncorrectPasswordException,BlInvalValueException,BlAlreadyExistsException, BlInvalidOperationException
[Serializable]
public class BlIncorrectPasswordException : Exception
{
    public BlIncorrectPasswordException() : base() { }
    public BlIncorrectPasswordException(string message = "Incorrect password") : base(message) { }
    public BlIncorrectPasswordException(string message, Exception inner) : base(message, inner) { }

    public override string ToString() =>
        $"BL Exception: Incorrect Password. {Message}\n {InnerException}\n";

}


[Serializable]

public class BlInvalidValueException : Exception
{
    public int? EntityId { get; }

    public BlInvalidValueException() : base() { }

    public BlInvalidValueException(string message) : base(message) { }

    public BlInvalidValueException(string message, Exception inner)
        : base(message, inner) { }

    public BlInvalidValueException(int entityId, string message = "Invalid id")
        : base(message)
    {
        EntityId = entityId;
    }

    public override string ToString() =>
     EntityId.HasValue
         ? $"BlInvalValueIdException: {Message}. ID={EntityId}."
         : $"BlInvalValueIdException: {Message}.";

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
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException() : base() { }
    public BlDoesNotExistException(string? message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException)
                : base(message, innerException) { }
    public override string ToString() =>
        $"BL Exception: Does Not Exist. {Message}\n {InnerException}\n";
}


[Serializable]
public class BlInvalidOperationException : Exception
{
    public BlInvalidOperationException() : base() { }
    public BlInvalidOperationException(string message) : base(message) { }
    public BlInvalidOperationException(string message, Exception innerException)
                : base(message, innerException) { }
    public override string ToString() =>
        $"BL Exception: Invalid Operation. {Message}\n {InnerException}\n";
}

[Serializable]
public class BlNoAccessException : Exception
{
    public BlNoAccessException() : base("You do not have permission to perform this action.") { }
    public BlNoAccessException(string message) : base(message) { }
    public BlNoAccessException(string message, Exception innerException)
                : base(message, innerException) { }
    public override string ToString() =>
        $"BL Exception: No Access. {Message}\n {InnerException}\n";
}

[Serializable]
public class BLTemporaryNotAvailableException : Exception
{
    public BLTemporaryNotAvailableException() : base("לא ניתן לעדכן כאשר הסימולטור פעיל.") { }
    public BLTemporaryNotAvailableException(string message) : base(message) { }
    public BLTemporaryNotAvailableException(string message, Exception innerException)
                : base(message, innerException) { }
    public override string ToString() =>
        $"BL Exception: No Access. {Message}\n {InnerException}\n";
}


[Serializable]
public class BLNoSendEmailException : Exception
{
    public BLNoSendEmailException() : base("Failed to send email.") { }
    public BLNoSendEmailException(string message) : base(message) { }
    public BLNoSendEmailException(string message, Exception innerException)
                : base(message, innerException) { }
    public override string ToString() =>
        $"BL Exception: No Send Email. {Message}\n {InnerException}\n";
}

[Serializable]
public class BLNoSendSmsException : Exception
{
    public BLNoSendSmsException() : base("Failed to send SMS.") { }
    public BLNoSendSmsException(string message) : base(message) { }
    public BLNoSendSmsException(string message, Exception innerException)
                : base(message, innerException) { }
    public override string ToString() =>
        $"BL Exception: No Send SMS. {Message}\n {InnerException}\n";
}