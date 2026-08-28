using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics;

public delegate float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction);
