using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common;

public class WorldXmlSerializer
{
	private List<Body> _bodies = new List<Body>();

	private List<Fixture> _serializedFixtures = new List<Fixture>();

	private List<Shape> _serializedShapes = new List<Shape>();

	private XmlWriter _writer;

	private void SerializeShape(Shape shape)
	{
		_writer.WriteStartElement("Shape");
		_writer.WriteAttributeString("Type", shape.ShapeType.ToString());
		switch (shape.ShapeType)
		{
		case ShapeType.Circle:
		{
			CircleShape circleShape = (CircleShape)shape;
			_writer.WriteElementString("Radius", circleShape.Radius.ToString());
			WriteElement("Position", circleShape.Position);
			break;
		}
		case ShapeType.Polygon:
		{
			PolygonShape polygonShape = (PolygonShape)shape;
			_writer.WriteStartElement("Vertices");
			foreach (Vector2 vertex in polygonShape.Vertices)
			{
				WriteElement("Vertex", vertex);
			}
			_writer.WriteEndElement();
			WriteElement("Centroid", polygonShape.MassData.Centroid);
			break;
		}
		case ShapeType.Edge:
		{
			EdgeShape edgeShape = (EdgeShape)shape;
			WriteElement("Vertex1", edgeShape.Vertex1);
			WriteElement("Vertex2", edgeShape.Vertex2);
			break;
		}
		default:
			throw new Exception();
		}
		_writer.WriteEndElement();
	}

	private void SerializeFixture(Fixture fixture)
	{
		_writer.WriteStartElement("Fixture");
		_writer.WriteElementString("Shape", FindShapeIndex(fixture.Shape).ToString());
		_writer.WriteElementString("Density", fixture.Shape.Density.ToString());
		_writer.WriteStartElement("FilterData");
		_writer.WriteElementString("CategoryBits", ((int)fixture.CollisionCategories).ToString());
		_writer.WriteElementString("MaskBits", ((int)fixture.CollidesWith).ToString());
		_writer.WriteElementString("GroupIndex", fixture.CollisionGroup.ToString());
		_writer.WriteEndElement();
		_writer.WriteElementString("Friction", fixture.Friction.ToString());
		_writer.WriteElementString("IsSensor", fixture.IsSensor.ToString());
		_writer.WriteElementString("Restitution", fixture.Restitution.ToString());
		if (fixture.UserData != null)
		{
			_writer.WriteStartElement("UserData");
			WriteDynamicType(fixture.UserData.GetType(), fixture.UserData);
			_writer.WriteEndElement();
		}
		_writer.WriteEndElement();
	}

	private void SerializeBody(Body body)
	{
		_writer.WriteStartElement("Body");
		_writer.WriteAttributeString("Type", body.BodyType.ToString());
		_writer.WriteElementString("Active", body.Enabled.ToString());
		_writer.WriteElementString("AllowSleep", body.SleepingAllowed.ToString());
		_writer.WriteElementString("Angle", body.Rotation.ToString());
		_writer.WriteElementString("AngularDamping", body.AngularDamping.ToString());
		_writer.WriteElementString("AngularVelocity", body.AngularVelocity.ToString());
		_writer.WriteElementString("Awake", body.Awake.ToString());
		_writer.WriteElementString("Bullet", body.IsBullet.ToString());
		_writer.WriteElementString("FixedRotation", body.FixedRotation.ToString());
		_writer.WriteElementString("LinearDamping", body.LinearDamping.ToString());
		WriteElement("LinearVelocity", body.LinearVelocity);
		WriteElement("Position", body.Position);
		if (body.UserData != null)
		{
			_writer.WriteStartElement("UserData");
			WriteDynamicType(body.UserData.GetType(), body.UserData);
			_writer.WriteEndElement();
		}
		_writer.WriteStartElement("Fixtures");
		for (int i = 0; i < body.FixtureList.Count; i++)
		{
			_writer.WriteElementString("ID", FindFixtureIndex(body.FixtureList[i]).ToString());
		}
		_writer.WriteEndElement();
		_writer.WriteEndElement();
	}

