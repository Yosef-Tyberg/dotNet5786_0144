using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BL.BO;

/// <summary>
/// Thrown when attempting to access a Bl instance that does not exist.
/// </summary>
[Serializable]
public class BlDoesNotExistException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlDoesNotExistException"/> class
    /// with a default error message.
    /// </summary>
    public BlDoesNotExistException()
        : base("Bl does not exist.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlDoesNotExistException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BlDoesNotExistException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlDoesNotExistException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public BlDoesNotExistException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlDoesNotExistException"/> class
    /// with serialized data.
    /// </summary>
    protected BlDoesNotExistException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

/// <summary>
/// Thrown when attempting to create a Bl instance that already exists.
/// </summary>
[Serializable]
public class BlAlreadyExistsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlAlreadyExistsException"/> class
    /// with a default error message.
    /// </summary>
    public BlAlreadyExistsException()
        : base("Bl already exists.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlAlreadyExistsException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BlAlreadyExistsException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlAlreadyExistsException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public BlAlreadyExistsException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlAlreadyExistsException"/> class
    /// with serialized data.
    /// </summary>
    protected BlAlreadyExistsException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

/// <summary>
/// Thrown when invalid data or parameters are provided to the Bl layer.
/// </summary>
[Serializable]
public class BlInvalidInputException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidInputException"/> class
    /// with a default error message.
    /// </summary>
    public BlInvalidInputException()
        : base("Invalid input to Bl.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidInputException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BlInvalidInputException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidInputException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public BlInvalidInputException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidInputException"/> class
    /// with serialized data.
    /// </summary>
    protected BlInvalidInputException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

public class BlXMLFileLoadCreateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlXMLFileLoadCreateException"/> class
    /// with a default error message.
    /// </summary>
    public BlXMLFileLoadCreateException()
        : base("Bl does not exist.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlXMLFileLoadCreateException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BlXMLFileLoadCreateException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlXMLFileLoadCreateException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public BlXMLFileLoadCreateException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlXMLFileLoadCreateException"/> class
    /// with serialized data.
    /// </summary>
    protected BlXMLFileLoadCreateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
