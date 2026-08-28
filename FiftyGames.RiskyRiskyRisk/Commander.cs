using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.RiskyRiskyRisk;

internal class Commander
{
	public enum State
	{
		SelectCountry,
		SelectEnemy,
		PreFight,
		Fight
	}

	public enum Direction
	{
		Up = 1,
		Down = -1
	}

	private Player _player;

	private Texture2D _texture;

	private Texture2D _collisionTexture;

	private Color _countryColor;

	private Color _diceColor;

	protected Vector2 _position = new Vector2(640f, 360f);

	protected Color _color;

	protected bool _alive = true;

	protected int _score;

	protected int _playerNum;

	protected Vector2 _size;

	protected List<Country> _countries;

	protected Country _selectedCountry;

	protected Country _enemyCountry;

	protected State _state;

	protected bool _isMyTurn;

	protected Vector2 _velocity = new Vector2(4f, -4f);

	protected PlayerManager _pm;

	protected bool _isAI;

	protected SoundManager _sounds;

	public bool Alive => _alive;

	public bool IsMyTurn
	{
		get
		{
			return _isMyTurn;
		}
		set
		{
			_isMyTurn = value;
		}
	}

	public Vector2 Position => _position;

	public virtual Color Color => _color;

	public virtual Color CountryColor => _countryColor;

	public virtual Color DiceColor => _diceColor;

	public int Score => _score;

	public int PlayerNum => _playerNum;

	public List<Country> Countries
	{
		get
		{
			return _countries;
		}
		set
		{
			_countries = value;
		}
	}

	public bool IsAI
	{
		get
		{
			return _isAI;
		}
		set
		{
			_isAI = value;
		}
	}

	public virtual string Name => _player.Name;

	public Commander(Player player, Color color, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds)
	{
		_player = player;
		_color = color;
		_countryColor = Color.Lerp(Color.GhostWhite, _color, 0.5f);
		_diceColor = Color.Lerp(Color.GhostWhite, _color, 0.2f);
		_playerNum = playerNum;
		_pm = pm;
		_countries = new List<Country>();
		_state = State.SelectCountry;
		_sounds = sounds;
	}

	public void LoadContent(GraphicsDevice gd, ContentManager content)
	{
		_texture = content.Load<Texture2D>("RiskyRiskyRisk/Sprites/cursor");
		_size = new Vector2(_texture.Width, _texture.Height);
		_collisionTexture = new Texture2D(gd, 1, 1);
		_collisionTexture.SetData(new Color[1] { Color.Black });
	}

