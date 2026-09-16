namespace Mirage.Math.Vectors;

/// <summary>
/// Represents a four-dimensional vector with X, Y, Z, and W components.
/// </summary>
public readonly struct Vector4(double x = 0, double y = 0, double z = 0, double w = 0)
    : IVector<Vector4>
{
    /// <summary>
    /// Gets the X component of the vector.
    /// </summary>
    public readonly double X = x;

    /// <summary>
    /// Gets the Y component of the vector.
    /// </summary>
    public readonly double Y = y;

    /// <summary>
    /// Gets the Z component of the vector.
    /// </summary>
    public readonly double Z = z;

    /// <summary>
    /// Gets the W component of the vector.
    /// </summary>
    public readonly double W = w;

    /// <inheritdoc />
    public double Length
        => System.Math.Sqrt(LengthSquared);

    /// <inheritdoc />
    public double LengthSquared
        => X * X + Y * Y + Z * Z + W * W;

    /// <inheritdoc />
    public Vector4 Normalized
        => this / Length;

    /// <inheritdoc />
    public static Vector4 operator +(Vector4 left, Vector4 right)
        => new(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z,
            left.W + right.W
        );

    /// <inheritdoc />
    public static Vector4 operator -(Vector4 left, Vector4 right)
        => new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z,
            left.W - right.W
        );

    /// <inheritdoc />
    public static Vector4 operator -(Vector4 value)
        => new(
            -value.X,
            -value.Y,
            -value.Z,
            -value.W
        );

    /// <inheritdoc />
    public static Vector4 operator *(Vector4 value, double scalar)
        => new(
            value.X * scalar,
            value.Y * scalar,
            value.Z * scalar,
            value.W * scalar
        );

    /// <summary>
    /// Multiplies a scalar by a vector.
    /// </summary>
    public static Vector4 operator *(double scalar, Vector4 value)
        => value * scalar;

    /// <inheritdoc />
    public static Vector4 operator /(Vector4 value, double scalar)
        => new(
            value.X / scalar,
            value.Y / scalar,
            value.Z / scalar,
            value.W / scalar
        );

    /// <inheritdoc />
    public static bool operator ==(Vector4 left, Vector4 right)
        => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(Vector4 left, Vector4 right)
        => !left.Equals(right);

    /// <inheritdoc />
    public static double Dot(Vector4 left, Vector4 right)
        => left.X * right.X
        + left.Y * right.Y
        + left.Z * right.Z
        + left.W * right.W;

    /// <inheritdoc />
    public static double Distance(Vector4 left, Vector4 right)
        => (left - right).Length;

    /// <inheritdoc />
    public static double DistanceSquared(Vector4 left, Vector4 right)
        => (left - right).LengthSquared;

    /// <inheritdoc />
    public static Vector4 Lerp(Vector4 start, Vector4 end, double amount)
        => start + (end - start) * amount;

    /// <inheritdoc />
    public static Vector4 Min(Vector4 left, Vector4 right)
        => new(
            System.Math.Min(left.X, right.X),
            System.Math.Min(left.Y, right.Y),
            System.Math.Min(left.Z, right.Z),
            System.Math.Min(left.W, right.W)
        );

    /// <inheritdoc />
    public static Vector4 Max(Vector4 left, Vector4 right)
        => new(
            System.Math.Max(left.X, right.X),
            System.Math.Max(left.Y, right.Y),
            System.Math.Max(left.Z, right.Z),
            System.Math.Max(left.W, right.W)
        );

    /// <inheritdoc />
    public static Vector4 Clamp(
        Vector4 value,
        Vector4 minimum,
        Vector4 maximum)
        => new(
            System.Math.Clamp(value.X, minimum.X, maximum.X),
            System.Math.Clamp(value.Y, minimum.Y, maximum.Y),
            System.Math.Clamp(value.Z, minimum.Z, maximum.Z),
            System.Math.Clamp(value.W, minimum.W, maximum.W)
        );

    /// <inheritdoc />
    public static Vector4 Cross(Vector4 left, Vector4 right)
        => new(
            left.Y * right.Z - left.Z * right.Y,
            left.Z * right.X - left.X * right.Z,
            left.X * right.Y - left.Y * right.X
        );

    /// <inheritdoc />
    public bool Equals(Vector4 other)
        => X == other.X
        && Y == other.Y
        && Z == other.Z
        && W == other.W;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Vector4 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z, W);
}