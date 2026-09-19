namespace Mirage.Math.Vectors;

/// <summary>
/// Represents a two-dimensional vector with X and Y components.
/// </summary>
public readonly struct Vector2D(double x = 0, double y = 0) : IVector<Vector2D>
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
    public double Length => System.Math.Sqrt(LengthSquared);

    /// <inheritdoc />
    public double LengthSquared => X * X + Y * Y;

    /// <inheritdoc />
    public Vector2D Normalized => this / Length;

    /// <inheritdoc />
    public static Vector2D operator +(Vector2D left, Vector2D right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <inheritdoc />
    public static Vector2D operator -(Vector2D left, Vector2D right) =>
        new(left.X - right.X, left.Y - right.Y);

    /// <inheritdoc />
    public static Vector2D operator -(Vector2D value) => new(-value.X, -value.Y);

    /// <inheritdoc />
    public static Vector2D operator *(Vector2D value, double scalar) =>
        new(value.X * scalar, value.Y * scalar);

    /// <summary>
    /// Multiplies a scalar by a vector.
    /// </summary>
    public static Vector2D operator *(double scalar, Vector2D value) => value * scalar;

    /// <inheritdoc />
    public static Vector2D operator /(Vector2D value, double scalar) =>
        new(value.X / scalar, value.Y / scalar);

    /// <inheritdoc />
    public static bool operator ==(Vector2D left, Vector2D right) => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(Vector2D left, Vector2D right) => !left.Equals(right);

    /// <inheritdoc />
    public static double Dot(Vector2D left, Vector2D right) => left.X * right.X + left.Y * right.Y;

    /// <inheritdoc />
    public static double Distance(Vector2D left, Vector2D right) => (left - right).Length;

    /// <inheritdoc />
    public static double DistanceSquared(Vector2D left, Vector2D right) =>
        (left - right).LengthSquared;

    /// <inheritdoc />
    public static Vector2D Lerp(Vector2D start, Vector2D end, double amount) =>
        start + (end - start) * amount;

    /// <inheritdoc />
    public static Vector2D Min(Vector2D left, Vector2D right) =>
        new(System.Math.Min(left.X, right.X), System.Math.Min(left.Y, right.Y));

    /// <inheritdoc />
    public static Vector2D Max(Vector2D left, Vector2D right) =>
        new(System.Math.Max(left.X, right.X), System.Math.Max(left.Y, right.Y));

    /// <inheritdoc />
    public static Vector2D Clamp(Vector2D value, Vector2D minimum, Vector2D maximum) =>
        new(
            System.Math.Clamp(value.X, minimum.X, maximum.X),
            System.Math.Clamp(value.Y, minimum.Y, maximum.Y)
        );

    /// <inheritdoc />
    public static Vector2D Cross(Vector2D left, Vector2D right) =>
        new(left.Y - right.Y, right.X - left.X);

    /// <inheritdoc />
    public bool Equals(Vector2D other) => X == other.X && Y == other.Y;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Vector2D other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// Returns a string representation of the vector.
    /// </summary>
    public override string ToString() => $"Vector2D({X}, {Y})";
}
