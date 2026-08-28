using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Zombie;

internal class LightMapEditor
{
	private class EditorLight
	{
		private string _lightMaskPath;

		private Texture2D _lightMask;

		private RenderTarget2D _lightMaskRT;

		private Light _light;

		private float _scale;

		private float _intensity;

		private Color _color;

		public string LightMaskPath => _lightMaskPath;

		public Texture2D LightMask => _lightMask;

		public RenderTarget2D LightMaskRT => _lightMaskRT;

		public Light Light => _light;

		public float Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
			}
		}

		public float Intensity
		{
			get
			{
				return _intensity;
			}
			set
			{
				_intensity = value;
			}
		}

		public Color Color
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
			}
		}

		public EditorLight(Light light, Texture2D lightMask, string lightMaskPath, RenderTarget2D lightMaskRT, float scale, float intesity, Color color)
		{
			_light = light;
			_lightMask = lightMask;
			_lightMaskPath = lightMaskPath;
			_lightMaskRT = lightMaskRT;
			_scale = scale;
			_intensity = intesity;
			_color = color;
		}

		public void WriteToStream(BinaryWriter bw)
		{
			bw.Write(_light.Position.X);
			bw.Write(_light.Position.Y);
			bw.Write(_light.MaskScale.X);
			bw.Write(_light.MaskScale.Y);
			bw.Write(_light.MaskRotation);
			bw.Write(_light.Radius);
			bw.Write(_lightMaskPath);
			bw.Write(_scale);
			bw.Write(_intensity);
			bw.Write(_color.PackedValue);
		}

		public static EditorLight InitFromStream(Stream stream, GraphicsDevice graphicsDevice, ContentManager content)
		{
			Color color = default(Color);
			BinaryReader binaryReader = new BinaryReader(stream);
			Vector2 position = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
			Vector2 scale = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
			float rotation = binaryReader.ReadSingle();
			float radius = binaryReader.ReadSingle();
			string text = binaryReader.ReadString();
			float scale2 = binaryReader.ReadSingle();
			float intesity = binaryReader.ReadSingle();
			color.PackedValue = binaryReader.ReadUInt32();
			color.A = byte.MaxValue;
			Texture2D texture2D = content.Load<Texture2D>(text);
			RenderTarget2D renderTarget2D = new RenderTarget2D(graphicsDevice, texture2D.Width, texture2D.Height);
			Light light = new Light(position, radius, renderTarget2D, renderTarget2D, rotation, scale);
			return new EditorLight(light, texture2D, text, renderTarget2D, scale2, intesity, color);
		}
	}

	private GraphicsDevice _graphicsDevice;

	private ContentManager _contentManager;

	private Texture2D _circleLightTexture;

	private RenderTarget2D _lightMap;

	private RenderTarget2D _bluredFinalLightMap;

	private RenderTarget2D _saveOutLightMap;

	private ShadowHelper2D _shadowHelper;

	private Texture2D _occulderMap;

	private int _clickedLight = -1;

	private SpriteFont _font;

	private List<Texture2D> _lightMasks = new List<Texture2D>();

	private List<RenderTarget2D> _lightMaskRTs = new List<RenderTarget2D>();

	private List<Light> _lights = new List<Light>();

	private List<float> _lightScales = new List<float>();

	private List<EditorLight> _editorLights = new List<EditorLight>();

	private bool _hasLoadedLightsFromStream;

	private int[] _lightColor = new int[3];

	private int _selectedColor;

	private SinglePixelTexture _singlePixelTexture;

	public RenderTarget2D LightMap
	{
		get
		{
			return _lightMap;
		}
		set
		{
			_lightMap = value;
		}
	}

	public LightMapEditor(GraphicsDevice gd, ContentManager contentManager, Texture2D occluderMap)
	{
		_lightMap = new RenderTarget2D(gd, occluderMap.Width, occluderMap.Height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		_bluredFinalLightMap = new RenderTarget2D(gd, 1280, 720, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		_saveOutLightMap = new RenderTarget2D(gd, occluderMap.Width, occluderMap.Height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
		_singlePixelTexture = new SinglePixelTexture(gd);
		_occulderMap = occluderMap;
		_graphicsDevice = gd;
		_contentManager = contentManager;
		_circleLightTexture = contentManager.Load<Texture2D>("Zombie/LightNode");
		_font = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_shadowHelper = new ShadowHelper2D(ShadowMapSize._1024, occluderMap.Width, occluderMap.Height);
		_lightColor[0] = 255;
		_lightColor[1] = 255;
		_lightColor[2] = 255;
	}

	public void BakeToTextureFile(SpriteBatch spriteBatch)
	{
		if (!_lightMap.IsDisposed)
		{
			_graphicsDevice.SetRenderTarget(_saveOutLightMap);
			_graphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			spriteBatch.Draw(_lightMap, Vector2.Zero, Color.White);
			spriteBatch.End();
			_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
			string text = DateTime.Now.ToString("MM-dd-yyyy-HH-mm");
			StreamWriter streamWriter = new StreamWriter(text + ".png");
			_saveOutLightMap.SaveAsPng(streamWriter.BaseStream, _saveOutLightMap.Width, _saveOutLightMap.Height);
			streamWriter.Flush();
			streamWriter.Close();
			streamWriter = new StreamWriter("latestBake.png");
			_saveOutLightMap.SaveAsPng(streamWriter.BaseStream, _saveOutLightMap.Width, _saveOutLightMap.Height);
			streamWriter.Flush();
			streamWriter.Close();
			streamWriter = new StreamWriter(text + ".lme");
			BinaryWriter binaryWriter = new BinaryWriter(streamWriter.BaseStream);
			binaryWriter.Write(_editorLights.Count);
			for (int i = 0; i < _editorLights.Count; i++)
			{
				_editorLights[i].WriteToStream(binaryWriter);
			}
			streamWriter.Flush();
			streamWriter.Close();
			streamWriter = new StreamWriter("latest.lme");
			binaryWriter = new BinaryWriter(streamWriter.BaseStream);
			binaryWriter.Write(_editorLights.Count);
			for (int j = 0; j < _editorLights.Count; j++)
			{
				_editorLights[j].WriteToStream(binaryWriter);
			}
			streamWriter.Flush();
			streamWriter.Close();
		}
	}

	private bool CheckSelectedLightIndexOk(int index)
	{
		if (index < _editorLights.Count && index >= 0)
		{
			return true;
		}
		return false;
	}

	public void Update(Vector2 offset)
	{
		if (!_hasLoadedLightsFromStream)
		{
			StreamReader streamReader = new StreamReader("latest.lme");
			BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream);
			int num = (int)binaryReader.ReadUInt32();
			for (int i = 0; i < num; i++)
			{
				_editorLights.Add(EditorLight.InitFromStream(streamReader.BaseStream, _graphicsDevice, _contentManager));
			}
			binaryReader.Close();
			_hasLoadedLightsFromStream = true;
		}
		if (InputState.LeftButtonClicked())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl))
			{
				Texture2D texture2D = _contentManager.Load<Texture2D>("Zombie/RealCircleLight");
				RenderTarget2D renderTarget2D = new RenderTarget2D(_graphicsDevice, texture2D.Width, texture2D.Height);
				Light light = new Light(InputState.GetMouseCoords() - offset, 10f, renderTarget2D, renderTarget2D, 0f, Vector2.One);
				int clickedLight = _clickedLight;
				_clickedLight = _editorLights.Count;
				_editorLights.Add(new EditorLight(light, texture2D, "Zombie/RealCircleLight", renderTarget2D, 1f, 2f, Color.White));
				if (clickedLight != -1)
				{
					_lightColor[0] = _editorLights[clickedLight].Color.R;
					_lightColor[1] = _editorLights[clickedLight].Color.G;
					_lightColor[2] = _editorLights[clickedLight].Color.B;
					Color color = _editorLights[_clickedLight].Color;
					color.PackedValue = _editorLights[clickedLight].Color.PackedValue;
					_editorLights[_clickedLight].Color = color;
					_editorLights[_clickedLight].Intensity = _editorLights[clickedLight].Intensity;
					_editorLights[_clickedLight].Scale = _editorLights[clickedLight].Scale;
				}
				else
				{
					_lightColor[0] = 255;
					_lightColor[1] = 255;
					_lightColor[2] = 255;
				}
			}
			else
			{
				for (int j = 0; j < _editorLights.Count; j++)
				{
					float num2 = Vector2.Distance(_editorLights[j].Light.Position, InputState.GetMouseCoords() - offset);
					Console.WriteLine((object)num2);
					if (num2 < 100f)
					{
						_clickedLight = j;
						_lightColor[0] = _editorLights[j].Color.R;
						_lightColor[1] = _editorLights[j].Color.G;
						_lightColor[2] = _editorLights[j].Color.B;
						break;
					}
				}
			}
		}
		if (InputState.MiddleButtonClicked())
		{
			for (int k = 0; k < _editorLights.Count; k++)
			{
				float num3 = Vector2.Distance(_editorLights[k].Light.Position, InputState.GetMouseCoords() - offset);
				Console.WriteLine((object)num3);
				if (num3 < 100f)
				{
					_editorLights.RemoveAt(k);
					break;
				}
			}
		}
		if (InputState.LeftButtonHeld() && CheckSelectedLightIndexOk(_clickedLight))
		{
			_editorLights[_clickedLight].Light.Position = InputState.GetMouseCoords() - offset;
		}
		if (InputState.MouseStateChanged())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl))
			{
				if (InputState.GetCurrentMouseState().ScrollWheelValue > InputState.GetPreviousMouseState().ScrollWheelValue)
				{
					if (CheckSelectedLightIndexOk(_clickedLight))
					{
						_editorLights[_clickedLight].Intensity += 0.05f;
					}
				}
				else if (InputState.GetCurrentMouseState().ScrollWheelValue < InputState.GetPreviousMouseState().ScrollWheelValue && CheckSelectedLightIndexOk(_clickedLight))
				{
					_editorLights[_clickedLight].Intensity -= 0.05f;
				}
			}
			else if (InputState.GetCurrentMouseState().ScrollWheelValue > InputState.GetPreviousMouseState().ScrollWheelValue)
			{
				if (CheckSelectedLightIndexOk(_clickedLight))
				{
					_editorLights[_clickedLight].Scale += 0.05f;
				}
			}
			else if (InputState.GetCurrentMouseState().ScrollWheelValue < InputState.GetPreviousMouseState().ScrollWheelValue && CheckSelectedLightIndexOk(_clickedLight))
			{
				_editorLights[_clickedLight].Scale -= 0.05f;
			}
		}
		if (InputState.KeyboardStateChanged())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Up))
			{
				if (_selectedColor > 0)
				{
					_selectedColor--;
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Down))
			{
				if (_selectedColor < 2)
				{
					_selectedColor++;
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.D0) && _lightColor[_selectedColor] > 0)
			{
				_lightColor[_selectedColor] = 0;
				_editorLights[_clickedLight].Color = new Color(_lightColor[0], _lightColor[1], _lightColor[2]);
			}
		}
		if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Left))
		{
			if (_lightColor[_selectedColor] > 0)
			{
				_lightColor[_selectedColor]--;
				_editorLights[_clickedLight].Color = new Color(_lightColor[0], _lightColor[1], _lightColor[2]);
			}
		}
		else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Right) && _lightColor[_selectedColor] < 255)
		{
			_lightColor[_selectedColor]++;
			_editorLights[_clickedLight].Color = new Color(_lightColor[0], _lightColor[1], _lightColor[2]);
		}
	}

	public void ApplyInitialLightMap(SpriteBatch spriteBatch, Texture2D existingLightMap)
	{
		_graphicsDevice.SetRenderTarget(_lightMap);
		_graphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		spriteBatch.Draw(existingLightMap, Vector2.Zero, Color.White);
		spriteBatch.End();
		_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
	}

	public void BakeLights(SpriteBatch spriteBatch, Texture2D existingLightMap)
	{
		_graphicsDevice.SetRenderTarget(_lightMap);
		_graphicsDevice.Clear(Color.Black);
		_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		for (int i = 0; i < _editorLights.Count; i++)
		{
			_graphicsDevice.SetRenderTarget(_editorLights[i].LightMaskRT);
			_graphicsDevice.Clear(Color.Black);
			_shadowHelper.ShadowEffect.Parameters["InputTexture"].SetValue(_editorLights[i].LightMask);
			_shadowHelper.ShadowEffect.Parameters["copyMultiplier"].SetValue(_editorLights[i].Intensity);
			_shadowHelper.ShadowEffect.Parameters["copyColor"].SetValue(Color.White.ToVector4());
			_shadowHelper.ShadowEffect.CurrentTechnique = _shadowHelper.ShadowEffect.Techniques["Copy"];
			spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _shadowHelper.ShadowEffect);
			spriteBatch.Draw(_editorLights[i].LightMask, new Vector2(_editorLights[i].LightMask.Width / 2, _editorLights[i].LightMask.Height / 2), null, Color.White, 0f, new Vector2(_editorLights[i].LightMask.Width / 2, _editorLights[i].LightMask.Height / 2), _editorLights[i].Scale, SpriteEffects.None, 0f);
			spriteBatch.End();
			_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
			_shadowHelper.StartDrawingOccluders(_editorLights[i].Light);
			spriteBatch.Begin();
			spriteBatch.Draw(_occulderMap, Vector2.Zero, Color.White);
			spriteBatch.End();
			_shadowHelper.EndDrawingOccluders(spriteBatch, _lightMap, _editorLights[i].Color, blur: false, BlendState.Additive);
		}
	}

	private string IsSelected(int index)
	{
		if (index == _selectedColor)
		{
			return ">";
		}
		return "";
	}

	public void DrawLightEditorOverlay(SpriteBatch spriteBatch, Vector2 offset)
	{
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		for (int i = 0; i < _editorLights.Count; i++)
		{
			spriteBatch.Draw(_circleLightTexture, new Rectangle((int)_editorLights[i].Light.Position.X + (int)offset.X, (int)_editorLights[i].Light.Position.Y + (int)offset.Y, 64, 64), null, Color.Red, 0f, new Vector2(32f, 32f), SpriteEffects.None, 0f);
			spriteBatch.DrawString(_font, i.ToString(), _editorLights[i].Light.Position + offset, Color.White);
		}
		spriteBatch.DrawString(_font, IsSelected(0) + "RED: " + _lightColor[0], new Vector2(100f, 650f), Color.White);
		spriteBatch.DrawString(_font, IsSelected(1) + "GREEN: " + _lightColor[1], new Vector2(100f, 670f), Color.White);
		spriteBatch.DrawString(_font, IsSelected(2) + "BLUE: " + _lightColor[2], new Vector2(100f, 690f), Color.White);
		spriteBatch.End();
	}

	public void DrawLightMap(SpriteBatch spriteBatch, Vector2 offset)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_lightMap, offset, Color.White);
		spriteBatch.End();
	}
}
