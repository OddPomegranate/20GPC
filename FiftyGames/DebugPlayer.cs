using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames;

internal class DebugPlayer : Player
{
	public DebugPad _debugPad;

	public DebugPlayer(PlayerManager playerManager, SoundManager soundManager)
		: base(PlayerIndex.One, playerManager, soundManager)
	{
		_debugPad = new DebugPad();
		for (int i = 0; i != 4; i++)
		{
			if (GamePad.GetState((PlayerIndex)i).IsConnected)
			{
				_index = (PlayerIndex)i;
			}
		}
		_gamePad = new GamePadManager(PlayerIndex.One);
		_gamePad.Player = this;
	}
}
