using Microsoft.Xna.Framework;

namespace FiftyGames.RiskyRiskyRisk;

internal class AIDefensive : AICommander
{
	private bool _isDefensless;

	public AIDefensive(Color color, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds)
		: base(color, numOfPlayers, playerNum, ref pm, ref sounds, "Farfarello")
	{
	}

	protected override void SelectCountry(int i, int j)
	{
		if (j == 0)
		{
			_isDefensless = false;
		}
		if (_countries[i].Dice <= 1 || (_countries[i].ConnectedCountries[j].Owner != null && (_countries[i].ConnectedCountries[j].Owner == this || _countries[i].ConnectedCountries[j].Dice > _countries[i].Dice)))
		{
			return;
		}
		if (!_isDefensless && base.StaleMateCount < 2)
		{
			foreach (Country connectedCountry in _countries[i].ConnectedCountries)
			{
				if (connectedCountry != _countries[i].ConnectedCountries[j] && connectedCountry.Owner != this && connectedCountry.Owner != null)
				{
					_isDefensless = true;
					break;
				}
			}
		}
		if (!_isDefensless)
		{
			_selectedCountry = _countries[i];
			_selectedCountry.Select();
			_enemyCountry = _countries[i].ConnectedCountries[j];
			_state = State.SelectEnemy;
		}
	}
}
