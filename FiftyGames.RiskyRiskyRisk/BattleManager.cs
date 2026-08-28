using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RiskyRiskyRisk;

internal class BattleManager
{
	private List<Commander> _commanders;

	private Commander _currentCommander;

	private Commander.State _state;

	private Dice[] _selectedBattleDice;

	private Dice[] _enemyBattleDice;

	private Random _random;

	private int _totalCountries;

	private Country _selectedCountry;

	private Country _enemyCountry;

	private int _selectedDiceNum;

	private int _enemyDiceNum;

	private int _startPlayer;

	private int _turn;

	private bool _isBattling;

	private bool _defenderWon;

	public static int MaxCountrySize = 10;

	private int _timer;

	private int _selectedCurrentDice;

	private int _enemyCurrentDice;

	private bool _isDiceSynched;

	private int _selectedTotal;

	private int _enemyTotal;

	private bool _isFinished;

	private bool _isSelectedFinished;

	private bool _isEnemyFinished;

	private bool _isGameOver;

	private bool _isGameOverFinished;

	private bool _isForfeiting;

	private int _numCommanders;

	private int _numPlayers;

	private bool _isDemoMode;

	private int _fullCountries;

	private Prompt[] _prompts;

	private SoundManager _sounds;

	private bool _isRollingSound;

	private bool _isFlyingSound;

	public bool IsBattling => _isBattling;

	public bool IsGameOver
	{
		get
		{
			return _isGameOver;
		}
		set
		{
			_isGameOver = value;
			if (_isGameOver)
			{
				_prompts[0].IsDrawn = false;
				_prompts[1].IsDrawn = false;
				_prompts[2].IsDrawn = false;
				_prompts[3].IsDrawn = false;
			}
		}
	}

	public bool IsGameOverFinished => _isGameOverFinished;

	public bool DefenderWon => _defenderWon;

	public Commander CurrentCommander => _currentCommander;

