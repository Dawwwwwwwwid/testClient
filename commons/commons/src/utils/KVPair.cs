namespace HIVE.Commons.Utils
{
    /// <summary>
    /// Represents a key-value pair.
    /// </summary>
    /// <typeparam name="K">The type of the key.</typeparam>
    /// <typeparam name="V">The type of the value.</typeparam>
    public struct KVPair<K, V> {
        /// <summary>
        /// The key of the key-value pair.
        /// </summary>
        /// <remarks>
        /// This field is public and can be accessed directly.
        /// </remarks>
        public K Key { get; set; }
        /// <summary>
        /// The value of the key-value pair.
        /// </summary>
        /// <remarks>
        /// This field is public and can be accessed directly.
        /// </remarks>
        public V Value { get; set; }
    };
}