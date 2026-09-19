using Mirage.Math.Vectors;

namespace Mirage.Math.Geometry;

/// <summary>
/// Represents a plane in three-dimensional space.
/// </summary>
public readonly struct Plane : IEquatable<Plane>
{
    /// <summary>
    /// Gets the normal vector of the plane.
    /// </summary>
    public readonly Vector3D Normal;

    /// <summary>
    /// Gets the distance of the plane from the origin.
    /// </summary>
    public readonly double Distance;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plane"/> struct.
    /// </summary>
    public Plane(Vector3D normal, double distance)
    {
        Normal = normal;
        Distance = distance;
    }

    /// <summary>
    /// Gets the signed distance from a point to the plane.
    /// </summary>
    public double DistanceTo(Vector3D point) => Vector3D.Dot(Normal, point) + Distance;

    /// <summary>
    /// Determines whether a point lies on the plane.
    /// </summary>
    public bool Contains(Vector3D point) => DistanceTo(point) == 0;

    /// <summary>
    /// Determines whether two planes are equal.
    /// </summary>
    public bool Equals(Plane other) => Normal == other.Normal && Distance == other.Distance;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Plane other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Normal, Distance);

    /// <summary>
    /// Returns a string representation of the plane.
    /// </summary>
    public override string ToString() => $"Plane({Normal}, {Distance})";

    /// <summary>
    /// Determines whether two planes are equal.
    /// </summary>
    public static bool operator ==(Plane left, Plane right) => left.Equals(right);

    /// <summary>
    /// Determines whether two planes are not equal.
    /// </summary>
    public static bool operator !=(Plane left, Plane right) => !left.Equals(right);
}
