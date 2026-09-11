namespace Mirage.Core.Exceptions;

/// <summary>
/// Represents an exception thrown when an operation is attempted on an object
/// that has already been destroyed.
/// </summary>
public class DestroyedObjectException(string message) : InvalidOperationException(message) { }
