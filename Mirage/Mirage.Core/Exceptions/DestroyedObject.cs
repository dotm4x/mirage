namespace Mirage.Core.Exceptions;

public class DestroyedObjectException(string message) : InvalidOperationException(message) { }
