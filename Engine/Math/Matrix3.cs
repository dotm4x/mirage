using Mirage.Math.Vectors;

namespace Mirage.Math;

/// <summary>
/// Represents a three-by-three matrix.
/// </summary>
public readonly struct Matrix3 : IEquatable<Matrix3>
{
    /// <summary>
    /// Gets the value in the first row and first column.
    /// </summary>
    public readonly double M11;

    /// <summary>
    /// Gets the value in the first row and second column.
    /// </summary>
    public readonly double M12;

    /// <summary>
    /// Gets the value in the first row and third column.
    /// </summary>
    public readonly double M13;

    /// <summary>
    /// Gets the value in the second row and first column.
    /// </summary>
    public readonly double M21;

    /// <summary>
    /// Gets the value in the second row and second column.
    /// </summary>
    public readonly double M22;

    /// <summary>
    /// Gets the value in the second row and third column.
    /// </summary>
    public readonly double M23;

    /// <summary>
    /// Gets the value in the third row and first column.
    /// </summary>
    public readonly double M31;

    /// <summary>
    /// Gets the value in the third row and second column.
    /// </summary>
    public readonly double M32;

    /// <summary>
    /// Gets the value in the third row and third column.
    /// </summary>
    public readonly double M33;

    /// <summary>
    /// Represents the identity matrix.
    /// </summary>
    public static Matrix3 Identity => new(1, 0, 0, 0, 1, 0, 0, 0, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix3"/> struct.
    /// </summary>
    public Matrix3(
        double m11,
        double m12,
        double m13,
        double m21,
        double m22,
        double m23,
        double m31,
        double m32,
        double m33
    )
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M31 = m31;
        M32 = m32;
        M33 = m33;
    }

    /// <summary>
    /// Adds two matrices.
    /// </summary>
    public static Matrix3 operator +(Matrix3 left, Matrix3 right) =>
        new(
            left.M11 + right.M11,
            left.M12 + right.M12,
            left.M13 + right.M13,
            left.M21 + right.M21,
            left.M22 + right.M22,
            left.M23 + right.M23,
            left.M31 + right.M31,
            left.M32 + right.M32,
            left.M33 + right.M33
        );

    /// <summary>
    /// Subtracts one matrix from another.
    /// </summary>
    public static Matrix3 operator -(Matrix3 left, Matrix3 right) =>
        new(
            left.M11 - right.M11,
            left.M12 - right.M12,
            left.M13 - right.M13,
            left.M21 - right.M21,
            left.M22 - right.M22,
            left.M23 - right.M23,
            left.M31 - right.M31,
            left.M32 - right.M32,
            left.M33 - right.M33
        );

    /// <summary>
    /// Negates a matrix.
    /// </summary>
    public static Matrix3 operator -(Matrix3 value) =>
        new(
            -value.M11,
            -value.M12,
            -value.M13,
            -value.M21,
            -value.M22,
            -value.M23,
            -value.M31,
            -value.M32,
            -value.M33
        );

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    public static Matrix3 operator *(Matrix3 left, Matrix3 right) =>
        new(
            left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31,
            left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32,
            left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33,
            left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31,
            left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32,
            left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33,
            left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31,
            left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32,
            left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33
        );

    /// <summary>
    /// Multiplies a matrix by a scalar.
    /// </summary>
    public static Matrix3 operator *(Matrix3 value, double scalar) =>
        new(
            value.M11 * scalar,
            value.M12 * scalar,
            value.M13 * scalar,
            value.M21 * scalar,
            value.M22 * scalar,
            value.M23 * scalar,
            value.M31 * scalar,
            value.M32 * scalar,
            value.M33 * scalar
        );

    /// <summary>
    /// Multiplies a scalar by a matrix.
    /// </summary>
    public static Matrix3 operator *(double scalar, Matrix3 value) => value * scalar;

    /// <summary>
    /// Divides a matrix by a scalar.
    /// </summary>
    public static Matrix3 operator /(Matrix3 value, double scalar) => value * (1 / scalar);

    /// <summary>
    /// Transforms a vector by a matrix.
    /// </summary>
    public static Vector3D operator *(Matrix3 matrix, Vector3D vector) =>
        new(
            matrix.M11 * vector.X + matrix.M12 * vector.Y + matrix.M13 * vector.Z,
            matrix.M21 * vector.X + matrix.M22 * vector.Y + matrix.M23 * vector.Z,
            matrix.M31 * vector.X + matrix.M32 * vector.Y + matrix.M33 * vector.Z
        );

    /// <summary>
    /// Creates a translation matrix.
    /// </summary>
    public static Matrix3 CreateTranslation(Vector2D translation) =>
        new(1, 0, translation.X, 0, 1, translation.Y, 0, 0, 1);

    /// <summary>
    /// Creates a scale matrix.
    /// </summary>
    public static Matrix3 CreateScale(Vector2D scale) => new(scale.X, 0, 0, 0, scale.Y, 0, 0, 0, 1);

    /// <summary>
    /// Creates a rotation matrix around the Z axis.
    /// </summary>
    public static Matrix3 CreateRotation(double angle)
    {
        double cosine = System.Math.Cos(angle);
        double sine = System.Math.Sin(angle);

        return new Matrix3(cosine, -sine, 0, sine, cosine, 0, 0, 0, 1);
    }

    /// <summary>
    /// Gets the transpose of the matrix.
    /// </summary>
    public Matrix3 Transposed => new(M11, M21, M31, M12, M22, M32, M13, M23, M33);

    /// <summary>
    /// Gets the determinant of the matrix.
    /// </summary>
    public double Determinant =>
        M11 * (M22 * M33 - M23 * M32)
        - M12 * (M21 * M33 - M23 * M31)
        + M13 * (M21 * M32 - M22 * M31);

    /// <summary>
    /// Gets the inverse of the matrix.
    /// </summary>
    public Matrix3 Inversed
    {
        get
        {
            double determinant = Determinant;

            return new Matrix3(
                (M22 * M33 - M23 * M32) / determinant,
                (M13 * M32 - M12 * M33) / determinant,
                (M12 * M23 - M13 * M22) / determinant,
                (M23 * M31 - M21 * M33) / determinant,
                (M11 * M33 - M13 * M31) / determinant,
                (M13 * M21 - M11 * M23) / determinant,
                (M21 * M32 - M22 * M31) / determinant,
                (M12 * M31 - M11 * M32) / determinant,
                (M11 * M22 - M12 * M21) / determinant
            );
        }
    }

    /// <inheritdoc />
    public bool Equals(Matrix3 other) =>
        M11 == other.M11
        && M12 == other.M12
        && M13 == other.M13
        && M21 == other.M21
        && M22 == other.M22
        && M23 == other.M23
        && M31 == other.M31
        && M32 == other.M32
        && M33 == other.M33;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Matrix3 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(
            HashCode.Combine(M11, M12, M13),
            HashCode.Combine(M21, M22, M23),
            HashCode.Combine(M31, M32, M33)
        );

    /// <summary>
    /// Returns a string representation of the matrix.
    /// </summary>
    public override string ToString() =>
        $"Matrix3(({M11}, {M12}, {M13}), ({M21}, {M22}, {M23}), ({M31}, {M32}, {M33}))";

    /// <summary>
    /// Determines whether two matrices are equal.
    /// </summary>
    public static bool operator ==(Matrix3 left, Matrix3 right) => left.Equals(right);

    /// <summary>
    /// Determines whether two matrices are not equal.
    /// </summary>
    public static bool operator !=(Matrix3 left, Matrix3 right) => !left.Equals(right);
}
