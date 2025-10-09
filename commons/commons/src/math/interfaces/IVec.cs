namespace Hive.Commons.Math.Interfaces
{
    /// <summary>
    /// Vector interface
    /// </summary>
    /// <typeparam name="T">Vector type (e.g., Vec2, Vec3, Vec4)</typeparam>
    /// <typeparam name="V">Value type (e.g., float, double, etc.)</typeparam>
    public interface IVec<T, V> where T : IVec<T, V>
    {
      /// <summary>
      /// Add two vectors
      /// </summary>
      /// <param name="other">Other vector</param>
      /// <returns>Resulting vector</returns>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract T Add(T other);

      /// <summary>
      /// Subtract two vectors
      /// </summary>
      /// <param name="other">Other vector</param>
      /// <returns>Resulting vector</returns>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract T Sub(T other);

      /// <summary>
      /// Multiply the vector by a scalar
      /// </summary>
      /// <param name="scalar">Scalar value</param>
      /// <returns>Resulting vector</returns>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract T Mul(V scalar);

      /// <summary>
      /// Multiply two vectors
      /// </summary>
      /// <param name="other">Other vector</param>
      /// <returns>Resulting vector</returns>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract T Mul(T other);

      /// <summary>
      /// Divide the vector by a scalar
      /// </summary>
      /// <param name="scalar">Scalar value</param>
      /// <returns>Resulting vector</returns>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract T Div(V scalar);

      /// <summary>
      /// Calculate the dot product of two vectors
      /// </summary>
      /// <param name="other">Other vector</param>
      /// <returns>Dot product of the two vectors</returns>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract V Dot(T other);

      /// <summary>
      /// Calculate the magnitude of the vector
      /// </summary>
      /// <returns>Magnitude of the vector</returns>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract V Abs();

      /// <summary>
      /// Calculate the distance between two vectors
      /// </summary>
      /// <param name="other">Other vector</param>
      /// <returns>Distance between the two vectors</returns>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract V Distance(T other);

      /// <summary>
      /// Normalize the vector
      /// </summary>
      /// <remarks>
      /// This method is abstract and must be implemented in derived classes.
      /// </remarks>
      public abstract void Norm();
    }
}