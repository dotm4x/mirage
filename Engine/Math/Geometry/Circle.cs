using Mirage.Math.Vectors;

namespace Mirage.Math.Geometry;

/// <summary>
/// Represents a circle in two-dimensional space.
/// </summary>
public readonly struct Circle : IEquatable<Circle>
{
    /// <summary>
    /// Gets the center point of the circle.
    /// </summary>
    public readonly Vector2 Center;

    /// <summary>
    /// Gets the radius of the circle.
    /// </summary>
    public readonly double Radius;

    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> struct.
    /// </summary>
    public Circle(Vector2 center, double radius)
    {
        Center = center;
        Radius = radius;
    }

    /// <summary>
    /// Gets the diameter of the circle.
    /// </summary>
    public double Diameter => Radius * 2;

    /// <summary>
    /// Gets the circumference of the circle.
    /// </summary>
    public double Circumference => 2 * System.Math.PI * Radius;

    /// <summary>
    /// Gets the area of the circle.
    /// </summary>
    public double Area => System.Math.PI * Radius * Radius;

    /// <summary>
    /// Determines whether a point is inside or on the circle.
    /// </summary>
    public bool Contains(Vector2 point) =>
        Vector2.DistanceSquared(Center, point) <= Radius * Radius;

    /// <inheritdoc />
    public bool Equals(Circle other) => Center == other.Center && Radius == other.Radius;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Circle other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Center, Radius);

    /// <summary>
    /// Returns a string representation of the circle.
    /// </summary>
    public override string ToString() => $"Circle({Center}, {Radius})";

    /// <summary>
    /// Determines whether two circles are equal.
    /// </summary>
    public static bool operator ==(Circle left, Circle right) => left.Equals(right);

    /// <summary>
    /// Determines whether two circles are not equal.
    /// </summary>
    public static bool operator !=(Circle left, Circle right) => !left.Equals(right);
}
