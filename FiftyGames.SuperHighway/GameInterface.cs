using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SuperHighway;

internal class GameInterface
{
	protected const float CharacterSpacing = 5f;

	protected const float CharacterWidth = 10f;

	protected const float CharacterHeight = 20f;

	protected List<Car> _players = new List<Car>();

	protected Vector3[] _characterVerts;

	protected List<short[]> _characterIndex;

	protected List<short[]> _retryIndex = new List<short[]>();

	protected short[] _buttonIndex;

	protected Vector3[] _lightVerts;

	protected short[] _lightIndex;

	public GameInterface()
	{
		_characterVerts = new Vector3[12];
		ref Vector3 reference = ref _characterVerts[0];
		reference = new Vector3(0f, 0f, 0f);
		ref Vector3 reference2 = ref _characterVerts[1];
		reference2 = new Vector3(5f, 0f, 0f);
		ref Vector3 reference3 = ref _characterVerts[2];
		reference3 = new Vector3(10f, 0f, 0f);
		ref Vector3 reference4 = ref _characterVerts[3];
		reference4 = new Vector3(0f, 10f, 0f);
		ref Vector3 reference5 = ref _characterVerts[4];
		reference5 = new Vector3(5f, 10f, 0f);
		ref Vector3 reference6 = ref _characterVerts[5];
		reference6 = new Vector3(10f, 10f, 0f);
		ref Vector3 reference7 = ref _characterVerts[6];
		reference7 = new Vector3(0f, 20f, 0f);
		ref Vector3 reference8 = ref _characterVerts[7];
		reference8 = new Vector3(5f, 20f, 0f);
		ref Vector3 reference9 = ref _characterVerts[8];
		reference9 = new Vector3(10f, 20f, 0f);
		_characterIndex = new List<short[]>();
		_characterIndex.Add(new short[10] { 0, 2, 0, 6, 2, 8, 6, 8, 2, 6 });
		_characterIndex.Add(new short[6] { 0, 1, 1, 7, 6, 8 });
		_characterIndex.Add(new short[10] { 0, 2, 2, 5, 5, 3, 3, 6, 6, 8 });
		_characterIndex.Add(new short[8] { 2, 8, 2, 0, 5, 3, 8, 6 });
		_characterIndex.Add(new short[6] { 0, 3, 3, 5, 2, 8 });
		_characterIndex.Add(new short[10] { 2, 0, 0, 3, 3, 5, 5, 8, 8, 6 });
		_characterIndex.Add(new short[10] { 2, 0, 0, 6, 6, 8, 8, 5, 5, 3 });
		_characterIndex.Add(new short[4] { 0, 2, 2, 8 });
		_characterIndex.Add(new short[10] { 0, 2, 0, 6, 2, 8, 6, 8, 3, 5 });
		_characterIndex.Add(new short[8] { 8, 2, 2, 0, 0, 3, 3, 5 });
		_lightVerts = new Vector3[16];
		_lightIndex = new short[_lightVerts.Length * 2];
		ref Vector3 reference10 = ref _lightVerts[0];
		reference10 = new Vector3(0f, 25f, 0f);
		_lightIndex[0] = 0;
		_lightIndex[1] = 1;
		for (int i = 1; i < _lightVerts.Length; i++)
		{
			ref Vector3 reference11 = ref _lightVerts[i];
			reference11 = Vector3.Transform(_lightVerts[0], Matrix.CreateRotationZ((float)Math.PI * 2f / (float)_lightVerts.Length * (float)i));
			_lightIndex[i * 2] = (short)i;
			if (i != _lightVerts.Length - 1)
			{
				_lightIndex[i * 2 + 1] = (short)(i + 1);
			}
			else
			{
				_lightIndex[i * 2 + 1] = 0;
			}
		}
		_buttonIndex = new short[8] { 6, 0, 0, 2, 2, 8, 3, 5 };
		_retryIndex.Add(new short[8] { 6, 0, 0, 2, 2, 5, 5, 3 });
		_retryIndex.Add(new short[4] { 0, 6, 6, 8 });
		_retryIndex.Add(new short[8] { 6, 0, 8, 2, 0, 2, 3, 5 });
		_retryIndex.Add(new short[8] { 0, 3, 2, 5, 3, 5, 4, 7 });
		_retryIndex.Add(new short[0]);
		_retryIndex.Add(new short[8] { 6, 0, 8, 2, 0, 2, 3, 5 });
		_retryIndex.Add(new short[10] { 2, 0, 0, 6, 6, 8, 8, 5, 5, 4 });
		_retryIndex.Add(new short[8] { 6, 0, 8, 2, 0, 2, 3, 5 });
		_retryIndex.Add(new short[6] { 0, 2, 1, 7, 6, 8 });
		_retryIndex.Add(new short[6] { 6, 0, 0, 8, 2, 8 });
	}

