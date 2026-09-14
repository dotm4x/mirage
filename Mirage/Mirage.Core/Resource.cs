using Mirage.Core.Lifecycle;

namespace Mirage.Core;

/// <summary>
/// Represents reusable data that can be shared between objects.
/// </summary>
/// <remarks>
/// A resource can be destroyed when it is no longer needed.
/// </remarks>
public abstract class Resource : Destroyable { }