	public virtual void Update(List<Country> countries, GameTime gameTime, Random _random, ref BattleManager battleManager)
	{
		if (_player.GamePadManager.ButtonWasPressed(Buttons.Y))
		{
			battleManager.IsForfeiting = true;
		}
		if (battleManager.IsForfeiting)
		{
			if (_player.GamePadManager.ButtonWasPressed(Buttons.B))
			{
				battleManager.IsForfeiting = false;
			}
			else if (_player.GamePadManager.ButtonWasPressed(Buttons.A))
			{
				if (_selectedCountry != null)
				{
					_selectedCountry.DeSelect();
				}
				if (_enemyCountry != null)
				{
					_enemyCountry.DeSelect();
				}
				battleManager.ForfeitCommander(this);
			}
			return;
		}
		if (!battleManager.IsBattling)
		{
			Movement();
		}
		if (_player.GamePadManager.ButtonWasPressed(Buttons.A))
		{
			switch (_state)
			{
			case State.SelectCountry:
				foreach (Country country in _countries)
				{
					if (country.IsSelected || country.Dice == 1 || !Helper.PerPixelCollision(country.Texture, country.Position, _collisionTexture, _position + _size / 2f))
					{
						continue;
					}
					_selectedCountry = country;
					_selectedCountry.Select();
					_sounds.CreateGameSoundCue("riskyRiskyRisk Move").Play();
					foreach (Country connectedCountry in _selectedCountry.ConnectedCountries)
					{
						if (connectedCountry.Owner != this)
						{
							connectedCountry.Flash(active: true);
						}
					}
					countries.Sort(Country.SortSelected);
					ChangeState(Direction.Up, battleManager);
					break;
				}
				break;
			case State.SelectEnemy:
				foreach (Country connectedCountry2 in _selectedCountry.ConnectedCountries)
				{
					if (connectedCountry2.Owner == this || connectedCountry2.IsSelected || !Helper.PerPixelCollision(connectedCountry2.Texture, connectedCountry2.Position, _collisionTexture, _position + _size / 2f))
					{
						continue;
					}
					_enemyCountry = connectedCountry2;
					_enemyCountry.Select();
					_sounds.CreateGameSoundCue("riskyRiskyRisk Select").Play();
					foreach (Country connectedCountry3 in _selectedCountry.ConnectedCountries)
					{
						if (connectedCountry3.Owner != this)
						{
							connectedCountry3.Flash(active: false);
						}
					}
					countries.Sort(Country.SortSelected);
					ChangeState(Direction.Up, battleManager);
					break;
				}
				break;
			case State.PreFight:
				if (_enemyCountry.Owner == null)
				{
					BattleEnd(countries, _selectedCountry, _enemyCountry, battleManager);
					break;
				}
				battleManager.BeginBattle(_selectedCountry, _enemyCountry);
				_sounds.CreateGameSoundCue("riskyRiskyRisk Invade").Play();
				ChangeState(Direction.Up, battleManager);
				break;
			}
		}
		if (_player.GamePadManager.ButtonWasPressed(Buttons.B))
		{
			switch (_state)
			{
			case State.SelectEnemy:
				if (_selectedCountry == null)
				{
					break;
				}
				_selectedCountry.DeSelect();
				_sounds.CreateGameSoundCue("riskyRiskyRisk Deselect").Play();
				foreach (Country connectedCountry4 in _selectedCountry.ConnectedCountries)
				{
					if (connectedCountry4.Owner != this)
					{
						connectedCountry4.Flash(active: false);
					}
				}
				_selectedCountry = null;
				countries.Sort(Country.SortSelected);
				ChangeState(Direction.Down, battleManager);
				break;
			case State.PreFight:
				if (_enemyCountry == null)
				{
					break;
				}
				_enemyCountry.DeSelect();
				_sounds.CreateGameSoundCue("riskyRiskyRisk Deselect").Play();
				foreach (Country connectedCountry5 in _selectedCountry.ConnectedCountries)
				{
					if (connectedCountry5.Owner != this)
					{
						connectedCountry5.Flash(active: true);
					}
				}
				_enemyCountry = null;
				countries.Sort(Country.SortSelected);
				ChangeState(Direction.Down, battleManager);
				break;
			}
		}
		if (_player.GamePadManager.ButtonWasPressed(Buttons.X))
		{
			switch (_state)
			{
			case State.SelectCountry:
				countries.Sort(Country.SortSelected);
				battleManager.EndTurn();
				break;
			case State.SelectEnemy:
				if (_selectedCountry != null)
				{
					_selectedCountry.DeSelect();
					foreach (Country connectedCountry6 in _selectedCountry.ConnectedCountries)
					{
						if (connectedCountry6.Owner != this)
						{
							connectedCountry6.Flash(active: false);
						}
					}
					_selectedCountry = null;
				}
				countries.Sort(Country.SortSelected);
				battleManager.EndTurn();
				break;
			case State.PreFight:
				if (_enemyCountry != null)
				{
					_enemyCountry.DeSelect();
				}
				if (_selectedCountry != null)
				{
					_selectedCountry.DeSelect();
				}
				countries.Sort(Country.SortSelected);
				battleManager.EndTurn();
				break;
			}
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
	}

	protected void BattleEnd(List<Country> countries, Country winner, Country loser, BattleManager battleManager)
	{
		if (winner.Owner == this)
		{
			if (loser.Owner != null && loser.Owner._countries.Count == 1)
			{
				battleManager.RemoveCommander(loser.Owner);
			}
			AddCountry(_enemyCountry);
			loser.Dice = ((loser.MaxDice >= winner.Dice - 1) ? (winner.Dice - 1) : loser.MaxDice);
			winner.Dice = ((loser.MaxDice >= winner.Dice - 1) ? 1 : (winner.Dice - loser.MaxDice));
			_sounds.CreateGameSoundCue("riskyRiskyRisk Succeed").Play();
		}
		else
		{
			loser.Dice = 1;
			_sounds.CreateGameSoundCue("riskyRiskyRisk Fail").Play();
		}
		loser.DeSelect();
		winner.DeSelect();
		countries.Sort(Country.SortSelected);
		ChangeState(State.SelectCountry, battleManager);
	}

	private void Movement()
	{
		if (_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X > 0f)
		{
			if (_position.X + _size.X < 1280f)
			{
				_position.X += _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X * (_velocity.X + _velocity.X * _player.GamePadManager.GamePadStateCurrent.Triggers.Right);
			}
		}
		else if (_position.X > 0f)
		{
			_position.X += _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X * (_velocity.X + _velocity.X * _player.GamePadManager.GamePadStateCurrent.Triggers.Right);
		}
		if (_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y < 0f)
		{
			if (_position.Y + _size.Y < 720f)
			{
				_position.Y += _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y * (_velocity.Y + _velocity.Y * _player.GamePadManager.GamePadStateCurrent.Triggers.Right);
			}
		}
		else if (_position.Y > 0f)
		{
			_position.Y += _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y * (_velocity.Y + _velocity.Y * _player.GamePadManager.GamePadStateCurrent.Triggers.Right);
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font)
	{
		spriteBatch.Draw(_texture, _position, Color.Lerp(Color.GhostWhite, _pm.GetPlayerColor(_player), 0.7f));
	}

	public void AddDice(Random _random)
	{
		int num = 1;
		List<Country> list = new List<Country>();
		foreach (Country country in _countries)
		{
			if (country.Dice != country.MaxDice)
			{
				list.Add(country);
			}
			foreach (Country country2 in _countries)
			{
				country2.IsFloodChecked = false;
			}
			num = Math.Max(num, country.Flood(this));
		}
		num = (int)((float)num * 1.25f);
		if (list.Count == 0)
		{
			return;
		}
		Helper.Shuffle(list, _random);
		for (int i = 0; i != num; i++)
		{
			list[i % list.Count].Dice++;
			if (list[i % list.Count].Dice == list[i % list.Count].MaxDice)
			{
				list.RemoveAt(i % list.Count);
				if (list.Count == 0)
				{
					break;
				}
			}
		}
	}

	public void ChangeState(Direction direction, BattleManager battleManager)
	{
		ChangeState((State)(((int)_state + (int)direction) % 4), battleManager);
	}

	public void ChangeState(State state, BattleManager battleManager)
	{
		_state = state;
		battleManager.State = _state;
	}

	public void ResetState()
	{
		_state = State.SelectCountry;
	}

	public void AddCountry(Country country)
	{
		if (country.Owner != null)
		{
			country.Owner._countries.Remove(country);
		}
		country.Owner = this;
		_countries.Add(country);
	}

	public void AddCountries(List<Country> countries)
	{
		foreach (Country country in countries)
		{
			if (country.Owner != null)
			{
				country.Owner._countries.Remove(country);
			}
			country.Owner = this;
			_countries.Add(country);
		}
	}

	public void RemoveCountry(Country country, Commander newOwner)
	{
		country.Owner = newOwner;
		_countries.Remove(country);
		newOwner.AddCountry(country);
	}
}
