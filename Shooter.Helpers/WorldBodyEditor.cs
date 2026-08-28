using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shooter.ISHelpers;

namespace Shooter.Helpers;

internal class WorldBodyEditor
{
	private World _world;

	private GraphicsDevice _graphicsDevice;

	private ContentManager _contentManager;

	private List<PhysObject> _possibleWorldBodies;

	private List<PhysObject> _worldBodies;

	private SpriteFont _font;

	private int _selectedIndex;

	private Body _selectedBody;

	private string _filename;

	public WorldBodyEditor(World world, GraphicsDevice graphicsDevice, ContentManager contentManager, string filename)
	{
		_world = world;
		_graphicsDevice = graphicsDevice;
		_contentManager = contentManager;
		_possibleWorldBodies = new List<PhysObject>();
		_worldBodies = new List<PhysObject>();
		_selectedIndex = 0;
		_filename = filename;
		_font = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
	}

	public void RegisterWorldBody(PhysObject worldBody)
	{
		_possibleWorldBodies.Add(worldBody);
	}

	public void Update(Vector2 mousePos)
	{
		if (InputState.MouseWheelIncremented() && _selectedIndex < _possibleWorldBodies.Count - 1)
		{
			_selectedIndex++;
		}
		if (InputState.MouseWheelDecremented() && _selectedIndex > 0)
		{
			_selectedIndex--;
		}
		if (InputState.LeftButtonClicked())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl) && InputState.GetCurrentMouseState().LeftButton == ButtonState.Pressed)
			{
				ConstructorInfo constructorInfo = _possibleWorldBodies[_selectedIndex].GetType().GetConstructors()[0];
				_worldBodies.Add((PhysObject)constructorInfo.Invoke(new object[3] { _world, _contentManager, mousePos }));
			}
			else
			{
				Body bodyMouseIsOver = GetBodyMouseIsOver(mousePos);
				if (bodyMouseIsOver != null)
				{
					_selectedBody = bodyMouseIsOver;
				}
			}
		}
		if (InputState.LeftButtonHeld())
		{
			Body bodyMouseIsOver2 = GetBodyMouseIsOver(mousePos);
			if (bodyMouseIsOver2 != null)
			{
				bodyMouseIsOver2.Position = ConvertUnits.ToSimUnits(mousePos);
				_selectedBody = bodyMouseIsOver2;
			}
		}
		if (InputState.MiddleButtonClicked())
		{
			GetBodyMouseIsOver(mousePos)?.Dispose();
		}
		if (InputState.RightButtonHeld())
		{
			Body bodyMouseIsOver3 = GetBodyMouseIsOver(mousePos);
			if (bodyMouseIsOver3 != null)
			{
				_selectedBody = bodyMouseIsOver3;
			}
			if (_selectedBody != null)
			{
				Vector2 v = _selectedBody.Position - ConvertUnits.ToSimUnits(mousePos);
				v.Normalize();
				float rotation = GeometryHelper.V2ToAngle(v);
				_selectedBody.Rotation = rotation;
			}
		}
		for (int i = 0; i < _worldBodies.Count; i++)
		{
			if (_worldBodies[i].Body.IsDisposed)
			{
				_worldBodies.RemoveAt(i);
				i--;
			}
		}
		if (InputState.IsKeyDown(Keys.S))
		{
			SavePositions(_filename);
		}
	}

	public Body GetBodyMouseIsOver(Vector2 mousePos)
	{
		AABB aabb = new AABB(ConvertUnits.ToSimUnits(mousePos), ConvertUnits.ToSimUnits(10), ConvertUnits.ToSimUnits(10));
		Body hitBody = null;
		_world.QueryAABB(delegate(Fixture found)
		{
			if (found.Body.UserData != null && found.Body.UserData is PhysObject)
			{
				hitBody = found.Body;
			}
			return false;
		}, ref aabb);
		return hitBody;
	}

	public void SavePositions(string filename)
	{
		BinaryWriter binaryWriter = new BinaryWriter(new StreamWriter(filename).BaseStream);
		binaryWriter.Write(_worldBodies.Count);
		for (int i = 0; i < _worldBodies.Count; i++)
		{
			for (int j = 0; j < _possibleWorldBodies.Count; j++)
			{
				if ((object)_worldBodies[i].GetType() == _possibleWorldBodies[j].GetType())
				{
					binaryWriter.Write(j);
				}
			}
			binaryWriter.Write(_worldBodies[i].Body.Position.X);
			binaryWriter.Write(_worldBodies[i].Body.Position.Y);
			binaryWriter.Write(_worldBodies[i].Body.Rotation);
		}
		binaryWriter.Close();
	}

	public void LoadPositions()
	{
		BinaryReader binaryReader = new BinaryReader(new StreamReader(_filename).BaseStream);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int index = binaryReader.ReadInt32();
			Vector2 position = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
			float rotation = binaryReader.ReadSingle();
			ConstructorInfo constructorInfo = _possibleWorldBodies[index].GetType().GetConstructors()[0];
			_worldBodies.Add((PhysObject)constructorInfo.Invoke(new object[3]
			{
				_world,
				_contentManager,
				Vector2.Zero
			}));
			_worldBodies[_worldBodies.Count - 1].Body.Position = position;
			_worldBodies[_worldBodies.Count - 1].Body.Rotation = rotation;
		}
	}

	public void LoadPositions(List<PhysObject> physCollection)
	{
		BinaryReader binaryReader = new BinaryReader(new StreamReader(_filename).BaseStream);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int index = binaryReader.ReadInt32();
			Vector2 position = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
			float rotation = binaryReader.ReadSingle();
			ConstructorInfo constructorInfo = _possibleWorldBodies[index].GetType().GetConstructors()[0];
			physCollection.Add((PhysObject)constructorInfo.Invoke(new object[3]
			{
				_world,
				_contentManager,
				Vector2.Zero
			}));
			physCollection[physCollection.Count - 1].Body.Position = position;
			physCollection[physCollection.Count - 1].Body.Rotation = rotation;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (_possibleWorldBodies.Count <= 0)
		{
			return;
		}
		spriteBatch.Begin();
		spriteBatch.DrawString(_font, _possibleWorldBodies[_selectedIndex].GetType().ToString(), new Vector2(50f, 100f), Color.White);
		spriteBatch.End();
		foreach (PhysObject worldBody in _worldBodies)
		{
			worldBody.Draw(spriteBatch);
		}
	}
}