	private void SerializeJoint(Joint joint)
	{
		if (!joint.IsFixedType())
		{
			_writer.WriteStartElement("Joint");
			_writer.WriteAttributeString("Type", joint.JointType.ToString());
			WriteElement("BodyA", FindBodyIndex(joint.BodyA));
			WriteElement("BodyB", FindBodyIndex(joint.BodyB));
			WriteElement("CollideConnected", joint.CollideConnected);
			WriteElement("Breakpoint", joint.Breakpoint);
			if (joint.UserData != null)
			{
				_writer.WriteStartElement("UserData");
				WriteDynamicType(joint.UserData.GetType(), joint.UserData);
				_writer.WriteEndElement();
			}
			switch (joint.JointType)
			{
			case JointType.Distance:
			{
				DistanceJoint distanceJoint = (DistanceJoint)joint;
				WriteElement("DampingRatio", distanceJoint.DampingRatio);
				WriteElement("FrequencyHz", distanceJoint.Frequency);
				WriteElement("Length", distanceJoint.Length);
				WriteElement("LocalAnchorA", distanceJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", distanceJoint.LocalAnchorB);
				break;
			}
			case JointType.Friction:
			{
				FrictionJoint frictionJoint = (FrictionJoint)joint;
				WriteElement("LocalAnchorA", frictionJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", frictionJoint.LocalAnchorB);
				WriteElement("MaxForce", frictionJoint.MaxForce);
				WriteElement("MaxTorque", frictionJoint.MaxTorque);
				break;
			}
			case JointType.Gear:
				throw new Exception("Gear joint not supported by serialization");
			case JointType.Line:
			{
				LineJoint lineJoint = (LineJoint)joint;
				WriteElement("EnableMotor", lineJoint.MotorEnabled);
				WriteElement("LocalAnchorA", lineJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", lineJoint.LocalAnchorB);
				WriteElement("MotorSpeed", lineJoint.MotorSpeed);
				WriteElement("DampingRatio", lineJoint.DampingRatio);
				WriteElement("MaxMotorTorque", lineJoint.MaxMotorTorque);
				WriteElement("FrequencyHz", lineJoint.Frequency);
				WriteElement("LocalXAxis", lineJoint.LocalXAxis);
				break;
			}
			case JointType.Prismatic:
			{
				PrismaticJoint prismaticJoint = (PrismaticJoint)joint;
				WriteElement("EnableLimit", prismaticJoint.LimitEnabled);
				WriteElement("EnableMotor", prismaticJoint.MotorEnabled);
				WriteElement("LocalAnchorA", prismaticJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", prismaticJoint.LocalAnchorB);
				WriteElement("LocalXAxis1", prismaticJoint.LocalXAxis1);
				WriteElement("LowerTranslation", prismaticJoint.LowerLimit);
				WriteElement("UpperTranslation", prismaticJoint.UpperLimit);
				WriteElement("MaxMotorForce", prismaticJoint.MaxMotorForce);
				WriteElement("MotorSpeed", prismaticJoint.MotorSpeed);
				break;
			}
			case JointType.Pulley:
			{
				PulleyJoint pulleyJoint = (PulleyJoint)joint;
				WriteElement("GroundAnchorA", pulleyJoint.GroundAnchorA);
				WriteElement("GroundAnchorB", pulleyJoint.GroundAnchorB);
				WriteElement("LengthA", pulleyJoint.LengthA);
				WriteElement("LengthB", pulleyJoint.LengthB);
				WriteElement("LocalAnchorA", pulleyJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", pulleyJoint.LocalAnchorB);
				WriteElement("MaxLengthA", pulleyJoint.MaxLengthA);
				WriteElement("MaxLengthB", pulleyJoint.MaxLengthB);
				WriteElement("Ratio", pulleyJoint.Ratio);
				break;
			}
			case JointType.Revolute:
			{
				RevoluteJoint revoluteJoint = (RevoluteJoint)joint;
				WriteElement("EnableLimit", revoluteJoint.LimitEnabled);
				WriteElement("EnableMotor", revoluteJoint.MotorEnabled);
				WriteElement("LocalAnchorA", revoluteJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", revoluteJoint.LocalAnchorB);
				WriteElement("LowerAngle", revoluteJoint.LowerLimit);
				WriteElement("MaxMotorTorque", revoluteJoint.MaxMotorTorque);
				WriteElement("MotorSpeed", revoluteJoint.MotorSpeed);
				WriteElement("ReferenceAngle", revoluteJoint.ReferenceAngle);
				WriteElement("UpperAngle", revoluteJoint.UpperLimit);
				break;
			}
			case JointType.Weld:
			{
				WeldJoint weldJoint = (WeldJoint)joint;
				WriteElement("LocalAnchorA", weldJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", weldJoint.LocalAnchorB);
				break;
			}
			case JointType.Rope:
			{
				RopeJoint ropeJoint = (RopeJoint)joint;
				WriteElement("LocalAnchorA", ropeJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", ropeJoint.LocalAnchorB);
				WriteElement("MaxLength", ropeJoint.MaxLength);
				break;
			}
			case JointType.Angle:
			{
				AngleJoint angleJoint = (AngleJoint)joint;
				WriteElement("BiasFactor", angleJoint.BiasFactor);
				WriteElement("MaxImpulse", angleJoint.MaxImpulse);
				WriteElement("Softness", angleJoint.Softness);
				WriteElement("TargetAngle", angleJoint.TargetAngle);
				break;
			}
			case JointType.Slider:
			{
				SliderJoint sliderJoint = (SliderJoint)joint;
				WriteElement("DampingRatio", sliderJoint.DampingRatio);
				WriteElement("FrequencyHz", sliderJoint.Frequency);
				WriteElement("MaxLength", sliderJoint.MaxLength);
				WriteElement("MinLength", sliderJoint.MinLength);
				WriteElement("LocalAnchorA", sliderJoint.LocalAnchorA);
				WriteElement("LocalAnchorB", sliderJoint.LocalAnchorB);
				break;
			}
			default:
				throw new Exception("Joint not supported");
			}
			_writer.WriteEndElement();
		}
	}

	private void WriteDynamicType(Type type, object val)
	{
		_writer.WriteElementString("Type", type.FullName);
		_writer.WriteStartElement("Value");
		XmlSerializer xmlSerializer = new XmlSerializer(type);
		XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
		xmlSerializerNamespaces.Add("", "");
		xmlSerializer.Serialize(_writer, val, xmlSerializerNamespaces);
		_writer.WriteEndElement();
	}

	private void WriteElement(string name, Vector2 vec)
	{
		_writer.WriteElementString(name, vec.X + " " + vec.Y);
	}

	private void WriteElement(string name, int val)
	{
		_writer.WriteElementString(name, val.ToString());
	}

	private void WriteElement(string name, bool val)
	{
		_writer.WriteElementString(name, val.ToString());
	}

	private void WriteElement(string name, float val)
	{
		_writer.WriteElementString(name, val.ToString());
	}

	public void Serialize(World world, Stream stream)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.NewLineOnAttributes = false;
		xmlWriterSettings.OmitXmlDeclaration = true;
		_writer = XmlWriter.Create(stream, xmlWriterSettings);
		_writer.WriteStartElement("World");
		_writer.WriteAttributeString("Version", "2");
		WriteElement("Gravity", world.Gravity);
		_writer.WriteStartElement("Shapes");
		for (int i = 0; i < world.BodyList.Count; i++)
		{
			Body body = world.BodyList[i];
			for (int j = 0; j < body.FixtureList.Count; j++)
			{
				Fixture fixture = body.FixtureList[j];
				bool flag = false;
				for (int k = 0; k < _serializedShapes.Count; k++)
				{
					Shape shape = _serializedShapes[k];
					if (fixture.Shape.CompareTo(shape))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					SerializeShape(fixture.Shape);
					_serializedShapes.Add(fixture.Shape);
				}
			}
		}
		_writer.WriteEndElement();
		_writer.WriteStartElement("Fixtures");
		for (int l = 0; l < world.BodyList.Count; l++)
		{
			Body body2 = world.BodyList[l];
			for (int m = 0; m < body2.FixtureList.Count; m++)
			{
				Fixture fixture2 = body2.FixtureList[m];
				bool flag2 = false;
				for (int n = 0; n < _serializedFixtures.Count; n++)
				{
					Fixture fixture3 = _serializedFixtures[n];
					if (fixture2.CompareTo(fixture3))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					SerializeFixture(fixture2);
					_serializedFixtures.Add(fixture2);
				}
			}
		}
		_writer.WriteEndElement();
		_writer.WriteStartElement("Bodies");
		for (int num = 0; num < world.BodyList.Count; num++)
		{
			Body body3 = world.BodyList[num];
			_bodies.Add(body3);
			SerializeBody(body3);
		}
		_writer.WriteEndElement();
		_writer.WriteStartElement("Joints");
		for (int num2 = 0; num2 < world.JointList.Count; num2++)
		{
			Joint joint = world.JointList[num2];
			SerializeJoint(joint);
		}
		_writer.WriteEndElement();
		_writer.WriteEndElement();
		_writer.Flush();
		_writer.Close();
	}

	private int FindBodyIndex(Body body)
	{
		for (int i = 0; i < _bodies.Count; i++)
		{
			if (_bodies[i] == body)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindFixtureIndex(Fixture fixture)
	{
		for (int i = 0; i < _serializedFixtures.Count; i++)
		{
			if (_serializedFixtures[i].CompareTo(fixture))
			{
				return i;
			}
		}
		return -1;
	}

	private int FindShapeIndex(Shape shape)
	{
		for (int i = 0; i < _serializedShapes.Count; i++)
		{
			if (_serializedShapes[i].CompareTo(shape))
			{
				return i;
			}
		}
		return -1;
	}
}
