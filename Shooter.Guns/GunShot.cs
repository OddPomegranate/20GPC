using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter.ISHelpers;

namespace Shooter.Guns;

internal class GunShot : Shot
{
	private Vector2 _currentPosition;

	private float _speed;

	private Texture2D _texture;

	private Random _random;

	private Vector2 _middleOfShot;

	private List<VertexPositionColor> _lineVerts;

	private bool _update;

	public GunShot(Texture2D shotTexture, Random random, Vector2 start, Vector2 end, float speed)
	{
		_texture = shotTexture;
		_start = start;
		_end = end;
		_direction = end - start;
		_direction.Normalize();
		_speed = speed;
		_currentPosition = start;
		_random = random;
		_middleOfShot = new Vector2(_texture.Bounds.Width, _texture.Bounds.Height);
		_lineVerts = new List<VertexPositionColor>();
		base.IsDead = false;
		_update = false;
	}

	public override void Update(GameTime gameTime)
	{
		if (_update)
		{
			base.IsDead = true;
		}
		float value = Vector2.Distance(_start, _end);
		_currentPosition = _start + _direction * MathHelper.Lerp(0f, value, (float)_random.NextDouble());
		_update = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!base.IsDead)
		{
			_lineVerts.Add(new VertexPositionColor(new Vector3(_start, 0f), Color.White));
			_lineVerts.Add(new VertexPositionColor(new Vector3(_end, 0f), Color.White));
			GeometryHelper.LineRenderer.DrawShape(_lineVerts.ToArray(), Vector2.Zero);
		}
	}
}
