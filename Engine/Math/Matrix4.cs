using Mirage.Math.Vectors;

namespace Mirage.Math;

/// <summary>
/// Represents a four-by-four matrix used for three-dimensional transformations.
/// </summary>
public readonly struct Matrix4 : IEquatable<Matrix4>
{
    /// <summary>
    /// Gets the identity matrix.
    /// </summary>
    public static Matrix4 Identity => new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

    /// <summary>
    /// Gets the M11 component of the matrix.
    /// </summary>
    public readonly double M11;

    /// <summary>
    /// Gets the M12 component of the matrix.
    /// </summary>
    public readonly double M12;

    /// <summary>
    /// Gets the M13 component of the matrix.
    /// </summary>
    public readonly double M13;

    /// <summary>
    /// Gets the M14 component of the matrix.
    /// </summary>
    public readonly double M14;

    /// <summary>
    /// Gets the M21 component of the matrix.
    /// </summary>
    public readonly double M21;

    /// <summary>
    /// Gets the M22 component of the matrix.
    /// </summary>
    public readonly double M22;

    /// <summary>
    /// Gets the M23 component of the matrix.
    /// </summary>
    public readonly double M23;

    /// <summary>
    /// Gets the M24 component of the matrix.
    /// </summary>
    public readonly double M24;

    /// <summary>
    /// Gets the M31 component of the matrix.
    /// </summary>
    public readonly double M31;

    /// <summary>
    /// Gets the M32 component of the matrix.
    /// </summary>
    public readonly double M32;

    /// <summary>
    /// Gets the M33 component of the matrix.
    /// </summary>
    public readonly double M33;

    /// <summary>
    /// Gets the M34 component of the matrix.
    /// </summary>
    public readonly double M34;

    /// <summary>
    /// Gets the M41 component of the matrix.
    /// </summary>
    public readonly double M41;

    /// <summary>
    /// Gets the M42 component of the matrix.
    /// </summary>
    public readonly double M42;

    /// <summary>
    /// Gets the M43 component of the matrix.
    /// </summary>
    public readonly double M43;

    /// <summary>
    /// Gets the M44 component of the matrix.
    /// </summary>
    public readonly double M44;

    /// <summary>
    /// Initializes a new matrix from its individual components.
    /// </summary>
    public Matrix4(
        double m11,
        double m12,
        double m13,
        double m14,
        double m21,
        double m22,
        double m23,
        double m24,
        double m31,
        double m32,
        double m33,
        double m34,
        double m41,
        double m42,
        double m43,
        double m44
    )
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M14 = m14;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M24 = m24;
        M31 = m31;
        M32 = m32;
        M33 = m33;
        M34 = m34;
        M41 = m41;
        M42 = m42;
        M43 = m43;
        M44 = m44;
    }

    /// <summary>
    /// Adds two matrices.
    /// </summary>
    public static Matrix4 operator +(Matrix4 left, Matrix4 right) =>
        new(
            left.M11 + right.M11,
            left.M12 + right.M12,
            left.M13 + right.M13,
            left.M14 + right.M14,
            left.M21 + right.M21,
            left.M22 + right.M22,
            left.M23 + right.M23,
            left.M24 + right.M24,
            left.M31 + right.M31,
            left.M32 + right.M32,
            left.M33 + right.M33,
            left.M34 + right.M34,
            left.M41 + right.M41,
            left.M42 + right.M42,
            left.M43 + right.M43,
            left.M44 + right.M44
        );

    /// <summary>
    /// Subtracts one matrix from another.
    /// </summary>
    public static Matrix4 operator -(Matrix4 left, Matrix4 right) =>
        new(
            left.M11 - right.M11,
            left.M12 - right.M12,
            left.M13 - right.M13,
            left.M14 - right.M14,
            left.M21 - right.M21,
            left.M22 - right.M22,
            left.M23 - right.M23,
            left.M24 - right.M24,
            left.M31 - right.M31,
            left.M32 - right.M32,
            left.M33 - right.M33,
            left.M34 - right.M34,
            left.M41 - right.M41,
            left.M42 - right.M42,
            left.M43 - right.M43,
            left.M44 - right.M44
        );

    /// <summary>
    /// Negates a matrix.
    /// </summary>
    public static Matrix4 operator -(Matrix4 value) =>
        new(
            -value.M11,
            -value.M12,
            -value.M13,
            -value.M14,
            -value.M21,
            -value.M22,
            -value.M23,
            -value.M24,
            -value.M31,
            -value.M32,
            -value.M33,
            -value.M34,
            -value.M41,
            -value.M42,
            -value.M43,
            -value.M44
        );

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    public static Matrix4 operator *(Matrix4 left, Matrix4 right) =>
        new(
            left.M11 * right.M11
                + left.M12 * right.M21
                + left.M13 * right.M31
                + left.M14 * right.M41,
            left.M11 * right.M12
                + left.M12 * right.M22
                + left.M13 * right.M32
                + left.M14 * right.M42,
            left.M11 * right.M13
                + left.M12 * right.M23
                + left.M13 * right.M33
                + left.M14 * right.M43,
            left.M11 * right.M14
                + left.M12 * right.M24
                + left.M13 * right.M34
                + left.M14 * right.M44,
            left.M21 * right.M11
                + left.M22 * right.M21
                + left.M23 * right.M31
                + left.M24 * right.M41,
            left.M21 * right.M12
                + left.M22 * right.M22
                + left.M23 * right.M32
                + left.M24 * right.M42,
            left.M21 * right.M13
                + left.M22 * right.M23
                + left.M23 * right.M33
                + left.M24 * right.M43,
            left.M21 * right.M14
                + left.M22 * right.M24
                + left.M23 * right.M34
                + left.M24 * right.M44,
            left.M31 * right.M11
                + left.M32 * right.M21
                + left.M33 * right.M31
                + left.M34 * right.M41,
            left.M31 * right.M12
                + left.M32 * right.M22
                + left.M33 * right.M32
                + left.M34 * right.M42,
            left.M31 * right.M13
                + left.M32 * right.M23
                + left.M33 * right.M33
                + left.M34 * right.M43,
            left.M31 * right.M14
                + left.M32 * right.M24
                + left.M33 * right.M34
                + left.M34 * right.M44,
            left.M41 * right.M11
                + left.M42 * right.M21
                + left.M43 * right.M31
                + left.M44 * right.M41,
            left.M41 * right.M12
                + left.M42 * right.M22
                + left.M43 * right.M32
                + left.M44 * right.M42,
            left.M41 * right.M13
                + left.M42 * right.M23
                + left.M43 * right.M33
                + left.M44 * right.M43,
            left.M41 * right.M14
                + left.M42 * right.M24
                + left.M43 * right.M34
                + left.M44 * right.M44
        );

    /// <summary>
    /// Multiplies a matrix by a scalar.
    /// </summary>
    public static Matrix4 operator *(Matrix4 value, double scalar) =>
        new(
            value.M11 * scalar,
            value.M12 * scalar,
            value.M13 * scalar,
            value.M14 * scalar,
            value.M21 * scalar,
            value.M22 * scalar,
            value.M23 * scalar,
            value.M24 * scalar,
            value.M31 * scalar,
            value.M32 * scalar,
            value.M33 * scalar,
            value.M34 * scalar,
            value.M41 * scalar,
            value.M42 * scalar,
            value.M43 * scalar,
            value.M44 * scalar
        );

    /// <summary>
    /// Multiplies a scalar by a matrix.
    /// </summary>
    public static Matrix4 operator *(double scalar, Matrix4 value) => value * scalar;

    /// <summary>
    /// Divides a matrix by a scalar.
    /// </summary>
    public static Matrix4 operator /(Matrix4 value, double scalar) => value * (1 / scalar);

    /// <summary>
    /// Multiplies a matrix by a vector.
    /// </summary>
    public static Vector4 operator *(Matrix4 matrix, Vector4 vector) =>
        new(
            matrix.M11 * vector.X
                + matrix.M12 * vector.Y
                + matrix.M13 * vector.Z
                + matrix.M14 * vector.W,
            matrix.M21 * vector.X
                + matrix.M22 * vector.Y
                + matrix.M23 * vector.Z
                + matrix.M24 * vector.W,
            matrix.M31 * vector.X
                + matrix.M32 * vector.Y
                + matrix.M33 * vector.Z
                + matrix.M34 * vector.W,
            matrix.M41 * vector.X
                + matrix.M42 * vector.Y
                + matrix.M43 * vector.Z
                + matrix.M44 * vector.W
        );

    /// <summary>
    /// Determines whether two matrices are equal.
    /// </summary>
    public static bool operator ==(Matrix4 left, Matrix4 right) => left.Equals(right);

    /// <summary>
    /// Determines whether two matrices are not equal.
    /// </summary>
    public static bool operator !=(Matrix4 left, Matrix4 right) => !left.Equals(right);

    /// <summary>
    /// Creates a translation matrix.
    /// </summary>
    public static Matrix4 CreateTranslation(Vector3 translation) =>
        new(1, 0, 0, translation.X, 0, 1, 0, translation.Y, 0, 0, 1, translation.Z, 0, 0, 0, 1);

    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    public static Matrix4 CreateScale(Vector3 scale) =>
        new(scale.X, 0, 0, 0, 0, scale.Y, 0, 0, 0, 0, scale.Z, 0, 0, 0, 0, 1);

    /// <summary>
    /// Creates a rotation matrix from a quaternion.
    /// </summary>
    public static Matrix4 CreateRotation(Quaternion rotation)
    {
        var xx = rotation.X * rotation.X;
        var yy = rotation.Y * rotation.Y;
        var zz = rotation.Z * rotation.Z;

        var xy = rotation.X * rotation.Y;
        var xz = rotation.X * rotation.Z;
        var yz = rotation.Y * rotation.Z;

        var wx = rotation.W * rotation.X;
        var wy = rotation.W * rotation.Y;
        var wz = rotation.W * rotation.Z;

        return new Matrix4(
            1 - 2 * (yy + zz),
            2 * (xy - wz),
            2 * (xz + wy),
            0,
            2 * (xy + wz),
            1 - 2 * (xx + zz),
            2 * (yz - wx),
            0,
            2 * (xz - wy),
            2 * (yz + wx),
            1 - 2 * (xx + yy),
            0,
            0,
            0,
            0,
            1
        );
    }

    /// <summary>
    /// Returns the transpose of the matrix.
    /// </summary>
    public Matrix4 Transposed =>
        new(M11, M21, M31, M41, M12, M22, M32, M42, M13, M23, M33, M43, M14, M24, M34, M44);

    /// <summary>
    /// Gets the determinant of the matrix.
    /// </summary>
    public double Determinant
    {
        get
        {
            var a = M11 * M22 - M12 * M21;
            var b = M11 * M23 - M13 * M21;
            var c = M11 * M24 - M14 * M21;
            var d = M12 * M23 - M13 * M22;
            var e = M12 * M24 - M14 * M22;
            var f = M13 * M24 - M14 * M23;
            var g = M31 * M42 - M32 * M41;
            var h = M31 * M43 - M33 * M41;
            var i = M31 * M44 - M34 * M41;
            var j = M32 * M43 - M33 * M42;
            var k = M32 * M44 - M34 * M42;
            var l = M33 * M44 - M34 * M43;

            return a * l - b * k + c * j + d * i - e * h + f * g;
        }
    }

    /// <summary>
    /// Returns the inverse of the matrix.
    /// </summary>
    public Matrix4 Inversed => CreateInverse(this);

    private static Matrix4 CreateInverse(Matrix4 value)
    {
        var determinant = value.Determinant;

        return new Matrix4(
                value.M22 * (value.M33 * value.M44 - value.M34 * value.M43)
                    - value.M23 * (value.M32 * value.M44 - value.M34 * value.M42)
                    + value.M24 * (value.M32 * value.M43 - value.M33 * value.M42),
                -value.M12 * (value.M33 * value.M44 - value.M34 * value.M43)
                    + value.M13 * (value.M32 * value.M44 - value.M34 * value.M42)
                    - value.M14 * (value.M32 * value.M43 - value.M33 * value.M42),
                value.M12 * (value.M23 * value.M44 - value.M24 * value.M43)
                    - value.M13 * (value.M22 * value.M44 - value.M24 * value.M42)
                    + value.M14 * (value.M22 * value.M43 - value.M23 * value.M42),
                -value.M12 * (value.M23 * value.M34 - value.M24 * value.M33)
                    + value.M13 * (value.M22 * value.M34 - value.M24 * value.M32)
                    - value.M14 * (value.M22 * value.M33 - value.M23 * value.M32),
                -value.M21 * (value.M33 * value.M44 - value.M34 * value.M43)
                    + value.M23 * (value.M31 * value.M44 - value.M34 * value.M41)
                    - value.M24 * (value.M31 * value.M43 - value.M33 * value.M41),
                value.M11 * (value.M33 * value.M44 - value.M34 * value.M43)
                    - value.M13 * (value.M31 * value.M44 - value.M34 * value.M41)
                    + value.M14 * (value.M31 * value.M43 - value.M33 * value.M41),
                -value.M11 * (value.M23 * value.M44 - value.M24 * value.M43)
                    + value.M13 * (value.M21 * value.M44 - value.M24 * value.M41)
                    - value.M14 * (value.M21 * value.M43 - value.M23 * value.M41),
                value.M11 * (value.M23 * value.M34 - value.M24 * value.M33)
                    - value.M13 * (value.M21 * value.M34 - value.M24 * value.M31)
                    + value.M14 * (value.M21 * value.M33 - value.M23 * value.M31),
                value.M21 * (value.M32 * value.M44 - value.M34 * value.M42)
                    - value.M22 * (value.M31 * value.M44 - value.M34 * value.M41)
                    + value.M24 * (value.M31 * value.M42 - value.M32 * value.M41),
                -value.M11 * (value.M32 * value.M44 - value.M34 * value.M42)
                    + value.M12 * (value.M31 * value.M44 - value.M34 * value.M41)
                    - value.M14 * (value.M31 * value.M42 - value.M32 * value.M41),
                value.M11 * (value.M22 * value.M44 - value.M24 * value.M42)
                    - value.M12 * (value.M21 * value.M44 - value.M24 * value.M41)
                    + value.M14 * (value.M21 * value.M42 - value.M22 * value.M41),
                -value.M11 * (value.M22 * value.M34 - value.M24 * value.M32)
                    + value.M12 * (value.M21 * value.M34 - value.M24 * value.M31)
                    - value.M14 * (value.M21 * value.M32 - value.M22 * value.M31),
                -value.M21 * (value.M32 * value.M43 - value.M33 * value.M42)
                    + value.M22 * (value.M31 * value.M43 - value.M33 * value.M41)
                    - value.M23 * (value.M31 * value.M42 - value.M32 * value.M41),
                value.M11 * (value.M32 * value.M43 - value.M33 * value.M42)
                    - value.M12 * (value.M31 * value.M43 - value.M33 * value.M41)
                    + value.M13 * (value.M31 * value.M42 - value.M32 * value.M41),
                -value.M11 * (value.M22 * value.M43 - value.M23 * value.M42)
                    + value.M12 * (value.M21 * value.M43 - value.M23 * value.M41)
                    - value.M13 * (value.M21 * value.M42 - value.M22 * value.M41),
                value.M11 * (value.M22 * value.M33 - value.M23 * value.M32)
                    - value.M12 * (value.M21 * value.M33 - value.M23 * value.M31)
                    + value.M13 * (value.M21 * value.M32 - value.M22 * value.M31)
            ) / determinant;
    }

    /// <inheritdoc />
    public bool Equals(Matrix4 other) =>
        M11 == other.M11
        && M12 == other.M12
        && M13 == other.M13
        && M14 == other.M14
        && M21 == other.M21
        && M22 == other.M22
        && M23 == other.M23
        && M24 == other.M24
        && M31 == other.M31
        && M32 == other.M32
        && M33 == other.M33
        && M34 == other.M34
        && M41 == other.M41
        && M42 == other.M42
        && M43 == other.M43
        && M44 == other.M44;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Matrix4 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(
            HashCode.Combine(M11, M12, M13, M14),
            HashCode.Combine(M21, M22, M23, M24),
            HashCode.Combine(M31, M32, M33, M34),
            HashCode.Combine(M41, M42, M43, M44)
        );

    /// <summary>
    /// Returns a string representation of the matrix.
    /// </summary>
    public override string ToString() =>
        $"Matrix4(({M11}, {M12}, {M13}, {M14}), "
        + $"({M21}, {M22}, {M23}, {M24}), "
        + $"({M31}, {M32}, {M33}, {M34}), "
        + $"({M41}, {M42}, {M43}, {M44}))";
}
