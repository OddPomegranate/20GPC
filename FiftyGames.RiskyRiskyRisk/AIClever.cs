using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FiftyGames.RiskyRiskyRisk;

internal class AIClever : AICommander
{
	private bool _isDefensless;

	private List<int> _smallestConnections;

	private List<int> _closestConnections;

	private int _smallness;

	private int _closeness;

	public AIClever(Color color, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds)
		: base(color, numOfPlayers, playerNum, ref pm, ref sounds, "Malacoda")
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
			_closeness = 0;
			_smallness = 0;
			_isDefensless = false;
		}
		if (!_isDefensless && base.StaleMateCount < 2 && _countries[i].Dice < 7)
		{
			foreach (Country connectedCountry in _countries[i].ConnectedCountries)
			{
				if (connectedCountry != _countries[i].ConnectedCountries[j] && connectedCountry.Owner != this && connectedCountry.Owner != null)
				{
					_isDefensless = true;
					break;
				}
			}
			if (!_isDefensless)
			{
				foreach (Country connectedCountry2 in _countries[i].ConnectedCountries[j].ConnectedCountries)
				{
					if (connectedCountry2.Owner != this && connectedCountry2.Dice > _countries[i].Dice + 1)
					{
						_isDefensless = true;
						break;
					}
				}
			}
		}
		if (!_isDefensless && _countries[i].Dice > 1 && (_countries[i].ConnectedCountries[j].Owner == null || (_countries[i].ConnectedCountries[j].Owner != this && ((_countries[i].ConnectedCountries[j].Dice > 6) ? (_countries[i].ConnectedCountries[j].Dice <= _countries[i].Dice) : (_countries[i].ConnectedCountries[j].Dice < _countries[i].Dice)))))
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
			foreach (Country connectedCountry3 in _countries[i].ConnectedCountries[j].ConnectedCountries)
			{
				if (connectedCountry3.Owner == this)
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
		if (_isDefensless || _smallestConnections.Count == 0 || j != _countries[i].ConnectedCountries.Count - 1)
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
