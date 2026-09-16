    namespace Mirage.Math.Vectors;

    /// <summary>
    /// Represents a three-dimensional vector with X, Y, and Z components.
    /// </summary>
    public readonly struct Vector3(double x = 0, double y = 0, double z = 0)
        : IVector<Vector3>
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

        /// <inheritdoc />
        public double Length
            => System.Math.Sqrt(LengthSquared);

        /// <inheritdoc />
        public double LengthSquared
            => X * X + Y * Y + Z * Z;

        /// <inheritdoc />
        public Vector3 Normalized
            => this / Length;

        /// <inheritdoc />
        public static Vector3 operator +(Vector3 left, Vector3 right)
            => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        /// <inheritdoc />
        public static Vector3 operator -(Vector3 left, Vector3 right)
            => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        /// <inheritdoc />
        public static Vector3 operator -(Vector3 value)
            => new(-value.X, -value.Y, -value.Z);

        /// <inheritdoc />
        public static Vector3 operator *(Vector3 value, double scalar)
            => new(value.X * scalar, value.Y * scalar, value.Z * scalar);

        /// <summary>
        /// Multiplies a scalar by a vector.
        /// </summary>
        public static Vector3 operator *(double scalar, Vector3 value)
            => value * scalar;

        /// <inheritdoc />
        public static Vector3 operator /(Vector3 value, double scalar)
            => new(value.X / scalar, value.Y / scalar, value.Z / scalar);

        /// <inheritdoc />
        public static bool operator ==(Vector3 left, Vector3 right)
            => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(Vector3 left, Vector3 right)
            => !left.Equals(right);

        /// <inheritdoc />
        public static double Dot(Vector3 left, Vector3 right)
            => left.X * right.X
            + left.Y * right.Y
            + left.Z * right.Z;

        /// <inheritdoc />
        public static double Distance(Vector3 left, Vector3 right)
            => (left - right).Length;

        /// <inheritdoc />
        public static double DistanceSquared(Vector3 left, Vector3 right)
            => (left - right).LengthSquared;

        /// <inheritdoc />
        public static Vector3 Lerp(Vector3 start, Vector3 end, double amount)
            => start + (end - start) * amount;

        /// <inheritdoc />
        public static Vector3 Min(Vector3 left, Vector3 right)
            => new(
                System.Math.Min(left.X, right.X),
                System.Math.Min(left.Y, right.Y),
                System.Math.Min(left.Z, right.Z)
            );

        /// <inheritdoc />
        public static Vector3 Max(Vector3 left, Vector3 right)
            => new(
                System.Math.Max(left.X, right.X),
                System.Math.Max(left.Y, right.Y),
                System.Math.Max(left.Z, right.Z)
            );

        /// <inheritdoc />
        public static Vector3 Clamp(
            Vector3 value,
            Vector3 minimum,
            Vector3 maximum)
            => new(
                System.Math.Clamp(value.X, minimum.X, maximum.X),
                System.Math.Clamp(value.Y, minimum.Y, maximum.Y),
                System.Math.Clamp(value.Z, minimum.Z, maximum.Z)
            );

        /// <inheritdoc />
        public static Vector3 Cross(Vector3 left, Vector3 right)
            => new(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X
            );

        /// <inheritdoc />
        public bool Equals(Vector3 other)
            => X == other.X
            && Y == other.Y
            && Z == other.Z;

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is Vector3 other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(X, Y, Z);
    }

