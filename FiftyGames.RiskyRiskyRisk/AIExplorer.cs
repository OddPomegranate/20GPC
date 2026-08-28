using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FiftyGames.RiskyRiskyRisk;

internal class AIExplorer : AICommander
{
	private List<int> _smallestConnections;

	private List<int> _closestConnections;

	private int _smallness;

	private int _closeness;

	public AIExplorer(Color color, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds)
		: base(color, numOfPlayers, playerNum, ref pm, ref sounds, "Scarmiglione")
	{
		_smallestConnections = new List<int>();
		_closestConnections = new List<int>();
	}

	protected override void SelectCountry(int i, int j)
	{
		if (j == 0)
		{
			_smallestConnections.Clear();
			_closestConnections.Clear();
			_smallness = 0;
		}
		if (_countries[i].Dice > 1 && (_countries[i].ConnectedCountries[j].Owner == null || (_countries[i].ConnectedCountries[j].Owner != this && _countries[i].ConnectedCountries[j].Dice < _countries[i].Dice)))
		{
			if (_smallestConnections.Count != 0)
			{
				if (_countries[i].ConnectedCountries[j].Dice < _smallness)
				{
					_smallestConnections.Clear();
					_smallestConnections.Add(j);
					_smallness = _countries[i].ConnectedCountries[j].Dice;
				}
				else if (_countries[i].ConnectedCountries[j].Dice == _smallness)
				{
					_smallestConnections.Add(j);
				}
			}
			else
			{
				_smallness = _countries[i].ConnectedCountries[j].Dice;
				_smallestConnections.Add(j);
			}
			int num = 0;
			foreach (Country connectedCountry in _countries[i].ConnectedCountries[j].ConnectedCountries)
			{
				if (connectedCountry.Owner == this)
				{
					num++;
				}
			}
			if (num > _closeness)
			{
				_closestConnections.Clear();
				_closestConnections.Add(j);
				_closeness = num;
			}
			else if (num == _closeness)
			{
				_closestConnections.Add(j);
			}
		}
		if (_smallestConnections.Count == 0 || j != _countries[i].ConnectedCountries.Count - 1)
		{
			return;
		}
		int index = _smallestConnections[0];
		foreach (int closestConnection in _closestConnections)
		{
			if (_smallestConnections.Contains(closestConnection))
			{
				index = closestConnection;
				break;
			}
		}
		_selectedCountry = _countries[i];
		_selectedCountry.Select();
		_enemyCountry = _countries[i].ConnectedCountries[index];
		_state = State.SelectEnemy;
	}
}
