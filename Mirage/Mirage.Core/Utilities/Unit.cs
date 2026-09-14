namespace Mirage.Core.Utilities;

/// <summary>
/// Represents a type with a single possible value and no associated data.
/// </summary>
/// <remarks>
/// <see cref="Unit"/> is useful when an operation needs to represent the
/// presence of a value without carrying any data.
/// </remarks>
public readonly struct Unit
{
    /// <summary>
    /// Gets the single value of <see cref="Unit"/>.
    /// </summary>
    public static readonly Unit Value = new();
}
