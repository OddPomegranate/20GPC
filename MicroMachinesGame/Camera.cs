using System.Collections.Generic;
using MicroMachinesGame.ISHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroMachinesGame;

internal class Camera
{
	private Rectangle _rect;

	private bool _isMovingTo;

	private bool _isZoomingTo;

	private Vector2 _position;

	private float _zoom;

	private Rectangle _screenBounds;

	private float _targetZoomLevel;

	private Vector2 _targetPosition;

	private float _zoomSpeed;

	private float _moveSpeed;

	private Rectangle _worldBounds;

	private float _minZoom;

	private float _maxZoom;

	public Camera(Rectangle cameraBounds, Rectangle worldBounds, float minZoom, float maxZoom)
	{
		_isMovingTo = false;
		_isZoomingTo = false;
		_screenBounds = cameraBounds;
		_worldBounds = worldBounds;
		_minZoom = minZoom;
		_maxZoom = maxZoom;
		_zoom = 1f;
		_position = new Vector2(cameraBounds.Width / 2, cameraBounds.Height / 2);
	}

	public void ZoomTo(float zoomLevel, float speed)
	{
		_isZoomingTo = true;
		if (zoomLevel < _minZoom)
		{
			_targetZoomLevel = _minZoom;
		}
		else if (zoomLevel > _maxZoom)
		{
			_targetZoomLevel = _maxZoom;
		}
		else
		{
			_targetZoomLevel = zoomLevel;
		}
		_zoomSpeed = speed;
	}

	public void SetZoom(float zoomLevel, bool stopZooming)
	{
		if (stopZooming)
		{
			_isZoomingTo = false;
		}
		if (zoomLevel < _minZoom)
		{
			_zoom = _minZoom;
		}
		else if (zoomLevel > _maxZoom)
		{
			_zoom = _maxZoom;
		}
		else
		{
			_zoom = zoomLevel;
		}
	}

	public void MoveTo(Vector2 destination, float speed)
	{
		_isMovingTo = true;
		_targetPosition = destination;
		_moveSpeed = speed;
	}

	public void SetPosition(Vector2 position, bool stopMoving)
	{
		if (stopMoving)
		{
			_isMovingTo = false;
		}
		_position = position;
	}

	public void Update(GameTime gameTime)
	{
		if (_isZoomingTo)
		{
			float num = _targetZoomLevel - _zoom;
			float num2 = num / _zoomSpeed;
			if (_zoom + num2 < num2)
			{
				_zoom = _targetZoomLevel;
			}
			else
			{
				_zoom += num2;
			}
		}
		if (_isMovingTo)
		{
			Vector2 vector = _targetPosition - _position;
			vector.Normalize();
			float num3 = Vector2.Distance(_targetPosition, _position);
			float num4 = num3 / _moveSpeed;
			if (!float.IsNaN(vector.X) && !float.IsNaN(vector.Y))
			{
				if (Vector2.Distance(_position + vector * num4, _targetPosition) < num4)
				{
					_position = _targetPosition;
				}
				else
				{
					_position += vector * num4;
				}
			}
		}
		ComputeRect(_position, _zoom, _screenBounds);
		_rect.X = (int)MathHelper.Clamp(_rect.X, 0f, _worldBounds.Width - _rect.Width);
		_rect.Y = (int)MathHelper.Clamp(_rect.Y, 0f, _worldBounds.Height - _rect.Height);
	}

	public void ComputeRect(Vector2 position, float zoom, Rectangle screenRect)
	{
		Rectangle rect = screenRect;
		rect.X = (int)(position.X - (float)screenRect.Width / 2f);
		rect.Y = (int)(position.Y - (float)screenRect.Height / 2f);
		int horizontalAmount = (int)((float)screenRect.Width - (float)screenRect.Width * zoom);
		int verticalAmount = (int)((float)screenRect.Height - (float)screenRect.Height * zoom);
		rect.Inflate(horizontalAmount, verticalAmount);
		_rect = rect;
	}

	public Vector2 GetPosition()
	{
		return new Vector2(_rect.X, _rect.Y);
	}

	public Rectangle GetRect()
	{
		return _rect;
	}

	public void SetPosition(Vector2 newPosition)
	{
		_position = newPosition;
	}

	public void ModifyPositionBy(float x, float y)
	{
		_position.X += x;
		_position.Y += y;
	}

	public float GetZoom()
	{
		return _zoom;
	}

	public void DrawDebugRect()
	{
		Rectangle rect = _rect;
		List<VertexPositionColor> list = new List<VertexPositionColor>();
		list.Add(new VertexPositionColor(new Vector3(rect.X, rect.Y, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X + rect.Width, rect.Y, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X, rect.Y, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X, rect.Y + rect.Height, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X + rect.Width, rect.Y, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X + rect.Width, rect.Y + rect.Height, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X, rect.Y + rect.Height, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X + rect.Width, rect.Y + rect.Height, 0f), Color.White));
		GeometryHelper.LineRenderer.DrawShape(list.ToArray(), Vector2.Zero);
	}

	public static void DrawDebugRect(Rectangle rect)
	{
		List<VertexPositionColor> list = new List<VertexPositionColor>();
		list.Add(new VertexPositionColor(new Vector3(rect.X, rect.Y, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X + rect.Width, rect.Y, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X, rect.Y, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X, rect.Y + rect.Height, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X + rect.Width, rect.Y, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X + rect.Width, rect.Y + rect.Height, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X, rect.Y + rect.Height, 0f), Color.White));
		list.Add(new VertexPositionColor(new Vector3(rect.X + rect.Width, rect.Y + rect.Height, 0f), Color.White));
		GeometryHelper.LineRenderer.DrawShape(list.ToArray(), Vector2.Zero);
	}
}
