using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal class ListButton : TextComponent
{
	protected bool _highlight;

	protected bool _enabled;

	public bool IsHighlighted
	{
		get
		{
			return _highlight;
		}
		set
		{
			_highlight = value;
		}
	}

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
		}
	}

	public ListButton()
	{
		_highlight = false;
		_enabled = true;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		if (_enabled)
		{
			if (_highlight)
			{
				_desiredSize.Y = 40f;
				_desiredColour = new Color(155, 155, 255) * 1f;
				_depth = 0.2f;
			}
			else
			{
				_desiredSize.Y = 34f;
				_desiredColour = new Color(102, 102, 255) * 0.8f;
				_depth = 0.01f;
			}
		}
		base.Update(gameTime);
	}

	public void ToggleHighlight()
	{
		_highlight = !_highlight;
	}
}
