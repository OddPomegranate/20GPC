using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class gridSystem
{
	private const float gridSpeedMultiplier = 0.8f;

	private GraphicsDevice graphicsDevice;

	private ContentManager contentManager;

	private Effect gridShader;

	private RenderTarget2D blankRenderTarget;

	private RenderTarget2D rawGridRT;

	private fullScreenQuad quad;

	private RenderTarget2D screenRT;

	private RenderTarget2D bulletRT;

	private Vector2 bulletPosition = new Vector2(640f, 480f);

	private Texture2D bullet;

	private Texture2D dummy;

	private Texture2D screenRTVTF;

	private Effect _effect;

	private RenderTarget2D bulletTempRT;

	private RenderTarget2D tempRenderTarget;

	private List<gridWarpEvent> warpEventList;

	private Effect diminishEffect;

	private Effect cloneEffect;

	private Effect maskOverlayEffect;

	private RenderTarget2D maskRotRT;

	private RenderTarget2D maskOverlayOutputRT;

	private Texture2D maskOverlayTexture;

	private Texture2D maskStripOverlayTexture;

	private Texture2D gridTileImage;

	private RenderTarget2D tempRenderTargetSmall;

	private Effect blueOverrideShader;

	private LineRender lineRenderer;

	private bool DEBUGView;

	private bool DEBUGKeyLock;

	private RenderTarget2D tempRT;

	private RenderTarget2D tempRT2;

	private RenderTarget2D tempRT3;

	private RenderTarget2D tempRT4;

	// TEMPORARY diagnostic (round 2, black-screen investigation): dumps the
	// grid tile render (pre-warp) and a duplicate post-warp composite to
	// RenderDiag/ForeverWars/ once, ~1.5s into the first Draw() call, to see
	// whether the grid is painted-but-covered or never painted at all.
	// Remove once findings are extracted.
	private RenderTarget2D warpCaptureRT;
	private int _renderDiagFrameCounter;
	private bool _renderDiagDumped;

	private void SaveRenderDiag(RenderTarget2D rt, string name, string dir)
	{
		using System.IO.FileStream stream = System.IO.File.Create(System.IO.Path.Combine(dir, name + ".png"));
		rt.SaveAsPng(stream, rt.Width, rt.Height);
	}

	public gridSystem(GraphicsDevice inGraphicsDevice, ContentManager inContentManager)
	{
		graphicsDevice = inGraphicsDevice;
		contentManager = inContentManager;
		quad = new fullScreenQuad(graphicsDevice);
		blankRenderTarget = new RenderTarget2D(graphicsDevice, 1280, 720);
		gridShader = contentManager.Load<Effect>("ForeverWars/Effects/gridShader");
		warpEventList = new List<gridWarpEvent>();
		_effect = contentManager.Load<Effect>("ForeverWars/GridWarp/Warp");
		diminishEffect = contentManager.Load<Effect>("ForeverWars/GridWarp/diminishEffect");
		tempRenderTarget = new RenderTarget2D(graphicsDevice, 2000, 2000);
		bulletTempRT = new RenderTarget2D(graphicsDevice, 1280, 720);
		screenRT = new RenderTarget2D(graphicsDevice, 1280, 720);
		rawGridRT = new RenderTarget2D(graphicsDevice, 1280, 720);
		bulletRT = new RenderTarget2D(graphicsDevice, 2000, 2000, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
		bullet = contentManager.Load<Texture2D>("ForeverWars/GridWarp/mask");
		dummy = new Texture2D(graphicsDevice, 10, 10);
		cloneEffect = contentManager.Load<Effect>("ForeverWars/GridWarp/cloneEffect");
		gridTileImage = contentManager.Load<Texture2D>("ForeverWars/Sprites/Tiled5");
		blueOverrideShader = contentManager.Load<Effect>("ForeverWars/GridWarp/blueEffect");
		maskStripOverlayTexture = contentManager.Load<Texture2D>("ForeverWars/GridWarp/stripMask");
		maskOverlayEffect = contentManager.Load<Effect>("ForeverWars/GridWarp/maskEffect");
		maskOverlayTexture = contentManager.Load<Texture2D>("ForeverWars/GridWarp/segmentMask");
		maskRotRT = new RenderTarget2D(graphicsDevice, 100, 100);
		maskOverlayOutputRT = new RenderTarget2D(graphicsDevice, 100, 100);
		tempRenderTargetSmall = new RenderTarget2D(graphicsDevice, 100, 100);
		lineRenderer = new LineRender(graphicsDevice, contentManager, new Rectangle(0, 0, 2000, 2000));
		tempRT = new RenderTarget2D(graphicsDevice, 100, 100);
		tempRT2 = new RenderTarget2D(graphicsDevice, 100, 100);
		tempRT3 = new RenderTarget2D(graphicsDevice, 1, 100);
		tempRT4 = new RenderTarget2D(graphicsDevice, 3000, 100);
	}

	public void purgeWarpBuffer()
	{
		graphicsDevice.SetRenderTarget(bulletRT);
		graphicsDevice.Clear(Color.Black);
		warpEventList.Clear();
	}

	public void Dispose()
	{
		blankRenderTarget.Dispose();
		blankRenderTarget = null;
		rawGridRT.Dispose();
		rawGridRT = null;
		tempRT.Dispose();
		tempRT = null;
		tempRT2.Dispose();
		tempRT2 = null;
		tempRT3.Dispose();
		tempRT3 = null;
		tempRT4.Dispose();
		tempRT4 = null;
		bulletTempRT.Dispose();
		bulletTempRT = null;
		tempRenderTarget.Dispose();
		tempRenderTarget = null;
		screenRT.Dispose();
		screenRT = null;
		bulletRT.Dispose();
		bulletRT = null;
		tempRenderTargetSmall.Dispose();
		tempRenderTargetSmall = null;
		maskRotRT.Dispose();
		maskRotRT = null;
		maskOverlayOutputRT.Dispose();
		maskOverlayOutputRT = null;
	}

	public void Update()
	{
		warpEventList.Clear();
	}

	public RenderTarget2D getTemplateTarget()
	{
		return bulletRT;
	}

	public void AddWarpEvent(Vector2 inPosition, float inScale, float inRotation, float inIntensity)
	{
		warpEventList.Add(new gridWarpEvent(inPosition, inScale, inRotation, inIntensity));
	}

	public void AddWarpEvent(Texture2D imageToUse, Vector2 inPosition, float inScale)
	{
		warpEventList.Add(new gridWarpEvent(imageToUse, inPosition, inScale));
	}

	public void AddWarpEvent(Texture2D imageToUse, Vector2 inPosition, float inScale, float inRotation)
	{
		warpEventList.Add(new gridWarpEvent(imageToUse, inPosition, inScale, inRotation));
	}

	public void AddWarpEvent(Texture2D imageToUse, Vector2 inPosition, float inScale, float inRotation, Vector2 inEndPosition)
	{
		warpEventList.Add(new gridWarpEvent(imageToUse, inPosition, inScale, inRotation, inEndPosition));
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition, Vector2 screenOrigin)
	{
		graphicsDevice.SetRenderTarget(tempRenderTarget);
		graphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		spriteBatch.Draw(bulletRT, Vector2.Zero, Color.White);
		spriteBatch.End();
		graphicsDevice.SetRenderTarget(bulletRT);
		graphicsDevice.Clear(Color.Black);
		diminishEffect.Parameters["diminishValue"].SetValue(0.03f);
		diminishEffect.Parameters["InputTexture"].SetValue(tempRenderTarget);
		diminishEffect.Parameters["visualCutoff"].SetValue(0.001f);
		diminishEffect.CurrentTechnique.Passes[0].Apply();
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, diminishEffect);
		spriteBatch.Draw(tempRenderTarget, Vector2.Zero, Color.White);
		spriteBatch.End();
		graphicsDevice.SetRenderTarget(blankRenderTarget);
		graphicsDevice.Clear(Color.Black);
		graphicsDevice.SetRenderTarget(rawGridRT);
		graphicsDevice.Clear(Color.Black);
		Vector2 vector = cameraPosition;
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicWrap, null, null);
		spriteBatch.Draw(gridTileImage, new Rectangle(0, 0, blankRenderTarget.Width, blankRenderTarget.Height), new Rectangle((int)(vector.X * 0.8f), (int)(vector.Y * 0.8f), blankRenderTarget.Width, blankRenderTarget.Height), Color.White);
		spriteBatch.End();
		_renderDiagFrameCounter++;
		if (!_renderDiagDumped && _renderDiagFrameCounter >= 90)
		{
			try
			{
				string diagDir = System.IO.Path.Combine(AppContext.BaseDirectory, "RenderDiag", "ForeverWars");
				System.IO.Directory.CreateDirectory(diagDir);
				SaveRenderDiag(rawGridRT, "1_rawGridRT_prewarp", diagDir);
			}
			catch
			{
			}
		}
		foreach (gridWarpEvent warpEvent in warpEventList)
		{
			if (warpEvent.isRadial)
			{
				if (warpEvent.imageToUse != null)
				{
					graphicsDevice.SetRenderTarget(bulletRT);
					spriteBatch.Begin();
					spriteBatch.Draw(bullet, warpEvent.position, null, Color.White, 0f, new Vector2(bullet.Width / 2, bullet.Height / 2), warpEvent.scale, SpriteEffects.None, 0f);
					spriteBatch.End();
					continue;
				}
				if (warpEvent.intensity != 1f)
				{
					graphicsDevice.SetRenderTarget(tempRenderTargetSmall);
					graphicsDevice.Clear(Color.Transparent);
					blueOverrideShader.Parameters["InputTexture"].SetValue(bullet);
					blueOverrideShader.Parameters["blueOverride"].SetValue(warpEvent.intensity);
					blueOverrideShader.CurrentTechnique.Passes[0].Apply();
					quad.Render(-Vector2.One, Vector2.One);
				}
				graphicsDevice.SetRenderTarget(bulletRT);
				spriteBatch.Begin();
				if (warpEvent.intensity != 1f)
				{
					spriteBatch.Draw(tempRenderTargetSmall, warpEvent.position, null, Color.White, warpEvent.rotation, new Vector2(bullet.Width / 2, bullet.Height / 2), warpEvent.scale, SpriteEffects.None, 0f);
				}
				else
				{
					spriteBatch.Draw(bullet, warpEvent.position, null, Color.White, warpEvent.rotation, new Vector2(bullet.Width / 2, bullet.Height / 2), warpEvent.scale, SpriteEffects.None, 0f);
				}
				spriteBatch.End();
			}
			else if (warpEvent.endPosition == Vector2.Zero)
			{
				graphicsDevice.SetRenderTarget(bulletRT);
				spriteBatch.Begin();
				spriteBatch.Draw(warpEvent.imageToUse, warpEvent.position, null, Color.White, 0f, new Vector2(warpEvent.imageToUse.Width / 2, warpEvent.imageToUse.Height / 2), warpEvent.scale, SpriteEffects.None, 0f);
				spriteBatch.End();
			}
			else
			{
				graphicsDevice.SetRenderTarget(bulletRT);
				spriteBatch.Begin();
				spriteBatch.Draw(warpEvent.imageToUse, warpEvent.position, null, Color.White, warpEvent.rotation, new Vector2(0f, warpEvent.imageToUse.Height / 2), 1f, SpriteEffects.None, 0f);
				spriteBatch.End();
				graphicsDevice.SetRenderTarget(bulletRT);
			}
		}
		warpEventList.Clear();
		graphicsDevice.SetRenderTarget(bulletTempRT);
		graphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		spriteBatch.Draw(bulletRT, screenOrigin, null, Color.White, 0f, cameraPosition, 1f, SpriteEffects.None, 0f);
		spriteBatch.End();
		if (!_renderDiagDumped && _renderDiagFrameCounter >= 90)
		{
			_renderDiagDumped = true;
			try
			{
				if (warpCaptureRT == null)
				{
					warpCaptureRT = new RenderTarget2D(graphicsDevice, 1280, 720);
				}
				graphicsDevice.SetRenderTarget(warpCaptureRT);
				graphicsDevice.Clear(Color.Black);
				_effect.Parameters["Texture2"]?.SetValue(bulletTempRT);
				_effect.Parameters["Texture"]?.SetValue(rawGridRT);
				_effect.CurrentTechnique.Passes[0].Apply();
				quad.Render(Vector2.One * -1f, Vector2.One);
				string diagDir = System.IO.Path.Combine(AppContext.BaseDirectory, "RenderDiag", "ForeverWars");
				System.IO.Directory.CreateDirectory(diagDir);
				SaveRenderDiag(warpCaptureRT, "2_postWarp_composite", diagDir);

				// CONTROL DIAGNOSTIC: same draw, but with NO custom effect at all
				// (default SpriteEffect) -- isolates whether the draw call itself
				// works independent of _effect/Warp.fx.
				RenderTarget2D controlCaptureRT = new RenderTarget2D(graphicsDevice, 1280, 720);
				graphicsDevice.SetRenderTarget(controlCaptureRT);
				graphicsDevice.Clear(Color.Black);
				spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null);
				spriteBatch.Draw(rawGridRT, Vector2.Zero, Color.White);
				spriteBatch.End();
				SaveRenderDiag(controlCaptureRT, "3_noEffectControl", diagDir);
			}
			catch (Exception diagEx)
			{
				try
				{
					System.IO.File.WriteAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "RenderDiag", "ForeverWars", "diag_error.txt"), diagEx.ToString());
				}
				catch
				{
				}
			}
		}
		graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		graphicsDevice.Clear(Color.Black);
		_effect.Parameters["Texture2"]?.SetValue(bulletTempRT);
		_effect.Parameters["Texture"]?.SetValue(rawGridRT);
		_effect.CurrentTechnique.Passes[0].Apply();
		quad.Render(Vector2.One * -1f, Vector2.One);
	}

	public void DEBUGdrawLine(Vector2 start, Vector2 end)
	{
		VertexPositionColor[] array = new VertexPositionColor[2];
		array[0].Position = new Vector3(start, 0f);
		array[0].Color = Color.Red;
		array[1].Position = new Vector3(end, 0f);
		array[1].Color = Color.Red;
		lineRenderer.DrawShape(array);
	}

	public void DEBUGdrawLine2(Vector2 start, float rotation)
	{
		VertexPositionColor[] array = new VertexPositionColor[2];
		array[0].Position = new Vector3(start, 0f);
		array[0].Color = Color.Red;
		array[1].Position = new Vector3(start + AngleToV2(rotation, 100f), 0f);
		array[1].Color = Color.Red;
		lineRenderer.DrawShape(array);
	}

	public Texture2D generateBeamImage(float inRotation)
	{
		SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
		graphicsDevice.SetRenderTarget(maskRotRT);
		graphicsDevice.Clear(Color.White);
		spriteBatch.Begin();
		spriteBatch.Draw(maskStripOverlayTexture, new Vector2(bullet.Width / 2, bullet.Height / 2), null, Color.White, inRotation, new Vector2(bullet.Width / 2, bullet.Height / 2), 1f, SpriteEffects.None, 0f);
		spriteBatch.End();
		graphicsDevice.SetRenderTarget(tempRT);
		graphicsDevice.Clear(Color.Transparent);
		graphicsDevice.BlendState = BlendState.NonPremultiplied;
		maskOverlayEffect.Parameters["Texture"].SetValue(bullet);
		maskOverlayEffect.Parameters["Texture2"].SetValue(maskRotRT);
		maskOverlayEffect.CurrentTechnique.Passes[0].Apply();
		quad.Render(-Vector2.One, Vector2.One);
		graphicsDevice.SetRenderTarget(tempRT2);
		graphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		spriteBatch.Draw(tempRT, new Vector2(bullet.Width / 2, bullet.Height / 2), null, Color.White, 0f - inRotation, new Vector2(bullet.Width / 2, bullet.Height / 2), 1f, SpriteEffects.None, 0f);
		spriteBatch.End();
		graphicsDevice.SetRenderTarget(tempRT3);
		graphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		spriteBatch.Draw(tempRT2, Vector2.Zero, new Rectangle(51, 0, 1, 100), Color.White);
		spriteBatch.End();
		graphicsDevice.SetRenderTarget(tempRT4);
		graphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicWrap, null, null);
		spriteBatch.Draw(tempRT3, new Rectangle(0, 0, tempRT4.Width, tempRT4.Height), new Rectangle(0, 0, tempRT4.Width, tempRT4.Height), Color.White);
		spriteBatch.End();
		return tempRT4;
	}

	public Texture2D generateDirectionalImage(float rotation)
	{
		SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
		RenderTarget2D renderTarget2D = new RenderTarget2D(graphicsDevice, 100, 100);
		graphicsDevice.SetRenderTarget(maskRotRT);
		graphicsDevice.Clear(Color.White);
		spriteBatch.Begin();
		spriteBatch.Draw(maskOverlayTexture, new Vector2(bullet.Width / 2, bullet.Height / 2), null, Color.White, rotation, new Vector2(bullet.Width / 2, bullet.Height / 2), 1f, SpriteEffects.None, 0f);
		spriteBatch.End();
		graphicsDevice.SetRenderTarget(renderTarget2D);
		graphicsDevice.Clear(Color.Transparent);
		graphicsDevice.BlendState = BlendState.NonPremultiplied;
		maskOverlayEffect.Parameters["Texture"].SetValue(bullet);
		maskOverlayEffect.Parameters["Texture2"].SetValue(maskRotRT);
		maskOverlayEffect.CurrentTechnique.Passes[0].Apply();
		quad.Render(-Vector2.One, Vector2.One);
		return renderTarget2D;
	}

	public float V2ToAngle(Vector2 vector)
	{
		return (float)Math.Atan2(vector.X, vector.Y);
	}

	public Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}
}
