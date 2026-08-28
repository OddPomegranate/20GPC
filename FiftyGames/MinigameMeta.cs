using System;

namespace FiftyGames;

public class MinigameMeta
{
	public enum SortMode
	{
		Unsorted,
		Rating,
		Name,
		MinPlayers,
		MaxPlayers,
		Genre,
		Competition
	}

	private short _minigameID;

	private Type _type;

	private string _name;

	private string _description;

	private string _image;

	private byte _minPlayers;

	private byte _maxPlayers;

	private GameGenre _genre;

	private GameCompetition _competition;

	private string _instructionImage;

	private string _songName;

	private string _scoreUnit;

	private byte _rating;

	private string _bestWinner;

	private float _bestScore;

	public short MinigameID => _minigameID;

	public Type Type => _type;

	public string Name => _name;

	public string Description => _description;

	public string Image => _image;

	public byte MinimumPlayers => _minPlayers;

	public byte MaximumPlayers => _maxPlayers;

	public GameGenre Genre => _genre;

	public GameCompetition Competition => _competition;

	public string InstructionImage => _instructionImage;

	public string SongName => _songName;

	public string ScoreUnit => _scoreUnit;

	public byte Rating
	{
		get
		{
			return _rating;
		}
		set
		{
			_rating = value;
		}
	}

	public float BestScore
	{
		get
		{
			return _bestScore;
		}
		set
		{
			_bestScore = value;
		}
	}

	public string BestWinner
	{
		get
		{
			return _bestWinner;
		}
		set
		{
			_bestWinner = value;
		}
	}

	public MinigameMeta(short minigameID, Type type, string name, string description, string image, byte minimumPlayers, byte maximumPlayers, GameGenre genre, GameCompetition competition, string songName, string scoreUnit, string instructions)
	{
		_minigameID = minigameID;
		_type = type;
		_name = name;
		_description = description;
		_image = image;
		_minPlayers = minimumPlayers;
		_maxPlayers = maximumPlayers;
		_genre = genre;
		_competition = competition;
		_songName = songName;
		_scoreUnit = scoreUnit;
		_instructionImage = instructions;
		_rating = 0;
		_bestScore = 0f;
		_bestWinner = "";
	}

	public MinigameMeta(MinigameMeta existingMinigame)
	{
		_minigameID = existingMinigame._minigameID;
		_type = existingMinigame._type;
		_name = existingMinigame._name;
		_description = existingMinigame._description;
		_image = existingMinigame._image;
		_minPlayers = existingMinigame._minPlayers;
		_maxPlayers = existingMinigame._maxPlayers;
		_genre = existingMinigame._genre;
		_competition = existingMinigame._competition;
		_songName = existingMinigame._songName;
		_scoreUnit = existingMinigame._scoreUnit;
		_instructionImage = existingMinigame._instructionImage;
		_rating = existingMinigame._rating;
		_bestScore = existingMinigame._bestScore;
		_bestWinner = existingMinigame._bestWinner;
	}

	public void SetScore(string playerNames, float score)
	{
		_bestWinner = playerNames;
		_bestScore = score;
	}

	public static void Sort(ref MinigameMeta[] minigameList, SortMode sortMode)
	{
		switch (sortMode)
		{
		case SortMode.Unsorted:
			Array.Sort(minigameList, (MinigameMeta minigame1, MinigameMeta minigame2) => minigame1._minigameID.CompareTo(minigame2._minigameID));
			break;
		case SortMode.Rating:
			Array.Sort(minigameList, (MinigameMeta minigame1, MinigameMeta minigame2) => minigame2._rating.CompareTo(minigame1._rating));
			break;
		case SortMode.Name:
			Array.Sort(minigameList, (MinigameMeta minigame1, MinigameMeta minigame2) => minigame1._name.CompareTo(minigame2._name));
			break;
		case SortMode.MinPlayers:
			Array.Sort(minigameList, (MinigameMeta minigame1, MinigameMeta minigame2) => minigame1._minPlayers.CompareTo(minigame2._minPlayers));
			break;
		case SortMode.MaxPlayers:
			Array.Sort(minigameList, (MinigameMeta minigame1, MinigameMeta minigame2) => minigame1._maxPlayers.CompareTo(minigame2._maxPlayers));
			break;
		case SortMode.Genre:
			Array.Sort(minigameList, (MinigameMeta minigame1, MinigameMeta minigame2) => minigame1._genre.CompareTo(minigame2._genre));
			break;
		case SortMode.Competition:
			Array.Sort(minigameList, (MinigameMeta minigame1, MinigameMeta minigame2) => minigame1._competition.CompareTo(minigame2._competition));
			break;
		}
	}
}
