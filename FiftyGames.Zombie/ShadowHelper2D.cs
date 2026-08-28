using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal class ShadowHelper2D
{
	private GraphicsDevice _graphicsDevice;

	public RenderTarget2D _userCasterMap;

	public RenderTarget2D _systemCasterMap;

	public RenderTarget2D _shadowMapWithMaskRT;

	public RenderTarget2D _distanceRT;

	public RenderTarget2D _distortRT;

	public RenderTarget2D _shadowMapRT;

	public RenderTarget2D _blurHorizontalMapRT;

	public RenderTarget2D _blurVerticalMapRT;

	public RenderTarget2D _rotatedMaskRT;

	private RenderTarget2D[] _reductionRTs;

	private Effect _effect;

	private FullscreenQuad _quad;

	private Vector2 _dimensions;

	private Vector2 _halfDimensions;

	private Light _currentLight;

	private int rtNeededCount;

	private Rectangle _screenRect;

	private BlendState _blendState;

	public Effect ShadowEffect => _effect;

	public FullscreenQuad QuadDrawer => _quad;

	public ShadowHelper2D(ShadowMapSize shadowMapSize, int screenWidth, int screenHeight)
	{
		_graphicsDevice = ZombieUtils.GraphicsDevice();
		_quad = new FullscreenQuad(_graphicsDevice);
		_userCasterMap = new RenderTarget2D(_graphicsDevice, screenWidth, screenHeight);
		_systemCasterMap = new RenderTarget2D(_graphicsDevice, (int)shadowMapSize, (int)shadowMapSize);
		_shadowMapWithMaskRT = new RenderTarget2D(_graphicsDevice, (int)shadowMapSize, (int)shadowMapSize);
		_distanceRT = new RenderTarget2D(_graphicsDevice, (int)shadowMapSize, (int)shadowMapSize, mipMap: false, SurfaceFormat.HalfVector2, DepthFormat.None);
		_distortRT = new RenderTarget2D(_graphicsDevice, (int)shadowMapSize, (int)shadowMapSize, mipMap: false, SurfaceFormat.HalfVector2, DepthFormat.None);
		_screenRect = default(Rectangle);
		_screenRect.Width = screenWidth;
		_screenRect.Height = screenHeight;
		rtNeededCount = 0;
		int num = (int)shadowMapSize;
		do
		{
			num /= 2;
			rtNeededCount++;
		}
		while (num != 2);
		_reductionRTs = new RenderTarget2D[rtNeededCount];
		int num2 = (int)shadowMapSize;
		for (int i = 0; i < rtNeededCount; i++)
		{
			num2 /= 2;
			_reductionRTs[i] = new RenderTarget2D(_graphicsDevice, num2, (int)shadowMapSize, mipMap: false, SurfaceFormat.HalfVector2, DepthFormat.None);
		}
		_shadowMapRT = new RenderTarget2D(_graphicsDevice, (int)shadowMapSize, (int)shadowMapSize);
		_blurHorizontalMapRT = new RenderTarget2D(_graphicsDevice, (int)shadowMapSize, (int)shadowMapSize);
		_blurVerticalMapRT = new RenderTarget2D(_graphicsDevice, (int)shadowMapSize, (int)shadowMapSize);
		_rotatedMaskRT = new RenderTarget2D(_graphicsDevice, (int)shadowMapSize, (int)shadowMapSize);
		_quad = new FullscreenQuad(_graphicsDevice);
		_effect = ZombieUtils.ContentManager().Load<Effect>("Zombie/ShadowHelper2DEffect");
		_dimensions = new Vector2((float)shadowMapSize, (float)shadowMapSize);
		_halfDimensions = _dimensions / 2f;
		_blendState = new BlendState();
		_blendState = BlendState.AlphaBlend;
	}

	public void StartDrawingOccluders(Light light)
	{
		_currentLight = light;
		_graphicsDevice.SetRenderTarget(_userCasterMap);
		_graphicsDevice.Clear(Color.Transparent);
	}

	public void EndDrawingOccluders(SpriteBatch spriteBatch, RenderTarget2D outRT, Color offsetColour, bool blur, BlendState blendState)
	{
		_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		_graphicsDevice.SetRenderTarget(_systemCasterMap);
		_graphicsDevice.Clear(Color.Transparent);
		Vector2 dimensions = _dimensions;
		dimensions /= 2f;
		int y = (int)dimensions.Y - (int)_currentLight.Position.Y;
		Rectangle destinationRectangle = new Rectangle((int)dimensions.X - (int)_currentLight.Position.X, y, _screenRect.Width, _screenRect.Height);
		spriteBatch.Begin();
		spriteBatch.Draw(_userCasterMap, destinationRectangle, Color.Black);
		spriteBatch.End();
		_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		_graphicsDevice.BlendState = BlendState.Opaque;
		_effect.GraphicsDevice.SetRenderTarget(_distanceRT);
		_graphicsDevice.Clear(Color.White);
		_effect.Parameters["outputDimensions"].SetValue(_dimensions);
		_effect.Parameters["InputTexture"].SetValue(_systemCasterMap);
		_effect.CurrentTechnique = _effect.Techniques["ComputeDistance"];
		foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			_quad.Render(Vector2.One * -1f, Vector2.One);
		}
		_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		_effect.GraphicsDevice.SetRenderTarget(_distortRT);
		_effect.GraphicsDevice.Clear(Color.White);
		_effect.Parameters["outputDimensions"].SetValue(_dimensions);
		_effect.Parameters["InputTexture"].SetValue(_distanceRT);
		_effect.CurrentTechnique = _effect.Techniques["Distort"];
		foreach (EffectPass pass2 in _effect.CurrentTechnique.Passes)
		{
			pass2.Apply();
			_quad.Render(Vector2.One * -1f, Vector2.One);
		}
		_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		_effect.CurrentTechnique = _effect.Techniques["HorizontalReduction"];
		_effect.Parameters["outputDimensions"].SetValue(_dimensions);
		RenderTarget2D renderTarget2D = _distortRT;
		_ = _reductionRTs[1];
		for (int i = 0; i < rtNeededCount; i++)
		{
			RenderTarget2D renderTarget2D2 = _reductionRTs[i];
			_graphicsDevice.SetRenderTarget(renderTarget2D2);
			_graphicsDevice.Clear(Color.White);
			_effect.Parameters["InputTexture"].SetValue(renderTarget2D);
			Vector2 value = new Vector2(1f / (float)renderTarget2D.Width, 1f / (float)renderTarget2D.Height);
			_effect.Parameters["TextureDimensions"].SetValue(value);
			foreach (EffectPass pass3 in _effect.CurrentTechnique.Passes)
			{
				pass3.Apply();
				_quad.Render(Vector2.One * -1f, Vector2.One);
			}
			_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
			renderTarget2D = renderTarget2D2;
		}
		_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		_effect.GraphicsDevice.SetRenderTarget(_shadowMapRT);
		_effect.GraphicsDevice.Clear(Color.White);
		_effect.Parameters["outputDimensions"].SetValue(_dimensions);
		_effect.Parameters["ShadowMapTexture"].SetValue(_reductionRTs[rtNeededCount - 1]);
		_effect.CurrentTechnique = _effect.Techniques["GetShadowMap"];
		foreach (EffectPass pass4 in _effect.CurrentTechnique.Passes)
		{
			pass4.Apply();
			_quad.Render(Vector2.One * -1f, Vector2.One);
		}
		_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		if (blur)
		{
			_effect.GraphicsDevice.SetRenderTarget(_blurHorizontalMapRT);
			_effect.GraphicsDevice.Clear(Color.Transparent);
			_effect.Parameters["outputDimensions"].SetValue(_dimensions);
			_effect.Parameters["InputTexture"].SetValue(_shadowMapRT);
			_effect.CurrentTechnique = _effect.Techniques["BlurHorizontally"];
			foreach (EffectPass pass5 in _effect.CurrentTechnique.Passes)
			{
				pass5.Apply();
				_quad.Render(Vector2.One * -1f, Vector2.One);
			}
			_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
			_effect.GraphicsDevice.SetRenderTarget(_blurVerticalMapRT);
			_effect.GraphicsDevice.Clear(Color.Transparent);
			_effect.Parameters["outputDimensions"].SetValue(_dimensions);
			_effect.Parameters["InputTexture"].SetValue(_blurHorizontalMapRT);
			_effect.CurrentTechnique = _effect.Techniques["BlurVerticallyAndAttenuate"];
			foreach (EffectPass pass6 in _effect.CurrentTechnique.Passes)
			{
				pass6.Apply();
				_quad.Render(Vector2.One * -1f, Vector2.One);
			}
			_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		}
		if (_currentLight.HasMask)
		{
			_effect.GraphicsDevice.SetRenderTarget(_rotatedMaskRT);
			_graphicsDevice.Clear(Color.Transparent);
			if (blur)
			{
				_effect.Parameters["InputTexture"].SetValue(_blurVerticalMapRT);
			}
			else
			{
				_effect.Parameters["InputTexture"].SetValue(_shadowMapRT);
			}
			_effect.Parameters["copyMultiplier"].SetValue(1);
			_effect.Parameters["copyColor"].SetValue(Color.White.ToVector4());
			_effect.CurrentTechnique = _effect.Techniques["Copy"];
			// FIX (round 6, offset-lighting bug): was drawn via SpriteBatch+customEffect
			// (spriteBatch.Begin(..., _effect)), the same pattern proven broken project-wide
			// (see Warp.fx/FinalPassEffect.fx/ScreenEffect.fx). Switched to Apply()+a raw
			// quad, matching every other fixed effect. The original SpriteBatch draw
			// centered the light mask sprite in the target, scaled to exactly fill it, and
			// rotated it around its own center -- RenderRotated() reproduces that with a
			// full -1..1 clip-space quad rotated the same way (see its own comment for the
			// Y-axis sign-flip reasoning).
			_effect.Parameters["Texture"]?.SetValue(_currentLight.LightMask);
			_effect.CurrentTechnique.Passes[0].Apply();
			_quad.RenderRotated(_currentLight.MaskRotation);
			_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
			_effect.GraphicsDevice.SetRenderTarget(_shadowMapWithMaskRT);
			_graphicsDevice.Clear(Color.Transparent);
			if (blur)
			{
				_effect.Parameters["ShadowMapTexture"].SetValue(_blurVerticalMapRT);
			}
			else
			{
				_effect.Parameters["ShadowMapTexture"].SetValue(_shadowMapRT);
			}
			_effect.CurrentTechnique = _effect.Techniques["ApplyMask"];
			// FIX (round 6, offset-lighting bug): same SpriteBatch+customEffect ->
			// Apply()+raw-quad conversion as the Copy pass above. This draw was already a
			// plain full-target 1:1 copy, so a full -1..1 quad reproduces it directly.
			_effect.Parameters["Texture"]?.SetValue(_rotatedMaskRT);
			_effect.CurrentTechnique.Passes[0].Apply();
			_quad.Render(Vector2.One * -1f, Vector2.One);
			_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		}
		else if (blur)
		{
			_shadowMapWithMaskRT = _blurVerticalMapRT;
		}
		else
		{
			_shadowMapWithMaskRT = _shadowMapRT;
		}
		_effect.GraphicsDevice.SetRenderTarget(outRT);
		_ = BlendState.AlphaBlend;
		_ = BlendState.Additive;
		spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
		y = (int)_halfDimensions.Y - (int)_currentLight.Position.Y;
		int num = (int)_halfDimensions.X - (int)_currentLight.Position.X;
		spriteBatch.Draw(_shadowMapWithMaskRT, new Rectangle(num * -1, y * -1, (int)_dimensions.X, (int)_dimensions.Y), offsetColour);
		spriteBatch.End();
		_effect.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
	}

	private Vector2 BackbufferSpaceToScreenSpace(Vector2 sourcePosition)
	{
		return new Vector2(sourcePosition.X / 512f, sourcePosition.Y / 512f);
	}
}
