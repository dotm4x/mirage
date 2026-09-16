using Mirage.Math.Vectors;

namespace Mirage.Math.Geometry;

/// <summary>
/// Represents an infinite line in three-dimensional space.
/// </summary>
public readonly struct Line : IEquatable<Line>
{
    /// <summary>
    /// Gets the origin point of the line.
    /// </summary>
    public readonly Vector3 Origin;

    /// <summary>
    /// Gets the direction of the line.
    /// </summary>
    public readonly Vector3 Direction;

    /// <summary>
    /// Initializes a new instance of the <see cref="Line"/> struct.
    /// </summary>
    public Line(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = direction;
    }

    /// <summary>
    /// Gets a point on the line at the specified parameter.
    /// </summary>
    public Vector3 GetPoint(double distance)
        => Origin + Direction * distance;

    /// <inheritdoc />
    public bool Equals(Line other)
        => Origin == other.Origin
           && Direction == other.Direction;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Line other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Origin, Direction);

    /// <summary>
    /// Determines whether two lines are equal.
    /// </summary>
    public static bool operator ==(Line left, Line right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two lines are not equal.
    /// </summary>
    public static bool operator !=(Line left, Line right)
        => !left.Equals(right);
}