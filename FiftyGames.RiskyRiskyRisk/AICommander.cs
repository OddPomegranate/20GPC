using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RiskyRiskyRisk;

internal abstract class AICommander : Commander
{
	private bool _isFinished;

	private int _timer;

	private string _name;

	private int _staleMateCount;

	public override Color Color => _color;

	public override string Name => _name;

	protected int StaleMateCount => _staleMateCount;

	public AICommander(Color color, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds, string name)
		: base(null, color, numOfPlayers, playerNum, ref pm, ref sounds)
	{
		_isAI = true;
		_name = name;
		_staleMateCount = 0;
	}

	public override void Update(List<Country> countries, GameTime gameTime, Random _random, ref BattleManager battleManager)
	{
		_timer += gameTime.ElapsedGameTime.Milliseconds;
		switch (_state)
		{
		case State.SelectCountry:
		{
			if (_timer <= 200)
			{
				break;
			}
			for (int i = 0; i < _countries.Count; i++)
			{
				for (int j = 0; j < _countries[i].ConnectedCountries.Count; j++)
				{
					SelectCountry(i, j);
					if (_state == State.SelectEnemy)
					{
						_sounds.CreateGameSoundCue("riskyRiskyRisk Move").Play();
						break;
					}
					if (_isFinished)
					{
						_staleMateCount++;
						break;
					}
				}
				if (_state == State.SelectEnemy || _isFinished)
				{
					_staleMateCount = -1;
					break;
				}
				if (i + 1 == _countries.Count)
				{
					_isFinished = true;
					_staleMateCount++;
				}
			}
			if (_state != State.SelectEnemy)
			{
				break;
			}
			_staleMateCount = 0;
			foreach (Country connectedCountry in _selectedCountry.ConnectedCountries)
			{
				if (connectedCountry.Owner != this)
				{
					connectedCountry.Flash(active: true);
				}
			}
			countries.Sort(Country.SortSelected);
			_timer = 0;
			break;
		}
		case State.SelectEnemy:
			if (_timer <= 250)
			{
				break;
			}
			_enemyCountry.Select();
			foreach (Country connectedCountry2 in _selectedCountry.ConnectedCountries)
			{
				if (connectedCountry2.Owner != this)
				{
					connectedCountry2.Flash(active: false);
				}
			}
			countries.Sort(Country.SortSelected);
			ChangeState(Direction.Up, battleManager);
			_sounds.CreateGameSoundCue("riskyRiskyRisk Select").Play();
			_timer = 0;
			break;
		case State.PreFight:
			if (_timer <= 250)
			{
				break;
			}
			if (_enemyCountry.Owner == null)
			{
				BattleEnd(countries, _selectedCountry, _enemyCountry, battleManager);
			}
			else
			{
				battleManager.BeginBattle(_selectedCountry, _enemyCountry);
				if (!_enemyCountry.Owner.IsAI)
				{
					_sounds.CreateGameSoundCue("riskyRiskyRisk Invade").Play();
				}
				ChangeState(Direction.Up, battleManager);
			}
			_timer = 0;
			break;
		}
		if (_state == State.Fight && !battleManager.IsBattling)
		{
			if (battleManager.DefenderWon)
			{
				BattleEnd(countries, _enemyCountry, _selectedCountry, battleManager);
			}
			else
			{
				BattleEnd(countries, _selectedCountry, _enemyCountry, battleManager);
			}
		}
		if (_isFinished)
		{
			_isFinished = false;
			battleManager.EndTurn();
		}
	}

	protected abstract void SelectCountry(int i, int j);

	public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
	{
	}
}
