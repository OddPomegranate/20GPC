using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SuperHighway;

internal class Road
{
	private const float RoadNearWidth = 500f;

	private const int StartingStraight = 180;

	private const int Linearity = 2;

	private const int TurnTime = 100;

	private const int TunnelYield = 5;

	private const int MaxArticles = 40;

	private const int ArticleSpeed = 2000;

	private const int ArticleSpacing = 60;

	private const int RoadLineSpacing = 20;

	private const float RoadLineSize = 0.2f;

	private const float LampFrequency = 0.2f;

	private const int LampAmount = 32;

	private const int MinLampGroup = 8;

	private const float PineFrequency = 0.4f;

	private const int PineAmount = 4;

	private const float ElmFrequency = 0.4f;

	private const int ElmAmount = 6;

	private const float BushFrequency = 0.5f;

	private const int BushAmount = 12;

	private VertexPositionColor[] _roadVerts;

	private short[] _roadIndex;

	private VertexPositionColor[] _horizonVerts;

	private short[] _horizonIndex;

	private Vector3[] _carVerts;

	private short[] _carBackIndex;

	private short[] _carLeftIndex;

	private short[] _carRightIndex;

	private Vector3[] _debrisVerts;

	private short[] _debrisIndex;

	private bool _tunnel;

	private List<int> _tunnelDist;

	private VertexPositionColor[] _tunnelVerts;

	private short[] _tunnelIndex;

	private List<int> _articleType;

	private List<int> _articleDist;

	private VertexPositionColor[] _lampVerts;

	private short[] _lampIndex;

	private VertexPositionColor[] _pineVerts;

	private short[] _pineIndex;

	private VertexPositionColor[] _elmVerts;

	private short[] _elmIndex;

	private VertexPositionColor[] _bushVerts;

	private short[] _bushIndex;

	private Random _ranGen;

	private float _angle;

	private float _nextDirection;

	private float _prevDirection;

	private int _frame;

