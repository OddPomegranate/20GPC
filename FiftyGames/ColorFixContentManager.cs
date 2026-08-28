using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

// PORTING FIX: the original content in this project is still the precompiled Xbox
// 360 XNA .xnb files (no source art survives to rebuild from) -- see the shader and
// audio reconstruction work elsewhere in this project for the same underlying
// situation. MonoGame's own Texture2DReader performs zero channel conversion for
// uncompressed texture formats: it copies the raw bytes from the .xnb straight into
// the texture and casts whatever int32 is embedded in the file directly to
// SurfaceFormat, with no remapping (confirmed directly against MonoGame's source).
//
// The Xbox-side content compiler wrote pixel data whose 4 raw bytes do not map
// 1:1 onto RGBA the way MonoGame assumes. Two DIFFERENT mislabelings turned out to
// be present in this project, affecting two different categories of texture:
//
//   1. A narrow set of Menu/UI icons (confirmed against "StarSelected"): raw Green
//      carries no information (constant 0 across the whole texture) and raw Alpha
//      is otherwise correct as exported. Real color recovers by swapping raw Green
//      and raw Blue and leaving Red/Alpha alone.
//   2. Almost everything else that's actually corrupted (confirmed against game
//      banners, PlatformsAreFalling's block/body/floor/eye, HeliChopper's Land5,
//      the menu button icons, "StarUnselected", GiantKillerCentipede's
//      CentipedeBody, SwingGems' Diamond, and most of the Zombie sprite sheet):
//      raw Alpha does NOT hold real opacity at all -- it was overwritten with a
//      duplicate of raw Green -- and the real opacity data lives in raw RED
//      instead. Real color recovers as true R = raw Alpha, true G = raw Blue,
//      true B = raw Green, true A = raw Red.
//
// Rather than hardcode a rule per asset path (fragile -- the categories don't
// cleanly separate by folder for every one of the ~345 files in this project),
// this detects which pattern applies per-texture at load time from two signals
// and applies whichever of the two known remaps matches. If neither matches, the
// texture is left exactly as MonoGame decoded it -- this correctly no-ops on
// legitimately-uncorrupted assets, which needed no fix either way.
//
// HISTORY OF FALSE NEGATIVES (why detection took several passes to get right):
// the first working version detected category 2 by checking whether raw RED is
// constant across the texture's visible (raw Alpha > 0) pixels. That correctly
// caught flat, hard-edged art (banners, block/body -- e.g. block's Red is exactly
// 255 across 100% of its ~62.5k opaque pixels) but produced a false negative on
// almost everything else, because:
//   (a) any texture with soft/anti-aliased edges has NATURALLY-varying raw Red at
//       its boundary pixels once raw Red becomes the true alpha channel -- a
//       "constant Red" test can never pass for those, no matter the tolerance.
//   (b) some textures (e.g. "floor") have raw Alpha stuck at 0 across literally
//       every pixel, which used to make the opaque-pixel scan see zero pixels and
//       bail out to "None" before ever checking Red or Green.
// The fix: detect category 2 via a SECOND, independent signal -- whether raw
// Alpha closely tracks raw Green pixel-for-pixel (a direct fingerprint of "alpha
// was overwritten with a copy of Green"), in addition to the original
// constant-Red check; and when the whole texture has zero visible (raw Alpha > 0)
// pixels, fall back to scanning/remapping every pixel instead of bailing out.
// Confirmed via direct evidence across banners/block/body/floor/eye/Land5/
// Buttons-A/StarUnselected/CentipedeBody/Diamond/Zombie: manually reversing all
// four channels (true RGBA = raw A,B,G,R) on real sampled pixels consistently
// produces plausible real colors (a green Xbox "A" button, a gray cave-wall tile,
// a black eye pupil, a dim gray "empty" star outline) -- and is a mathematical
// no-op on textures that were already correct (e.g. "dice", whose raw channels
// are all pinned to 255 wherever visible).
//
// The remap now also uses raw Red itself as the recovered opacity value (instead
// of hardcoding full/zero opacity) so anti-aliased edges stay soft instead of
// getting a hard cutoff; this is unchanged behavior for the flat, hard-edged
// assets that were already confirmed working, since their raw Red was already
// constant at (effectively) 255 wherever visible.
//
// The whole remap is wrapped in try/catch as a safety net -- GetData<Color> throws
// (rather than corrupting data) when the element size doesn't match the underlying
// format, so a bad guess just leaves that texture untouched instead of breaking it.
//
// DIAGNOSTIC LOGGING (temporary, safe to remove once the fix is confirmed working
// in-game across the whole game list): every DISTINCT asset name is logged once
// (format, which remap mode was applied) to "ColorFixLog.txt" next to the .exe.
// Wrapped in try/catch -- diagnostics must never be able to crash the game.
//
// A THIRD signal was later added ("alphaImplausiblyLow") for assets whose raw Red
// isn't constant and whose raw Alpha doesn't track raw Green, but whose raw Red
// still reaches a solid opaque core while raw Alpha never does (Shooter's Crate/
// Ammo, FruitsInARow's GreenPlayer). After that fix shipped, a further set of
// still-"remap=None" assets was reported wrong (Menu's competition-type icons,
// Shooter's Health pickup, Sumo's wrestler/underlay sprites, several Two Track
// Tanks tank/turret detail layers). Real pixel dumps (not just aggregate stats)
// showed all of them genuinely need the same reversal remap, just via TWO
// different robustness gaps in the existing signals: (1) IsNearlyConstant's
// strict min/max was defeated by a small minority of anti-aliased edge pixels on
// otherwise rock-solid-constant raw Red (Sumo/Sprites/Sumos, TwoTrackTanks's
// TankTurretBase) -- fixed by trimming the extreme 5% off each tail via a
// histogram before checking the spread (see IsNearlyConstantTrimmed). (2) A
// FOURTH signal ("alphaLagsRedOpacity") was added for assets where raw Red
// reaches a solid opaque core across a clear majority of pixels while raw Alpha
// reaches that same core far less often -- a relative gap, unlike the third
// signal's absolute cap, so it still fires even when raw Alpha does occasionally
// reach near-255 (Menu/Sprites/Competition/Team, Shooter/Objects/Health,
// TwoTrackTanks's TankTurretBase). All four confirmed via real pixel dumps
// reversed and visually inspected (Health becomes a crisp wood crate with a
// bright red cross; TankTurretBase becomes a textured brushed-metal turret
// plate instead of a flat color blob).
//
// Wire this in everywhere a ContentManager is constructed for loading game
// textures (FiftyGames.cs's base.Content and _minigameContentManager) rather than
// touching each individual Content.Load<Texture2D> call site.
//
// Known limitation: only corrects mip level 0.
internal class ColorFixContentManager : ContentManager
{
	private readonly HashSet<Texture2D> _fixedTextures = new HashSet<Texture2D>();