	public Commander.State State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
			if (!_currentCommander.IsAI)
			{
				switch (_state)
				{
				case Commander.State.SelectCountry:
					_prompts[0].IsDrawn = true;
					_prompts[0].Text = "Select";
					_prompts[1].IsDrawn = false;
					_prompts[2].IsDrawn = true;
					_prompts[2].Text = "End Turn";
					_prompts[3].IsDrawn = true;
					_prompts[3].Text = "Forfeit";
					break;
				case Commander.State.SelectEnemy:
					_prompts[0].IsDrawn = true;
					_prompts[0].Text = "Select";
					_prompts[1].IsDrawn = true;
					_prompts[1].Text = "Cancel";
					_prompts[2].IsDrawn = true;
					_prompts[2].Text = "End Turn";
					_prompts[3].IsDrawn = true;
					_prompts[3].Text = "Forfeit";
					break;
				case Commander.State.PreFight:
					_prompts[0].IsDrawn = true;
					_prompts[0].Text = "Attack";
					_prompts[1].IsDrawn = true;
					_prompts[1].Text = "Cancel";
					_prompts[2].IsDrawn = true;
					_prompts[2].Text = "End Turn";
					_prompts[2].IsDrawn = true;
					_prompts[3].Text = "Forfeit";
					break;
				case Commander.State.Fight:
					_prompts[0].IsDrawn = false;
					_prompts[1].IsDrawn = false;
					_prompts[2].IsDrawn = false;
					_prompts[3].IsDrawn = false;
					break;
				}
			}
		}
	}

	public bool IsForfeiting
	{
		get
		{
			return _isForfeiting;
		}
		set
		{
			_isForfeiting = value;
			if (!_currentCommander.IsAI)
			{
				if (_isForfeiting)
				{
					_prompts[0].IsDrawn = true;
					_prompts[0].Text = "Forfeit";
					_prompts[1].IsDrawn = true;
					_prompts[1].Text = "Cancel";
					_prompts[2].IsDrawn = false;
					_prompts[3].IsDrawn = false;
				}
				else
				{
					State = _state;
				}
			}
		}
	}

	public BattleManager(Commander[] commanders, int numCommanders, int numPlayers, ref Random random, int totalCountries, bool isDemoMode, SoundManager sounds, Rectangle titleSafeArea)
	{
		_commanders = new List<Commander>(4);
		_commanders.AddRange(commanders);
		_random = random;
		_startPlayer = _random.Next(0, _commanders.Count);
		_currentCommander = _commanders[_startPlayer];
		_currentCommander.IsMyTurn = true;
		_turn = 0;
		_totalCountries = totalCountries;
		_numCommanders = numCommanders;
		_numPlayers = numPlayers;
		_isDemoMode = isDemoMode;
		_isGameOver = false;
		_isGameOverFinished = false;
		_fullCountries = 0;
		_sounds = sounds;
		_prompts = new Prompt[6]
		{
			new Prompt(new Vector2(158f, titleSafeArea.Bottom + -60), Prompt.Button.A, "Select"),
			new Prompt(new Vector2(414f, titleSafeArea.Bottom + -60), Prompt.Button.B, "Cancel"),
			new Prompt(new Vector2(670f, titleSafeArea.Bottom + -60), Prompt.Button.X, "End Turn"),
			new Prompt(new Vector2(926f, titleSafeArea.Bottom + -60), Prompt.Button.Y, "Forfeit"),
			new Prompt(new Vector2(128f, titleSafeArea.Top), Prompt.Button.None, "Player 2"),
			new Prompt(new Vector2(896f, titleSafeArea.Top), Prompt.Button.None, _currentCommander.Name, _currentCommander.CountryColor)
		};
		_prompts[1].IsDrawn = false;
		_prompts[4].IsDrawn = false;
		if (_currentCommander.IsAI)
		{
			_prompts[0].IsDrawn = false;
			_prompts[2].IsDrawn = false;
			_prompts[3].IsDrawn = false;
		}
	}

	public void LoadContent(ContentManager content, Rectangle titleSafeArea)
	{
		_selectedBattleDice = new Dice[MaxCountrySize + 1];
		_enemyBattleDice = new Dice[MaxCountrySize + 1];
		for (int i = 0; i != MaxCountrySize + 1; i++)
		{
			_selectedBattleDice[i] = new Dice();
			_selectedBattleDice[i].LoadContent(content, ref _random, titleSafeArea);
			_enemyBattleDice[i] = new Dice();
			_enemyBattleDice[i].LoadContent(content, ref _random, titleSafeArea);
		}
		Prompt[] prompts = _prompts;
		foreach (Prompt prompt in prompts)
		{
			prompt.LoadContent(content);
		}
	}

	public void BeginBattle(Country selectedCountry, Country enemyCountry)
	{
		_selectedCountry = selectedCountry;
		_enemyCountry = enemyCountry;
		_prompts[4].IsDrawn = true;
		_prompts[4].Text = _enemyCountry.Owner.Name;
		_prompts[4].Color = _enemyCountry.Owner.CountryColor;
		_isBattling = true;
		_defenderWon = true;
		_selectedDiceNum = _selectedCountry.Dice;
		_enemyDiceNum = _enemyCountry.Dice;
		_isFlyingSound = true;
		if (_currentCommander.IsAI && _enemyCountry.Owner.IsAI)
		{
			_selectedTotal = _random.Next(_selectedDiceNum, _selectedDiceNum * 6);
			_enemyTotal = _random.Next(_enemyDiceNum, _enemyDiceNum * 6);
			_defenderWon = _selectedTotal <= _enemyTotal;
			_isBattling = false;
			return;
		}
		if (_selectedDiceNum > _enemyDiceNum * 6)
		{
			_isBattling = false;
			_defenderWon = false;
			return;
		}
		if (_enemyDiceNum >= _selectedDiceNum * 6)
		{
			_isBattling = false;
			_defenderWon = true;
			return;
		}
		_timer = 0;
		_selectedCurrentDice = 0;
		_enemyCurrentDice = 0;
		_isDiceSynched = false;
		_selectedTotal = 0;
		_enemyTotal = 0;
		_isFinished = false;
		_isSelectedFinished = false;
		_isEnemyFinished = false;
		for (int num = MaxCountrySize; num != -1; num--)
		{
			_selectedBattleDice[num].Value = 0;
			if (num < _selectedDiceNum)
			{
				_selectedBattleDice[num].Spawn(_selectedCountry.DicePosition(num), isDefending: false, _selectedDiceNum - 1 - num);
			}
		}
		for (int num2 = MaxCountrySize; num2 != -1; num2--)
		{
			_enemyBattleDice[num2].Value = 0;
			if (num2 < _enemyDiceNum)
			{
				_enemyBattleDice[num2].Spawn(_enemyCountry.DicePosition(num2), isDefending: true, _enemyDiceNum - 1 - num2);
			}
		}
		_selectedCountry.Dice--;
		_enemyCountry.Dice--;
		_selectedBattleDice[_selectedCountry.Dice].IsAlive = true;
		_enemyBattleDice[_enemyCountry.Dice].IsAlive = true;
	}

	public void EndTurn()
	{
		_currentCommander.IsMyTurn = false;
		_turn++;
		_currentCommander = _commanders[(_commanders.IndexOf(_currentCommander) + 1) % _commanders.Count];
		_currentCommander.IsMyTurn = true;
		_currentCommander.ResetState();
		_prompts[5].Text = _currentCommander.Name;
		_prompts[5].Color = _currentCommander.CountryColor;
		if (_currentCommander.IsAI)
		{
			_prompts[0].IsDrawn = false;
			_prompts[1].IsDrawn = false;
			_prompts[2].IsDrawn = false;
			_prompts[3].IsDrawn = false;
		}
		if (_turn >= _numCommanders)
		{
			_currentCommander.AddDice(_random);
		}
		State = _state;
		IsGameOver = _isGameOver;
	}

	public void Update(GameTime gameTime)
	{
		if (_isBattling)
		{
			_timer += gameTime.ElapsedGameTime.Milliseconds;
			for (int num = _selectedDiceNum - 1; num != -1; num--)
			{
				_selectedBattleDice[num].Update(gameTime, _selectedDiceNum - 1 - num, _selectedCurrentDice, _selectedBattleDice[_selectedCurrentDice].Scale, _isDiceSynched, (_isEnemyFinished && _selectedDiceNum != _enemyDiceNum && _enemyTotal < _selectedTotal) ? 0.25f : 1f);
				ControlDice();
				if (!_selectedBattleDice[num].IsAlive && (num == _selectedDiceNum - 1 || _selectedBattleDice[num + 1].DState == Dice.State.Waiting))
				{
					_selectedCountry.Dice--;
					_selectedBattleDice[num].IsAlive = true;
					_isFlyingSound = true;
				}
			}
			for (int num2 = _enemyDiceNum - 1; num2 != -1; num2--)
			{
				_enemyBattleDice[num2].Update(gameTime, _enemyDiceNum - 1 - num2, _enemyCurrentDice, _enemyBattleDice[_enemyCurrentDice].Scale, _isDiceSynched, (_isSelectedFinished && _selectedDiceNum != _enemyDiceNum && _selectedTotal < _enemyTotal) ? 0.25f : 1f);
				ControlDice();
				if (!_enemyBattleDice[num2].IsAlive && (num2 == _enemyDiceNum - 1 || _enemyBattleDice[num2 + 1].DState == Dice.State.Waiting))
				{
					_enemyCountry.Dice--;
					_enemyBattleDice[num2].IsAlive = true;
					_isFlyingSound = true;
				}
			}
			if (_isFlyingSound)
			{
				_sounds.CreateGameSoundCue("riskyRiskyRisk Dice Fly In").Play();
				_isFlyingSound = false;
			}
		}
		if (_isGameOver)
		{
			_timer += gameTime.ElapsedGameTime.Milliseconds;
			if (_commanders.Count == 1)
			{
				if (_fullCountries < _totalCountries)
				{
					if (_timer > 150)
					{
						_timer -= 150;
						EndTurn();
						if (_commanders[0].Countries.Count < _totalCountries)
						{
							List<Country> list = new List<Country>();
							_fullCountries = 0;
							foreach (Country country in _commanders[0].Countries)
							{
								if (country.Dice == country.MaxDice)
								{
									_fullCountries++;
								}
								foreach (Country connectedCountry in country.ConnectedCountries)
								{
									if (connectedCountry.Owner != _commanders[0])
									{
										list.Add(connectedCountry);
									}
								}
							}
							_commanders[0].AddCountries(list);
						}
						else
						{
							_fullCountries = 0;
							foreach (Country country2 in _commanders[0].Countries)
							{
								if (country2.Dice == country2.MaxDice)
								{
									_fullCountries++;
								}
							}
						}
					}
				}
				else if (_timer > 1500)
				{
					_isGameOverFinished = true;
				}
			}
			else if (_timer > 300)
			{
				_isGameOver = false;
				_isGameOverFinished = true;
			}
		}
		if (_isGameOverFinished)
		{
			_prompts[0].IsDrawn = true;
			_prompts[0].Text = "New Game";
		}
	}

	public void ControlDice()
	{
		if (_isDiceSynched)
		{
			if (_selectedCurrentDice != _selectedDiceNum)
			{
				if (!_isSelectedFinished && _selectedBattleDice[_selectedDiceNum - 1 - _selectedCurrentDice].DState == Dice.State.Finishing)
				{
					_selectedTotal = 0;
					for (int i = 0; i < _selectedCurrentDice + 1; i++)
					{
						_selectedTotal += _selectedBattleDice[_selectedDiceNum - 1 - i].Value;
					}
					if (_selectedDiceNum - 1 - _selectedCurrentDice == 0)
					{
						_isSelectedFinished = true;
					}
				}
				if (_selectedBattleDice[_selectedDiceNum - 1 - _selectedCurrentDice].DState == Dice.State.Waiting && _selectedBattleDice[0].DState == Dice.State.Waiting && _selectedCurrentDice < _selectedDiceNum)
				{
					_selectedBattleDice[_selectedDiceNum - 1 - _selectedCurrentDice].DState = Dice.State.Attacking;
					_selectedBattleDice[_selectedDiceNum - 1 - _selectedCurrentDice].ResetTimer();
					_timer = 0;
				}
				if (_selectedBattleDice[_selectedDiceNum - 1 - _selectedCurrentDice].DState == Dice.State.Finishing)
				{
					_isRollingSound = true;
					_selectedCurrentDice++;
				}
			}
			if (_enemyCurrentDice != _enemyDiceNum)
			{
				if (!_isEnemyFinished && _enemyBattleDice[_enemyDiceNum - 1 - _enemyCurrentDice].DState == Dice.State.Finishing)
				{
					_enemyTotal = 0;
					for (int j = 0; j < _enemyCurrentDice + 1; j++)
					{
						_enemyTotal += _enemyBattleDice[_enemyDiceNum - 1 - j].Value;
					}
					if (_enemyDiceNum - 1 - _enemyCurrentDice == 0)
					{
						_isEnemyFinished = true;
					}
				}
				if (_enemyBattleDice[_enemyDiceNum - 1 - _enemyCurrentDice].DState == Dice.State.Waiting && _enemyBattleDice[0].DState == Dice.State.Waiting && _enemyCurrentDice < _enemyDiceNum)
				{
					_enemyBattleDice[_enemyDiceNum - 1 - _enemyCurrentDice].DState = Dice.State.Attacking;
					_enemyBattleDice[_enemyDiceNum - 1 - _enemyCurrentDice].ResetTimer();
					_timer = 0;
				}
				if (_enemyBattleDice[_enemyDiceNum - 1 - _enemyCurrentDice].DState == Dice.State.Finishing)
				{
					_isRollingSound = true;
					_enemyCurrentDice++;
				}
			}
			if (_isRollingSound)
			{
				_sounds.CreateGameSoundCue("riskyRiskyRisk Roll Dice").Play();
				_isRollingSound = false;
			}
		}
		else if (_selectedBattleDice[0].DState == Dice.State.Waiting && _enemyBattleDice[0].DState == Dice.State.Waiting)
		{
			_isDiceSynched = true;
		}
		if (_selectedCurrentDice == _selectedDiceNum && _enemyCurrentDice == _enemyDiceNum && _selectedBattleDice[0].DState == Dice.State.Finishing && _enemyBattleDice[0].DState == Dice.State.Finishing)
		{
			if (!_isFinished)
			{
				_timer = 0;
				_isFinished = true;
			}
			else if (_timer > 1000)
			{
				_defenderWon = _selectedTotal <= _enemyTotal;
				_selectedCountry.Dice = _selectedDiceNum;
				_enemyCountry.Dice = _enemyDiceNum;
				_prompts[4].IsDrawn = false;
				_isBattling = false;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, SpriteFont font, Rectangle titleSafeArea)
	{
		if (_isBattling)
		{
			for (int i = 0; i != _selectedDiceNum; i++)
			{
				_selectedBattleDice[i].Draw(spriteBatch, _selectedCountry.DiceColor);
			}
			for (int j = 0; j != _enemyDiceNum; j++)
			{
				_enemyBattleDice[j].Draw(spriteBatch, _enemyCountry.DiceColor);
			}
			Helper.DrawOutlinedText(spriteBatch, font, _selectedTotal.ToString(), new Vector2(640 + _selectedBattleDice[0].Size.X / 2, (float)(titleSafeArea.Top + 50) - font.MeasureString(_selectedTotal.ToString()).Y), _selectedCountry.Owner.CountryColor, Color.Black, Helper.OutlineType.Orthogonal);
			Helper.DrawOutlinedText(spriteBatch, font, _enemyTotal.ToString(), new Vector2((float)(640 - _selectedBattleDice[0].Size.X / 2) - font.MeasureString(_enemyTotal.ToString()).X, (float)(titleSafeArea.Top + 50) - font.MeasureString(_enemyTotal.ToString()).Y), _enemyCountry.Owner.CountryColor, Color.Black, Helper.OutlineType.Orthogonal);
		}
		if (!_isDemoMode)
		{
			Prompt[] prompts = _prompts;
			foreach (Prompt prompt in prompts)
			{
				prompt.Draw(spriteBatch, font);
			}
		}
	}

	public void RemoveCommander(Commander commander)
	{
		_commanders.Remove(commander);
		if (!commander.IsAI)
		{
			_numPlayers--;
		}
		if (_commanders.Count == 1 || (!_isDemoMode && _numPlayers == 0))
		{
			_isGameOver = true;
		}
	}

	public void ForfeitCommander(Commander commander)
	{
		_isForfeiting = false;
		foreach (Country country in commander.Countries)
		{
			country.Dice = 0;
			country.Owner = null;
		}
		RemoveCommander(commander);
		EndTurn();
	}
}
