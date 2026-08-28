using FarseerPhysics.Dynamics;
using FiftyGames.Zombie.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Pickups;

internal abstract class Pickup
{
	protected Vector2 _position;

	protected Texture2D _sprite;

	protected Body _body;

	protected bool _pickedUp;

	protected int _numberSupplied;

	protected int _id;

	protected PickupManager _pickupManager;

	protected bool _dummy;

	public Texture2D Sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			_sprite = value;
		}
	}

	public Vector2 Position => _position;

	public bool PickedUp => _pickedUp;

	public int NumberSupplied
	{
		get
		{
			return _numberSupplied;
		}
		set
		{
			_numberSupplied = value;
		}
	}

	public int ProbabilityOfSpawn => _id;

	public Pickup(PickupManager pickupManager, Vector2 position, int probabilityOfSpawn, bool dummy)
	{
		_position = position;
		_pickedUp = false;
		_id = probabilityOfSpawn;
		_pickupManager = pickupManager;
		_dummy = dummy;
	}

	public virtual void OnPlayerTouch(ZombiePlayer player)
	{
		_body.Dispose();
		if (_pickupManager != null)
		{
			_pickupManager.AvailibleNodes[_id] = true;
		}
		_pickedUp = true;
	}

	public void Destory()
	{
		_body.Dispose();
		_body = null;
	}

	public abstract void Draw();
}
