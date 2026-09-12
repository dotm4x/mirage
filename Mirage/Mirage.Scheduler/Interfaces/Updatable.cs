namespace Mirage.Core.Interfaces;

/// <summary>
/// Defines an object that can be updated.
/// </summary>
public interface IUpdatable
{
    /// <summary>
    /// Updates the object.
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed time, in seconds, since the previous update.
    /// </param>
    void Update(double deltaTime);
}
