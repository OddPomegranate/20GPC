using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal class AnimationSequence
{
	private int _animationTime;

	protected int _animationTimeLimit;

	protected bool _animationFinished;

	protected SoundManager _soundManager;

	public int AnimationTimeElapsed => _animationTime;

	public int AnimationTimeLimit
	{
		get
		{
			return _animationTimeLimit;
		}
		set
		{
			_animationTimeLimit = ((value >= 0) ? value : 0);
		}
	}

	public bool AnimationFinished => _animationFinished;

	public virtual void Initialise()
	{
		_animationTime = 0;
		_animationTimeLimit = 0;
		_animationFinished = false;
	}

	public virtual void Load(ContentManager contentManager, SoundManager soundManager)
	{
		_soundManager = soundManager;
	}

	public virtual void Update(GameTime gameTime)
	{
		if (_animationTimeLimit != 0 && _animationTime >= _animationTimeLimit)
		{
			_animationFinished = true;
		}
		_animationTime += gameTime.ElapsedGameTime.Milliseconds;
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
	}
}
