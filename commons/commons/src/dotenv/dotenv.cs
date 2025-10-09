using System;
using System.IO;

namespace HIVE.Commons.DotEnv
{
    /// <summary>
    /// This class is used to load environment variables from a .env file.
    /// </summary>
    /// <remarks>
    /// This class is public and static.
    /// </remarks>
    public class DotEnv
    {
        
        /// <summary>
        /// Loads environment variables from a .env file.
        /// </summary>
        /// <param name="path">Path to the .env file.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The file '{path}' does not exist.");
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                {
                    continue;
                }

                var parts = trimmedLine.Split('=', 2);
                if (parts.Length != 2)
                {
                    throw new FormatException($"The line '{line}' is not in the correct format.");
                }

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                Environment.SetEnvironmentVariable(key, value);
            }
        }

        /// <summary>
        /// Loads environment variables from a .env file with the default name.
        /// </summary>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void Load() { Load(".env"); }

        /// <summary>
        /// Gets the value of an environment variable.
        /// </summary>
        /// <param name="key">Key of the environment variable.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        /// <returns>
        /// Value of the environment variable.
        /// </returns>
        public static string Get(string key) { return Environment.GetEnvironmentVariable(key); }
    }
}