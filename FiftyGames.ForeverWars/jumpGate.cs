using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class jumpGate
{
	private GraphicsDevice graphicsDevice;

	private ContentManager contentManager;

	private Vector2 position;

	private float rotation;

	private Vector2 origin;

	private BoundingBox collisionBox;

	private Effect gateEffect;

	private RenderTarget2D normalRenderTarget;

	private RenderTarget2D partialRenderTarget1;

	private RenderTarget2D partialRenderTarget2;

	private float delta;

	private Vector2 dimensions;

	private fullScreenQuad quad;

	private Texture2D dummyTexture;

	private RenderTarget2D largeBackingRenderTarget;

	private List<Vector3> backingPostDraw = new List<Vector3>();

	public jumpGate(GraphicsDevice inGraphicsDevice, ContentManager inContentManager, RenderTarget2D inLargeBackingRenderTarget)
	{
		graphicsDevice = inGraphicsDevice;
		contentManager = inContentManager;
		largeBackingRenderTarget = inLargeBackingRenderTarget;
		dimensions = new Vector2(300f, 100f);
		gateEffect = contentManager.Load<Effect>("ForeverWars/Effects/gateShader");
		origin = new Vector2(dimensions.X / 2f, dimensions.Y / 2f);
		dummyTexture = new Texture2D(graphicsDevice, (int)dimensions.X, (int)dimensions.Y);
		quad = new fullScreenQuad(graphicsDevice);
		normalRenderTarget = new RenderTarget2D(graphicsDevice, (int)dimensions.X, (int)dimensions.Y);
	}

	public bool Update()
	{
		backingPostDraw.Clear();
		delta += 0.01f;
		if (delta >= (float)Math.PI / 4f)
		{
			delta = 0f;
		}
		return false;
	}

	public void prepShaderDraw(SpriteBatch spriteBatch)
	{
		graphicsDevice.SetRenderTarget(normalRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		graphicsDevice.BlendState = BlendState.NonPremultiplied;
		gateEffect.Parameters["delta"].SetValue(delta);
		gateEffect.Parameters["screenDimensions"].SetValue(new Vector2(dimensions.X, dimensions.Y));
		gateEffect.Parameters["InputTexture"].SetValue(dummyTexture);
		gateEffect.CurrentTechnique.Passes[0].Apply();
		quad.Render(Vector2.One * -1f, Vector2.One);
		graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		graphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		spriteBatch.Draw(normalRenderTarget, Vector2.Zero, Color.White);
		spriteBatch.End();
		graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
	}

	public void DrawGate(SpriteBatch spriteBatch, Vector2 inPosition, float inRotation)
	{
		position = inPosition;
		rotation = inRotation;
		spriteBatch.Draw(normalRenderTarget, position, null, Color.White, rotation, dimensions / 2f, 1f, SpriteEffects.None, 0f);
		backingPostDraw.Add(new Vector3(position, rotation));
	}

	public void postDrawBackingCall(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		spriteBatch.GraphicsDevice.SetRenderTarget(largeBackingRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		foreach (Vector3 item in backingPostDraw)
		{
			spriteBatch.Draw(normalRenderTarget, new Vector2(item.X, item.Y) + Vector2.One * 100f, null, Color.White, item.Z, dimensions / 2f, 1f, SpriteEffects.None, 0f);
		}
		spriteBatch.End();
		spriteBatch.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		spriteBatch.Begin();
	}

	public Vector2 getPosition()
	{
		return position;
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
