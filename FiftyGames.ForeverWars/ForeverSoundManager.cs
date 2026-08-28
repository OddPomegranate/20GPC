using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace FiftyGames.ForeverWars;

internal static class ForeverSoundManager
{
	private static SoundManager soundManagerRef;

	private static Cue rocketInFlightCue;

	private static void initaliseManager()
	{
		soundManagerRef = ForeverHelper.soundManager;
	}

	public static void checkforRocketsInFlight(List<eBullet> eBulletList)
	{
		bool flag = false;
		foreach (eBullet eBullet in eBulletList)
		{
			if (eBullet.getTypeOfBullet() == typeOfEnemyBullet.Rocket)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (rocketInFlightCue == null)
			{
				rocketInFlightCue = ForeverHelper.soundManager.CreateGameSoundCue("geometryWars RocketNoise");
				rocketInFlightCue.Play();
			}
			else if (!rocketInFlightCue.IsPlaying)
			{
				rocketInFlightCue = ForeverHelper.soundManager.CreateGameSoundCue("geometryWars RocketNoise");
				rocketInFlightCue.Play();
			}
		}
		else if (rocketInFlightCue != null && rocketInFlightCue.IsPlaying)
		{
			rocketInFlightCue.Stop(AudioStopOptions.AsAuthored);
		}
	}
}
