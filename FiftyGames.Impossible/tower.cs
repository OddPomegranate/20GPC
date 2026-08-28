using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Impossible;

internal class tower
{
	private const int buildMaxHeight = 6;

	private const int buildMaxStep = 2;

	private const int buildHeightChangeChance = 4;

	private const int minTowerInsideHeight = 35;

	private const int maxTowerInsideHeight = 55;

	private const int towerHeightJumpRange = 3;

	private towerType towerID;

	private Vector2 position;

	private Vector2 origin;

	private float floorHeight = 5f;

	private float gapHeight;

	private float roofHeight;

	private float topHeight;

	private int towerWidth;

	private Texture2D bottomImage;

	private Texture2D bricksImage;

	private Texture2D backingImage;

	private Texture2D roofImage;

	private Texture2D testImage;

	private BoundingBox collisionBox = default(BoundingBox);

	private BoundingBox sideCollisionBox = default(BoundingBox);

	private BoundingBox roofCollisionBox = default(BoundingBox);

	private BoundingBox roofSideCollisionBox = default(BoundingBox);

	public tower(GraphicsDevice graphicsDevice, ContentManager inContent, towerImages inImagesStruct, towerType inType, Vector4 lastTowerDimensions, Random randomRef)
	{
		testImage = inImagesStruct.testImage;
		towerID = inType;
		switch (towerID)
		{
		case towerType.Tiny:
			towerWidth = 19;
			position = new Vector2(404f, 0f);
			if (randomRef.Next(10) < 4)
			{
				floorHeight = lastTowerDimensions.X + (float)randomRef.Next(-3, 3);
				floorHeight = Math.Max(floorHeight, 1f);
				floorHeight = Math.Min(floorHeight, 6f);
			}
			else
			{
				floorHeight = lastTowerDimensions.X;
			}
			gapHeight = lastTowerDimensions.Y;
			roofHeight = lastTowerDimensions.Z;
			topHeight = lastTowerDimensions.W;
			bottomImage = inImagesStruct.tinyBottom;
			bricksImage = inImagesStruct.tinyBricks;
			backingImage = inImagesStruct.medBacking;
			roofImage = null;
			break;
		case towerType.Medium:
			towerWidth = 33;
			position = new Vector2(404f, 0f);
			if (randomRef.Next(10) < 4)
			{
				floorHeight = lastTowerDimensions.X + (float)randomRef.Next(-3, 3);
				floorHeight = Math.Max(floorHeight, 1f);
				floorHeight = Math.Min(floorHeight, 6f);
			}
			else
			{
				floorHeight = lastTowerDimensions.X;
			}
			gapHeight = lastTowerDimensions.X + (float)randomRef.Next(-3, 3);
			gapHeight = Math.Max(gapHeight, 35f);
			gapHeight = Math.Min(gapHeight, 55f);
			roofHeight = 0f;
			topHeight = randomRef.Next(3);
			bottomImage = inImagesStruct.medBottom;
			bricksImage = inImagesStruct.medBricks;
			backingImage = inImagesStruct.medBacking;
			roofImage = inImagesStruct.medRoof;
			break;
		case towerType.Big:
			towerWidth = 41;
			position = new Vector2(404f, 0f);
			if (randomRef.Next(10) < 4)
			{
				floorHeight = lastTowerDimensions.X + (float)randomRef.Next(-3, 3);
				floorHeight = Math.Max(floorHeight, 1f);
				floorHeight = Math.Min(floorHeight, 6f);
			}
			else
			{
				floorHeight = lastTowerDimensions.X;
			}
			gapHeight = lastTowerDimensions.X + (float)randomRef.Next(-3, 3);
			gapHeight = Math.Max(gapHeight, 35f);
			gapHeight = Math.Min(gapHeight, 55f);
			roofHeight = 0f;
			topHeight = randomRef.Next(3);
			bottomImage = inImagesStruct.bigBottom;
			bricksImage = inImagesStruct.bigBricks;
			backingImage = inImagesStruct.bigBacking;
			roofImage = inImagesStruct.bigRoof;
			break;
		case towerType.Huge:
			towerWidth = 56;
			position = new Vector2(404f, 0f);
			if (randomRef.Next(10) < 4)
			{
				floorHeight = lastTowerDimensions.X + (float)randomRef.Next(-3, 3);
				floorHeight = Math.Max(floorHeight, 1f);
				floorHeight = Math.Min(floorHeight, 6f);
			}
			else
			{
				floorHeight = lastTowerDimensions.X;
			}
			gapHeight = lastTowerDimensions.X + (float)randomRef.Next(-3, 3);
			gapHeight = Math.Max(gapHeight, 35f);
			gapHeight = Math.Min(gapHeight, 55f);
			roofHeight = 0f;
			topHeight = randomRef.Next(3);
			bottomImage = inImagesStruct.hugeBottom;
			bricksImage = inImagesStruct.hugeBricks;
			backingImage = inImagesStruct.hugeBacking;
			roofImage = inImagesStruct.hugeRoof;
			break;
		case towerType.StartingBlockA:
		case towerType.StartingBlockB:
			position = new Vector2(0f, 0f);
			towerWidth = 56;
			bottomImage = inImagesStruct.hugeBottom;
			bricksImage = inImagesStruct.hugeBricks;
			backingImage = inImagesStruct.hugeBacking;
			roofImage = inImagesStruct.hugeRoof;
			break;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		Vector2 zero = Vector2.Zero;
		_ = Vector2.Zero;
		switch (towerID)
		{
		case towerType.Tiny:
		{
			Vector2 vector;
			for (int num5 = (int)floorHeight; num5 > 0; num5--)
			{
				zero.Y = 132 - num5 * 6;
				vector = zero + position;
				vector.X = RobsMath.TruncF(vector.X);
				vector.Y = RobsMath.TruncF(vector.Y);
				spriteBatch.Draw(bricksImage, vector, Color.White);
			}
			zero.Y = 132f;
			vector = zero + position;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(bottomImage, vector, Color.White);
			break;
		}
		case towerType.Medium:
		{
			spriteBatch.Draw(backingImage, new Rectangle((int)position.X, (int)(132f - floorHeight * 6f - gapHeight), backingImage.Width, (int)gapHeight), Color.White);
			Vector2 vector;
			for (int num6 = (int)topHeight; num6 > 0; num6--)
			{
				zero.Y = 132f - floorHeight * 6f - gapHeight - (float)(num6 * 6);
				vector = zero + position;
				vector.X = RobsMath.TruncF(vector.X);
				vector.Y = RobsMath.TruncF(vector.Y);
				spriteBatch.Draw(bricksImage, vector, Color.White);
			}
			zero.Y = 132f - floorHeight * 6f - gapHeight - topHeight * 6f - (float)roofImage.Height;
			vector = position + zero - Vector2.UnitY - Vector2.UnitX;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(roofImage, vector, Color.White);
			zero.Y = 132f - floorHeight * 6f - gapHeight - topHeight * 6f - 2f;
			vector = position + zero + Vector2.UnitY;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(backingImage, position + zero + Vector2.UnitY, Color.White);
			zero.Y = 132f;
			vector = zero + position;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(bottomImage, vector, Color.White);
			for (int num7 = (int)floorHeight; num7 > 0; num7--)
			{
				zero.Y = 132 - num7 * 6;
				vector = zero + position;
				vector.X = RobsMath.TruncF(vector.X);
				vector.Y = RobsMath.TruncF(vector.Y);
				spriteBatch.Draw(bricksImage, vector, Color.White);
			}
			break;
		}
		case towerType.Big:
		{
			spriteBatch.Draw(backingImage, new Rectangle((int)position.X, (int)(132f - floorHeight * 6f - gapHeight), backingImage.Width, (int)gapHeight + 1), Color.White);
			Vector2 vector;
			for (int num3 = (int)topHeight; num3 > 0; num3--)
			{
				zero.Y = 132f - floorHeight * 6f - gapHeight - (float)(num3 * 6);
				vector = zero + position;
				vector.X = RobsMath.TruncF(vector.X);
				vector.Y = RobsMath.TruncF(vector.Y);
				spriteBatch.Draw(bricksImage, vector, Color.White);
			}
			zero.Y = 132f - floorHeight * 6f - gapHeight - topHeight * 6f - (float)roofImage.Height - 1f;
			vector = zero + position;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(roofImage, vector, Color.White);
			zero.Y = 132f - floorHeight * 6f - gapHeight - topHeight * 6f - (float)roofImage.Height - 1f + (float)roofImage.Height;
			vector = zero + position;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(backingImage, vector, Color.White);
			zero.Y = 132f;
			vector = zero + position;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(bottomImage, vector, Color.White);
			for (int num4 = (int)floorHeight; num4 > 0; num4--)
			{
				zero.Y = 132 - num4 * 6;
				vector = zero + position;
				vector.X = RobsMath.TruncF(vector.X);
				vector.Y = RobsMath.TruncF(vector.Y);
				spriteBatch.Draw(bricksImage, vector, Color.White);
			}
			break;
		}
		case towerType.Huge:
		{
			spriteBatch.Draw(backingImage, new Rectangle((int)position.X, (int)(132f - floorHeight * 6f - gapHeight) - 1, backingImage.Width, (int)gapHeight + 1), Color.White);
			Vector2 vector;
			for (int num = (int)topHeight; num > 0; num--)
			{
				zero.Y = 132f - floorHeight * 6f - gapHeight - (float)(num * 6);
				vector = zero + position;
				vector.X = RobsMath.TruncF(vector.X);
				vector.Y = RobsMath.TruncF(vector.Y);
				spriteBatch.Draw(bricksImage, vector, Color.White);
			}
			zero.Y = 132f - floorHeight * 6f - gapHeight - topHeight * 6f - (float)roofImage.Height - 1f;
			vector = zero + position;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(roofImage, vector, Color.White);
			zero.Y = 132f - floorHeight * 6f - gapHeight - topHeight * 6f - 2f;
			vector = zero + position;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(backingImage, vector + Vector2.UnitY, Color.White);
			zero.Y = 132f;
			vector = zero + position;
			vector.X = RobsMath.TruncF(vector.X);
			vector.Y = RobsMath.TruncF(vector.Y);
			spriteBatch.Draw(bottomImage, vector, Color.White);
			for (int num2 = (int)floorHeight; num2 > 0; num2--)
			{
				zero.Y = 132 - num2 * 6;
				vector = zero + position;
				vector.X = RobsMath.TruncF(vector.X);
				vector.Y = RobsMath.TruncF(vector.Y);
				spriteBatch.Draw(bricksImage, vector, Color.White);
			}
			break;
		}
		case towerType.StartingBlockB:
		{
			spriteBatch.Draw(roofImage, position, Color.White);
			zero.Y = 34f;
			zero.Y = 34f;
			spriteBatch.Draw(backingImage, new Rectangle((int)position.X, (int)position.Y + 45, backingImage.Width, 80), Color.White);
			spriteBatch.Draw(backingImage, position + zero, Color.White);
			for (int k = 0; k < 4; k++)
			{
				zero.Y = k * 6 + 35;
				spriteBatch.Draw(bricksImage, position + zero, Color.White);
			}
			floorHeight = 6f;
			for (int l = 0; l < 6; l++)
			{
				zero.Y = l * 6 + 96;
				spriteBatch.Draw(bricksImage, position + zero, Color.White);
			}
			zero.Y = 132f;
			spriteBatch.Draw(bottomImage, position + zero, Color.White);
			break;
		}
		case towerType.StartingBlockA:
		{
			zero.Y = 34f;
			spriteBatch.Draw(backingImage, position + zero, Color.White);
			for (int i = 0; i < 4; i++)
			{
				zero.Y = i * 6 + 35;
				spriteBatch.Draw(bricksImage, position + zero, Color.White);
			}
			floorHeight = 6f;
			for (int j = 0; j < 6; j++)
			{
				zero.Y = j * 6 + 96;
				spriteBatch.Draw(bricksImage, position + zero, Color.White);
			}
			zero.Y = 132f;
			spriteBatch.Draw(bottomImage, position + zero, Color.White);
			break;
		}
		}
	}

	public float getBuildingHeight()
	{
		return floorHeight;
	}

	public float calculateRoofHeight()
	{
		return 132f - floorHeight * 6f - gapHeight;
	}

	public Vector2 getPosition()
	{
		return position;
	}

	public int getWidth()
	{
		return towerWidth;
	}

	public void incrementXPosition(float incrementValue)
	{
		position.X += incrementValue;
	}

	public void decrementXPosition(float decrementValue)
	{
		position.X -= decrementValue;
	}

	public Vector4 getDimensionVector()
	{
		return new Vector4(floorHeight, gapHeight, roofHeight, topHeight);
	}

	public void setCollisionBox(BoundingBox inBox)
	{
		collisionBox = inBox;
	}

	public BoundingBox getCollisionBox()
	{
		return collisionBox;
	}

	public void setSideCollisionBox(BoundingBox inBox)
	{
		if (towerID == towerType.StartingBlockA || towerID == towerType.StartingBlockB)
		{
			sideCollisionBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
		}
		else
		{
			sideCollisionBox = inBox;
		}
	}

	public BoundingBox getSideCollisionBox()
	{
		return sideCollisionBox;
	}

	public void setRoofCollisionBox(BoundingBox inBox)
	{
		if (towerID == towerType.StartingBlockA || towerID == towerType.StartingBlockB)
		{
			roofCollisionBox = new BoundingBox(new Vector3(position, 0f), new Vector3(position, 0f) + Vector3.UnitX * backingImage.Width);
			roofCollisionBox.Min.Y = 58f;
			roofCollisionBox.Max.Y = 59f;
		}
		else if (towerID == towerType.Tiny)
		{
			roofCollisionBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
		}
		else
		{
			roofCollisionBox = inBox;
		}
	}

	public BoundingBox getRoofCollisionBox()
	{
		return roofCollisionBox;
	}

	public void setRoofSideCollisionBox()
	{
		if (towerID == towerType.StartingBlockA || towerID == towerType.StartingBlockB || towerID == towerType.Tiny)
		{
			roofSideCollisionBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
			return;
		}
		roofSideCollisionBox = new BoundingBox(new Vector3(position, 0f), new Vector3(position, 0f) + Vector3.UnitX);
		roofSideCollisionBox.Min.Y = 0f;
		roofSideCollisionBox.Max.Y = 132f - floorHeight * 6f - gapHeight;
	}

	public BoundingBox getRoofSideCollisionBox()
	{
		return roofSideCollisionBox;
	}
}
