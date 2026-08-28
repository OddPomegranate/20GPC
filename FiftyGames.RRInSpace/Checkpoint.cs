using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RRInSpace;

internal class Checkpoint
{
	private Vector2 position;

	private Texture2D spriteImage;

	private BoundingBox collisionBox;

	private int checkPointIndex;

	private Color alphaMaskColor = Color.White;

	private bool alphaFlashOn;

	private bool alphaUp;

	private bool isStartFlag;

	private float alphaCounter;

	public Checkpoint(GraphicsDevice graphicsDevice, ContentManager inContent, Vector2 inPosition, bool startFlag, int index)
	{
		position = inPosition;
		alphaMaskColor.A = 0;
		isStartFlag = startFlag;
		if (startFlag)
		{
			spriteImage = inContent.Load<Texture2D>("RRinSpace/Sprites/FinishLine");
		}
		else
		{
			spriteImage = inContent.Load<Texture2D>("RRinSpace/Sprites/Checkpoint");
		}
		collisionBox = new BoundingBox(new Vector3(position, 0f), new Vector3(position.X + (float)spriteImage.Width + 3f, position.Y + (float)spriteImage.Height, 0f));
		checkPointIndex = index;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (alphaFlashOn)
		{
			spriteBatch.Draw(spriteImage, position, Color.White);
			if (alphaUp)
			{
				alphaCounter += 0.05f;
				if (alphaCounter > 1f)
				{
					alphaUp = false;
				}
			}
			else
			{
				alphaCounter -= 0.05f;
				if (alphaCounter < 0f)
				{
					alphaFlashOn = false;
				}
			}
		}
		spriteBatch.Draw(spriteImage, position, Color.White);
	}

	public void pointFlash()
	{
		alphaFlashOn = true;
		alphaUp = true;
		alphaCounter = 0f;
		if (isStartFlag)
		{
			RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles Lap").Play();
		}
		else
		{
			RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles Checkpoint").Play();
		}
	}

	public int getCheckpointIndex()
	{
		return checkPointIndex;
	}

	public BoundingBox getCollisionBox()
	{
		return collisionBox;
	}
}
