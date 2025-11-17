using System;
using System.Runtime.Serialization;

namespace DO;
/* Used chatGPT to add comments and recommended constructors to exception classes (despite not being in the stage 2 worksheet,
 our instructor said to include them)
*/
/// <summary>
/// Thrown when attempting to access a DAL instance that does not exist.
/// </summary>
[Serializable]
public class DalDoesNotExistException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class
    /// with a default error message.
    /// </summary>
    public DalDoesNotExistException()
        : base("DAL does not exist.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DalDoesNotExistException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public DalDoesNotExistException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class
    /// with serialized data.
    /// </summary>
    protected DalDoesNotExistException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

/// <summary>
/// Thrown when attempting to create a DAL instance that already exists.
/// </summary>
[Serializable]
public class DalAlreadyExistsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class
    /// with a default error message.
    /// </summary>
    public DalAlreadyExistsException()
        : base("DAL already exists.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DalAlreadyExistsException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public DalAlreadyExistsException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class
    /// with serialized data.
    /// </summary>
    protected DalAlreadyExistsException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

/// <summary>
/// Thrown when invalid data or parameters are provided to the DAL layer.
/// </summary>
[Serializable]
public class DalInvalidInputException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DalInvalidInputException"/> class
    /// with a default error message.
    /// </summary>
    public DalInvalidInputException()
        : base("Invalid input to DAL.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalInvalidInputException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DalInvalidInputException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalInvalidInputException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public DalInvalidInputException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DalInvalidInputException"/> class
    /// with serialized data.
    /// </summary>
    protected DalInvalidInputException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