	private static readonly HashSet<string> _loggedAssets = new HashSet<string>();

	private static readonly object _logLock = new object();

	private enum RemapMode
	{
		None,
		GreenBlueSwap,
		AlphaBlueGreenToRgb,
	}

	public ColorFixContentManager(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
	}

	public ColorFixContentManager(IServiceProvider serviceProvider, string rootDirectory)
		: base(serviceProvider, rootDirectory)
	{
	}

	public override T Load<T>(string assetName)
	{
		T asset = base.Load<T>(assetName);
		if (asset is Texture2D tex && !_fixedTextures.Contains(tex))
		{
			bool attempted = IsPlausibleColorFormat(tex.Format);
			RemapMode mode = attempted ? TryRemapChannels(assetName, tex) : RemapMode.None;
			LogAssetOnce(assetName, tex.Format, mode);
			_fixedTextures.Add(tex);
		}
		return asset;
	}

	// Every 4-byte-per-pixel uncompressed format that could plausibly be a
	// mislabeled RGBA/BGRA color texture. Deliberately excludes packed 16-bit
	// formats (Bgra4444/Bgra5551/Bgr565), float/HDR formats, and Rgba1010102 --
	// those are NOT simple byte reinterpretations of Color and swapping them
	// blindly could silently corrupt an already-correct texture instead of
	// throwing.
	private static bool IsPlausibleColorFormat(SurfaceFormat format)
	{
		switch (format)
		{
		case SurfaceFormat.Color:
		case SurfaceFormat.ColorSRgb:
		case SurfaceFormat.Bgra32:
		case SurfaceFormat.Bgra32SRgb:
		case SurfaceFormat.Bgr32:
		case SurfaceFormat.Bgr32SRgb:
			return true;
		default:
			return false;
		}
	}

