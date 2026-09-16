namespace Mirage.Math.Vectors;

/// <summary>
/// Represents a two-dimensional vector with X and Y components.
/// </summary>
public readonly struct Vector2(double x = 0, double y = 0)
    : IVector<Vector2>
{
    /// <summary>
    /// Gets the X component of the vector.
    /// </summary>
    public readonly double X = x;

    /// <summary>
    /// Gets the Y component of the vector.
    /// </summary>
    public readonly double Y = y;

    /// <inheritdoc />
    public double Length
        => System.Math.Sqrt(LengthSquared);

    /// <inheritdoc />
    public double LengthSquared
        => X * X + Y * Y;

    /// <inheritdoc />
    public Vector2 Normalized
        => this / Length;

    /// <inheritdoc />
    public static Vector2 operator +(Vector2 left, Vector2 right)
        => new(left.X + right.X, left.Y + right.Y);

    /// <inheritdoc />
    public static Vector2 operator -(Vector2 left, Vector2 right)
        => new(left.X - right.X, left.Y - right.Y);

    /// <inheritdoc />
    public static Vector2 operator -(Vector2 value)
        => new(-value.X, -value.Y);

    /// <inheritdoc />
    public static Vector2 operator *(Vector2 value, double scalar)
        => new(value.X * scalar, value.Y * scalar);

    /// <summary>
    /// Multiplies a scalar by a vector.
    /// </summary>
    public static Vector2 operator *(double scalar, Vector2 value)
        => value * scalar;

    /// <inheritdoc />
    public static Vector2 operator /(Vector2 value, double scalar)
        => new(value.X / scalar, value.Y / scalar);

    /// <inheritdoc />
    public static bool operator ==(Vector2 left, Vector2 right)
        => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(Vector2 left, Vector2 right)
        => !left.Equals(right);

    /// <inheritdoc />
    public static double Dot(Vector2 left, Vector2 right)
        => left.X * right.X
        + left.Y * right.Y;

    /// <inheritdoc />
    public static double Distance(Vector2 left, Vector2 right)
        => (left - right).Length;

    /// <inheritdoc />
    public static double DistanceSquared(Vector2 left, Vector2 right)
        => (left - right).LengthSquared;

    /// <inheritdoc />
    public static Vector2 Lerp(Vector2 start, Vector2 end, double amount)
        => start + (end - start) * amount;

    /// <inheritdoc />
    public static Vector2 Min(Vector2 left, Vector2 right)
        => new(
            System.Math.Min(left.X, right.X),
            System.Math.Min(left.Y, right.Y)
        );

    /// <inheritdoc />
    public static Vector2 Max(Vector2 left, Vector2 right)
        => new(
            System.Math.Max(left.X, right.X),
            System.Math.Max(left.Y, right.Y)
        );

    /// <inheritdoc />
    public static Vector2 Clamp(
        Vector2 value,
        Vector2 minimum,
        Vector2 maximum)
        => new(
            System.Math.Clamp(value.X, minimum.X, maximum.X),
            System.Math.Clamp(value.Y, minimum.Y, maximum.Y)
        );

    /// <inheritdoc />
    public static Vector2 Cross(Vector2 left, Vector2 right)
        => new(
            left.Y - right.Y,
            right.X - left.X
        );

    /// <inheritdoc />
    public bool Equals(Vector2 other)
        => X == other.X
        && Y == other.Y;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Vector2 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(X, Y);
}