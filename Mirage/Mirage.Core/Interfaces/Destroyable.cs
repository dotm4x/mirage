namespace Mirage.Core.Interfaces;

/// <summary>
///   Defines an object that can be destroyed or disposed of when it is no longer needed.
/// </summary>
public interface IDestroyable
{
  /// <summary>
  ///   Indicates whether the object has been destroyed.
  /// </summary>
  bool Disposed { get; }

  /// <summary>
  ///   Destroy the object.
  /// </summary>
  void Dispose();
}