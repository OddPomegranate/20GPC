using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.HammerFight;

internal class TimedBreakablePiece : BreakablePiece
{
	private Vector2 _positionCurrent;

	private Vector2 _positionPrevious;

	public TimedBreakablePiece(Vertices polyVerts, float width, float height, GraphicsDevice gd, Matrix projection, int time)
		: base(polyVerts, width, height, gd, projection)
	{
	}

	public void Update(GameTime gameTime, bool isBroken)
	{
		if (isBroken)
		{
			if (_fixture.Body != null)
			{
				_positionPrevious = _positionCurrent;
				_positionCurrent = _fixture.Body.Position;
			}
			if (_positionPrevious == _positionCurrent)
			{
				Dispose();
			}
		}
	}
}
