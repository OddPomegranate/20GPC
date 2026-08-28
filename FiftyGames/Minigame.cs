using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FiftyGames;

internal class Minigame : DrawableGameComponent
{
	protected FiftyGames _framework;

	protected MinigameMeta _minigameMeta;

	protected PlayerManager _playerManager;

	protected ContentManager _contentManager;

	protected SoundManager _soundManager;

	protected Rectangle _titleSafeArea;

	protected bool _demoMode;

	public Minigame(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game)
	{
		_framework = (FiftyGames)game;
		_minigameMeta = minigame;
		_playerManager = playerManager;
		_contentManager = contentManager;
		_soundManager = soundManager;
		_titleSafeArea = ((FiftyGames)game).TitleSafeArea;
		_demoMode = demoMode;
	}

	public virtual void Quit()
	{
	}
}
