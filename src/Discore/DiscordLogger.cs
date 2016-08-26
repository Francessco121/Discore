using System;
using System.Collections.Generic;

namespace Discore
{
    public class DiscordLogEventArgs : EventArgs
    {
        public readonly DiscordLogLine Line;

        internal DiscordLogEventArgs(DiscordLogLine line)
        {
            Line = line;
        }
    }

    public class DiscordLogLine
    {
        public readonly string Message;
        public readonly DiscordLogType Type;

        internal DiscordLogLine(string msg, DiscordLogType type)
        {
            Message = msg;
            Type = type;
        }
    }

    public enum DiscordLogType
    {
        Heartbeat,
        Unnecessary,
        Verbose,
        Info,
        Important,
        Warning,
        Error
    }

    public class DiscordLoggerFilter
    {
        Dictionary<DiscordLogType, bool> settings;

        public DiscordLoggerFilter()
        {
            settings = new Dictionary<DiscordLogType, bool>();

            foreach (object v in Enum.GetValues(typeof(DiscordLogType)))
                settings[(DiscordLogType)v] = false;
        }

        public void Set(DiscordLogType type, bool filter)
        {
            settings[type] = filter;
        }

        public bool IsFiltered(DiscordLogType type)
        {
            return settings[type];
        }
    }

    public class DiscordLogger
    {
        public static event EventHandler<DiscordLogEventArgs> OnLog;
        public static event EventHandler<DiscordLogEventArgs> OnFilteredLog;

        public static bool PrependTimestamp { get; set; } = true;
        public static DiscordLogger Default { get; }
        public static DiscordLoggerFilter GlobalFilter { get; }

        public string Prefix { get; set; }
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

        public DiscordLogger(string prefix)
        {
            Prefix = prefix;
            Filter = new DiscordLoggerFilter();
        }

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

        public void LogHeartbeat(string msg)
        {
            Log(msg, DiscordLogType.Heartbeat);
        }

        public void LogUnnecessary(string msg)
        {
            Log(msg, DiscordLogType.Unnecessary);
        }

        public void LogVerbose(string msg)
        {
            Log(msg, DiscordLogType.Verbose);
        }

        public void LogInfo(string msg)
        {
            Log(msg, DiscordLogType.Info);
        }

        public void LogImportant(string msg)
        {
            Log(msg, DiscordLogType.Important);
        }

        public void LogWarning(string msg)
        {
            Log(msg, DiscordLogType.Warning);
        }

        public void LogError(string msg)
        {
            Log(msg, DiscordLogType.Error);
        }

        public void LogError(Exception ex)
        {
            Log(ex.ToString(), DiscordLogType.Error);
        }
    }
}
