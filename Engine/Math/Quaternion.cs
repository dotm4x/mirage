using Mirage.Math.Vectors;

namespace Mirage.Math;

/// <summary>
/// Represents a quaternion used to represent rotations in three-dimensional space.
/// </summary>
public readonly struct Quaternion(
    double x = 0,
    double y = 0,
    double z = 0,
    double w = 1)
    : IEquatable<Quaternion>
{
    /// <summary>
    /// Gets the X component of the quaternion.
    /// </summary>
    public readonly double X = x;

    /// <summary>
    /// Gets the Y component of the quaternion.
    /// </summary>
    public readonly double Y = y;

    /// <summary>
    /// Gets the Z component of the quaternion.
    /// </summary>
    public readonly double Z = z;

    /// <summary>
    /// Gets the W component of the quaternion.
    /// </summary>
    public readonly double W = w;

    /// <summary>
    /// Gets the identity quaternion.
    /// </summary>
    public static Quaternion Identity
        => new();

    /// <summary>
    /// Gets the length of the quaternion.
    /// </summary>
    public double Length
        => System.Math.Sqrt(LengthSquared);

    /// <summary>
    /// Gets the squared length of the quaternion.
    /// </summary>
    public double LengthSquared
        => X * X + Y * Y + Z * Z + W * W;

    /// <summary>
    /// Gets a normalized copy of the quaternion.
    /// </summary>
    public Quaternion Normalized
        => this / Length;

    /// <summary>
    /// Returns the conjugate of the quaternion.
    /// </summary>
    public Quaternion Conjugated
        => new(-X, -Y, -Z, W);

    /// <summary>
    /// Returns the inverse of the quaternion.
    /// </summary>
    public Quaternion Inversed
        => Conjugated / LengthSquared;

    /// <summary>
    /// Adds two quaternions.
    /// </summary>
    public static Quaternion operator +(Quaternion left, Quaternion right)
        => new(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z,
            left.W + right.W
        );

    /// <summary>
    /// Subtracts one quaternion from another.
    /// </summary>
    public static Quaternion operator -(Quaternion left, Quaternion right)
        => new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z,
            left.W - right.W
        );

    /// <summary>
    /// Negates a quaternion.
    /// </summary>
    public static Quaternion operator -(Quaternion value)
        => new(
            -value.X,
            -value.Y,
            -value.Z,
            -value.W
        );

    /// <summary>
    /// Multiplies two quaternions.
    /// </summary>
    public static Quaternion operator *(Quaternion left, Quaternion right)
        => new(
            left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y,
            left.W * right.Y - left.X * right.Z + left.Y * right.W + left.Z * right.X,
            left.W * right.Z + left.X * right.Y - left.Y * right.X + left.Z * right.W,
            left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z
        );

    /// <summary>
    /// Multiplies a quaternion by a scalar.
    /// </summary>
    public static Quaternion operator *(Quaternion value, double scalar)
        => new(
            value.X * scalar,
            value.Y * scalar,
            value.Z * scalar,
            value.W * scalar
        );

    /// <summary>
    /// Multiplies a scalar by a quaternion.
    /// </summary>
    public static Quaternion operator *(double scalar, Quaternion value)
        => value * scalar;

    /// <summary>
    /// Divides a quaternion by a scalar.
    /// </summary>
    public static Quaternion operator /(Quaternion value, double scalar)
        => new(
            value.X / scalar,
            value.Y / scalar,
            value.Z / scalar,
            value.W / scalar
        );

    /// <summary>
    /// Determines whether two quaternions are equal.
    /// </summary>
    public static bool operator ==(Quaternion left, Quaternion right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two quaternions are not equal.
    /// </summary>
    public static bool operator !=(Quaternion left, Quaternion right)
        => !left.Equals(right);

    /// <summary>
    /// Creates a quaternion from an axis and an angle in radians.
    /// </summary>
    public static Quaternion FromAxisAngle(Vector3 axis, double angle)
    {
        var halfAngle = angle / 2;
        var sine = System.Math.Sin(halfAngle);

        return new Quaternion(
            axis.X * sine,
            axis.Y * sine,
            axis.Z * sine,
            System.Math.Cos(halfAngle)
        );
    }

    /// <summary>
    /// Creates a quaternion from Euler angles in radians.
    /// </summary>
    public static Quaternion FromEulerAngles(
        double x,
        double y,
        double z)
    {
        var halfX = x / 2;
        var halfY = y / 2;
        var halfZ = z / 2;

        var sinX = System.Math.Sin(halfX);
        var cosX = System.Math.Cos(halfX);
        var sinY = System.Math.Sin(halfY);
        var cosY = System.Math.Cos(halfY);
        var sinZ = System.Math.Sin(halfZ);
        var cosZ = System.Math.Cos(halfZ);

        return new Quaternion(
            sinX * cosY * cosZ - cosX * sinY * sinZ,
            cosX * sinY * cosZ + sinX * cosY * sinZ,
            cosX * cosY * sinZ - sinX * sinY * cosZ,
            cosX * cosY * cosZ + sinX * sinY * sinZ
        );
    }

    /// <summary>
    /// Rotates a vector by this quaternion.
    /// </summary>
    public Vector3 Transform(Vector3 value)
    {
        Quaternion vector = new(value.X, value.Y, value.Z, 0);
        var result = this * vector * Inversed;

        return new Vector3(result.X, result.Y, result.Z);
    }

    /// <summary>
    /// Spherically interpolates between two quaternions.
    /// </summary>
    public static Quaternion Slerp(
        Quaternion start,
        Quaternion end,
        double amount)
    {
        var dot = start.X * end.X
                  + start.Y * end.Y
                  + start.Z * end.Z
                  + start.W * end.W;

        if (dot < 0)
        {
            end = -end;
            dot = -dot;
        }

        if (dot > 0.9995)
        {
            return (start + (end - start) * amount).Normalized;
        }

        var angle = System.Math.Acos(dot);
        var sine = System.Math.Sin(angle);

        var startWeight = System.Math.Sin((1 - amount) * angle) / sine;
        var endWeight = System.Math.Sin(amount * angle) / sine;

        return start * startWeight + end * endWeight;
    }

    /// <inheritdoc />
    public bool Equals(Quaternion other)
        => X == other.X
        && Y == other.Y
        && Z == other.Z
        && W == other.W;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Quaternion other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z, W);
}