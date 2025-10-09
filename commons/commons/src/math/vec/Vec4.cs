using System;
using System.Runtime.CompilerServices;
using Antilatency.Math;
using Hive.Commons.Math.Interfaces;
using HIVE.Commons.Flatbuffers.Interfaces;

namespace HIVE.Commons.Math.Vec
{
    /// <summary>
    /// Represents a 4-dimensional vector with float components.
    /// </summary>
    public struct Vec4 :
      IFBSerde<Flatbuffers.Generated.Vec4, Flatbuffers.Generated.Vec4T, Vec4>,
      IFBSerde<Antilatency.Math.floatQ, Antilatency.Math.floatQ, Vec4>,
      IVec<Vec4, float>
    {
      /// <summary>
      /// Gets or sets the X component of the vector.
      /// </summary>
      public float X { get; set; }

      /// <summary>
      /// Gets or sets the Y component of the vector.
      /// </summary>
      public float Y { get; set; }

      /// <summary>
      /// Gets or sets the Z component of the vector.
      /// </summary>
      public float Z { get; set; }

      /// <summary>
      /// Gets or sets the W component of the vector.
      /// </summary>
      public float W { get; set; }

      /// <summary>
      /// Initializes a new instance of the <see cref="Vec4"/> struct with specified components.
      /// </summary>
      /// <param name="x">The X component of the vector.</param>
      /// <param name="y">The Y component of the vector.</param>
      /// <param name="z">The Z component of the vector.</param>
      /// <param name="w">The W component of the vector.</param>
      public Vec4(float x, float y, float z, float w)
      {
        X = x;
        Y = y;
        Z = z;
        W = w;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Vec4"/> struct from a <see cref="Flatbuffers.Generated.Vec4"/> object.
      /// </summary>
      /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec4"/> object to initialize from.</param>
      public Vec4(Flatbuffers.Generated.Vec4 obj)
      {
        X = obj.X;
        Y = obj.Y;
        Z = obj.Z;
        W = obj.W;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Vec4"/> struct from a <see cref="Flatbuffers.Generated.Vec4T"/> object.
      /// </summary>
      /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec4T"/> object to initialize from.</param>
      public Vec4(Flatbuffers.Generated.Vec4T obj)
      {
        X = obj.X;
        Y = obj.Y;
        Z = obj.Z;
        W = obj.W;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Vec4"/> struct from a <see cref="floatQ"/> object.
      /// </summary>
      /// <param name="obj">The <see cref="floatQ"/> object to initialize from.</param>
      public Vec4(floatQ obj)
      {
        X = obj.x;
        Y = obj.y;
        Z = obj.z;
        W = obj.w;
      }

      //* ========== Math ==========

      public Vec4 Add(Vec4 other)
          => new() { X = X + other.X, Y = Y + other.Y, Z = Z + other.Z, W = W + other.W };

      public Vec4 Sub(Vec4 other)
          => new() { X = X - other.X, Y = Y - other.Y, Z = Z - other.Z, W = W - other.W };

      public Vec4 Mul(float scalar)
          => new() { X = X * scalar, Y = Y * scalar, Z = Z * scalar, W = W * scalar };

      public Vec4 Mul(Vec4 other)
          => new() { 
              X = W * other.X + X * other.W + Y * other.Z - Z * other.Y,
              Y = W * other.Y - X * other.Z + Y * other.W + Z * other.X,
              Z = W * other.Z + X * other.Y - Y * other.X + Z * other.W,
              W = W * other.W - X * other.X - Y * other.Y - Z * other.Z
          };

      public Vec4 Div(float scalar)
          => new() { X = X / scalar, Y = Y / scalar, Z = Z / scalar, W = W / scalar };

      public readonly float Dot(Vec4 other)
          => X * other.X + Y * other.Y + Z * other.Z + W * other.W;

      public readonly float Abs()
          => MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);

      public readonly Vec4 Conjugate()
          => new() { X = -X, Y = -Y, Z = -Z, W = W };

      public readonly Vec4 Inverse()
      {
          float magnitudeSquared = Abs() * Abs();
          return new Vec4
          {
              X = -X / magnitudeSquared,
              Y = -Y / magnitudeSquared,
              Z = -Z / magnitudeSquared,
              W = W / magnitudeSquared
          };
      }

      public void Norm()
      {
          float mag = Abs();
          X /= mag;
          Y /= mag;
          Z /= mag;
          W /= mag;
      }

      public Vec4 Rotate(Vec3 v)
      {
          Vec4 q = this;
          Vec4 vQuat = new() { X = v.X, Y = v.Y, Z = v.Z, W = 0 };
          Vec4 result = q * vQuat * q.Conjugate();
          return result;
      }

      public static Vec4 operator +(Vec4 a, Vec4 b) 
        => a.Add(b);

      public static Vec4 operator -(Vec4 a, Vec4 b) 
        => a.Sub(b);

      public static Vec4 operator *(Vec4 a, float scalar) 
        => a.Mul(scalar);

      public static Vec4 operator *(float scalar, Vec4 a) 
        => a.Mul(scalar);

      public static Vec4 operator *(Vec4 a, Vec4 b) 
        => a.Mul(b);

      public static Vec4 operator /(Vec4 a, float scalar) 
        => a.Div(scalar);

      public static bool operator ==(Vec4 a, Vec4 b)
          => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;

      public static bool operator !=(Vec4 a, Vec4 b)
          => !(a == b);

      public override readonly bool Equals(object obj)
          => obj is Vec4 other && this == other;

      public override readonly int GetHashCode()
          => HashCode.Combine(X, Y, Z, W);

      //* ========== IFBSerde Implementations ==========

      /// <summary>
      /// Deserializes a <see cref="floatQ"/> object to a <see cref="Vec4"/>.
      /// </summary>
      /// <param name="obj">The <see cref="floatQ"/> object to deserialize.</param>
      /// <returns>A new <see cref="Vec4"/> instance.</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static Vec4 Deserialize(floatQ obj) 
        => new() { X = obj.x, Y = obj.y, Z = obj.z, W = obj.w };

      /// <summary>
      /// Deserializes a <see cref="Flatbuffers.Generated.Vec4"/> object to a <see cref="Vec4"/>.
      /// </summary>
      /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec4"/> object to deserialize.</param>
      /// <returns>A new <see cref="Vec4"/> instance.</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static Vec4 Deserialize(Flatbuffers.Generated.Vec4 obj) 
        => new() { X = obj.X, Y = obj.Y, Z = obj.Z, W = obj.W };

      /// <summary>
      /// Serializes the current <see cref="Vec4"/> instance to a <see cref="floatQ"/> object.
      /// </summary>
      /// <param name="obj">The <see cref="floatQ"/> object to serialize to.</param>
      /// <returns>A new <see cref="floatQ"/> instance.</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public readonly floatQ Serialize(ref floatQ obj) 
        => new() { x = X, y = Y, z = Z, w = W };

      /// <summary>
      /// Serializes the current <see cref="Vec4"/> instance to a <see cref="Flatbuffers.Generated.Vec4T"/> object.
      /// </summary>
      /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec4T"/> object to serialize to.</param>
      /// <returns>A new <see cref="Flatbuffers.Generated.Vec4T"/> instance.</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public readonly Flatbuffers.Generated.Vec4T Serialize(ref Flatbuffers.Generated.Vec4T obj) 
        => new() { X = X, Y = Y, Z = Z, W = W };

      //* ========== Implicit Conversions ==========

      /// <summary>
      /// Implicitly converts a <see cref="floatQ"/> object to a <see cref="Vec4"/>.
      /// </summary>
      /// <param name="obj">The <see cref="floatQ"/> object to convert.</param>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static implicit operator Vec4(floatQ obj) 
        => new() { X = obj.x, Y = obj.y, Z = obj.z, W = obj.w };

      /// <summary>
      /// Implicitly converts a <see cref="Flatbuffers.Generated.Vec4"/> object to a <see cref="Vec4"/>.
      /// </summary>
      /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec4"/> object to convert.</param>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static implicit operator Vec4(Flatbuffers.Generated.Vec4 obj) 
        => new() { X = obj.X, Y = obj.Y, Z = obj.Z, W = obj.W };

      /// <summary>
      /// Explicitly converts a <see cref="Vec4"/> object to a <see cref="Flatbuffers.Generated.Vec4T"/>.
      /// </summary>
      /// <param name="obj">The <see cref="Vec4"/> object to convert.</param>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static explicit operator Flatbuffers.Generated.Vec4T(Vec4 obj) 
        => new() { X = obj.X, Y = obj.Y, Z = obj.Z, W = obj.W };

      /// <summary>
      /// Explicitly converts a <see cref="Vec4"/> object to a <see cref="floatQ"/>.
      /// </summary>
      /// <param name="obj">The <see cref="Vec4"/> object to convert.</param>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static explicit operator floatQ(Vec4 obj) 
        => new() { x = obj.X, y = obj.Y, z = obj.Z, w = obj.W };

      //* ========== Conversion Methods ==========

      /// <summary>
      /// Converts a <see cref="floatQ"/> object to a <see cref="Flatbuffers.Generated.Vec4T"/>.
      /// </summary>
      /// <param name="obj">The <see cref="floatQ"/> object to convert.</param>
      /// <returns>A new <see cref="Flatbuffers.Generated.Vec4T"/> instance.</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static Flatbuffers.Generated.Vec4T ConvertToVec4T(floatQ obj) 
        => new() { X = obj.x, Y = obj.y, Z = obj.z, W = obj.w };

      /// <summary>
      /// Converts a <see cref="Flatbuffers.Generated.Vec4T"/> object to a <see cref="floatQ"/>.
      /// </summary>
      /// <param name="obj">The <see cref="Flatbuffers.Generated.Vec4T"/> object to convert.</param>
      /// <returns>A new <see cref="floatQ"/> instance.</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static floatQ ConvertToFloatQ(Flatbuffers.Generated.Vec4T obj) 
        => new() { x = obj.X, y = obj.Y, z = obj.Z, w = obj.W };

        public float Distance(Vec4 other)
        {
            throw new NotImplementedException();
        }
    }
}