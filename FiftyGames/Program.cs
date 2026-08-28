namespace FiftyGames;

internal static class Program
{
	private static void Main(string[] args)
	{
		CrashLogger.Initialize();
		using FiftyGames fiftyGames = new FiftyGames();
		try
		{
			fiftyGames.Run();
		}
		catch (System.Exception ex)
		{
			CrashLogger.Log(ex, null, "Main thread");
			throw;
		}
	}
}
