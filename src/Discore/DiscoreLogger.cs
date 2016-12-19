using System;

namespace Discore
{
    public class DiscoreLogEventArgs : EventArgs
    {
        public readonly DiscoreLogLine Line;

        internal DiscoreLogEventArgs(DiscoreLogLine line)
        {
            Line = line;
        }
    }

    /// <summary>
    /// Represents a single logged line.
    /// </summary>
    public class DiscoreLogLine
    {
        /// <summary>
        /// The contents of this line.
        /// </summary>
        public readonly string Message;
        /// <summary>
        /// The type of line.
        /// </summary>
        public readonly DiscoreLogType Type;
        /// <summary>
        /// The date/time the line was logged.
        /// </summary>
        public readonly DateTime Timestamp;

        internal DiscoreLogLine(string msg, DiscoreLogType type, DateTime timestamp)
        {
            Message = msg;
            Type = type;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// The type of a <see cref="DiscoreLogLine"/>.
    /// </summary>
    public enum DiscoreLogType
    {
        Verbose,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// A logger for Discore related systems.
    /// </summary>
    public class DiscoreLogger
    {
        /// <summary>
        /// Called when a line is logged from any <see cref="DiscoreLogger"/>.
        /// </summary>
        public static event EventHandler<DiscoreLogEventArgs> OnLog;

        /// <summary>
        /// Gets the default <see cref="DiscoreLogger"/>.
        /// </summary>
        public static DiscoreLogger Default { get; }

        /// <summary>
        /// Gets or sets the prefix for this <see cref="DiscoreLogger"/>.
        /// </summary>
        public string Prefix { get; set; }

        static DiscoreLogger()
        {
            Default = new DiscoreLogger("");
        }

        /// <summary>
        /// Creates a new <see cref="DiscoreLogger"/> instance.
        /// </summary>
        /// <param name="prefix">The prefix of this logger.</param>
        public DiscoreLogger(string prefix)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// Logs a line to this logger.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        /// <param name="type">The type of log.</param>
        public void Log(string msg, DiscoreLogType type)
        {
            if (!string.IsNullOrWhiteSpace(Prefix))
                // Prefix
                msg = $"[{Prefix}] {msg}";

            OnLog?.Invoke(this, new DiscoreLogEventArgs(new DiscoreLogLine(msg, type, DateTime.Now)));
        }

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogVerbose(string msg)
        {
            Log(msg, DiscoreLogType.Verbose);
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogInfo(string msg)
        {
            Log(msg, DiscoreLogType.Info);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogWarning(string msg)
        {
            Log(msg, DiscoreLogType.Warning);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogError(string msg)
        {
            Log(msg, DiscoreLogType.Error);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public void LogError(Exception ex)
        {
            Log(ex.ToString(), DiscoreLogType.Error);
        }
    }
}
