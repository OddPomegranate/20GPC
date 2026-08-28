using System;
using System.Collections.Generic;
using System.IO;
using FiftyGames.Zombie.Pickups;
using FiftyGames.Zombie.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Zombie.Editors;

internal class PickupEditor
{
	private List<EditorPickup> _editorPickups = new List<EditorPickup>();

	private List<Vector2> _spawners = new List<Vector2>();

	private int _clickedPickup = -1;

	private int _pickupTypeIndex;

	private int _currentProbablity = 100;

	private int _spawnProbability = 100;

	private int _currentSupply;

	private Vector2 _lastClickPosition = Vector2.Zero;

	private PickupManager _pickupManager;

	public PickupEditor()
	{
		_editorPickups = LoadPickups();
		_pickupManager = new PickupManager();
		_pickupManager.PossiblePickups.Add(new DeaglePickup(null, Vector2.Zero, 50, 9, dummy: true));
		_pickupManager.PossiblePickups.Add(new M4Pickup(null, Vector2.Zero, 50, 100, dummy: true));
		_pickupManager.PossiblePickups.Add(new ShotgunPickup(null, Vector2.Zero, 50, 100, dummy: true));
		_pickupManager.PossiblePickups.Add(new SubmachineGunPickup(null, Vector2.Zero, 50, 100, dummy: true));
		_pickupManager.PossiblePickups.Add(new GrenadeLauncherPickup(null, Vector2.Zero, 50, 100, dummy: true));
	}

