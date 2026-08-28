using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.LunarLander;

internal class GameInterface
{
	protected const float CharacterSpacing = 5f;

	protected const float CharacterWidth = 10f;

	protected const float CharacterHeight = 20f;

	protected List<Pod> _players = new List<Pod>();

	protected VertexPositionColor[] _characterVerts;

	protected List<short[]> _characterIndex = new List<short[]>();

	protected List<short[]> _retryIndex = new List<short[]>();

	protected short[] _buttonIndex;

	protected VertexPositionColor[] _lifeVerts;

	protected short[] _lifeIndex;

	protected VertexPositionColor[] _fuelContainerVerts;

	protected short[] _fuelContainerIndex;

	protected VertexPositionColor[] _fuelBarVerts;

	protected short[] _fuelBarIndex;

	protected VertexPositionColor[] _flagVerts;

	protected short[] _flagIndex;

	protected VertexPositionColor[] _circleVerts;

	protected short[] _circleIndex;

	public GameInterface()
	{
		_characterVerts = new VertexPositionColor[12];
		ref VertexPositionColor reference = ref _characterVerts[0];
		reference = new VertexPositionColor(new Vector3(0f, 0f, 0f), Color.White);
		ref VertexPositionColor reference2 = ref _characterVerts[1];
		reference2 = new VertexPositionColor(new Vector3(5f, 0f, 0f), Color.White);
		ref VertexPositionColor reference3 = ref _characterVerts[2];
		reference3 = new VertexPositionColor(new Vector3(10f, 0f, 0f), Color.White);
		ref VertexPositionColor reference4 = ref _characterVerts[3];
		reference4 = new VertexPositionColor(new Vector3(0f, 10f, 0f), Color.White);
		ref VertexPositionColor reference5 = ref _characterVerts[4];
		reference5 = new VertexPositionColor(new Vector3(5f, 10f, 0f), Color.White);
		ref VertexPositionColor reference6 = ref _characterVerts[5];
		reference6 = new VertexPositionColor(new Vector3(10f, 10f, 0f), Color.White);
		ref VertexPositionColor reference7 = ref _characterVerts[6];
		reference7 = new VertexPositionColor(new Vector3(0f, 20f, 0f), Color.White);
		ref VertexPositionColor reference8 = ref _characterVerts[7];
		reference8 = new VertexPositionColor(new Vector3(5f, 20f, 0f), Color.White);
		ref VertexPositionColor reference9 = ref _characterVerts[8];
		reference9 = new VertexPositionColor(new Vector3(10f, 20f, 0f), Color.White);
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
		_lifeVerts = new VertexPositionColor[4];
		ref VertexPositionColor reference10 = ref _lifeVerts[0];
		reference10 = new VertexPositionColor(new Vector3(5f, 0f, 0f), Color.White);
		ref VertexPositionColor reference11 = ref _lifeVerts[1];
		reference11 = new VertexPositionColor(new Vector3(0f, 5f, 0f), Color.White);
		ref VertexPositionColor reference12 = ref _lifeVerts[2];
		reference12 = new VertexPositionColor(new Vector3(10f, 5f, 0f), Color.White);
		ref VertexPositionColor reference13 = ref _lifeVerts[3];
		reference13 = new VertexPositionColor(new Vector3(5f, 10f, 0f), Color.White);
		_lifeIndex = new short[8] { 0, 2, 2, 3, 3, 1, 1, 0 };
		_fuelContainerVerts = new VertexPositionColor[4];
		ref VertexPositionColor reference14 = ref _fuelContainerVerts[0];
		reference14 = new VertexPositionColor(new Vector3(0f, 0f, 0f), Color.White);
		ref VertexPositionColor reference15 = ref _fuelContainerVerts[1];
		reference15 = new VertexPositionColor(new Vector3(40f, 0f, 0f), Color.White);
		ref VertexPositionColor reference16 = ref _fuelContainerVerts[2];
		reference16 = new VertexPositionColor(new Vector3(0f, 10f, 0f), Color.White);
		ref VertexPositionColor reference17 = ref _fuelContainerVerts[3];
		reference17 = new VertexPositionColor(new Vector3(40f, 10f, 0f), Color.White);
		_fuelContainerIndex = new short[8] { 0, 1, 1, 3, 3, 2, 2, 0 };
		_fuelBarVerts = new VertexPositionColor[2];
		ref VertexPositionColor reference18 = ref _fuelBarVerts[0];
		reference18 = new VertexPositionColor(new Vector3(0f, 0f, 0f), Color.White);
		ref VertexPositionColor reference19 = ref _fuelBarVerts[1];
		reference19 = new VertexPositionColor(new Vector3(0f, 10f, 0f), Color.White);
		_fuelBarIndex = new short[2] { 0, 1 };
		_flagVerts = new VertexPositionColor[4];
		ref VertexPositionColor reference20 = ref _flagVerts[0];
		reference20 = new VertexPositionColor(new Vector3(10f, 0f, 0f), Color.Gold);
		ref VertexPositionColor reference21 = ref _flagVerts[1];
		reference21 = new VertexPositionColor(new Vector3(10f, 10f, 0f), Color.Gold);
		ref VertexPositionColor reference22 = ref _flagVerts[2];
		reference22 = new VertexPositionColor(new Vector3(10f, 20f, 0f), Color.Gold);
		ref VertexPositionColor reference23 = ref _flagVerts[3];
		reference23 = new VertexPositionColor(new Vector3(0f, 5f, 0f), Color.Gold);
		_flagIndex = new short[6] { 2, 0, 0, 3, 3, 1 };
		_circleVerts = new VertexPositionColor[16];
		_circleIndex = new short[_circleVerts.Length * 2];
		ref VertexPositionColor reference24 = ref _circleVerts[0];
		reference24 = new VertexPositionColor(new Vector3(0f, 20f, 0f), Color.Green);
		_circleIndex[0] = 0;
		_circleIndex[1] = 1;
		for (int i = 1; i < _circleVerts.Length; i++)
		{
			_circleVerts[i].Position = Vector3.Transform(_circleVerts[0].Position, Matrix.CreateRotationZ((float)Math.PI * 2f / (float)_circleVerts.Length * (float)i));
			_circleVerts[i].Color = Color.Green;
			_circleIndex[i * 2] = (short)i;
			if (i != _circleVerts.Length - 1)
			{
				_circleIndex[i * 2 + 1] = (short)(i + 1);
			}
			else
			{
				_circleIndex[i * 2 + 1] = 0;
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

	public void DrawHUD(LineRender graphics, Vector2 position)
	{
		Vector2 position2 = position;
		foreach (Pod player in _players)
		{
			DrawPodStatus(graphics, player, position2);
			position2.X += 50f;
		}
	}

	public void AddPlayer(Pod pod)
	{
		_players.Add(pod);
	}

	public void RemovePlayer(Pod pod)
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
			Vector3.Transform(ref _characterVerts[i].Position, ref matrix, out array[i].Position);
			array[i].Color = colour;
		}
		short[] indices = _characterIndex[character - 48];
		graphics.DrawIndexedShape(array, indices);
	}

	private void DrawPodStatus(LineRender graphics, Pod pod, Vector2 position)
	{
		Vector2 value = position;
		VertexPositionColor[] array = new VertexPositionColor[_lifeVerts.Length];
		Matrix matrix;
		for (int i = 0; i < pod.Lives; i++)
		{
			matrix = Matrix.CreateTranslation(new Vector3(value, 0f));
			for (int j = 0; j < array.Length; j++)
			{
				Vector3.Transform(ref _lifeVerts[j].Position, ref matrix, out array[j].Position);
				array[j].Color = pod.Colour;
			}
			graphics.DrawIndexedShape(array, _lifeIndex);
			value.X += 15f;
		}
		value.X = position.X;
		value.Y = position.Y + 14f;
		matrix = Matrix.CreateTranslation(new Vector3(value, 0f));
		VertexPositionColor[] array2 = new VertexPositionColor[_fuelContainerVerts.Length];
		for (int k = 0; k < array2.Length; k++)
		{
			Vector3.Transform(ref _fuelContainerVerts[k].Position, ref matrix, out array2[k].Position);
			array2[k].Color = pod.Colour;
		}
		graphics.DrawIndexedShape(array2, _fuelContainerIndex);
		value.X = position.X;
		VertexPositionColor[] array3 = new VertexPositionColor[_fuelBarVerts.Length];
		for (int l = 0; l < (int)((float)pod.Fuel / 400f * 40f); l++)
		{
			matrix = Matrix.CreateTranslation(new Vector3(value, 0f));
			for (int m = 0; m < array3.Length; m++)
			{
				Vector3.Transform(ref _fuelBarVerts[0].Position, ref matrix, out array3[0].Position);
				Vector3.Transform(ref _fuelBarVerts[1].Position, ref matrix, out array3[1].Position);
				array3[0].Color = pod.Colour;
				array3[1].Color = pod.Colour;
			}
			graphics.DrawIndexedShape(array3, _fuelBarIndex);
			value.X++;
		}
	}

	public void DrawFlag(LineRender graphics, Vector2 position, float scale)
	{
		Matrix matrix = Matrix.Multiply(Matrix.CreateScale(scale), Matrix.CreateTranslation(new Vector3(position, 0f)));
		VertexPositionColor[] array = new VertexPositionColor[_flagVerts.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Vector3.Transform(ref _flagVerts[i].Position, ref matrix, out array[i].Position);
			array[i].Color = _flagVerts[i].Color;
		}
		graphics.DrawIndexedShape(array, _flagIndex);
	}

	public void DrawRetry(LineRender graphics, Vector3 position)
	{
		VertexPositionColor[] array = new VertexPositionColor[_circleVerts.Length];
		Matrix matrix = Matrix.CreateTranslation(position + new Vector3(-80f, 0f, 0f));
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Position = Vector3.Transform(_circleVerts[i].Position, matrix);
			array[i].Color = _circleVerts[i].Color;
		}
		graphics.DrawIndexedShape(array, _circleIndex);
		array = new VertexPositionColor[_characterVerts.Length];
		matrix = Matrix.CreateTranslation(position + new Vector3(-85f, -10f, 0f));
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Position = Vector3.Transform(_characterVerts[j].Position, matrix);
			array[j].Color = _circleVerts[0].Color;
		}
		graphics.DrawIndexedShape(array, _buttonIndex);
		for (int k = 0; k < _retryIndex.Count; k++)
		{
			array = new VertexPositionColor[_characterVerts.Length];
			matrix = Matrix.CreateTranslation(position + new Vector3(-50f + 15f * (float)k, -10f, 0f));
			for (int l = 0; l < array.Length; l++)
			{
				array[l].Position = Vector3.Transform(_characterVerts[l].Position, matrix);
				array[l].Color = Color.White;
			}
			if (_retryIndex[k].Length != 0)
			{
				graphics.DrawIndexedShape(array, _retryIndex[k]);
			}
		}
	}
}
