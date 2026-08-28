namespace FiftyGames.ForeverWars;

internal class enemyScoreElement
{
	private string shipName;

	private int numberOfKills;

	public enemyScoreElement(string inShipName)
	{
		shipName = inShipName;
		numberOfKills = 1;
	}

	public void incrementKills()
	{
		numberOfKills++;
	}

	public int getKills()
	{
		return numberOfKills;
	}

	public string getName()
	{
		return shipName;
	}
}
