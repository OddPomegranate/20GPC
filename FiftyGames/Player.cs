using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace FiftyGames;

public class Player
{
	protected PlayerIndex _index;

	protected PlayerManager _manager;

	protected GamePadManager _gamePad;

	protected Gamer _gamer;

	protected string _name;

	protected float _musicVolume;

	protected float _effectVolume;

	protected byte _sortMode;

	protected byte _colorIndex;

	protected bool _vibrate;

	protected bool _loadingProfile;

	protected bool _profileGamerProblem;

	public PlayerIndex PlayerIndex => _index;

	public GamePadManager GamePadManager
	{
		get
		{
			return _gamePad;
		}
		set
		{
			_gamePad = value;
			_gamePad.Player = this;
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public float MusicVolume
	{
		get
		{
			return _musicVolume;
		}
		set
		{
			_musicVolume = value;
		}
	}

	public float EffectVolume
	{
		get
		{
			return _effectVolume;
		}
		set
		{
			_effectVolume = value;
		}
	}

	public byte SortMode
	{
		get
		{
			return _sortMode;
		}
		set
		{
			_sortMode = value;
		}
	}

	public byte ColorIndex
	{
		get
		{
			return _colorIndex;
		}
		set
		{
			_colorIndex = value;
		}
	}

	public bool AllowsVibration
	{
		get
		{
			return _vibrate;
		}
		set
		{
			_vibrate = value;
		}
	}

	public bool WaitingForProfileLoad
	{
		get
		{
			return _loadingProfile;
		}
		set
		{
			_loadingProfile = value;
		}
	}

	public PlayerManager Manager => _manager;

	public Gamer Gamer => _gamer;

	public bool GamerProblem
	{
		get
		{
			return _profileGamerProblem;
		}
		set
		{
			_profileGamerProblem = value;
		}
	}

	public Player(PlayerIndex index, PlayerManager playerManager, SoundManager soundManager)
	{
		_manager = playerManager;
		_index = index;
		_gamer = Gamer.SignedInGamers[index];
		if (_gamer == null)
		{
			_name = "Player " + (int)(index + 1);
		}
		else
		{
			_name = _gamer.Gamertag;
		}
		_gamePad = null;
		_musicVolume = soundManager.MusicVolume;
		_effectVolume = soundManager.EffectVolume;
		_colorIndex = (byte)index;
		_vibrate = true;
		SignedInGamer.SignedOut += GamerSignedOut;
	}

	private void GamerSignedOut(object sender, SignedOutEventArgs e)
	{
		SignedInGamer gamer = e.Gamer;
		if (gamer.PlayerIndex == _index && gamer.Gamertag == _name)
		{
			GameConsole.PrintString("Player: " + gamer.Gamertag + " has signed out. Player " + (int)(_index + 1) + " is conflicted.");
			_profileGamerProblem = true;
		}
	}

	public Color Colour()
	{
		return _manager.GetPlayerColor(this);
	}

	public Color Colour(float saturation, float luminescence)
	{
		return _manager.GetPlayerColor(this, luminescence, saturation);
	}
}
