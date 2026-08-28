using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.LightBikes;

internal class Grid
{
	private const int gridHeight = 130;

	private const int gridWidth = 240;

	private const int pixelGap = 5;

	private const int backgroundCounterLimit = 2;

	private Vector2 position = new Vector2(20f, 20f);

	private Texture2D pixelTexture;

	private Random randomGenerator;

	private Effect backgroundShader;

	private int backgroundCounter;

	private bool firstDraw = true;

	private fullScreenQuad quad;

	public GridBlock[,] gridArray = new GridBlock[241, 131];

	private RenderTarget2D backgroundRT;

	private float delta;

	public int getGridHeight()
	{
		return 130;
	}

	public int getGridWidth()
	{
		return 240;
	}

	public void purge()
	{
		backgroundRT.Dispose();
		backgroundRT = null;
	}

	public Grid(GraphicsDevice graphicsDevice, Texture2D pixel, Effect effect)
	{
		randomGenerator = new Random();
		pixelTexture = pixel;
		backgroundRT = new RenderTarget2D(graphicsDevice, 1280, 720);
		quad = new fullScreenQuad(graphicsDevice);
		backgroundShader = effect;
		for (int i = 0; i < 241; i++)
		{
			for (int j = 0; j < 131; j++)
			{
				gridArray[i, j] = new GridBlock(pixel);
			}
		}
	}

	public void setGridElement(int inX, int inY, Color inColor)
	{
		if (inX >= 0 && inY >= 0 && inX <= gridArray.GetLength(0) && inY <= gridArray.GetLength(1))
		{
			gridArray[inX, inY].setElement(inColor);
		}
	}

	public Vector2 getScreenPosition()
	{
		return position;
	}

	public int getPixelGap()
	{
		return 5;
	}

	public void clearGrid()
	{
		for (int i = 0; i < 241; i++)
		{
			for (int j = 0; j < 131; j++)
			{
				gridArray[i, j] = new GridBlock(pixelTexture);
			}
		}
	}

	public void Update()
	{
		for (int i = 1; i < 240; i++)
		{
			for (int j = 1; j < 130; j++)
			{
				gridArray[i, j].Update();
			}
		}
		delta += 0.003f;
		if (delta > 6f)
		{
			delta = 0f;
		}
	}

	public void DrawBackground(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D finalOutRT)
	{
		if (firstDraw)
		{
			firstDraw = false;
			graphicsDevice.SetRenderTarget(backgroundRT);
			graphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			for (int i = 1; i < 240; i++)
			{
				for (int j = 1; j < 130; j++)
				{
					gridArray[i, j].DrawBackingOnly(spriteBatch, position, 5, i, j, clearMode: false);
				}
			}
			spriteBatch.End();
			graphicsDevice.SetRenderTarget(finalOutRT);
		}
		backgroundShader.Parameters["delta"].SetValue(delta);
		backgroundShader.Parameters["colorDelta"].SetValue(delta + 0.1f);
		backgroundShader.Parameters["positionDelta"].SetValue(delta);
		backgroundShader.Parameters["InputTexture"].SetValue(backgroundRT);
		graphicsDevice.BlendState = BlendState.NonPremultiplied;
		backgroundShader.CurrentTechnique.Passes[0].Apply();
		quad.Render(Vector2.One * -1f, Vector2.One);
	}

	public bool getWall(int gridXPosition, int gridYPosition)
	{
		if (gridXPosition < 0 || gridYPosition < 0 || gridXPosition > gridArray.GetLength(0) - 1 || gridYPosition > gridArray.GetLength(1) - 1)
		{
			return true;
		}
		return gridArray[gridXPosition, gridYPosition].getSet();
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		for (int i = 1; i < 240; i++)
		{
			for (int j = 1; j < 130; j++)
			{
				gridArray[i, j].Draw(spriteBatch, position, 5, i, j, clearMode: false);
			}
		}
	}
}
