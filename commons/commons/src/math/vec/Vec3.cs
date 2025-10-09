
using System;
using System.Runtime.CompilerServices;
using Antilatency.Math;
using Hive.Commons.Math.Interfaces;
using HIVE.Commons.Flatbuffers.Interfaces;

namespace HIVE.Commons.Math.Vec
{
    /// <summary>
    /// Represents a 3-dimensional vector with float components.
    /// </summary>
    public struct Vec3 :
        IFBSerde<Flatbuffers.Generated.Vec3, Flatbuffers.Generated.Vec3T, Vec3>,
        IFBSerde<Antilatency.Math.float3, Antilatency.Math.float3, Vec3>,
        IVec<Vec3, float>
    {
        private Flatbuffers.Generated.Vec3? destination;

        /// <summary>
        /// Gets or sets the X component of the vector.
        /// </summary>
        /// <remarks>
        /// This field is public and can be accessed directly.
        /// </remarks>
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the Y component of the vector.
        /// </summary>
        /// <remarks>
        /// This field is public and can be accessed directly.
        /// </remarks>
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the Z component of the vector.
        /// </summary>
        /// <remarks>
        /// This field is public and can be accessed directly.
        /// </remarks>
        public float Z { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vec3"/> struct with the specified components.
        /// </summary>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        /// <param name="z">The Z component of the vector.</param>
        /// <returns>
        /// A new instance of the <see cref="Vec3"/> struct with the specified components.
        /// </returns>
        /// <remarks>
        /// This constructor is public and can be accessed directly.
        /// </remarks>
        public Vec3(float x, float y, float z) { X = x; Y = y; Z = z; destination = null; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vec3"/> struct from a <see cref="Flatbuffers.Generated.Vec3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec3"/> object to initialize from.</param>
        /// <remarks>
        /// This constructor is public and can be accessed directly.
        /// </remarks>
        /// <returns>
        /// A new instance of the <see cref="Vec3"/> struct with the specified components.
        /// </returns>
        public Vec3(Flatbuffers.Generated.Vec3 obj) { X = obj.X; Y = obj.Y; Z = obj.Z; destination = null; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vec3"/> struct from a <see cref="Flatbuffers.Generated.Vec3T"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec3T"/> object to initialize from.</param>
        /// <remarks>
        /// This constructor is public and can be accessed directly.
        /// </remarks>
        /// <returns>
        /// A new instance of the <see cref="Vec3"/> struct with the specified components.
        /// </returns>
        public Vec3(Flatbuffers.Generated.Vec3T obj) { X = obj.X; Y = obj.Y; Z = obj.Z; destination = null; }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="Vec3"/> struct from a <see cref="float3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="float3"/> object to initialize from.</param>
        /// <remarks>
        /// This constructor is public and can be accessed directly.
        /// </remarks>
        /// <returns>
        /// A new instance of the <see cref="Vec3"/> struct with the specified components.
        /// </returns>
        public Vec3(float3 obj) { X = obj.x; Y = obj.y; Z = obj.z; destination = null; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vec3"/> struct from a nullable <see cref="Flatbuffers.Generated.Vec3"/> object.
        /// /// </summary>
        /// <param name="destination">The nullable <see cref="Flatbuffers.Generated.Vec3"/> object to initialize from.</param>

        /// </returns>
        /// <remarks>
        /// This constructor is public and can be accessed directly.
        /// </remarks>
        /// <returns>
        /// A new instance of the <see cref="Vec3"/> struct with the specified components.
        /// </returns>
        public Vec3(Flatbuffers.Generated.Vec3? destination) : this() { this.destination = destination; }

        //* ========== Math ==========

        public Vec3 Add(Vec3 other)
            => new() { X = X + other.X, Y = Y + other.Y, Z = Z + other.Z };

        public Vec3 Sub(Vec3 other)
            => new() { X = X - other.X, Y = Y - other.Y, Z = Z - other.Z };

        public Vec3 Mul(float scalar)
            => new() { X = X * scalar, Y = Y * scalar, Z = Z * scalar };

        public Vec3 Mul(Vec3 other)
            => new() { X = X * other.X, Y = Y * other.Y, Z = Z * other.Z };

        public Vec3 Div(float scalar)
            => new() { X = X / scalar, Y = Y / scalar, Z = Z / scalar };

        public readonly float Dot(Vec3 other)
            => X * other.X + Y * other.Y + Z * other.Z;

        public readonly float Abs()
            => MathF.Sqrt(X * X + Y * Y + Z * Z);

        public readonly float Distance(Vec3 other)
            => MathF.Sqrt(MathF.Pow(X - other.X, 2) + MathF.Pow(Y - other.Y, 2) + MathF.Pow(Z - other.Z, 2));

        public void Norm()
        {
            float mag = Abs();
            X /= mag;
            Y /= mag;
            Z /= mag;
        }

        public Vec3 Rotate(Vec4 q)
        {
            Vec3 u = new Vec3 { X = q.X, Y = q.Y, Z = q.Z };
            float s = q.W;
            return (2.0f * u.Dot(this) * u) + ((s * s) - u.Dot(u)) * this + (2.0f * s * Cross(u));
        }

        public Vec3 Cross(Vec3 other)
            => new()
            {
                X = Y * other.Z - Z * other.Y,
                Y = Z * other.X - X * other.Z,
                Z = X * other.Y - Y * other.X
            };

        public static Vec3 operator +(Vec3 a, Vec3 b)
            => a.Add(b);

        public static Vec3 operator -(Vec3 a, Vec3 b)
            => a.Sub(b);

        public static Vec3 operator *(Vec3 a, float scalar)
            => a.Mul(scalar);

        public static Vec3 operator *(float scalar, Vec3 a)
            => a.Mul(scalar);

        public static Vec3 operator *(Vec3 a, Vec3 b)
            => a.Mul(b);

        public static Vec3 operator /(Vec3 a, float scalar)
            => a.Div(scalar);

        public static bool operator ==(Vec3 a, Vec3 b)
            => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        public static bool operator !=(Vec3 a, Vec3 b)
            => !(a == b);

        public override readonly bool Equals(object obj)
            => obj is Vec3 other && this == other;

        public override readonly int GetHashCode()
            => HashCode.Combine(X, Y, Z);

        //* ========== IFBSerde Implementations ==========

        /// <summary>
        /// Deserializes a <see cref="float3"/> object into a <see cref="Vec3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="float3"/> object to deserialize.</param>
        /// <returns>A new <see cref="Vec3"/> object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 Deserialize(float3 obj)
            => new() { X = obj.x, Y = obj.y, Z = obj.z };

        /// <summary>
        /// Deserializes a <see cref="Flatbuffers.Generated.Vec3"/> object into a <see cref="Vec3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec3"/> object to deserialize.</param>
        /// <returns>A new <see cref="Vec3"/> object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 Deserialize(Flatbuffers.Generated.Vec3 obj)
            => new() { X = obj.X, Y = obj.Y, Z = obj.Z };

        /// <summary>
        /// Serializes the current <see cref="Vec3"/> object into a <see cref="Flatbuffers.Generated.Vec3T"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec3T"/> object to serialize into.</param>
        /// <returns>A new <see cref="Flatbuffers.Generated.Vec3T"/> object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Flatbuffers.Generated.Vec3T Serialize(ref Flatbuffers.Generated.Vec3T obj)
            => new() { X = X, Y = Y, Z = Z };

        /// <summary>
        /// Serializes the current <see cref="Vec3"/> object into a <see cref="float3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="float3"/> object to serialize into.</param>
        /// <returns>A new <see cref="float3"/> object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 Serialize(ref float3 obj)
            => new() { x = X, y = Y, z = Z };

        //* ========== Implicit Conversions ==========

        /// <summary>
        /// Implicitly converts a <see cref="float3"/> object to a <see cref="Vec3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="float3"/> object to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vec3(float3 obj)
            => new() { X = obj.x, Y = obj.y, Z = obj.z };

        /// <summary>
        /// Implicitly converts a <see cref="Flatbuffers.Generated.Vec3"/> object to a <see cref="Vec3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec3"/> object to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vec3(Flatbuffers.Generated.Vec3 obj)
            => new() { X = obj.X, Y = obj.Y, Z = obj.Z };

        /// <summary>
        /// Explicitly converts a <see cref="Vec3"/> object to a <see cref="Flatbuffers.Generated.Vec3T"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Vec3"/> object to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Flatbuffers.Generated.Vec3T(Vec3 obj)
            => new() { X = obj.X, Y = obj.Y, Z = obj.Z };

        /// <summary>
        /// Explicitly converts a <see cref="Vec3"/> object to a <see cref="float3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Vec3"/> object to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float3(Vec3 obj)
            => new() { x = obj.X, y = obj.Y, z = obj.Z };

        //* ========== Conversion Methods ==========

        /// <summary>
        /// Converts a <see cref="float3"/> object to a <see cref="Flatbuffers.Generated.Vec3T"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="float3"/> object to convert.</param>
        /// <returns>A new <see cref="Flatbuffers.Generated.Vec3T"/> object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Flatbuffers.Generated.Vec3T ConvertToVec3T(float3 obj)
            => new() { X = obj.x, Y = obj.y, Z = obj.z };

        /// <summary>
        /// Converts a <see cref="Flatbuffers.Generated.Vec3T"/> object to a <see cref="float3"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec3T"/> object to convert.</param>
        /// <returns>A new <see cref="float3"/> object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ConvertToFloat3(Flatbuffers.Generated.Vec3T obj)
            => new() { x = obj.X, y = obj.Y, z = obj.Z };
    }
}