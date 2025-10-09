using System;
using System.Reflection.Metadata;
using System.Text;

namespace HIVE.Commons.Logging
{
    /// <summary>
    /// This class is used to log messages to the console.
    /// </summary>
    /// <remarks>
    /// This class is public and static.
    /// </remarks>
    public static class Logger
    {
        /// <summary>
        /// Severity level of the logger.
        /// </summary>
        /// <remarks>
        /// This field is public and can be set privately.
        /// </remarks>
        public static int Severity { get; private set; } = 0;

        /// <summary>
        /// Tag for the logger.
        /// </summary>
        /// <remarks>
        /// This field is public and can be set privately.
        /// </remarks>
        public static string Tag { get; private set; } = "DEFAULT";

        /// <summary>
        /// Sets the severity level of the logger. Everything below this level will be logged.
        /// </summary>
        /// <param name="severity">Severity level to set.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void SetSeverity(int severity)
        {
            if (severity < 0) Severity = 0;
            else if (severity > 5) Severity = 5;
            else Severity = severity;
        }

        /// <summary>
        /// Sets the severity level of the logger. Everything below this level will be logged.
        /// </summary>
        /// <param name="severity">Severity level to set.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void SetSeverity(LogType.ELogType severity) { SetSeverity((int)severity); }

        /// <summary>
        /// Sets the tag for the logger.
        /// </summary>
        /// <param name="tag">Tag to set.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void SetTag(string tag) { Tag = tag; }

        /// <summary>
        /// Logs a message to the console with the specified severity level.
        /// </summary>
        /// <param name="msg">Message to log.</param>
        /// <param name="type">Severity level of the message.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void Log(string msg, LogType.ELogType type)
        {
            if (type <= (LogType.ELogType)Severity) return;

            StringBuilder hstr = new();

            DateTime now = DateTime.Now;
            hstr.Append(now.ToString("yyyyMMdd-HH:mm:ss.fff"));

            Console.Write("[" + hstr.ToString() + "] ");
            Console.Write(LogType.ToString(type) + ": ");
            Console.Write(msg + "\n");
        }

        /// <summary>
        /// Logs a message to the console with the default severity level.
        /// </summary>
        /// <param name="msg">Message to log.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void Log(string msg) { Log(msg, LogType.ELogType.Info); }

        /// <summary>
        /// Logs a message to the console with the specified severity level and tag.
        /// </summary>
        /// <param name="msg">Message to log.</param>
        /// <param name="tag">Tag to log with the message.</param>
        /// <param name="type">Severity level of the message.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void Log(string msg, string tag, LogType.ELogType type) { Log(msg + " (" + tag + ")", type); }

        /// <summary>
        /// Logs a message to the console with the specified tag and default severity level.
        /// </summary>
        /// <param name="msg">Message to log.</param>
        /// <param name="tag">Tag to log with the message.</param>
        /// <remarks>
        /// This method is public and static.
        /// </remarks>
        public static void Log(string msg, string tag) { Log(msg, tag, LogType.ELogType.Info); }

    }
}