	public Road()
	{
		_roadVerts = new VertexPositionColor[3];
		ref VertexPositionColor reference = ref _roadVerts[0];
		reference = new VertexPositionColor(new Vector3(-80.00001f, 540f, 0f), Color.White);
		ref VertexPositionColor reference2 = ref _roadVerts[1];
		reference2 = new VertexPositionColor(new Vector3(720f, 540f, 0f), Color.White);
		ref VertexPositionColor reference3 = ref _roadVerts[2];
		reference3 = new VertexPositionColor(new Vector3(320f, 120f, 0f), Color.White);
		_roadIndex = new short[4] { 0, 2, 1, 2 };
		_horizonVerts = new VertexPositionColor[4];
		ref VertexPositionColor reference4 = ref _horizonVerts[0];
		reference4 = new VertexPositionColor(new Vector3(0f, 120f, 0f), Color.DimGray);
		ref VertexPositionColor reference5 = ref _horizonVerts[1];
		reference5 = new VertexPositionColor(new Vector3(320f, 120f, 0f), Color.DimGray);
		ref VertexPositionColor reference6 = ref _horizonVerts[2];
		reference6 = new VertexPositionColor(new Vector3(320f, 120f, 0f), Color.DimGray);
		ref VertexPositionColor reference7 = ref _horizonVerts[3];
		reference7 = new VertexPositionColor(new Vector3(640f, 120f, 0f), Color.DimGray);
		_horizonIndex = new short[4] { 0, 1, 2, 3 };
		_carVerts = new Vector3[42];
		ref Vector3 reference8 = ref _carVerts[0];
		reference8 = new Vector3(-1f, 0f, -1f);
		ref Vector3 reference9 = ref _carVerts[1];
		reference9 = new Vector3(1f, 0f, -1f);
		ref Vector3 reference10 = ref _carVerts[2];
		reference10 = new Vector3(-1f, 0f, 0f);
		ref Vector3 reference11 = ref _carVerts[3];
		reference11 = new Vector3(1f, 0f, 0f);
		ref Vector3 reference12 = ref _carVerts[4];
		reference12 = new Vector3(-1f, -1f, 1f);
		ref Vector3 reference13 = ref _carVerts[5];
		reference13 = new Vector3(1f, -1f, 1f);
		ref Vector3 reference14 = ref _carVerts[6];
		reference14 = new Vector3(-1f, -0.7f, 1f);
		ref Vector3 reference15 = ref _carVerts[7];
		reference15 = new Vector3(1f, -0.7f, 1f);
		ref Vector3 reference16 = ref _carVerts[8];
		reference16 = new Vector3(-0.8f, -0.7f, 1f);
		ref Vector3 reference17 = ref _carVerts[9];
		reference17 = new Vector3(0.8f, -0.7f, 1f);
		ref Vector3 reference18 = ref _carVerts[10];
		reference18 = new Vector3(-0.8f, 0f, 1f);
		ref Vector3 reference19 = ref _carVerts[11];
		reference19 = new Vector3(0.8f, 0f, 1f);
		ref Vector3 reference20 = ref _carVerts[12];
		reference20 = new Vector3(-1f, -0f, 0.6f);
		ref Vector3 reference21 = ref _carVerts[13];
		reference21 = new Vector3(-1f, -0.5f, 0.6f);
		ref Vector3 reference22 = ref _carVerts[14];
		reference22 = new Vector3(-1f, -0.7f, 0.8f);
		ref Vector3 reference23 = ref _carVerts[15];
		reference23 = new Vector3(1f, -0f, 0.6f);
		ref Vector3 reference24 = ref _carVerts[16];
		reference24 = new Vector3(1f, -0.5f, 0.6f);
		ref Vector3 reference25 = ref _carVerts[17];
		reference25 = new Vector3(1f, -0.7f, 0.8f);
		ref Vector3 reference26 = ref _carVerts[18];
		reference26 = new Vector3(-0.8f, -0.7f, 1.1f);
		ref Vector3 reference27 = ref _carVerts[19];
		reference27 = new Vector3(-0.8f, -0.5f, 1.3f);
		ref Vector3 reference28 = ref _carVerts[20];
		reference28 = new Vector3(-0.8f, -0.3f, 1.3f);
		ref Vector3 reference29 = ref _carVerts[21];
		reference29 = new Vector3(-0.8f, -0f, 1.1f);
		ref Vector3 reference30 = ref _carVerts[22];
		reference30 = new Vector3(-1.2f, -0.7f, 1.1f);
		ref Vector3 reference31 = ref _carVerts[23];
		reference31 = new Vector3(-1.2f, -0.5f, 1.3f);
		ref Vector3 reference32 = ref _carVerts[24];
		reference32 = new Vector3(-1.2f, -0.3f, 1.3f);
		ref Vector3 reference33 = ref _carVerts[25];
		reference33 = new Vector3(-1.2f, -0f, 1.1f);
		ref Vector3 reference34 = ref _carVerts[26];
		reference34 = new Vector3(-1.2f, -0.7f, 0.8f);
		ref Vector3 reference35 = ref _carVerts[27];
		reference35 = new Vector3(-1.2f, -0.5f, 0.6f);
		ref Vector3 reference36 = ref _carVerts[28];
		reference36 = new Vector3(-1.2f, -0.3f, 0.6f);
		ref Vector3 reference37 = ref _carVerts[29];
		reference37 = new Vector3(-1.2f, -0f, 0.8f);
		ref Vector3 reference38 = ref _carVerts[30];
		reference38 = new Vector3(0.8f, -0.7f, 1.1f);
		ref Vector3 reference39 = ref _carVerts[31];
		reference39 = new Vector3(0.8f, -0.5f, 1.3f);
		ref Vector3 reference40 = ref _carVerts[32];
		reference40 = new Vector3(0.8f, -0.3f, 1.3f);
		ref Vector3 reference41 = ref _carVerts[33];
		reference41 = new Vector3(0.8f, -0f, 1.1f);
		ref Vector3 reference42 = ref _carVerts[34];
		reference42 = new Vector3(1.2f, -0.7f, 1.1f);
		ref Vector3 reference43 = ref _carVerts[35];
		reference43 = new Vector3(1.2f, -0.5f, 1.3f);
		ref Vector3 reference44 = ref _carVerts[36];
		reference44 = new Vector3(1.2f, -0.3f, 1.3f);
		ref Vector3 reference45 = ref _carVerts[37];
		reference45 = new Vector3(1.2f, -0f, 1.1f);
		ref Vector3 reference46 = ref _carVerts[38];
		reference46 = new Vector3(1.2f, -0.7f, 0.8f);
		ref Vector3 reference47 = ref _carVerts[39];
		reference47 = new Vector3(1.2f, -0.5f, 0.6f);
		ref Vector3 reference48 = ref _carVerts[40];
		reference48 = new Vector3(1.2f, -0.3f, 0.6f);
		ref Vector3 reference49 = ref _carVerts[41];
		reference49 = new Vector3(1.2f, -0f, 0.8f);
		_carBackIndex = new short[58]
		{
			0, 1, 0, 4, 1, 5, 4, 5, 4, 6,
			5, 7, 6, 8, 7, 9, 10, 11, 8, 18,
			18, 19, 19, 20, 20, 21, 26, 22, 22, 23,
			23, 24, 24, 25, 26, 14, 21, 25, 9, 30,
			30, 31, 31, 32, 32, 33, 38, 34, 34, 35,
			35, 36, 36, 37, 38, 17, 33, 37
		};
		_carLeftIndex = new short[20]
		{
			0, 2, 22, 23, 23, 24, 24, 25, 25, 29,
			26, 27, 27, 28, 28, 29, 9, 11, 11, 33
		};
		_carRightIndex = new short[20]
		{
			1, 3, 34, 35, 35, 36, 36, 37, 37, 41,
			38, 39, 39, 40, 40, 41, 8, 10, 10, 21
		};
		_debrisVerts = new Vector3[3];
		ref Vector3 reference50 = ref _debrisVerts[0];
		reference50 = new Vector3(0f, 0f, 0.0404f);
		ref Vector3 reference51 = ref _debrisVerts[1];
		reference51 = new Vector3(-0.035f, 0f, -0.0202f);
		ref Vector3 reference52 = ref _debrisVerts[2];
		reference52 = new Vector3(0.035f, 0f, -0.0202f);
		_debrisIndex = new short[6] { 0, 1, 1, 2, 2, 0 };
		_tunnelDist = new List<int>();
		_tunnelVerts = new VertexPositionColor[4];
		ref VertexPositionColor reference53 = ref _tunnelVerts[0];
		reference53 = new VertexPositionColor(new Vector3(320f, 360f, 0f), Color.Gray);
		ref VertexPositionColor reference54 = ref _tunnelVerts[1];
		reference54 = new VertexPositionColor(new Vector3(320f, 360f, 0f), Color.Gray);
		ref VertexPositionColor reference55 = ref _tunnelVerts[2];
		reference55 = new VertexPositionColor(new Vector3(320f, 120f, 0f), Color.Gray);
		ref VertexPositionColor reference56 = ref _tunnelVerts[3];
		reference56 = new VertexPositionColor(new Vector3(320f, 120f, 0f), Color.Gray);
		_tunnelIndex = new short[6] { 0, 2, 1, 3, 2, 3 };
		_articleType = new List<int>();
		_articleDist = new List<int>();
		_lampVerts = new VertexPositionColor[8];
		ref VertexPositionColor reference57 = ref _lampVerts[0];
		reference57 = new VertexPositionColor(new Vector3(1f, 0f, 0f), Color.White);
		ref VertexPositionColor reference58 = ref _lampVerts[1];
		reference58 = new VertexPositionColor(new Vector3(1f, -680f, 0f), Color.White);
		ref VertexPositionColor reference59 = ref _lampVerts[2];
		reference59 = new VertexPositionColor(new Vector3(0.91f, -700f, 0f), Color.White);
		ref VertexPositionColor reference60 = ref _lampVerts[3];
		reference60 = new VertexPositionColor(new Vector3(0.85f, -700f, 0f), Color.White);
		ref VertexPositionColor reference61 = ref _lampVerts[4];
		reference61 = new VertexPositionColor(new Vector3(0.7f, -700f, 0f), Color.White);
		ref VertexPositionColor reference62 = ref _lampVerts[5];
		reference62 = new VertexPositionColor(new Vector3(0.69f, -690f, 0f), Color.White);
		ref VertexPositionColor reference63 = ref _lampVerts[6];
		reference63 = new VertexPositionColor(new Vector3(0.7f, -680f, 0f), Color.Yellow);
		ref VertexPositionColor reference64 = ref _lampVerts[7];
		reference64 = new VertexPositionColor(new Vector3(0.85f, -680f, 0f), Color.Yellow);
		_lampIndex = new short[16]
		{
			0, 1, 1, 2, 2, 3, 3, 4, 4, 5,
			5, 6, 6, 7, 7, 3
		};
		_pineVerts = new VertexPositionColor[17];
		ref VertexPositionColor reference65 = ref _pineVerts[0];
		reference65 = new VertexPositionColor(new Vector3(1.4f, 0f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference66 = ref _pineVerts[1];
		reference66 = new VertexPositionColor(new Vector3(1.4f, -96f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference67 = ref _pineVerts[2];
		reference67 = new VertexPositionColor(new Vector3(1f, -76f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference68 = ref _pineVerts[3];
		reference68 = new VertexPositionColor(new Vector3(1.25f, -282f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference69 = ref _pineVerts[4];
		reference69 = new VertexPositionColor(new Vector3(1.1f, -262f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference70 = ref _pineVerts[5];
		reference70 = new VertexPositionColor(new Vector3(1.3f, -468f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference71 = ref _pineVerts[6];
		reference71 = new VertexPositionColor(new Vector3(1.2f, -448f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference72 = ref _pineVerts[7];
		reference72 = new VertexPositionColor(new Vector3(1.35f, -654f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference73 = ref _pineVerts[8];
		reference73 = new VertexPositionColor(new Vector3(1.3f, -634f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference74 = ref _pineVerts[9];
		reference74 = new VertexPositionColor(new Vector3(1.4f, -900f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference75 = ref _pineVerts[10];
		reference75 = new VertexPositionColor(new Vector3(1.5f, -634f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference76 = ref _pineVerts[11];
		reference76 = new VertexPositionColor(new Vector3(1.45f, -654f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference77 = ref _pineVerts[12];
		reference77 = new VertexPositionColor(new Vector3(1.6f, -448f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference78 = ref _pineVerts[13];
		reference78 = new VertexPositionColor(new Vector3(1.5f, -468f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference79 = ref _pineVerts[14];
		reference79 = new VertexPositionColor(new Vector3(1.7f, -262f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference80 = ref _pineVerts[15];
		reference80 = new VertexPositionColor(new Vector3(1.55f, -282f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference81 = ref _pineVerts[16];
		reference81 = new VertexPositionColor(new Vector3(1.8f, -76f, 0f), Color.DarkGreen);
		_pineIndex = new short[34]
		{
			0, 1, 1, 2, 2, 3, 3, 4, 4, 5,
			5, 6, 6, 7, 7, 8, 8, 9, 9, 10,
			10, 11, 11, 12, 12, 13, 13, 14, 14, 15,
			15, 16, 16, 1
		};
		_elmVerts = new VertexPositionColor[25];
		ref VertexPositionColor reference82 = ref _elmVerts[0];
		reference82 = new VertexPositionColor(new Vector3(1.4f, 0f, 0f), Color.Brown);
		ref VertexPositionColor reference83 = ref _elmVerts[1];
		reference83 = new VertexPositionColor(new Vector3(1.4f, -260f, 0f), Color.Brown);
		ref VertexPositionColor reference84 = ref _elmVerts[2];
		reference84 = new VertexPositionColor(new Vector3(1.4f, -480f, 0f), Color.Brown);
		ref VertexPositionColor reference85 = ref _elmVerts[3];
		reference85 = new VertexPositionColor(new Vector3(1.2f, -340f, 0f), Color.Brown);
		ref VertexPositionColor reference86 = ref _elmVerts[4];
		reference86 = new VertexPositionColor(new Vector3(1.3f, -550f, 0f), Color.Brown);
		ref VertexPositionColor reference87 = ref _elmVerts[5];
		reference87 = new VertexPositionColor(new Vector3(1.5f, -550f, 0f), Color.Brown);
		ref VertexPositionColor reference88 = ref _elmVerts[6];
		reference88 = new VertexPositionColor(new Vector3(1.39f, -160f, 0f), Color.LightGreen);
		ref VertexPositionColor reference89 = ref _elmVerts[7];
		reference89 = new VertexPositionColor(new Vector3(1.25f, -190f, 0f), Color.LightGreen);
		ref VertexPositionColor reference90 = ref _elmVerts[8];
		reference90 = new VertexPositionColor(new Vector3(1.185f, -250f, 0f), Color.LightGreen);
		ref VertexPositionColor reference91 = ref _elmVerts[9];
		reference91 = new VertexPositionColor(new Vector3(1.13f, -300f, 0f), Color.LightGreen);
		ref VertexPositionColor reference92 = ref _elmVerts[10];
		reference92 = new VertexPositionColor(new Vector3(1.12f, -360f, 0f), Color.LightGreen);
		ref VertexPositionColor reference93 = ref _elmVerts[11];
		reference93 = new VertexPositionColor(new Vector3(1.19f, -410f, 0f), Color.LightGreen);
		ref VertexPositionColor reference94 = ref _elmVerts[12];
		reference94 = new VertexPositionColor(new Vector3(1.28f, -420f, 0f), Color.LightGreen);
		ref VertexPositionColor reference95 = ref _elmVerts[13];
		reference95 = new VertexPositionColor(new Vector3(1.35f, -405f, 0f), Color.LightGreen);
		ref VertexPositionColor reference96 = ref _elmVerts[14];
		reference96 = new VertexPositionColor(new Vector3(1.17f, -490f, 0f), Color.LightGreen);
		ref VertexPositionColor reference97 = ref _elmVerts[15];
		reference97 = new VertexPositionColor(new Vector3(1.2f, -565f, 0f), Color.LightGreen);
		ref VertexPositionColor reference98 = ref _elmVerts[16];
		reference98 = new VertexPositionColor(new Vector3(1.3f, -650f, 0f), Color.LightGreen);
		ref VertexPositionColor reference99 = ref _elmVerts[17];
		reference99 = new VertexPositionColor(new Vector3(1.42f, -685f, 0f), Color.LightGreen);
		ref VertexPositionColor reference100 = ref _elmVerts[18];
		reference100 = new VertexPositionColor(new Vector3(1.52f, -640f, 0f), Color.LightGreen);
		ref VertexPositionColor reference101 = ref _elmVerts[19];
		reference101 = new VertexPositionColor(new Vector3(1.56f, -560f, 0f), Color.LightGreen);
		ref VertexPositionColor reference102 = ref _elmVerts[20];
		reference102 = new VertexPositionColor(new Vector3(1.6f, -390f, 0f), Color.LightGreen);
		ref VertexPositionColor reference103 = ref _elmVerts[21];
		reference103 = new VertexPositionColor(new Vector3(1.58f, -320f, 0f), Color.LightGreen);
		ref VertexPositionColor reference104 = ref _elmVerts[22];
		reference104 = new VertexPositionColor(new Vector3(1.5f, -280f, 0f), Color.LightGreen);
		ref VertexPositionColor reference105 = ref _elmVerts[23];
		reference105 = new VertexPositionColor(new Vector3(1.45f, -235f, 0f), Color.LightGreen);
		ref VertexPositionColor reference106 = ref _elmVerts[24];
		reference106 = new VertexPositionColor(new Vector3(1.41f, -190f, 0f), Color.LightGreen);
		_elmIndex = new short[44]
		{
			0, 2, 1, 3, 2, 4, 2, 5, 6, 7,
			7, 8, 8, 9, 9, 10, 10, 11, 11, 12,
			12, 13, 11, 14, 14, 15, 15, 16, 16, 17,
			17, 18, 18, 19, 19, 20, 20, 21, 21, 22,
			22, 23, 23, 24
		};
		_bushVerts = new VertexPositionColor[9];
		ref VertexPositionColor reference107 = ref _bushVerts[0];
		reference107 = new VertexPositionColor(new Vector3(1.2f, 0f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference108 = ref _bushVerts[1];
		reference108 = new VertexPositionColor(new Vector3(1.18f, -50f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference109 = ref _bushVerts[2];
		reference109 = new VertexPositionColor(new Vector3(1.24f, -20f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference110 = ref _bushVerts[3];
		reference110 = new VertexPositionColor(new Vector3(1.25f, -60f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference111 = ref _bushVerts[4];
		reference111 = new VertexPositionColor(new Vector3(1.26f, -25f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference112 = ref _bushVerts[5];
		reference112 = new VertexPositionColor(new Vector3(1.31f, -45f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference113 = ref _bushVerts[6];
		reference113 = new VertexPositionColor(new Vector3(1.29f, -20f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference114 = ref _bushVerts[7];
		reference114 = new VertexPositionColor(new Vector3(1.33f, -30f, 0f), Color.DarkGreen);
		ref VertexPositionColor reference115 = ref _bushVerts[8];
		reference115 = new VertexPositionColor(new Vector3(1.3f, 0f, 0f), Color.DarkGreen);
		_bushIndex = new short[16]
		{
			0, 1, 1, 2, 2, 3, 3, 4, 4, 5,
			5, 6, 6, 7, 7, 8
		};
		_ranGen = new Random();
		_prevDirection = 0f;
		_nextDirection = 0f;
		_angle = 0f;
		_frame = 0;
		_tunnel = false;
	}

	public void Update(GameTime gameTime)
	{
		if (Math.Abs(_angle - _nextDirection) > 0.02f)
		{
			_angle = Vector2.Hermite(new Vector2(_prevDirection, 0f), new Vector2(0f, 1f), new Vector2(_nextDirection, 0f), new Vector2(0f, -1f), (float)(_frame % 100) / 100f).X;
		}
		_roadVerts[0].Position.X = -80.00001f + _angle * 200f;
		_roadVerts[1].Position.X = 720f + _angle * 200f;
		_roadVerts[2].Position.X = 320f - _angle * 200f;
		if (_frame > 180)
		{
			if (_frame % 100 == 0)
			{
				_prevDirection = _nextDirection;
				if (_nextDirection != 0f && _ranGen.Next(2) > 0)
				{
					_nextDirection = 0f;
				}
				else if (_nextDirection == 0f && _ranGen.Next(2) == 0)
				{
					_nextDirection = (float)_ranGen.NextDouble() * 2f - 1f;
				}
				if (!_tunnel && _ranGen.Next(5) == 0)
				{
					_tunnel = true;
					for (int i = 0; i < _articleDist.Count; i++)
					{
						if (_articleDist[i] < 0)
						{
							_articleDist.RemoveAt(i);
							_articleType.RemoveAt(i);
							i--;
						}
					}
				}
				else if (_tunnel && _ranGen.Next(5) >= 0)
				{
					_tunnel = false;
				}
			}
			if (!_tunnel && _articleDist.Count < 40 && _frame % 60 == 0)
			{
				int num = _ranGen.Next(2);
				switch (_ranGen.Next(4))
				{
				case 0:
					if ((float)_ranGen.NextDouble() < 0.2f)
					{
						int num2 = _ranGen.Next(32);
						if (num2 < 8)
						{
							num2 = 8;
						}
						for (int l = 0; l < num2; l++)
						{
							num += 1 - num * 2;
							_articleDist.Add(-(l * 60));
							_articleType.Add(num);
						}
					}
					break;
				case 1:
					if ((float)_ranGen.NextDouble() < 0.4f)
					{
						int num2 = _ranGen.Next(4);
						for (int k = 0; k < num2; k++)
						{
							_articleDist.Add(-(k * 60));
							_articleType.Add(2 + num);
						}
					}
					break;
				case 2:
					if ((float)_ranGen.NextDouble() < 0.4f)
					{
						int num2 = _ranGen.Next(6);
						for (int m = 0; m < num2; m++)
						{
							_articleDist.Add(-(m * 60));
							_articleType.Add(4 + num);
						}
					}
					break;
				default:
					if ((float)_ranGen.NextDouble() < 0.5f)
					{
						int num2 = _ranGen.Next(12);
						for (int j = 0; j < num2; j++)
						{
							_articleDist.Add(-(j * 60));
							_articleType.Add(6 + _ranGen.Next(2));
						}
					}
					break;
				}
			}
		}
		else if (_frame < 180 && _frame % 60 == 0)
		{
			_articleDist.Add(-120);
			_articleType.Add(0);
			_articleDist.Add(0);
			_articleType.Add(1);
		}
		if (_frame % 20 == 0)
		{
			_articleDist.Add(0);
			_articleType.Add(8);
		}
		for (int n = 0; n < _articleDist.Count; n++)
		{
			if (_articleDist[n] >= 0)
			{
				_articleDist[n] += 1 + Math.Abs(_articleDist[n]) / 25;
			}
			else
			{
				_articleDist[n] += 4;
			}
		}
		for (int num3 = 0; num3 < _articleDist.Count; num3++)
		{
			if (_articleDist[num3] > 2000)
			{
				_articleType.RemoveAt(num3);
				_articleDist.RemoveAt(num3);
			}
		}
		if (_tunnel && _frame % 6 == 0)
		{
			_tunnelDist.Add(0);
		}
		for (int num4 = 0; num4 < _tunnelDist.Count; num4++)
		{
			_tunnelDist[num4] += 1 + _tunnelDist[num4] / 25;
		}
		for (int num5 = 0; num5 < _tunnelDist.Count; num5++)
		{
			if (_tunnelDist[num5] > 2000)
			{
				_tunnelDist.RemoveAt(num5);
			}
		}
		_frame++;
	}

	public void Draw(LineRender graphics)
	{
		graphics.DrawIndexedShape(_roadVerts, _roadIndex);
		for (int i = 0; i < _articleType.Count; i++)
		{
			if (_articleDist[i] <= 0)
			{
				continue;
			}
			VertexPositionColor[] array;
			VertexPositionColor[] array2;
			short[] indices;
			switch (_articleType[i] - _articleType[i] % 2)
			{
			case 0:
				array = _lampVerts;
				array2 = new VertexPositionColor[_lampVerts.Length];
				indices = _lampIndex;
				break;
			case 2:
				array = _pineVerts;
				array2 = new VertexPositionColor[_pineVerts.Length];
				indices = _pineIndex;
				break;
			case 4:
				array = _elmVerts;
				array2 = new VertexPositionColor[_elmVerts.Length];
				indices = _elmIndex;
				break;
			case 6:
				array = _bushVerts;
				array2 = new VertexPositionColor[_bushVerts.Length];
				indices = _bushIndex;
				break;
			case 8:
				array = new VertexPositionColor[2]
				{
					new VertexPositionColor(new Vector3(0.5f, 0f, 0f), Color.DimGray),
					new VertexPositionColor(new Vector3(0.5f, 0f, -0.2f), Color.DimGray)
				};
				array2 = new VertexPositionColor[2];
				indices = new short[2] { 0, 1 };
				break;
			default:
			{
				array = new VertexPositionColor[1]
				{
					new VertexPositionColor(Vector3.Zero, Color.Black)
				};
				array2 = new VertexPositionColor[1];
				short[] array3 = new short[2];
				indices = array3;
				break;
			}
			}
			for (int j = 0; j != array2.Length; j++)
			{
				float num = (float)(_articleDist[i] % 2000) / 2000f;
				Vector3 vector = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[0].Position, num + array[j].Position.Z * num);
				Vector3 vector2 = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[1].Position, num + array[j].Position.Z * num);
				if (_articleType[i] % 2 == 1)
				{
					array2[j].Position = Vector3.Lerp(vector2, vector, array[j].Position.X);
				}
				else
				{
					array2[j].Position = Vector3.Lerp(vector, vector2, array[j].Position.X);
				}
				array2[j].Position.Y += array[j].Position.Y * num;
				array2[j].Color = array[j].Color;
			}
			graphics.DrawIndexedShape(array2, indices);
		}
		for (int k = 0; k < _tunnelDist.Count; k++)
		{
			_tunnelVerts[0].Position = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[0].Position, (float)(_tunnelDist[k] % 2000) / 2000f);
			_tunnelVerts[1].Position = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[1].Position, (float)(_tunnelDist[k] % 2000) / 2000f);
			_tunnelVerts[2].Position = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[0].Position + new Vector3(0f, -580f, 0f), (float)(_tunnelDist[k] % 2000) / 2000f);
			_tunnelVerts[3].Position = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[1].Position + new Vector3(0f, -580f, 0f), (float)(_tunnelDist[k] % 2000) / 2000f);
			graphics.DrawIndexedShape(_tunnelVerts, _tunnelIndex);
			if (_tunnel && k == 0)
			{
				_horizonVerts[1].Position.X = _tunnelVerts[0].Position.X;
				_horizonVerts[2].Position.X = _tunnelVerts[1].Position.X;
				_horizonVerts[0].Position.X = 0f;
				_horizonVerts[3].Position.X = 640f;
			}
			if (!_tunnel && k == _tunnelDist.Count - 1)
			{
				_horizonVerts[0].Position.X = _tunnelVerts[0].Position.X;
				_horizonVerts[3].Position.X = _tunnelVerts[1].Position.X;
				_horizonVerts[1].Position.X = _roadVerts[2].Position.X;
				_horizonVerts[2].Position.X = _roadVerts[2].Position.X;
			}
		}
		if (_tunnelDist.Count == 0)
		{
			_horizonVerts[0].Position.X = 0f;
			_horizonVerts[3].Position.X = 640f;
		}
		graphics.DrawIndexedShape(_horizonVerts, _horizonIndex);
	}

	public void DrawCar(LineRender graphics, Car car)
	{
		VertexPositionColor[] array = new VertexPositionColor[_carVerts.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 value = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[0].Position, car.Position.Y + _carVerts[i].Z * 0.05f * car.Position.Y);
			Vector3 value2 = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[1].Position, car.Position.Y + _carVerts[i].Z * 0.05f * car.Position.Y);
			array[i].Position = Vector3.Lerp(value, value2, car.Position.X + _carVerts[i].X * 0.05f);
			array[i].Position.Y += _carVerts[i].Y * car.Position.Y * 30f;
			array[i].Color = car.Colour;
		}
		graphics.DrawIndexedShape(array, _carBackIndex);
		if ((car.Position.X + 0.05f) * 640f < _roadVerts[2].Position.X)
		{
			graphics.DrawIndexedShape(array, _carRightIndex);
		}
		if ((car.Position.X - 0.05f) * 640f > _roadVerts[2].Position.X)
		{
			graphics.DrawIndexedShape(array, _carLeftIndex);
		}
	}

	public void DrawDebris(LineRender graphics, Debris debris)
	{
		if (!_tunnel || (debris._position.X > 0f && debris._position.X < 1f))
		{
			VertexPositionColor[] array = new VertexPositionColor[_debrisVerts.Length];
			Matrix matrix = Matrix.CreateFromYawPitchRoll(debris._rotation.Y, debris._rotation.X, debris._rotation.Z);
			for (int i = 0; i != 3; i++)
			{
				array[i].Position = Vector3.Transform(_debrisVerts[i], matrix);
				Vector3 value = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[0].Position, debris._position.Z + array[i].Position.Z * debris._position.Z);
				Vector3 value2 = Vector3.Lerp(_roadVerts[2].Position, _roadVerts[1].Position, debris._position.Z + array[i].Position.Z * debris._position.Z);
				array[i].Position = Vector3.Lerp(value, value2, debris._position.X + array[i].Position.X);
				array[i].Position.Y = debris._position.Y + array[i].Position.Y + array[i].Position.Z * debris._position.Z;
				array[i].Color = debris._colour;
			}
			graphics.DrawIndexedShape(array, _debrisIndex);
		}
	}
}
