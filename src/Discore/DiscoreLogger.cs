using System;

namespace Discore
{
    public class DiscoreLogEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message that was logged.
        /// </summary>
        public DiscoreLogMessage Message { get; }

        internal DiscoreLogEventArgs(DiscoreLogMessage message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Represents a single logged line.
    /// </summary>
    public class DiscoreLogMessage
    {
        /// <summary>
        /// Gets the contents of this message.
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// Gets the severity of this message.
        /// </summary>
        public DiscoreLogLevel Type { get; }
        /// <summary>
        /// Gets the date/time this message was logged.
        /// </summary>
        public DateTime Timestamp { get; }

        internal DiscoreLogMessage(string content, DiscoreLogLevel type, DateTime timestamp)
        {
            Content = content;
            Type = type;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// The severity level of a log message.
    /// </summary>
    public enum DiscoreLogLevel
    {
        Debug,
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
        /// Fired when a message is logged from Discore.
        /// </summary>
        public static event EventHandler<DiscoreLogEventArgs> OnLog;

        internal static DiscoreLogger Global { get; }

        /// <summary>
        /// Gets or sets the minimum log level to be sent through the <see cref="OnLog"/> event.
        /// <para>
        /// For example: A log level of <see cref="DiscoreLogLevel.Info"/> will log 
        /// everything except <see cref="DiscoreLogLevel.Debug"/>.
        /// </para>
        /// <para>
        /// This defaults to <see cref="DiscoreLogLevel.Info"/>.
        /// </para>
        /// </summary>
        public static DiscoreLogLevel MinimumLevel { get; set; }

        /// <summary>
        /// Gets or sets the prefix for this <see cref="DiscoreLogger"/>.
        /// </summary>
        public string Prefix { get; set; }

        static DiscoreLogger()
        {
            Global = new DiscoreLogger("Global");
            MinimumLevel = DiscoreLogLevel.Info;
        }

        /// <summary>
        /// Creates a new <see cref="DiscoreLogger"/> instance.
        /// </summary>
        /// <param name="prefix">The prefix of this logger.</param>
        internal DiscoreLogger(string prefix)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// Logs a line to this logger.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        /// <param name="type">The type of log.</param>
        public void Log(string msg, DiscoreLogLevel type)
        {
            if (type >= MinimumLevel)
            {
                if (!string.IsNullOrWhiteSpace(Prefix))
                    // Prefix
                    msg = $"[{Prefix}] {msg}";

                try { OnLog?.Invoke(this, new DiscoreLogEventArgs(new DiscoreLogMessage(msg, type, DateTime.Now))); }
                // Log methods need to be guaranteed to never throw exceptions.
                catch { }
            }
        }

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogVerbose(string msg)
        {
            Log(msg, DiscoreLogLevel.Debug);
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogInfo(string msg)
        {
            Log(msg, DiscoreLogLevel.Info);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogWarning(string msg)
        {
            Log(msg, DiscoreLogLevel.Warning);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogError(string msg)
        {
            Log(msg, DiscoreLogLevel.Error);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public void LogError(Exception ex)
        {
            Log(ex.ToString(), DiscoreLogLevel.Error);
        }
    }
}
