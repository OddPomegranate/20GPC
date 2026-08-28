using System;
using System.IO;
using System.Threading.Tasks;

namespace FiftyGames;

// DIAGNOSTIC AID (added while investigating Linux/Steam Deck-only crashes in
// Acid Escape, Not My Brains!, Battle Dice, Gun Lab and Sunken Ruin): when the
// game is launched through Steam/Gamescope there is normally no visible
// console, so an unhandled exception's message and stack trace - which would
// otherwise be the single most useful piece of information for diagnosing a
// platform-specific crash - is simply lost. This writes it to a plain text
// file next to the game instead, so it survives after the process dies.
//
// This does NOT fix anything by itself. It only makes the next crash (on any
// platform) leave behind a readable trail: exception type, message, full
// stack trace, and inner exceptions, timestamped, appended so multiple runs
// accumulate. Once a game crashes again, "crash_log.txt" (next to the game's
// executable, or under the OS's local-app-data folder if the game's own
// folder isn't writable) should show exactly where and why.
internal static class CrashLogger
{
	private static readonly object _lock = new object();

	private static string _logPath;

	public static void Initialize()
	{
		AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = e.ExceptionObject as Exception;
			Log(ex, ex == null ? e.ExceptionObject?.ToString() : null, "AppDomain.UnhandledException" + (e.IsTerminating ? " (terminating)" : ""));
		};

		TaskScheduler.UnobservedTaskException += delegate(object sender, UnobservedTaskExceptionEventArgs e)
		{
			Log(e.Exception, null, "TaskScheduler.UnobservedTaskException");
			e.SetObserved();
		};
	}

	public static void Log(Exception ex, string fallbackDescription, string context)
	{
		try
		{
			string body = ex != null ? ex.ToString() : (fallbackDescription ?? "(no exception object available)");
			string entry = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}{3}{4}{4}",
				DateTime.Now,
				string.IsNullOrEmpty(context) ? "" : "(" + context + ") ",
				"Unhandled exception:",
				Environment.NewLine + body,
				Environment.NewLine);

			lock (_lock)
			{
				File.AppendAllText(GetLogPath(), entry);
			}
		}
		catch
		{
			// Logging the crash must never itself throw during a crash.
		}
	}

	private static string GetLogPath()
	{
		if (_logPath != null)
		{
			return _logPath;
		}

		try
		{
			string dir = AppContext.BaseDirectory;
			string probe = Path.Combine(dir, ".crashlog_write_test");
			File.WriteAllText(probe, "");
			File.Delete(probe);
			_logPath = Path.Combine(dir, "crash_log.txt");
		}
		catch
		{
			string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "20GPC");
			Directory.CreateDirectory(dir);
			_logPath = Path.Combine(dir, "crash_log.txt");
		}

		return _logPath;
	}
}