	// Small slack for compression/export noise -- a channel doesn't need to be
	// bit-exact-constant/matching to count, just tightly clustered. Confirmed safe:
	// every genuinely-varying comparison we've sampled has a spread/mismatch far
	// larger than this (e.g. CentipedeBody's opaque Red ranges 15-255).
	private const int ChannelToleranceUnits = 4;

	private static bool IsNearlyConstant(byte min, byte max)
	{
		return max - min <= ChannelToleranceUnits;
	}

	// Round 8: IsNearlyConstant (strict min/max) turned out fragile against a small
	// minority of outlier pixels -- a handful of anti-aliased edge texels can drag
	// the observed min or max wide even when the vast majority of the channel's
	// mass is tightly clustered (confirmed on Sumo/Sprites/Sumos and
	// TwoTrackTanks/Image/TankTurretBase: raw Red's 5th-95th percentile spread is
	// exactly 0 on both -- rock solid constant near 255 for ~95% of pixels -- yet
	// the strict min/max spread was much wider, because a small number of edge
	// pixels have much lower raw Red). This trims the extreme 5% off each tail (via
	// a histogram) before checking the spread, so a minority of outliers can no
	// longer defeat an otherwise-genuine constant channel.
	private static bool IsNearlyConstantTrimmed(int[] histogram256, int totalCount, int toleranceUnits)
	{
		if (totalCount == 0)
		{
			return false;
		}
		int trimCount = totalCount / 20; // trim 5% off each tail
		int low = 0;
		int lowSeen = 0;
		while (low < 255 && lowSeen + histogram256[low] <= trimCount)
		{
			lowSeen += histogram256[low];
			low++;
		}
		int high = 255;
		int highSeen = 0;
		while (high > 0 && highSeen + histogram256[high] <= trimCount)
		{
			highSeen += histogram256[high];
			high--;
		}
		if (high < low)
		{
			return true;
		}
		return (high - low) <= toleranceUnits;
	}

