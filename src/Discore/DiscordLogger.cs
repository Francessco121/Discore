using System;
using System.Collections.Generic;

namespace Discore
{
#pragma warning disable 1591
    public class DiscordLogEventArgs : EventArgs
    {
        public readonly DiscordLogLine Line;

        internal DiscordLogEventArgs(DiscordLogLine line)
        {
            Line = line;
        }
    }
#pragma warning restore 1591

    /// <summary>
    /// Represents a single logged line.
    /// </summary>
    public class DiscordLogLine
    {
        /// <summary>
        /// The contents of this line.
        /// </summary>
        public readonly string Message;
        /// <summary>
        /// The type of line.
        /// </summary>
        public readonly DiscordLogType Type;

        internal DiscordLogLine(string msg, DiscordLogType type)
        {
            Message = msg;
            Type = type;
        }
    }

    /// <summary>
    /// The type of a <see cref="DiscordLogLine"/>.
    /// </summary>
    public enum DiscordLogType
    {
        /// <summary>
        /// A heartbeat log.
        /// </summary>
        Heartbeat,
        /// <summary>
        /// An unncessary log.
        /// </summary>
        Unnecessary,
        /// <summary>
        /// A verbose log.
        /// </summary>
        Verbose,
        /// <summary>
        /// An info log.
        /// </summary>
        Info,
        /// <summary>
        /// An important log.
        /// </summary>
        Important,
        /// <summary>
        /// A warning log.
        /// </summary>
        Warning,
        /// <summary>
        /// An error log.
        /// </summary>
        Error
    }

    /// <summary>
    /// A filter for a <see cref="DiscordLogger"/>.
    /// </summary>
    public class DiscordLoggerFilter
    {
        Dictionary<DiscordLogType, bool> settings;

        internal DiscordLoggerFilter()
        {
            settings = new Dictionary<DiscordLogType, bool>();

            foreach (object v in Enum.GetValues(typeof(DiscordLogType)))
                settings[(DiscordLogType)v] = false;
        }

        /// <summary>
        /// Sets the filtered state of a <see cref="DiscordLogType"/>.
        /// </summary>
        /// <param name="type">The type of <see cref="DiscordLogLine"/> to affect.</param>
        /// <param name="filter">The new filtered state.</param>
        public void Set(DiscordLogType type, bool filter)
        {
            settings[type] = filter;
        }

        /// <summary>
        /// Gets whether or not a <see cref="DiscordLogType"/> is filtered.
        /// </summary>
        /// <param name="type">The type of <see cref="DiscordLogLine"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordLogType"/> is filtered.</returns>
        public bool IsFiltered(DiscordLogType type)
        {
            return settings[type];
        }
    }

    /// <summary>
    /// A logger for Discord related systems.
    /// </summary>
    public class DiscordLogger
    {
        /// <summary>
        /// Called when a line is logged from any <see cref="DiscordLogger"/>.
        /// </summary>
        public static event EventHandler<DiscordLogEventArgs> OnLog;
        /// <summary>
        /// Called when a filtered line is logged from any <see cref="DiscordLogger"/>.
        /// </summary>
        public static event EventHandler<DiscordLogEventArgs> OnFilteredLog;

        /// <summary>
        /// Gets or sets whether or not timestamps are prepended to every logged line.
        /// </summary>
        public static bool PrependTimestamp { get; set; } = true;
        /// <summary>
        /// Gets the default <see cref="DiscordLogger"/>.
        /// </summary>
        public static DiscordLogger Default { get; }
        /// <summary>
        /// Gets the global filter for every <see cref="DiscordLogger"/>.
        /// </summary>
        public static DiscordLoggerFilter GlobalFilter { get; }

        /// <summary>
        /// Gets or sets the prefix for this <see cref="DiscordLogger"/>.
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Gets the filter for this <see cref="DiscordLogger"/>.
        /// </summary>
        public DiscordLoggerFilter Filter { get; }

        static DiscordLogger()
        {
            GlobalFilter = new DiscordLoggerFilter();
#if !DEBUG
            GlobalFilter.Set(DiscordLogType.Heartbeat, true);
            GlobalFilter.Set(DiscordLogType.Unnecessary, true);
            GlobalFilter.Set(DiscordLogType.Verbose, true);
#endif

            Default = new DiscordLogger("");
        }

        /// <summary>
        /// Creates a new <see cref="DiscordLogger"/> instance.
        /// </summary>
        /// <param name="prefix">The prefix of this logger.</param>
        public DiscordLogger(string prefix)
        {
            Prefix = prefix;
            Filter = new DiscordLoggerFilter();
        }

        /// <summary>
        /// Logs a line to this logger.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        /// <param name="type">The type of log.</param>
        public void Log(string msg, DiscordLogType type)
        {
            if (PrependTimestamp && !string.IsNullOrWhiteSpace(Prefix))
                // Timestamp and prefix
                msg = $"[{DateTime.Now.ToString()}] [{Prefix}] {msg}";
            else if (PrependTimestamp)
                // Timestamp
                msg = $"[{DateTime.Now.ToString()}] {msg}";
            else if (!string.IsNullOrWhiteSpace(Prefix))
                // Prefix
                msg = $"[{Prefix}] {msg}";

            if (GlobalFilter.IsFiltered(type) || Filter.IsFiltered(type))
                OnFilteredLog?.Invoke(null, new DiscordLogEventArgs(new DiscordLogLine(msg, type)));
            else
                OnLog?.Invoke(null, new DiscordLogEventArgs(new DiscordLogLine(msg, type)));
        }

        /// <summary>
        /// Logs a heartbeat message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogHeartbeat(string msg)
        {
            Log(msg, DiscordLogType.Heartbeat);
        }

        /// <summary>
        /// Logs an unnecessary message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogUnnecessary(string msg)
        {
            Log(msg, DiscordLogType.Unnecessary);
        }

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogVerbose(string msg)
        {
            Log(msg, DiscordLogType.Verbose);
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogInfo(string msg)
        {
            Log(msg, DiscordLogType.Info);
        }

        /// <summary>
        /// Logs an important message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogImportant(string msg)
        {
            Log(msg, DiscordLogType.Important);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogWarning(string msg)
        {
            Log(msg, DiscordLogType.Warning);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="msg">The contents of this log.</param>
        public void LogError(string msg)
        {
            Log(msg, DiscordLogType.Error);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public void LogError(Exception ex)
        {
            Log(ex.ToString(), DiscordLogType.Error);
        }
    }
}
