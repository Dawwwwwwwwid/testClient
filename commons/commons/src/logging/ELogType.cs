namespace HIVE.Commons.Logging
{

    /// <summary>
    /// This class represents different log types.
    /// </summary>
    /// <remarks>
    /// This class is public and static.
    /// </remarks>
    public static class LogType
    {
        /// <summary>
        /// Enumeration of different log types.
        /// </summary>
        /// <remarks>
        /// This enumeration is public and can be accessed directly.
        /// </remarks>
        public enum ELogType
        {
            Critical = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
            Debug = 4
        }

        /// <summary>
        /// Converts the log type to a string representation.
        /// </summary>
        /// <param name="logType">Log type to convert.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        /// <returns>
        /// String representation of the log type.
        /// </returns>
        public static string ToString(ELogType logType)
        {
            return logType switch
            {
                ELogType.Debug => "DEBUG",
                ELogType.Info => "INFO",
                ELogType.Warning => "WARN",
                ELogType.Error => "ERROR",
                ELogType.Critical => "CRITICAL",
                _ => "UNKNOWN"
            };
        }

        /// <summary>
        /// Converts a string representation of a log type to its corresponding enumeration value.
        /// </summary>
        /// <param name="logType">String representation of the log type.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        /// <returns>
        /// Enumeration value of the log type.
        /// </returns>
        public static ELogType FromString(string logType)
        {
            return logType switch
            {
                "DEBUG" => ELogType.Debug,
                "INFO" => ELogType.Info,
                "WARNING" => ELogType.Warning,
                "ERROR" => ELogType.Error,
                "FATAL" => ELogType.Critical,
                _ => ELogType.Debug
            };
        }
    }
}