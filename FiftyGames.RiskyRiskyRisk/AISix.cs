using Microsoft.Xna.Framework;

namespace FiftyGames.RiskyRiskyRisk;

internal class AISix : AICommander
{
	public AISix(Color color, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds)
		: base(color, numOfPlayers, playerNum, ref pm, ref sounds, "Alichino")
	{
	}

	protected override void SelectCountry(int i, int j)
	{
		if (((_countries.Count < 6 && _countries[i].Dice > 1) || (_countries.Count >= 6 && _countries[i].Dice > 4)) && (_countries[i].ConnectedCountries[j].Owner == null || (_countries[i].ConnectedCountries[j].Owner != this && _countries[i].ConnectedCountries[j].Dice <= _countries[i].Dice)))
		{
			_selectedCountry = _countries[i];
			_selectedCountry.Select();
			_enemyCountry = _countries[i].ConnectedCountries[j];
			_state = State.SelectEnemy;
		}
	}
}