	// Detects which of the two confirmed corruption patterns (if any) applies to
	// this texture, and applies the matching remap. Returns RemapMode.None (texture
	// left untouched) if neither pattern is present.
	// Round 9: two assets (Sumo/Sprites/ArmsUnderlay, TwoTrackTanks/Image/
	// TankTurretDetail) survived fix 8 still logging remap=None, and were confirmed
	// visually wrong (Sumo's arm-glow color, the tank turret's accessory/detail
	// layer). Real dumped pixel data showed why NEITHER existing signal fires for
	// either: ArmsUnderlay's raw Red and raw Alpha are both already highly
	// near-opaque (93.9% vs 90.1% -- too small a gap for alphaLagsRedOpacity, and
	// raw Red's spread is 17 units even after trimming outliers -- too wide for
	// IsNearlyConstantTrimmed's 4-unit tolerance, since this asset has genuine soft
	// anti-aliased falloff rather than a small minority of outlier edge pixels).
	// TankTurretDetail's raw Alpha never reaches near-max at all (0.0%) but its raw
	// Red only reaches near-max 38.8% of the time -- just under the 50% majority
	// alphaLagsRedOpacity requires. Both were nonetheless confirmed correct under
	// the SAME reversal formula as every other corrupted asset in the game (tested
	// via all 24 raw-channel permutations against the real dumped pixels and
	// visually inspected: ArmsUnderlay reverses into a skin-toned arm with a crisp
	// black outline matching the wrestler body; TankTurretDetail reverses into a
	// proper hazard-striped access panel and bronze pipe fittings instead of a
	// washed-out pink/red blob). Given EVERY corrupted asset found across this
	// entire 20-game project except the original StarSelected/StarUnselected pair
	// has needed this exact same reversal, and both of these are individually
	// evidence-confirmed via real pixel data (not guessed), they're force-applied
	// by name here rather than chasing a 5th speculative statistical signal that
	// risks misfiring on genuinely-untouched assets elsewhere.
	private static readonly HashSet<string> _forcedReversalAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"Sumo/Sprites/ArmsUnderlay",
		"TwoTrackTanks/Image/TankTurretDetail",
		"TwoTrackTanks\\Image\\TankTurretDetail",
	};

	private static RemapMode TryRemapChannels(string assetName, Texture2D tex)
	{
		try
		{
			bool forcedReversal = _forcedReversalAssets.Contains(assetName);
			Color[] pixels = new Color[tex.Width * tex.Height];
			tex.GetData(pixels);
			if (pixels.Length == 0)
			{
				return RemapMode.None;
			}

			// Scope the detection scan to pixels that are actually part of the visible
			// sprite (raw Alpha > 0) -- transparent padding in sprite-sheet-style
			// textures can hold arbitrary garbage that would otherwise defeat this
			// check. If NO pixel has raw Alpha > 0 (raw Alpha stuck at 0 across the
			// whole texture -- seen on flat, fully-opaque tiles whose true alpha data
			// was itself overwritten), fall back to scanning/remapping every pixel,
			// since there's no usable transparency signal to restrict to anyway.
			int opaqueCount = 0;
			for (int i = 0; i < pixels.Length; i++)
			{
				if (pixels[i].A > 0)
				{
					opaqueCount++;
				}
			}
			bool wholeImageFallback = opaqueCount == 0;

			byte rMin = 255, rMax = 0, gMin = 255, gMax = 0, aMax = 0;
			int scanCount = 0;
			int alphaMatchesGreenCount = 0;
			int[] rHist = new int[256];
			int rNearMaxCount = 0;
			int aNearMaxCount = 0;
			const int NearMaxThreshold = 255 - ChannelToleranceUnits;
			for (int i = 0; i < pixels.Length; i++)
			{
				Color p = pixels[i];
				if (!wholeImageFallback && p.A == 0)
				{
					continue;
				}
				scanCount++;
				if (p.R < rMin) rMin = p.R;
				if (p.R > rMax) rMax = p.R;
				if (p.G < gMin) gMin = p.G;
				if (p.G > gMax) gMax = p.G;
				if (p.A > aMax) aMax = p.A;
				rHist[p.R]++;
				if (p.R >= NearMaxThreshold) rNearMaxCount++;
				if (p.A >= NearMaxThreshold) aNearMaxCount++;
				if (Math.Abs(p.A - p.G) <= ChannelToleranceUnits)
				{
					alphaMatchesGreenCount++;
				}
			}
			if (scanCount == 0)
			{
				return RemapMode.None;
			}

			bool redIsConstant = IsNearlyConstantTrimmed(rHist, scanCount, ChannelToleranceUnits);
			bool greenIsConstant = IsNearlyConstant(gMin, gMax);
			// A clear majority of visible pixels having raw Alpha within tolerance of
			// raw Green is the fingerprint of "alpha was overwritten with a copy of
			// Green" -- confirmed at or near 100% on every known-corrupted texture we
			// sampled (CentipedeBody 1871/1871, Diamond 5955/5955, Zombie 20050/36012)
			// and at 0% on the one texture that genuinely needs the OTHER remap
			// instead (StarSelected, 0/342).
			bool alphaTracksGreen = alphaMatchesGreenCount >= scanCount / 2;
			// Third signal: raw Red reaches a solid, fully-opaque-looking core (>=~251)
			// somewhere in the visible region, while raw Alpha -- what MonoGame is
			// CURRENTLY using as real opacity, untouched -- never gets anywhere close to
			// 255. A genuine, uncorrupted alpha channel almost always has some solidly
			// opaque interior; one that's capped well below 255 everywhere, on a
			// texture whose (true-alpha) raw Red DOES reach a solid core, is the
			// fingerprint of this same corruption on an asset with neither a constant
			// raw Red nor an alpha-tracks-green majority (confirmed on Shooter's Crate,
			// Ammo, and FruitsInARow's GreenPlayer -- reversing all three produces a
			// plausible opaque core plus, for GreenPlayer specifically, a clearly
			// green-dominant true color). Deliberately requires BOTH halves: an asset
			// that's genuinely meant to render translucent throughout (a soft glow or
			// smoke particle, say) would have its true alpha -- raw Red -- ALSO capped
			// low, so it correctly will not trip this condition.
			bool alphaImplausiblyLow = rMax >= (255 - ChannelToleranceUnits) && aMax <= 200;
			// Fourth signal (round 8): raw Red reaches a solid, fully-opaque-looking
			// core across a clear MAJORITY of visible pixels (>=50%), while raw Alpha
			// -- what MonoGame is currently using as real opacity -- reaches that same
			// near-255 core meaningfully less often (a >=30 percentage-point gap). A
			// genuinely correct alpha channel on a mostly-solid sprite (an icon, a
			// pickup, a vehicle panel) would reach full opacity about as often as its
			// true color reaches a solid core; a wide, one-sided gap is the sign raw
			// Alpha isn't real opacity at all here. Confirmed via real pixel dumps
			// (Menu/Sprites/Competition/Team: R>=251 63.5% vs A>=251 11.1%; Shooter/
			// Objects/Health: 76.2% vs 14.1%; TwoTrackTanks/Image/TankTurretBase:
			// 97.2% vs 0.0% -- all three visually confirmed correct once reversed,
			// e.g. Health becomes a crisp wood crate with a bright red cross).
			// Deliberately a RELATIVE gap rather than an absolute cap on Alpha (unlike
			// alphaImplausiblyLow above) so it still fires even when Alpha occasionally
			// does reach near-255 (Sumo/Sprites/Sumos hits 84.7% -- close enough to its
			// own Red's 97.6% that this signal correctly stays quiet there; that asset
			// is instead caught by the trimmed redIsConstant above).
			bool alphaLagsRedOpacity = scanCount > 0
				&& (double)rNearMaxCount / scanCount >= 0.5
				&& (double)rNearMaxCount / scanCount - (double)aNearMaxCount / scanCount >= 0.3;

			if (redIsConstant || alphaTracksGreen || alphaImplausiblyLow || alphaLagsRedOpacity || forcedReversal)
			{
				// Category 2 -- true color comes from raw Alpha/Blue/Green, true
				// opacity comes from raw Red itself. Apply this UNCONDITIONALLY to
				// every pixel, with no raw-Alpha-based mask: raw Alpha in this
				// category is really the TRUE RED channel, which is a legitimate
				// color component that is very often exactly 0 for perfectly
				// ordinary, fully-VISIBLE content -- pure black (true R=0), pure
				// green/cyan grass or foliage (true R=0), etc. A mask that reads
				// "raw Alpha == 0 -> treat as transparent padding" cannot tell that
				// case apart from real padding, and was silently erasing every
				// legitimately-black or low-Red pixel in the image (confirmed: this
				// is exactly why a shared black-outline character sprite lost its
				// outline, why brick sprites lost their black background, why pure
				// green grass had holes in it, and a likely cause of some
				// near-solid-black backgrounds rendering as an apparently blank/
				// black screen). Raw Red IS the true opacity value directly -- where
				// it's genuinely 0 the pixel is genuinely fully transparent, and no
				// separate gate is needed on top of that.
				for (int i = 0; i < pixels.Length; i++)
				{
					Color p = pixels[i];
					pixels[i] = new Color((int)p.A, (int)p.B, (int)p.G, (int)p.R);
				}
				tex.SetData(pixels);
				return RemapMode.AlphaBlueGreenToRgb;
			}

			if (greenIsConstant)
			{
				// Category 1 -- raw Green carries no data, raw Alpha is already
				// correct as exported.
				for (int i = 0; i < pixels.Length; i++)
				{
					Color p = pixels[i];
					pixels[i] = new Color((int)p.R, (int)p.B, (int)p.G, (int)p.A);
				}
				tex.SetData(pixels);
				return RemapMode.GreenBlueSwap;
			}

			return RemapMode.None;
		}
		catch
		{
			// GetData/SetData<Color> didn't like this format after all (size
			// mismatch) -- leave the texture exactly as MonoGame decoded it.
			return RemapMode.None;
		}
	}

	private static void LogAssetOnce(string assetName, SurfaceFormat format, RemapMode mode)
	{
		lock (_logLock)
		{
			if (!_loggedAssets.Add(assetName))
			{
				return;
			}
		}
		try
		{
			string line = string.Format("asset={0} format={1} remap={2}{3}", assetName, format, mode, Environment.NewLine);
			File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "ColorFixLog.txt"), line);
		}
		catch
		{
			// Logging must never be able to crash the game.
		}
	}
}
