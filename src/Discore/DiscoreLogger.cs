using System;
using System.Collections.Generic;

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
        Heartbeat,
        Verbose,
        Info,
        Important,
        Warning,
        Error
    }

    /// <summary>
    /// A filter for a <see cref="DiscoreLogger"/>.
    /// </summary>
    public class DiscoreLoggerFilter
    {
        Dictionary<DiscoreLogType, bool> settings;

        internal DiscoreLoggerFilter()
        {
            settings = new Dictionary<DiscoreLogType, bool>();

            foreach (object v in Enum.GetValues(typeof(DiscoreLogType)))
                settings[(DiscoreLogType)v] = false;
        }

        /// <summary>
        /// Sets the filtered state of a <see cref="DiscoreLogType"/>.
        /// </summary>
        /// <param name="type">The type of <see cref="DiscoreLogLine"/> to affect.</param>
        /// <param name="filter">The new filtered state.</param>
        public void Set(DiscoreLogType type, bool filter)
        {
            settings[type] = filter;
        }

        /// <summary>
        /// Gets whether or not a <see cref="DiscoreLogType"/> is filtered.
        /// </summary>
        /// <param name="type">The type of <see cref="DiscoreLogLine"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscoreLogType"/> is filtered.</returns>
        public bool IsFiltered(DiscoreLogType type)
        {
            return settings[type];
        }
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
        /// Called when a filtered line is logged from any <see cref="DiscoreLogger"/>.
        /// </summary>
        public static event EventHandler<DiscoreLogEventArgs> OnFilteredLog;

        /// <summary>
        /// Gets the default <see cref="DiscoreLogger"/>.
        /// </summary>
        public static DiscoreLogger Default { get; }
        /// <summary>
        /// Gets the global filter for every <see cref="DiscoreLogger"/>.
        /// </summary>
        public static DiscoreLoggerFilter GlobalFilter { get; }

        /// <summary>
        /// Gets or sets the prefix for this <see cref="DiscoreLogger"/>.
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Gets the filter for this <see cref="DiscoreLogger"/>.
        /// </summary>
        public DiscoreLoggerFilter Filter { get; }

        static DiscoreLogger()
        {
            GlobalFilter = new DiscoreLoggerFilter();
#if !DEBUG
            GlobalFilter.Set(DiscoreLogType.Heartbeat, true);
            GlobalFilter.Set(DiscoreLogType.Verbose, true);
#endif

            Default = new DiscoreLogger("");
        }

        /// <summary>
        /// Creates a new <see cref="DiscoreLogger"/> instance.
        /// </summary>
        /// <param name="prefix">The prefix of this logger.</param>
        public DiscoreLogger(string prefix)
        {
            Prefix = prefix;
            Filter = new DiscoreLoggerFilter();
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

            if (GlobalFilter.IsFiltered(type) || Filter.IsFiltered(type))
                OnFilteredLog?.Invoke(null, new DiscoreLogEventArgs(new DiscoreLogLine(msg, type, DateTime.Now)));
            else
                OnLog?.Invoke(null, new DiscoreLogEventArgs(new DiscoreLogLine(msg, type, DateTime.Now)));
        }

        /// <summary>
        /// Logs a heartbeat message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogHeartbeat(string msg)
        {
            Log(msg, DiscoreLogType.Heartbeat);
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
        /// Logs an important message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogImportant(string msg)
        {
            Log(msg, DiscoreLogType.Important);
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
