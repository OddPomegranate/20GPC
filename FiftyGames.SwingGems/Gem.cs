using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.SwingGems;

internal class Gem
{
	private enum ClawStatusEnum
	{
		Home,
		OutBound,
		Inbound,
		Attached
	}

	private const float m_Scale = 0.4f;

	private const float gravity = 0.1f;

	private const float airResistanceY = 0.8f;

	private const float airResistanceX = 0.99f;

	private const float ropeMaxLength = 500f;

	private const float winchSpeed = 50f;

	private const float momentumCap = 10f;

	private const float thumbFlickdeadZone = 0.2f;

	private Player m_Player;

	private Vector2 m_Position;

	private Color m_Colour;

	private bool m_Alive;

	private float m_Rotation;

	private Texture2D m_GemSprite;

	private Texture2D m_ClawSprite;

	private Vector2 clawTrueOrigin;

	private float clawAngle;

	private float stickAngle;

	private float ropeLength;

	private float angularMomentum;

	private Vector2 m_GemOrigin;

	private Vector2 m_ClawOrigin;

	private BoundingSphere clawCollisionSphere;

	private BoundingSphere gemCollisionSphere;

	private Vector2 m_momentum;

	private Vector2 m_velocity;

	private Vector2 m_clawPosition;

	private bool aPressedLock = true;

	private bool thumbFlickLeft;

	private bool thumbFlickRight;

	private bool falling = true;

	private Texture2D DEBUGSprite;

	private bool firstTickAfterLock;

	private ClawStatusEnum clawStatus;

	private Vector2 tempVector2;

	private LineRender lineRenderer;

	private VertexPositionColor[] ropeArray = new VertexPositionColor[2];

	private PlayerManager pManager;

	private float persistantDebugVar;

	private Random randomGen;

	private Texture2D Broken1;

	private Texture2D Broken2;

	private Texture2D Broken3;

	private Texture2D Shard1;

	private Texture2D Shard2;

	private Texture2D Shard3;

	private List<GemShard> shardList = new List<GemShard>();

	public Gem(Player player, PlayerManager inPManager, Vector2 position, float scale, Texture2D gemSprite, Texture2D clawSprite, bool alive, GraphicsDevice graphicsDevice, ContentManager contentManager, Rectangle backBufferArea, Random inRand)
	{
		m_Player = player;
		pManager = inPManager;
		lineRenderer = new LineRender(graphicsDevice, contentManager, backBufferArea);
		randomGen = inRand;
		Broken1 = contentManager.Load<Texture2D>("SwingGems/Sprites/Broken/Broken1");
		Broken2 = contentManager.Load<Texture2D>("SwingGems/Sprites/Broken/Broken2");
		Broken3 = contentManager.Load<Texture2D>("SwingGems/Sprites/Broken/Broken3");
		Shard1 = contentManager.Load<Texture2D>("SwingGems/Sprites/Broken/Shard1");
		Shard2 = contentManager.Load<Texture2D>("SwingGems/Sprites/Broken/Shard2");
		Shard3 = contentManager.Load<Texture2D>("SwingGems/Sprites/Broken/Shard3");
		ropeArray[0].Position = new Vector3(200f, 200f, 0f);
		ropeArray[0].Color = Color.White;
		ropeArray[1].Position = new Vector3(200f, 300f, 0f);
		ropeArray[1].Color = Color.White;
		m_Position = position;
		m_Alive = true;
		m_GemSprite = gemSprite;
		m_GemOrigin = new Vector2((float)gemSprite.Width / 2f, (float)gemSprite.Height / 2f);
		m_ClawSprite = clawSprite;
		m_ClawOrigin = new Vector2((float)clawSprite.Width / 2f - m_GemOrigin.X, (float)clawSprite.Height / 2f);
		clawTrueOrigin = new Vector2(0f, (float)clawSprite.Height / 2f);
		m_Colour = pManager.GetPlayerColor(player);
	}

	public Player getPlayer()
	{
		return m_Player;
	}

