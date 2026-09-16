using Mirage.Math.Vectors;

namespace Mirage.Math.Geometry;

/// <summary>
/// Represents an axis-aligned rectangle in two-dimensional space.
/// </summary>
public readonly struct Rect : IEquatable<Rect>
{
    /// <summary>
    /// Gets the minimum point of the rectangle.
    /// </summary>
    public readonly Vector2 Minimum;

    /// <summary>
    /// Gets the maximum point of the rectangle.
    /// </summary>
    public readonly Vector2 Maximum;

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct.
    /// </summary>
    public Rect(Vector2 minimum, Vector2 maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Gets the width of the rectangle.
    /// </summary>
    public double Width
        => Maximum.X - Minimum.X;

    /// <summary>
    /// Gets the height of the rectangle.
    /// </summary>
    public double Height
        => Maximum.Y - Minimum.Y;

    /// <summary>
    /// Gets the center of the rectangle.
    /// </summary>
    public Vector2 Center
        => (Minimum + Maximum) / 2;

    /// <summary>
    /// Gets the area of the rectangle.
    /// </summary>
    public double Area
        => Width * Height;

    /// <summary>
    /// Determines whether a point is inside or on the rectangle.
    /// </summary>
    public bool Contains(Vector2 point)
        => point.X >= Minimum.X
        && point.X <= Maximum.X
        && point.Y >= Minimum.Y
        && point.Y <= Maximum.Y;

    /// <summary>
    /// Determines whether this rectangle intersects another rectangle.
    /// </summary>
    public bool Intersects(Rect other)
        => Minimum.X <= other.Maximum.X
        && Maximum.X >= other.Minimum.X
        && Minimum.Y <= other.Maximum.Y
        && Maximum.Y >= other.Minimum.Y;

    /// <inheritdoc />
    public bool Equals(Rect other)
        => Minimum == other.Minimum
        && Maximum == other.Maximum;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Rect other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Minimum, Maximum);

    /// <summary>
    /// Determines whether two rectangles are equal.
    /// </summary>
    public static bool operator ==(Rect left, Rect right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two rectangles are not equal.
    /// </summary>
    public static bool operator !=(Rect left, Rect right)
        => !left.Equals(right);
}