	public void Update(GameTime gameTime)
	{
	}

	public void AddPlayer(Car pod)
	{
		_players.Add(pod);
	}

	public void RemovePlayer(Car pod)
	{
		_players.Remove(pod);
	}

	public void RemoveAllPlayers()
	{
		_players.Clear();
	}

	public void DrawString(LineRender graphics, string text, Vector2 position, float scale, Color colour)
	{
		for (int i = 0; i < text.Length; i++)
		{
			DrawCharacter(graphics, text[i], position, scale, colour);
			position.X += 15f * scale;
		}
	}

	public void DrawCharacter(LineRender graphics, char character, Vector2 position, float scale, Color colour)
	{
		VertexPositionColor[] array = new VertexPositionColor[_characterVerts.Length];
		Matrix matrix = Matrix.Multiply(Matrix.CreateScale(scale), Matrix.CreateTranslation(new Vector3(position, 0f)));
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Position = Vector3.Transform(_characterVerts[i], matrix);
			array[i].Color = colour;
		}
		short[] indices = _characterIndex[character - 48];
		graphics.DrawIndexedShape(array, indices);
	}

	public void DrawLight(LineRender graphics, Vector3 position, Color colour)
	{
		VertexPositionColor[] array = new VertexPositionColor[_lightVerts.Length];
		Matrix matrix = Matrix.CreateTranslation(position);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Position = Vector3.Transform(_lightVerts[i], matrix);
			array[i].Color = colour;
		}
		graphics.DrawIndexedShape(array, _lightIndex);
	}

	public void DrawRetry(LineRender graphics, Vector3 position)
	{
		VertexPositionColor[] array = new VertexPositionColor[_lightVerts.Length];
		Matrix matrix = Matrix.Multiply(Matrix.CreateTranslation(position + new Vector3(-20f, 65f, 0f)), Matrix.CreateScale(0.8f));
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Position = Vector3.Transform(_lightVerts[i], matrix);
			array[i].Color = Color.Green;
		}
		graphics.DrawIndexedShape(array, _lightIndex);
		array = new VertexPositionColor[_characterVerts.Length];
		matrix = Matrix.CreateTranslation(position + new Vector3(-85f, -10f, 0f));
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Position = Vector3.Transform(_characterVerts[j], matrix);
			array[j].Color = Color.Green;
		}
		graphics.DrawIndexedShape(array, _buttonIndex);
		for (int k = 0; k < _retryIndex.Count; k++)
		{
			array = new VertexPositionColor[_characterVerts.Length];
			matrix = Matrix.CreateTranslation(position + new Vector3(-50f + 15f * (float)k, -10f, 0f));
			for (int l = 0; l < array.Length; l++)
			{
				array[l].Position = Vector3.Transform(_characterVerts[l], matrix);
				array[l].Color = Color.White;
			}
			if (_retryIndex[k].Length != 0)
			{
				graphics.DrawIndexedShape(array, _retryIndex[k]);
			}
		}
	}
}