	public void Update(Cave caveReference, float screenPositionIncrement)
	{
		m_Position.X -= screenPositionIncrement;
		m_clawPosition.X -= screenPositionIncrement;
		if ((m_Position.X < -200f || m_Position.Y < 0f || m_Position.Y > 720f) && m_Alive)
		{
			killPlayer(OffScreenDeath: true);
		}
		if (m_Alive)
		{
			if (clawStatus == ClawStatusEnum.Attached && falling && m_Position.Y > m_clawPosition.Y - 1f)
			{
				falling = false;
				ropeLength = Vector2.Distance(m_clawPosition, m_Position);
			}
			if (falling)
			{
				m_momentum.Y += 0.056f;
				m_momentum.X *= 0.99f;
				m_Position += m_momentum;
			}
			if (!falling)
			{
				if (firstTickAfterLock)
				{
					firstTickAfterLock = false;
					m_velocity = Vector2.Zero;
					m_momentum.Y *= -5f;
				}
				float num = V2ToAngle(Vector2.Normalize(m_Position - m_clawPosition)) - (float)Math.PI / 2f;
				persistantDebugVar = Math.Abs(num / ((float)Math.PI / 2f));
				m_velocity = Vector2.UnitY * (0.4f * Math.Abs(num / ((float)Math.PI / 2f)));
				m_momentum.Y *= Math.Abs(num / ((float)Math.PI / 2f));
				m_velocity += Vector2.UnitX * 1f * num;
				m_velocity += Vector2.UnitX * 0.2f * m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X;
				if (thumbFlickLeft && m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X > -0.2f)
				{
					thumbFlickLeft = false;
				}
				if (thumbFlickRight && m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X < 0.2f)
				{
					thumbFlickRight = false;
				}
				if (m_velocity.Y < 0f)
				{
					m_velocity.Y *= -1f;
				}
				if (m_momentum.Y < 0f)
				{
					m_momentum.Y *= -1f;
				}
				m_momentum += m_velocity;
				m_Position += m_momentum;
				num = V2ToAngle(Vector2.Normalize(m_Position - m_clawPosition));
				m_Position = m_clawPosition + AngleToV2(num, ropeLength);
			}
			stickAngle = 0f - V2ToAngle(m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left);
			if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Pressed && !aPressedLock)
			{
				aPressedLock = true;
				if (clawStatus == ClawStatusEnum.Home)
				{
					SwingGemsHelper.soundManager.CreateGameSoundCue("swingSideways Fire").Play();
					clawStatus = ClawStatusEnum.OutBound;
					clawAngle = stickAngle;
					m_clawPosition = m_Position;
				}
				else if (clawStatus == ClawStatusEnum.Attached)
				{
					clawStatus = ClawStatusEnum.Inbound;
					if (!falling)
					{
						m_momentum.Y *= -7f;
						falling = true;
					}
				}
			}
			if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Released && aPressedLock)
			{
				aPressedLock = false;
			}
			if (clawStatus == ClawStatusEnum.OutBound)
			{
				m_clawPosition += AngleToV2(clawAngle, 50f);
			}
			else if (clawStatus == ClawStatusEnum.Inbound)
			{
				clawAngle = (float)Math.PI / 2f + (0f - V2ToAngle(Vector2.Normalize(m_clawPosition - m_Position)));
				m_clawPosition += Vector2.Normalize(m_Position - m_clawPosition) * new Vector2(50f);
			}
			if (clawStatus == ClawStatusEnum.OutBound && (m_clawPosition - m_Position).Length() > 500f)
			{
				clawStatus = ClawStatusEnum.Inbound;
			}
			clawCollisionSphere = new BoundingSphere(new Vector3(m_clawPosition, 0f), 9.2f);
			gemCollisionSphere = new BoundingSphere(new Vector3(m_Position, 0f), 17.2f);
			if (clawStatus == ClawStatusEnum.OutBound)
			{
				for (int i = 0; i < caveReference.getRoof().Count; i++)
				{
					if (clawCollisionSphere.Intersects(caveReference.getRoof()[i].getBoundingBox()))
					{
						clawStatus = ClawStatusEnum.Attached;
						SwingGemsHelper.soundManager.CreateGameSoundCue("swingSideways Hook").Play();
						firstTickAfterLock = true;
						ropeLength = Vector2.Distance(m_clawPosition, m_Position);
					}
				}
				for (int j = 0; j < caveReference.getFloor().Count; j++)
				{
					if (clawCollisionSphere.Intersects(caveReference.getFloor()[j].getBoundingBox()))
					{
						clawStatus = ClawStatusEnum.Attached;
						firstTickAfterLock = true;
						ropeLength = Vector2.Distance(m_clawPosition, m_Position);
					}
				}
			}
			else if (clawStatus == ClawStatusEnum.Inbound && clawCollisionSphere.Intersects(gemCollisionSphere))
			{
				clawStatus = ClawStatusEnum.Home;
			}
			for (int k = 0; k < caveReference.getRoof().Count; k++)
			{
				if (gemCollisionSphere.Intersects(caveReference.getRoof()[k].getBoundingBox()))
				{
					if (m_Position.X < 0f)
					{
						killPlayer(OffScreenDeath: true);
					}
					else
					{
						killPlayer(OffScreenDeath: false);
					}
				}
			}
			for (int l = 0; l < caveReference.getFloor().Count; l++)
			{
				if (gemCollisionSphere.Intersects(caveReference.getFloor()[l].getBoundingBox()))
				{
					if (m_Position.X < 0f)
					{
						killPlayer(OffScreenDeath: true);
					}
					else
					{
						killPlayer(OffScreenDeath: false);
					}
				}
			}
			if (clawStatus == ClawStatusEnum.Home)
			{
				m_clawPosition = m_Position;
			}
		}
		if (m_Alive)
		{
			return;
		}
		foreach (GemShard shard in shardList)
		{
			shard.Update(screenPositionIncrement);
		}
	}

	public void killPlayer(bool OffScreenDeath)
	{
		if (m_Alive)
		{
			SwingGemsHelper.soundManager.CreateGameSoundCue("swingSideways Collide").Play();
			m_Alive = false;
			if (OffScreenDeath)
			{
				Vector2 gemMomentum = new Vector2(4f, 0f);
				shardList.Add(new GemShard(m_Colour, m_Position * Vector2.UnitY, gemMomentum, Shard1, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position * Vector2.UnitY, gemMomentum, Shard2, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position * Vector2.UnitY, gemMomentum, Shard3, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position * Vector2.UnitY, gemMomentum, Broken1, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position * Vector2.UnitY, gemMomentum, Broken2, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position * Vector2.UnitY, gemMomentum, Broken3, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_clawPosition * Vector2.UnitY, gemMomentum, m_ClawSprite, randomGen, 0.4f));
			}
			else
			{
				Vector2 gemMomentum = Vector2.Zero;
				shardList.Add(new GemShard(m_Colour, m_Position, gemMomentum, Shard1, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position, gemMomentum, Shard2, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position, gemMomentum, Shard3, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position, gemMomentum, Broken1, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position, gemMomentum, Broken2, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_Position, gemMomentum, Broken3, randomGen, 0.4f));
				shardList.Add(new GemShard(m_Colour, m_clawPosition, gemMomentum, m_ClawSprite, randomGen, 0.4f));
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (m_Alive)
		{
			switch (clawStatus)
			{
			case ClawStatusEnum.Home:
				spriteBatch.Draw(m_ClawSprite, m_Position, null, m_Colour, stickAngle, m_ClawOrigin, 0.4f, SpriteEffects.None, 0f);
				break;
			case ClawStatusEnum.OutBound:
				spriteBatch.Draw(m_ClawSprite, m_clawPosition, null, m_Colour, clawAngle, m_ClawOrigin, 0.4f, SpriteEffects.None, 0f);
				ropeArray[0].Position = new Vector3(m_Position.X, m_Position.Y, 0f);
				ropeArray[1].Position = new Vector3(m_clawPosition.X, m_clawPosition.Y, 0f);
				spriteBatch.End();
				spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				spriteBatch.Begin();
				lineRenderer.DrawShape(ropeArray);
				break;
			case ClawStatusEnum.Attached:
				spriteBatch.Draw(m_ClawSprite, m_clawPosition, null, m_Colour, clawAngle, clawTrueOrigin, 0.4f, SpriteEffects.None, 0f);
				ropeArray[0].Position = new Vector3(m_Position.X, m_Position.Y, 0f);
				ropeArray[1].Position = new Vector3(m_clawPosition.X, m_clawPosition.Y, 0f);
				spriteBatch.End();
				spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				spriteBatch.Begin();
				lineRenderer.DrawShape(ropeArray);
				break;
			case ClawStatusEnum.Inbound:
				spriteBatch.Draw(m_ClawSprite, m_clawPosition, null, m_Colour, clawAngle, m_ClawOrigin, 0.4f, SpriteEffects.None, 0f);
				ropeArray[0].Position = new Vector3(m_Position.X, m_Position.Y, 0f);
				ropeArray[1].Position = new Vector3(m_clawPosition.X, m_clawPosition.Y, 0f);
				spriteBatch.End();
				spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				spriteBatch.Begin();
				lineRenderer.DrawShape(ropeArray);
				break;
			}
			spriteBatch.Draw(m_GemSprite, m_Position, null, m_Colour, m_Rotation, m_GemOrigin, 0.4f, SpriteEffects.None, 0f);
			return;
		}
		foreach (GemShard shard in shardList)
		{
			shard.Draw(spriteBatch);
		}
	}

	public bool getAlive()
	{
		return m_Alive;
	}

	public float getXPosition()
	{
		return m_Position.X;
	}

	public float getXSpeed()
	{
		return m_velocity.X;
	}

	public float V2ToAngle(Vector2 vector)
	{
		return (float)Math.Atan2(vector.Y, vector.X);
	}

	public Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}
}