	public static List<EditorPickup> LoadPickups()
	{
		List<EditorPickup> list = new List<EditorPickup>();
		StreamReader streamReader = new StreamReader("Content/Zombie/Data/latest.pick");
		BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			list.Add(new EditorPickup
			{
				_count = binaryReader.ReadInt32(),
				_pickupIndex = binaryReader.ReadInt32(),
				_position = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle()),
				_probability = binaryReader.ReadInt32(),
				_spawnProbability = binaryReader.ReadInt32()
			});
		}
		return list;
	}

	public void SaveToFile()
	{
		string text = DateTime.Now.ToString("MM-dd-yyyy-HH-mm");
		StreamWriter streamWriter = new StreamWriter(text + ".pick");
		BinaryWriter binaryWriter = new BinaryWriter(streamWriter.BaseStream);
		binaryWriter.Write(_editorPickups.Count);
		for (int i = 0; i < _editorPickups.Count; i++)
		{
			binaryWriter.Write(_editorPickups[i]._count);
			binaryWriter.Write(_editorPickups[i]._pickupIndex);
			binaryWriter.Write(_editorPickups[i]._position.X);
			binaryWriter.Write(_editorPickups[i]._position.Y);
			binaryWriter.Write(_editorPickups[i]._probability);
			binaryWriter.Write(_editorPickups[i]._spawnProbability);
		}
		binaryWriter.Flush();
		binaryWriter.Close();
		streamWriter = new StreamWriter("latest.pick");
		binaryWriter = new BinaryWriter(streamWriter.BaseStream);
		binaryWriter.Write(_editorPickups.Count);
		for (int j = 0; j < _editorPickups.Count; j++)
		{
			binaryWriter.Write(_editorPickups[j]._count);
			binaryWriter.Write(_editorPickups[j]._pickupIndex);
			binaryWriter.Write(_editorPickups[j]._position.X);
			binaryWriter.Write(_editorPickups[j]._position.Y);
			binaryWriter.Write(_editorPickups[j]._probability);
			binaryWriter.Write(_editorPickups[j]._spawnProbability);
		}
		binaryWriter.Flush();
		binaryWriter.Close();
	}

	public void Update()
	{
		if (InputState.LeftButtonClicked() && InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl))
		{
			_clickedPickup = _editorPickups.Count;
			EditorPickup item = new EditorPickup
			{
				_position = InputState.GetMouseCoords() - ZombieUtils.Offset - new Vector2(29f, 18f),
				_pickupIndex = _pickupTypeIndex,
				_probability = _currentProbablity,
				_spawnProbability = _spawnProbability,
				_count = _currentSupply
			};
			_editorPickups.Add(item);
		}
		if (InputState.GetCurrentMouseState() != InputState.GetPreviousMouseState())
		{
			int clickedPickup = GetClickedPickup();
			if (clickedPickup >= 0)
			{
				_clickedPickup = clickedPickup;
				_pickupTypeIndex = _editorPickups[_clickedPickup]._pickupIndex;
				_lastClickPosition = InputState.GetMouseCoords() - ZombieUtils.Offset;
			}
		}
		if (InputState.LeftButtonHeld() && _clickedPickup >= 0 && _clickedPickup < _editorPickups.Count)
		{
			EditorPickup value = _editorPickups[_clickedPickup];
			value._position = InputState.GetMouseCoords() - ZombieUtils.Offset - new Vector2(29f, 18f);
			_editorPickups[_clickedPickup] = value;
		}
		if (InputState.MouseWheelIncremented())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl))
			{
				if (_currentProbablity < 100)
				{
					_currentProbablity++;
					EditorPickup value2 = _editorPickups[_clickedPickup];
					value2._probability = _currentProbablity;
					_editorPickups[_clickedPickup] = value2;
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				if (_currentSupply < 100)
				{
					_currentSupply++;
					EditorPickup value3 = _editorPickups[_clickedPickup];
					value3._count = _currentSupply;
					_editorPickups[_clickedPickup] = value3;
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Space))
			{
				if (_spawnProbability < 100)
				{
					_spawnProbability++;
					EditorPickup value4 = _editorPickups[_clickedPickup];
					value4._spawnProbability = _spawnProbability;
					_editorPickups[_clickedPickup] = value4;
				}
			}
			else if (_pickupTypeIndex > 0)
			{
				_pickupTypeIndex--;
				EditorPickup value5 = _editorPickups[_clickedPickup];
				value5._pickupIndex = _pickupTypeIndex;
				value5._count = _pickupManager.PossiblePickups[value5._pickupIndex].NumberSupplied;
				_currentSupply = value5._count;
				_editorPickups[_clickedPickup] = value5;
			}
		}
		else if (InputState.MouseWheelDecremented())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl))
			{
				if (_currentProbablity > 0)
				{
					_currentProbablity--;
					EditorPickup value6 = _editorPickups[_clickedPickup];
					value6._probability = _currentProbablity;
					_currentSupply = value6._count;
					_editorPickups[_clickedPickup] = value6;
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				if (_currentSupply > 1)
				{
					_currentSupply--;
					EditorPickup value7 = _editorPickups[_clickedPickup];
					value7._count = _currentSupply;
					_editorPickups[_clickedPickup] = value7;
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Space))
			{
				if (_spawnProbability > 0)
				{
					_spawnProbability--;
					EditorPickup value8 = _editorPickups[_clickedPickup];
					value8._spawnProbability = _spawnProbability;
					_editorPickups[_clickedPickup] = value8;
				}
			}
			else if (_pickupTypeIndex < _pickupManager.PossiblePickups.Count - 1)
			{
				_pickupTypeIndex++;
				EditorPickup value9 = _editorPickups[_clickedPickup];
				value9._pickupIndex = _pickupTypeIndex;
				value9._count = _pickupManager.PossiblePickups[value9._pickupIndex].NumberSupplied;
				_currentSupply = value9._count;
				_editorPickups[_clickedPickup] = value9;
			}
		}
		if (InputState.MiddleButtonClicked())
		{
			int clickedPickup2 = GetClickedPickup();
			if (clickedPickup2 >= 0)
			{
				_editorPickups.RemoveAt(clickedPickup2);
			}
		}
		if (InputState.KeyboardStateChanged() && InputState.GetCurrentKeyboardState().IsKeyDown(Keys.S))
		{
			SaveToFile();
		}
	}

	private int GetClickedPickup()
	{
		Vector2 vector = InputState.GetMouseCoords() - ZombieUtils.Offset;
		for (int i = 0; i < _editorPickups.Count; i++)
		{
			Rectangle bounds = _pickupManager.PossiblePickups[_editorPickups[i]._pickupIndex].Sprite.Bounds;
			bounds.Location = new Point((int)_editorPickups[i]._position.X, (int)_editorPickups[i]._position.Y);
			if (bounds.Contains(new Point((int)vector.X, (int)vector.Y)))
			{
				return i;
			}
		}
		return -1;
	}

	public void Draw()
	{
		ZombieUtils.SpriteBatch.Begin();
		for (int i = 0; i < _editorPickups.Count; i++)
		{
			int x = (int)_editorPickups[i]._position.X + (int)ZombieUtils.Offset.X;
			int y = (int)_editorPickups[i]._position.Y + (int)ZombieUtils.Offset.Y;
			Texture2D sprite = _pickupManager.PossiblePickups[_editorPickups[i]._pickupIndex].Sprite;
			if (_clickedPickup == i)
			{
				ZombieUtils.SpriteBatch.Draw(sprite, new Rectangle(x, y, sprite.Width, sprite.Height), Color.Red);
			}
			else
			{
				ZombieUtils.SpriteBatch.Draw(sprite, new Rectangle(x, y, sprite.Width, sprite.Height), Color.White);
			}
		}
		ZombieUtils.SpriteBatch.DrawString(ZombieUtils.Font(), _pickupManager.PossiblePickups[_pickupTypeIndex].GetType().ToString(), new Vector2(100f, 650f), Color.White);
		ZombieUtils.SpriteBatch.End();
		_pickupManager.Draw();
	}
}
