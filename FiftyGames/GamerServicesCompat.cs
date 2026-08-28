using System;
using System.Text;
using Microsoft.Xna.Framework;

// PORTING NOTE: The original Xbox 360 XNA build of this game used
// Microsoft.Xna.Framework.GamerServices (Xbox Live sign-in, gamertags,
// purchase privileges, the on-screen "Guide" keyboard/marketplace/sign-in
// UI). None of this exists in MonoGame - it was an Xbox-only API. Rather
// than rip these calls out of every game-facing file (Menu, ConnectPanel,
// GameConsole, Player, PlayerManager, FiftyGames all reference it), this
// file re-creates just enough of the same namespace/types for the rest of
// the project to keep compiling unchanged, with PC-appropriate behaviour:
//   - Every controller slot is always "signed in" as a local profile
//     named "Player N", so profile save/load works without any sign-in
//     step.
//   - There is no trial mode and no purchase restriction - everything is
//     unlocked.
//   - The on-screen keyboard is replaced with a real (if basic) text
//     capture driven by the game window's TextInput event.
namespace Microsoft.Xna.Framework.GamerServices;

public sealed class GamerPrivileges
{
	// No Xbox Live marketplace on PC - nothing is ever gated behind a
	// purchase, so this is always true.
	public bool AllowPurchaseContent => true;
}

public class Gamer
{
	public string Gamertag { get; internal set; } = string.Empty;

	public PlayerIndex PlayerIndex { get; internal set; }

	public GamerPrivileges Privileges { get; } = new GamerPrivileges();

	public sealed class GamerCollection
	{
		private readonly Gamer[] _gamers;

		internal GamerCollection(Gamer[] gamers)
		{
			_gamers = gamers;
		}

		public Gamer this[PlayerIndex index] => _gamers[(int)index];
	}

	private static readonly Gamer[] _slots =
	{
		new SignedInGamer { PlayerIndex = PlayerIndex.One, Gamertag = "Player 1" },
		new SignedInGamer { PlayerIndex = PlayerIndex.Two, Gamertag = "Player 2" },
		new SignedInGamer { PlayerIndex = PlayerIndex.Three, Gamertag = "Player 3" },
		new SignedInGamer { PlayerIndex = PlayerIndex.Four, Gamertag = "Player 4" }
	};

	// On PC every player slot is always considered "signed in" locally -
	// there is no Xbox Live account to attach, so this never returns null.
	public static readonly GamerCollection SignedInGamers = new GamerCollection(_slots);
}

public class SignedInGamer : Gamer
{
	// Nobody ever signs in or out on PC (there is no Guide sign-in UI
	// backing this), so these events are never raised, but they need to
	// exist so existing += / -= subscriptions still compile.
	public static event EventHandler<SignedInEventArgs> SignedIn;

	public static event EventHandler<SignedOutEventArgs> SignedOut;
}

public class SignedInEventArgs : EventArgs
{
	public SignedInGamer Gamer { get; init; }
}

public class SignedOutEventArgs : EventArgs
{
	public SignedInGamer Gamer { get; init; }
}

public sealed class GamerServicesComponent : GameComponent
{
	public GamerServicesComponent(Game game)
		: base(game)
	{
		Guide.AttachWindow(game.Window);
	}
}

internal sealed class KeyboardInputAsyncResult : IAsyncResult
{
	internal string ResultText;

	internal volatile bool Completed;

	public object AsyncState { get; internal set; }

	public System.Threading.WaitHandle AsyncWaitHandle => throw new NotSupportedException("Polling via IsCompleted only.");

	public bool CompletedSynchronously => false;

	public bool IsCompleted => Completed;
}

// Minimal PC replacement for the Xbox 360 "Guide" overlay. Only the
// keyboard-input and trial/marketplace/sign-in surface actually used
// elsewhere in this project is implemented.
public static class Guide
{
	private static GameWindow _window;

	private static KeyboardInputAsyncResult _active;

	private static StringBuilder _buffer;

	private static AsyncCallback _callback;

	// There is no trial/demo build on PC - everything is unlocked.
	public static bool IsTrialMode => false;

	public static bool IsVisible => _active != null && !_active.IsCompleted;

	internal static void AttachWindow(GameWindow window)
	{
		if (_window != null || window == null)
		{
			return;
		}
		_window = window;
		_window.TextInput += OnTextInput;
	}

	private static void OnTextInput(object sender, TextInputEventArgs e)
	{
		if (_active == null || _active.IsCompleted)
		{
			return;
		}
		char character = e.Character;
		if (character == '\r' || character == '\n')
		{
			_active.ResultText = _buffer.ToString();
			_active.Completed = true;
			_callback?.Invoke(_active);
		}
		else if (character == '\b')
		{
			if (_buffer.Length > 0)
			{
				_buffer.Length--;
			}
		}
		else if (!char.IsControl(character))
		{
			_buffer.Append(character);
		}
	}

	public static IAsyncResult BeginShowKeyboardInput(PlayerIndex player, string title, string description, string defaultText, AsyncCallback callback, object state)
	{
		_buffer = new StringBuilder(defaultText ?? string.Empty);
		_callback = callback;
		_active = new KeyboardInputAsyncResult { AsyncState = state };
		return _active;
	}

	public static string EndShowKeyboardInput(IAsyncResult result)
	{
		if (result is KeyboardInputAsyncResult keyboardInputAsyncResult)
		{
			if (_active == keyboardInputAsyncResult)
			{
				_active = null;
			}
			return keyboardInputAsyncResult.ResultText;
		}
		return null;
	}

	// No Xbox Live on PC - both are no-ops.
	public static void ShowSignIn(int panes, bool onlineOnly)
	{
	}

	public static void ShowMarketplace(PlayerIndex player)
	{
	}
}
