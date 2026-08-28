using Microsoft.Xna.Framework;

namespace FiftyGames.RiskyRiskyRisk;

internal class AIAgressive : AICommander
{
	public AIAgressive(Color color, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds)
		: base(color, numOfPlayers, playerNum, ref pm, ref sounds, "Cagnazzo")
	{
	}

	protected override void SelectCountry(int i, int j)
	{
		if (_countries[i].Dice > 1 && (_countries[i].ConnectedCountries[j].Owner == null || (_countries[i].ConnectedCountries[j].Owner != this && ((_countries[i].Dice < 5 && _countries[i].ConnectedCountries[j].Dice < _countries[i].Dice) || (_countries[i].Dice >= 5 && _countries[i].ConnectedCountries[j].Dice <= _countries[i].Dice)))))
		{
			_selectedCountry = _countries[i];
			_selectedCountry.Select();
			_enemyCountry = _countries[i].ConnectedCountries[j];
			_state = State.SelectEnemy;
		}
	}
}
