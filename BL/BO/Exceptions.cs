using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BO;

/// <summary>
/// Thrown when attempting to access a BO instance that does not exist.
/// </summary>
[Serializable]
public class BlDoesNotExistException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlDoesNotExistException"/> class
    /// with a default error message.
    /// </summary>
    public BlDoesNotExistException()
        : base("Business entity does not exist.") { }

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
/// Thrown when attempting to create a BO instance that already exists.
/// </summary>
[Serializable]
public class BlAlreadyExistsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlAlreadyExistsException"/> class
    /// with a default error message.
    /// </summary>
    public BlAlreadyExistsException()
        : base("Business entity already exists.") { }

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
/// Thrown when invalid data or parameters are provided to the BL layer.
/// </summary>
[Serializable]
public class BlInvalidInputException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidInputException"/> class
    /// with a default error message.
    /// </summary>
    public BlInvalidInputException()
        : base("Invalid input to business layer.") { }

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

/// <summary>
/// Thrown when a required property is missing or null when it should not be.
/// </summary>
[Serializable]
public class BlMissingPropertyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlMissingPropertyException"/> class
    /// with a default error message.
    /// </summary>
    public BlMissingPropertyException()
        : base("Required property is missing.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlMissingPropertyException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BlMissingPropertyException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlMissingPropertyException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public BlMissingPropertyException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlMissingPropertyException"/> class
    /// with serialized data.
    /// </summary>
    protected BlMissingPropertyException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

/// <summary>
/// Thrown when an XML file cannot be loaded or created.
/// </summary>
[Serializable]
public class BlXMLFileLoadCreateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlXMLFileLoadCreateException"/> class
    /// with a default error message.
    /// </summary>
    public BlXMLFileLoadCreateException()
        : base("Failed to load or create XML file.") { }

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

/// <summary>
/// Thrown when attempting to modify a delivery that is currently in progress.
/// </summary>
[Serializable]
public class BlDeliveryInProgressException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlDeliveryInProgressException"/> class
    /// with a default error message.
    /// </summary>
    public BlDeliveryInProgressException()
        : base("Cannot modify delivery while it is in progress.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlDeliveryInProgressException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BlDeliveryInProgressException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlDeliveryInProgressException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public BlDeliveryInProgressException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlDeliveryInProgressException"/> class
    /// with serialized data.
    /// </summary>
    protected BlDeliveryInProgressException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

/// <summary>
/// Thrown when an invalid email address format is provided.
/// </summary>
[Serializable]
public class BlInvalidEmailException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidEmailException"/> class
    /// with a default error message.
    /// </summary>
    public BlInvalidEmailException()
        : base("Invalid email address format.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidEmailException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BlInvalidEmailException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidEmailException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public BlInvalidEmailException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidEmailException"/> class
    /// with serialized data.
    /// </summary>
    protected BlInvalidEmailException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

/// <summary>
/// Thrown when an invalid address is provided.
/// </summary>
[Serializable]
public class BlInvalidAddressException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidAddressException"/> class
    /// with a default error message.
    /// </summary>
    public BlInvalidAddressException()
        : base("Invalid address.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidAddressException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BlInvalidAddressException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidAddressException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public BlInvalidAddressException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlInvalidAddressException"/> class
    /// with serialized data.
    /// </summary>
    protected BlInvalidAddressException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
