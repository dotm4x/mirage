using Mirage.Core.Exceptions;

namespace Mirage.Core.Interfaces;

/// <summary>
/// Defines an object that can be destroyed when it is no longer needed.
/// </summary>
public interface IDestroyable
{
    /// <summary>
    /// Gets a value indicating whether the object has been destroyed.
    /// </summary>
    bool Destroyed { get; }

    /// <summary>
    /// Destroys the object.
    /// </summary>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the object has already been destroyed.
    /// </exception>
    void Destroy();
}
