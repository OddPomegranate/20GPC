using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal class Monitor
{
	private GraphicsDevice _graphicsDevice;

	private SpriteBatch _spriteBatch;

	private RenderTarget2D _rt;

	private Texture2D _onePixelTexture;

	private Color _color;

	public Queue<int> _dataStream;

	private LineRenderer _lineRenderer;

	private List<Vector2> _points;

	public List<VertexPositionColor> _verts;

	private SpriteFont _font;

	private float _spaceInterval;

	public Monitor(GraphicsDevice graphicsDevice, ContentManager contentManager, int width, int height, float spaceInterval, Color graphColor, float backgroundAlpha)
	{
		_rt = new RenderTarget2D(graphicsDevice, width, height);
		_onePixelTexture = new Texture2D(graphicsDevice, 1, 1);
		Color[] data = new Color[1]
		{
			new Color(0.2f, 0.2f, 0.2f, 0.8f)
		};
		_onePixelTexture.SetData(data);
		_spaceInterval = spaceInterval;
		_color = graphColor;
		_graphicsDevice = graphicsDevice;
		_font = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_spriteBatch = new SpriteBatch(graphicsDevice);
		_dataStream = new Queue<int>();
		_lineRenderer = new LineRenderer(_graphicsDevice, contentManager, _rt.Bounds);
		_points = new List<Vector2>();
		_verts = new List<VertexPositionColor>();
	}

	public void AddEntry(int value)
	{
		lock (_dataStream)
		{
			value = (int)MathHelper.Clamp(value, 0f, 100f);
			_dataStream.Enqueue(value);
			if ((float)_dataStream.Count * _spaceInterval > (float)_rt.Bounds.Width)
			{
				_dataStream.Dequeue();
			}
			_points.Clear();
			for (int i = 0; i < _dataStream.Count; i++)
			{
				_points.Add(new Vector2((float)i * _spaceInterval, _rt.Bounds.Height - _rt.Bounds.Height / 100 * _dataStream.ElementAt(i)));
			}
		}
		lock (_verts)
		{
			_verts.Clear();
			for (int j = 0; j < _points.Count - 2; j++)
			{
				_verts.Add(new VertexPositionColor(new Vector3(_points[j], 0f), _color));
				_verts.Add(new VertexPositionColor(new Vector3(_points[j + 1], 0f), _color));
			}
		}
	}

	public Texture2D DrawToTexture()
	{
		_graphicsDevice.SetRenderTarget(_rt);
		_graphicsDevice.Clear(Color.Transparent);
		_spriteBatch.Begin();
		_spriteBatch.Draw(_onePixelTexture, _rt.Bounds, Color.DarkGray);
		if (_dataStream.Count > 0)
		{
			_spriteBatch.DrawString(_font, _dataStream.ElementAt(_dataStream.Count - 1).ToString(), new Vector2(10f, 10f), Color.White);
		}
		_spriteBatch.End();
		_lineRenderer.DrawShape(_verts.ToArray(), Vector2.Zero);
		_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		return _rt;
	}
}
