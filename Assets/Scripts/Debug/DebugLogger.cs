using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

// add to this if you want additional channels
public enum LogChannel
{
    Global,
    Gameplay,
    AI,
    UI,
    Audio,
    Network,
    Persistence, 
    Analytics,
    Systems      
}

public enum LogLevel
{
    Verbose = 0,
    Info    = 1,
    Warning = 2,
    Error   = 3
}

public static class DebugLogger
{
    private static readonly Dictionary<LogChannel, LogLevel> channelLevels =
        new Dictionary<LogChannel, LogLevel>();

    private static readonly Dictionary<LogChannel, string> channelFileNames =
        new Dictionary<LogChannel, string>();

    private static readonly HashSet<LogChannel> wroteHeader =
        new HashSet<LogChannel>();

    private static readonly object sync = new object();
    
    private static readonly string logsFolder;
    private static readonly string sessionStamp;
    private static readonly string sessionFolder;
    
    public static bool EchoToUnityConsole = true;

    // Timestamp used for this process session, looks like:
    // 2025-09-16_10-42-03.</summary>
    public static string SessionStamp => sessionStamp;

    // Absolute path to this session's log directory
    public static string SessionFolderPath => sessionFolder;

    static DebugLogger()
    {
        logsFolder = Path.Combine(Application.persistentDataPath, "Logs");
        Directory.CreateDirectory(logsFolder);

        sessionStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        sessionFolder = Path.Combine(logsFolder, sessionStamp);
        Directory.CreateDirectory(sessionFolder);
        
        foreach (LogChannel channel in Enum.GetValues(typeof(LogChannel)))
        {
            channelLevels[channel]    = LogLevel.Info;
            channelFileNames[channel] = channel.ToString().ToLower() + ".log";
        }
    }

    // Set the minimum level required for a channel to write
    public static void SetLevel(LogChannel channel, LogLevel level)
    {
        lock (sync) { channelLevels[channel] = level; }
    }

    // Set the same minimum level for ALL channels
    public static void SetAllLevels(LogLevel level)
    {
        lock (sync)
        {
            foreach (LogChannel c in Enum.GetValues(typeof(LogChannel)))
                channelLevels[c] = level;
        }
    }

    // If you want to override the file name used by a channel (like "combat.log")
    public static void SetChannelFileName(LogChannel channel, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;
        lock (sync)
        {
            channelFileNames[channel] = fileName;
            if (wroteHeader.Contains(channel)) wroteHeader.Remove(channel);
        }
    }
    private static string GetChannelFilePath(LogChannel channel)
    {
        string fileName;
        lock (sync) { fileName = channelFileNames[channel]; }
        return Path.Combine(sessionFolder, fileName);
    }

    // Call from anywhere to log something
    public static void Log(LogChannel channel, string message, LogLevel level = LogLevel.Info)
    {
        LogInternal(channel, message, level, null);
    }

    // If you want to log exceptions: includes message + stack trace.</summary>
    public static void LogException(LogChannel channel, Exception ex, string contextMessage = null)
    {
        string msg = contextMessage == null
            ? $"Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"
            : $"{contextMessage}\nException: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
        LogInternal(channel, msg, LogLevel.Error, ex);
    }

    private static void LogInternal(LogChannel channel, string message, LogLevel level, Exception ex)
    {
        LogLevel threshold;
        string filePath;

        lock (sync)
        {
            threshold = channelLevels[channel];
            filePath  = GetChannelFilePath(channel);
        }

        if (level < threshold) return;

        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string entry = $"{timeStamp} [{level}] [{channel}] {message}";

        lock (sync)
        {
            if (!wroteHeader.Contains(channel))
            {
                File.AppendAllText(filePath,
                    $"--- SESSION START {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---\n");
                wroteHeader.Add(channel);
            }
            File.AppendAllText(filePath, entry + Environment.NewLine);
        }

        if (EchoToUnityConsole)
        {
            if (level == LogLevel.Error)
            {
                if (ex != null) Debug.LogException(ex);
                else Debug.LogError(entry);
            }
            else if (level == LogLevel.Warning)
            {
                Debug.LogWarning(entry);
            }
            else
            {
                Debug.Log(entry);
            }
        }
    }


    // (Optional) Keep only the newest N session folders and delete older ones
    // Call at startup if you want automatic cleanup.
    public static void PruneOldSessions(int keepNewest = 5)
    {
        try
        {
            if (keepNewest < 1) return;
            if (!Directory.Exists(logsFolder)) return;

            var dirs = new DirectoryInfo(logsFolder).GetDirectories();
            Array.Sort(dirs, (a, b) => b.CreationTimeUtc.CompareTo(a.CreationTimeUtc)); // newest first

            for (int i = keepNewest; i < dirs.Length; i++)
            {
                if (string.Equals(dirs[i].FullName, sessionFolder, StringComparison.OrdinalIgnoreCase))
                    continue;
                dirs[i].Delete(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"DebugLogger: Failed to prune old sessions: {ex.Message}");
        }
    }
}
