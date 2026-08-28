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

public class WorldXmlDeserializer
{
	private List<Body> _bodies = new List<Body>();

	private List<Fixture> _fixtures = new List<Fixture>();

	private List<Joint> _joints = new List<Joint>();

	private List<Shape> _shapes = new List<Shape>();

	public World Deserialize(Stream stream)
	{
		World world = new World(Vector2.Zero);
		Deserialize(world, stream);
		return world;
	}

	public void Deserialize(World world, Stream stream)
	{
		world.Clear();
		XMLFragmentElement xMLFragmentElement = XMLFragmentParser.LoadFromStream(stream);
		if (xMLFragmentElement.Name.ToLower() != "world")
		{
			throw new Exception();
		}
		foreach (XMLFragmentElement element in xMLFragmentElement.Elements)
		{
			if (element.Name.ToLower() == "gravity")
			{
				world.Gravity = ReadVector(element);
			}
		}
		foreach (XMLFragmentElement element2 in xMLFragmentElement.Elements)
		{
			if (!(element2.Name.ToLower() == "shapes"))
			{
				continue;
			}
			foreach (XMLFragmentElement element3 in element2.Elements)
			{
				if (element3.Name.ToLower() != "shape")
				{
					throw new Exception();
				}
				switch ((ShapeType)Enum.Parse(typeof(ShapeType), element3.Attributes[0].Value, ignoreCase: true))
				{
				case ShapeType.Circle:
				{
					CircleShape circleShape = new CircleShape();
					foreach (XMLFragmentElement element4 in element3.Elements)
					{
						switch (element4.Name.ToLower())
						{
						case "radius":
							circleShape.Radius = float.Parse(element4.Value);
							break;
						case "position":
							circleShape.Position = ReadVector(element4);
							break;
						default:
							throw new Exception();
						}
					}
					_shapes.Add(circleShape);
					break;
				}
				case ShapeType.Polygon:
				{
					PolygonShape polygonShape = new PolygonShape();
					foreach (XMLFragmentElement element5 in element3.Elements)
					{
						switch (element5.Name.ToLower())
						{
						case "vertices":
						{
							List<Vector2> list = new List<Vector2>();
							foreach (XMLFragmentElement element6 in element5.Elements)
							{
								list.Add(ReadVector(element6));
							}
							polygonShape.Set(new Vertices(list.ToArray()));
							break;
						}
						case "centroid":
							polygonShape.MassData.Centroid = ReadVector(element5);
							break;
						}
					}
					_shapes.Add(polygonShape);
					break;
				}
				case ShapeType.Edge:
				{
					EdgeShape edgeShape = new EdgeShape();
					foreach (XMLFragmentElement element7 in element3.Elements)
					{
						switch (element7.Name.ToLower())
						{
						case "hasvertex0":
							edgeShape.HasVertex0 = bool.Parse(element7.Value);
							break;
						case "hasvertex3":
							edgeShape.HasVertex0 = bool.Parse(element7.Value);
							break;
						case "vertex0":
							edgeShape.Vertex0 = ReadVector(element7);
							break;
						case "vertex1":
							edgeShape.Vertex1 = ReadVector(element7);
							break;
						case "vertex2":
							edgeShape.Vertex2 = ReadVector(element7);
							break;
						case "vertex3":
							edgeShape.Vertex3 = ReadVector(element7);
							break;
						default:
							throw new Exception();
						}
					}
					_shapes.Add(edgeShape);
					break;
				}
				}
			}
		}
		foreach (XMLFragmentElement element8 in xMLFragmentElement.Elements)
		{
			if (!(element8.Name.ToLower() == "fixtures"))
			{
				continue;
			}
			foreach (XMLFragmentElement element9 in element8.Elements)
			{
				Fixture fixture = new Fixture();
				if (element9.Name.ToLower() != "fixture")
				{
					throw new Exception();
				}
				foreach (XMLFragmentElement element10 in element9.Elements)
				{
					switch (element10.Name.ToLower())
					{
					case "shape":
						fixture.Shape = _shapes[int.Parse(element10.Value)];
						break;
					case "density":
						fixture.Shape.Density = float.Parse(element10.Value);
						break;
					case "filterdata":
						foreach (XMLFragmentElement element11 in element10.Elements)
						{
							switch (element11.Name.ToLower())
							{
							case "categorybits":
								fixture._collisionCategories = (Category)int.Parse(element11.Value);
								break;
							case "maskbits":
								fixture._collidesWith = (Category)int.Parse(element11.Value);
								break;
							case "groupindex":
								fixture._collisionGroup = short.Parse(element11.Value);
								break;
							}
						}
						break;
					case "friction":
						fixture.Friction = float.Parse(element10.Value);
						break;
					case "issensor":
						fixture.IsSensor = bool.Parse(element10.Value);
						break;
					case "restitution":
						fixture.Restitution = float.Parse(element10.Value);
						break;
					case "userdata":
						fixture.UserData = ReadSimpleType(element10, null, outer: false);
						break;
					}
				}
				_fixtures.Add(fixture);
			}
		}
		foreach (XMLFragmentElement element12 in xMLFragmentElement.Elements)
		{
			if (!(element12.Name.ToLower() == "bodies"))
			{
				continue;
			}
			foreach (XMLFragmentElement element13 in element12.Elements)
			{
				Body body = new Body(world);
				if (element13.Name.ToLower() != "body")
				{
					throw new Exception();
				}
				body.BodyType = (BodyType)Enum.Parse(typeof(BodyType), element13.Attributes[0].Value, ignoreCase: true);
				foreach (XMLFragmentElement element14 in element13.Elements)
				{
					switch (element14.Name.ToLower())
					{
					case "active":
						if (bool.Parse(element14.Value))
						{
							body.Flags |= BodyFlags.Enabled;
						}
						else
						{
							body.Flags &= ~BodyFlags.Enabled;
						}
						break;
					case "allowsleep":
						body.SleepingAllowed = bool.Parse(element14.Value);
						break;
					case "angle":
					{
						Vector2 position2 = body.Position;
						body.SetTransformIgnoreContacts(ref position2, float.Parse(element14.Value));
						break;
					}
					case "angulardamping":
						body.AngularDamping = float.Parse(element14.Value);
						break;
					case "angularvelocity":
						body.AngularVelocity = float.Parse(element14.Value);
						break;
					case "awake":
						body.Awake = bool.Parse(element14.Value);
						break;
					case "bullet":
						body.IsBullet = bool.Parse(element14.Value);
						break;
					case "fixedrotation":
						body.FixedRotation = bool.Parse(element14.Value);
						break;
					case "lineardamping":
						body.LinearDamping = float.Parse(element14.Value);
						break;
					case "linearvelocity":
						body.LinearVelocity = ReadVector(element14);
						break;
					case "position":
					{
						float rotation = body.Rotation;
						Vector2 position = ReadVector(element14);
						body.SetTransformIgnoreContacts(ref position, rotation);
						break;
					}
					case "userdata":
						body.UserData = ReadSimpleType(element14, null, outer: false);
						break;
					case "fixtures":
						foreach (XMLFragmentElement element15 in element14.Elements)
						{
							Fixture fixture2 = _fixtures[int.Parse(element15.Value)];
							Fixture fixture3 = new Fixture(body, fixture2.Shape);
							fixture3.Restitution = fixture2.Restitution;
							fixture3.UserData = fixture2.UserData;
							fixture3.Friction = fixture2.Friction;
							fixture3.CollidesWith = fixture2.CollidesWith;
							fixture3.CollisionCategories = fixture2.CollisionCategories;
							fixture3.CollisionGroup = fixture2.CollisionGroup;
						}
						break;
					}
				}
				_bodies.Add(body);
			}
		}
		foreach (XMLFragmentElement element16 in xMLFragmentElement.Elements)
		{
			if (!(element16.Name.ToLower() == "joints"))
			{
				continue;
			}
			foreach (XMLFragmentElement element17 in element16.Elements)
			{
				if (element17.Name.ToLower() != "joint")
				{
					throw new Exception();
				}
				JointType jointType = (JointType)Enum.Parse(typeof(JointType), element17.Attributes[0].Value, ignoreCase: true);
				int index = -1;
				int index2 = -1;
				bool collideConnected = false;
				object userData = null;
				foreach (XMLFragmentElement element18 in element17.Elements)
				{
					switch (element18.Name.ToLower())
					{
					case "bodya":
						index = int.Parse(element18.Value);
						break;
					case "bodyb":
						index2 = int.Parse(element18.Value);
						break;
					case "collideconnected":
						collideConnected = bool.Parse(element18.Value);
						break;
					case "userdata":
						userData = ReadSimpleType(element18, null, outer: false);
						break;
					}
				}
				Body bodyA = _bodies[index];
				Body bodyB = _bodies[index2];
				Joint joint = jointType switch
				{
					JointType.Distance => new DistanceJoint(), 
					JointType.Friction => new FrictionJoint(), 
					JointType.Line => new LineJoint(), 
					JointType.Prismatic => new PrismaticJoint(), 
					JointType.Pulley => new PulleyJoint(), 
					JointType.Revolute => new RevoluteJoint(), 
					JointType.Weld => new WeldJoint(), 
					JointType.Rope => new RopeJoint(), 
					JointType.Angle => new AngleJoint(), 
					JointType.Slider => new SliderJoint(), 
					JointType.Gear => throw new Exception("GearJoint is not supported."), 
					_ => throw new Exception("Invalid or unsupported joint."), 
				};
				joint.CollideConnected = collideConnected;
				joint.UserData = userData;
				joint.BodyA = bodyA;
				joint.BodyB = bodyB;
				_joints.Add(joint);
				world.AddJoint(joint);
				foreach (XMLFragmentElement element19 in element17.Elements)
				{
					switch (jointType)
					{
					case JointType.Distance:
						switch (element19.Name.ToLower())
						{
						case "dampingratio":
							((DistanceJoint)joint).DampingRatio = float.Parse(element19.Value);
							break;
						case "frequencyhz":
							((DistanceJoint)joint).Frequency = float.Parse(element19.Value);
							break;
						case "length":
							((DistanceJoint)joint).Length = float.Parse(element19.Value);
							break;
						case "localanchora":
							((DistanceJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((DistanceJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						}
						break;
					case JointType.Friction:
						switch (element19.Name.ToLower())
						{
						case "localanchora":
							((FrictionJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((FrictionJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						case "maxforce":
							((FrictionJoint)joint).MaxForce = float.Parse(element19.Value);
							break;
						case "maxtorque":
							((FrictionJoint)joint).MaxTorque = float.Parse(element19.Value);
							break;
						}
						break;
					case JointType.Line:
						switch (element19.Name.ToLower())
						{
						case "enablemotor":
							((LineJoint)joint).MotorEnabled = bool.Parse(element19.Value);
							break;
						case "localanchora":
							((LineJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((LineJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						case "motorspeed":
							((LineJoint)joint).MotorSpeed = float.Parse(element19.Value);
							break;
						case "dampingratio":
							((LineJoint)joint).DampingRatio = float.Parse(element19.Value);
							break;
						case "maxmotortorque":
							((LineJoint)joint).MaxMotorTorque = float.Parse(element19.Value);
							break;
						case "frequencyhz":
							((LineJoint)joint).Frequency = float.Parse(element19.Value);
							break;
						case "localxaxis":
							((LineJoint)joint).LocalXAxis = ReadVector(element19);
							break;
						}
						break;
					case JointType.Prismatic:
						switch (element19.Name.ToLower())
						{
						case "enablelimit":
							((PrismaticJoint)joint).LimitEnabled = bool.Parse(element19.Value);
							break;
						case "enablemotor":
							((PrismaticJoint)joint).MotorEnabled = bool.Parse(element19.Value);
							break;
						case "localanchora":
							((PrismaticJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((PrismaticJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						case "local1axis1":
							((PrismaticJoint)joint).LocalXAxis1 = ReadVector(element19);
							break;
						case "maxmotorforce":
							((PrismaticJoint)joint).MaxMotorForce = float.Parse(element19.Value);
							break;
						case "motorspeed":
							((PrismaticJoint)joint).MotorSpeed = float.Parse(element19.Value);
							break;
						case "lowertranslation":
							((PrismaticJoint)joint).LowerLimit = float.Parse(element19.Value);
							break;
						case "uppertranslation":
							((PrismaticJoint)joint).UpperLimit = float.Parse(element19.Value);
							break;
						case "referenceangle":
							((PrismaticJoint)joint).ReferenceAngle = float.Parse(element19.Value);
							break;
						}
						break;
					case JointType.Pulley:
						switch (element19.Name.ToLower())
						{
						case "groundanchora":
							((PulleyJoint)joint).GroundAnchorA = ReadVector(element19);
							break;
						case "groundanchorb":
							((PulleyJoint)joint).GroundAnchorB = ReadVector(element19);
							break;
						case "lengtha":
							((PulleyJoint)joint).LengthA = float.Parse(element19.Value);
							break;
						case "lengthb":
							((PulleyJoint)joint).LengthB = float.Parse(element19.Value);
							break;
						case "localanchora":
							((PulleyJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((PulleyJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						case "maxlengtha":
							((PulleyJoint)joint).MaxLengthA = float.Parse(element19.Value);
							break;
						case "maxlengthb":
							((PulleyJoint)joint).MaxLengthB = float.Parse(element19.Value);
							break;
						case "ratio":
							((PulleyJoint)joint).Ratio = float.Parse(element19.Value);
							break;
						}
						break;
					case JointType.Revolute:
						switch (element19.Name.ToLower())
						{
						case "enablelimit":
							((RevoluteJoint)joint).LimitEnabled = bool.Parse(element19.Value);
							break;
						case "enablemotor":
							((RevoluteJoint)joint).MotorEnabled = bool.Parse(element19.Value);
							break;
						case "localanchora":
							((RevoluteJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((RevoluteJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						case "maxmotortorque":
							((RevoluteJoint)joint).MaxMotorTorque = float.Parse(element19.Value);
							break;
						case "motorspeed":
							((RevoluteJoint)joint).MotorSpeed = float.Parse(element19.Value);
							break;
						case "lowerangle":
							((RevoluteJoint)joint).LowerLimit = float.Parse(element19.Value);
							break;
						case "upperangle":
							((RevoluteJoint)joint).UpperLimit = float.Parse(element19.Value);
							break;
						case "referenceangle":
							((RevoluteJoint)joint).ReferenceAngle = float.Parse(element19.Value);
							break;
						}
						break;
					case JointType.Weld:
						switch (element19.Name.ToLower())
						{
						case "localanchora":
							((WeldJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((WeldJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						}
						break;
					case JointType.Rope:
						switch (element19.Name.ToLower())
						{
						case "localanchora":
							((RopeJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((RopeJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						case "maxlength":
							((RopeJoint)joint).MaxLength = float.Parse(element19.Value);
							break;
						}
						break;
					case JointType.Gear:
						throw new Exception("Gear joint is unsupported");
					case JointType.Angle:
						switch (element19.Name.ToLower())
						{
						case "biasfactor":
							((AngleJoint)joint).BiasFactor = float.Parse(element19.Value);
							break;
						case "maximpulse":
							((AngleJoint)joint).MaxImpulse = float.Parse(element19.Value);
							break;
						case "softness":
							((AngleJoint)joint).Softness = float.Parse(element19.Value);
							break;
						case "targetangle":
							((AngleJoint)joint).TargetAngle = float.Parse(element19.Value);
							break;
						}
						break;
					case JointType.Slider:
						switch (element19.Name.ToLower())
						{
						case "dampingratio":
							((SliderJoint)joint).DampingRatio = float.Parse(element19.Value);
							break;
						case "frequencyhz":
							((SliderJoint)joint).Frequency = float.Parse(element19.Value);
							break;
						case "maxlength":
							((SliderJoint)joint).MaxLength = float.Parse(element19.Value);
							break;
						case "minlength":
							((SliderJoint)joint).MinLength = float.Parse(element19.Value);
							break;
						case "localanchora":
							((SliderJoint)joint).LocalAnchorA = ReadVector(element19);
							break;
						case "localanchorb":
							((SliderJoint)joint).LocalAnchorB = ReadVector(element19);
							break;
						}
						break;
					}
				}
			}
		}
	}

	private Vector2 ReadVector(XMLFragmentElement node)
	{
		string[] array = node.Value.Split(' ');
		return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
	}

	private object ReadSimpleType(XMLFragmentElement node, Type type, bool outer)
	{
		if ((object)type == null)
		{
			return ReadSimpleType(node.Elements[1], Type.GetType(node.Elements[0].Value), outer);
		}
		XmlSerializer xmlSerializer = new XmlSerializer(type);
		XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
		xmlSerializerNamespaces.Add("", "");
		using MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(outer ? node.OuterXml : node.InnerXml);
		streamWriter.Flush();
		memoryStream.Position = 0L;
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
		return xmlSerializer.Deserialize(XmlReader.Create(memoryStream, xmlReaderSettings));
	}
}
