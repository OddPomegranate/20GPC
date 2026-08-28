// Zombie/ShadowHelper2DEffect.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect (see
// the 20GPC project's ForeverWars shader work for why this rewrite-from-
// scratch approach was necessary -- no original .fx source survived).
//
// Usage (from FiftyGames.Zombie.ShadowHelper2D.EndDrawingOccluders): this
// drives a classic 2D dynamic shadow-mapping pipeline (the well-known
// "unwrap occluders into polar coordinates, reduce to a per-angle nearest-
// distance, redraw as a lit/unlit radial mask, blur" technique popularized
// by Catalin Zima's "2D lights and shadows in XNA" tutorial). All eight
// passes, selected by technique name, run via a raw full-screen quad
// (FullscreenQuad.Render/.RenderRotated -- positions pre-transformed to
// clip space, no matrix needed, and textures are read purely from
// explicit parameters set in C# before each Apply()):
//   "ComputeDistance"          - InputTexture = the occluder silhouette
//                                 (light-centered alpha map). Outputs, per
//                                 pixel, the distance from center if this
//                                 pixel is occluded, else 1 (max/no hit).
//   "Distort"                  - InputTexture = the distance map above.
//                                 Unwraps it from Cartesian into polar
//                                 coordinates (output x = angle 0..2pi,
//                                 output y = radius 0..1).
//   "HorizontalReduction"      - InputTexture = previous iteration's
//                                 result; TextureDimensions = its texel
//                                 size. Run repeatedly, halving width each
//                                 time, taking the minimum (nearest) of
//                                 each adjacent pair -- a parallel min-
//                                 reduction collapsing the radius axis down
//                                 to "nearest occluder distance per angle".
//   "GetShadowMap"             - ShadowMapTexture = the fully-reduced
//                                 per-angle nearest-distance strip. Re-
//                                 wraps back into Cartesian space: lit
//                                 (white) where a pixel's radius is closer
//                                 than the nearest occluder at its angle,
//                                 unlit (black) otherwise -- i.e. the
//                                 visible-light silhouette around this
//                                 light source.
//   "BlurHorizontally" / "BlurVerticallyAndAttenuate" - InputTexture = the
//                                 shadow silhouette; a small separable blur
//                                 to soften the shadow edges, the vertical
//                                 pass also attenuating (fading) intensity
//                                 with radius for a soft falloff at the
//                                 light's edge.
//   "Copy"                     - Texture = the light's mask/cookie sprite
//                                 (LightMask), tinted by copyColor and
//                                 scaled by copyMultiplier, rotated to
//                                 match the light's facing direction.
//   "ApplyMask"                - Texture = the rotated mask render target;
//                                 multiplies the shadow silhouette
//                                 (ShadowMapTexture) by the mask's alpha.
//   "ApplyShadowMap"           - the final full-screen composite of the
//                                 rendered scene against the shadow
//                                 silhouette (invoked directly on
//                                 ShadowHelper2D's shared effect instance
//                                 from FiftyGames.Zombie.Zombie's main
//                                 Draw, not from ShadowHelper2D.cs itself).
// Best-effort recreation of this pipeline; original visual not recoverable
// byte-exact, but the parameter names/types match the driving C# exactly.
//
// FIX (round 6): "Copy" and "ApplyMask" were previously the only two
// passes still applied via SpriteBatch (spriteBatch.Begin(..., this
// effect)), using a separate SpriteVertexShader (MatrixTransform + COLOR0
// convention). Render-target diagnostics across the rest of this project
// already proved that pattern produces no output at all in this build,
// regardless of what the pixel shader does (see Warp.fx/FinalPassEffect.fx
// /ScreenEffect.fx) -- meaning these two passes were silently no-ops the
// whole time: "Copy" never actually copied the light's mask/cookie sprite
// into _rotatedMaskRT, so "ApplyMask" (shadow * mask.a) always multiplied
// by zero, and the masked-light pipeline degraded into a nonsensical/
// misaligned result -- the likely cause of a reported "lighting looks
// offset" bug. Converted both to the same proven-working
// Apply()+FullscreenQuad pattern as every other pass in this file:
// SpriteVertexShader/SpriteVSOutput/MatrixTransform removed entirely
// (nothing referenced them once these two passes stopped using them), and
// both pixel shaders switched to take QuadVSOutput (they never read
// input.Color in the first place, so no shader logic changed beyond the
// input struct type). The "Copy" pass's rotation (previously a
// SpriteBatch.Draw rotation parameter) is now applied by
// FullscreenQuad.RenderRotated() on the C# side instead.

float2 outputDimensions;

texture InputTexture;
sampler InputSampler = sampler_state
{
	Texture = (InputTexture);
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

float2 TextureDimensions;

texture ShadowMapTexture;
sampler ShadowMapSampler = sampler_state
{
	Texture = (ShadowMapTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

int copyMultiplier;
float4 copyColor;

// Used only by the "ApplyShadowMap" technique (invoked directly on
// ShadowHelper2D's shared effect instance from FiftyGames.Zombie.Zombie's
// main Draw, not from ShadowHelper2D.cs itself): the final full-screen
// composite of the rendered scene against the shadow silhouette.
texture InLightMap;
sampler InLightSampler = sampler_state
{
	Texture = (InLightMap);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};
// FIX (round 7, light-offset bug): Offset used to shift where
// ShadowMapSampler was read (see ApplyShadowMapPS below) -- removed. The
// shadow silhouette (_finalShadowmap, set as ShadowMapTexture) is a
// screen-space, 1280x720 render target with the camera offset ALREADY baked
// in (ShadowHelper2D.EndDrawingOccluders positions it using
// _light.Position = _offset + player.Position), landing it at exactly the
// same screen position the player sprite itself is drawn at afterward (also
// playerPos + _offset). Re-shifting the READ of that already-correctly-
// placed texture by _offset a second time displaced the shadow/light circle
// away from the player by the full camera-scroll amount -- the reported
// "flashlight and underglow not connected to the player" bug. Sampling
// ShadowMapTexture at the same unshifted input.TexCoord as InputTexture/
// InLightMap (both already screen-space aligned, neither ever shifted)
// keeps all three layers correctly registered with each other.
float BackgroundDarkness;

// FIX (round 8, "roof textures" bug): InputTexture (set from _backgroundRT,
// see Zombie.cs) is a 1900x1200 WORLD-space canvas -- _background, decals,
// and particles are all drawn into it at their raw world position with no
// camera offset applied (Zombie.cs draws them at Vector2.Zero). It was
// never intended to be sampled 0..1 directly: doing so squashes the ENTIRE
// 1900x1200 world into the 1280x720 output every frame, and -- since UV
// 0..1 always maps to the same texels regardless of camera position -- the
// background never scrolls at all. Every other layer composited here
// (_occluderMap/midground "roof" overlay, ShadowMapTexture, the player,
// InLightMap) IS drawn screen-space at _offset and correctly scrolls with
// the camera, so relative to the frozen, squashed background they appear
// to be too small and to slide around with the player -- exactly the
// reported symptoms. BackgroundOffset (set to _offset in pixels from C#)
// crops+positions the sample so it matches what every other layer already
// does: world pixel (screenPixel - offset) out of the 1900x1200 canvas.
float2 BackgroundOffset;

texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

// ---- Every pass below is a full-screen-quad pass (FullscreenQuad.Render/
// .RenderRotated, no matrix) ----

struct QuadVSOutput
{
	float4 Position : SV_Position;
	float2 TexCoord : TEXCOORD0;
};

QuadVSOutput QuadVertexShader(float4 position : POSITION0, float2 texCoord : TEXCOORD0)
{
	QuadVSOutput output;
	output.Position = float4(position.xy, 0, 1);
	output.TexCoord = texCoord;
	return output;
}

float4 ComputeDistancePS(QuadVSOutput input) : COLOR0
{
	float4 occluder = tex2D(InputSampler, input.TexCoord);
	float2 centered = input.TexCoord - 0.5;
	float dist = saturate(length(centered) * 2.0);
	float value = (occluder.a > 0.5) ? dist : 1.0;
	return float4(value, value, value, 1);
}

float4 DistortPS(QuadVSOutput input) : COLOR0
{
	// Angle on Y (height, preserved through the width-collapsing reduction
	// passes below), radius on X (width, the axis HorizontalReductionPS
	// actually collapses) -- this has to match HorizontalReductionPS's
	// reduction axis and the render-target shapes ShadowHelper2D.cs
	// allocates (width shrinks each pass, height stays fixed), or the
	// per-angle "nearest occluder" reduction merges the wrong axis and
	// the resulting shadow silhouette is effectively garbage.
	float angle = input.TexCoord.y * 6.2831853;
	float radius = input.TexCoord.x;
	float2 dir = float2(cos(angle), sin(angle));
	float2 sampleUV = saturate(0.5 + dir * radius * 0.5);
	return tex2D(InputSampler, sampleUV);
}

float4 HorizontalReductionPS(QuadVSOutput input) : COLOR0
{
	float2 texel = float2(TextureDimensions.x, 0);
	float a = tex2D(InputSampler, input.TexCoord - texel * 0.25).r;
	float b = tex2D(InputSampler, input.TexCoord + texel * 0.25).r;
	float value = min(a, b);
	return float4(value, value, value, 1);
}

float4 GetShadowMapPS(QuadVSOutput input) : COLOR0
{
	float2 centered = input.TexCoord - 0.5;
	float angle = atan2(centered.y, centered.x);
	if (angle < 0)
	{
		angle += 6.2831853;
	}
	float angleUV = angle / 6.2831853;
	float radius = saturate(length(centered) * 2.0);
	// Sample X=0.5 (the fully radius-collapsed axis), Y=angleUV (the
	// preserved per-angle axis) -- matches the angle/radius axes DistortPS
	// now writes and the width-collapsing reduction above.
	float minDist = tex2D(ShadowMapSampler, float2(0.5, angleUV)).r;
	float lit = step(radius, minDist);
	return float4(lit, lit, lit, lit);
}

float4 BlurHorizontallyPS(QuadVSOutput input) : COLOR0
{
	float2 texel = float2(1.0 / outputDimensions.x, 0);
	float4 sum = tex2D(InputSampler, input.TexCoord - texel * 2) * 0.06;
	sum += tex2D(InputSampler, input.TexCoord - texel) * 0.24;
	sum += tex2D(InputSampler, input.TexCoord) * 0.40;
	sum += tex2D(InputSampler, input.TexCoord + texel) * 0.24;
	sum += tex2D(InputSampler, input.TexCoord + texel * 2) * 0.06;
	return sum;
}

float4 BlurVerticallyAndAttenuatePS(QuadVSOutput input) : COLOR0
{
	float2 texel = float2(0, 1.0 / outputDimensions.y);
	float4 sum = tex2D(InputSampler, input.TexCoord - texel * 2) * 0.06;
	sum += tex2D(InputSampler, input.TexCoord - texel) * 0.24;
	sum += tex2D(InputSampler, input.TexCoord) * 0.40;
	sum += tex2D(InputSampler, input.TexCoord + texel) * 0.24;
	sum += tex2D(InputSampler, input.TexCoord + texel * 2) * 0.06;

	float2 centered = input.TexCoord - 0.5;
	float attenuation = saturate(1.0 - length(centered) * 2.0);
	sum *= attenuation;
	return sum;
}

// ApplyShadowMap: InputTexture = the fully-drawn background scene,
// ShadowMapTexture = the lit/unlit shadow silhouette (from GetShadowMap /
// the blur passes), InLightMap = a separately-rendered "always visible"
// layer (enemies/projectiles/etc, drawn to _aiRT -- not subject to shadow
// darkening). Unlit background pixels are darkened toward
// BackgroundDarkness; the InLightMap layer is composited on top at full
// brightness via its own alpha.
float4 ApplyShadowMapPS(QuadVSOutput input) : COLOR0
{
	float2 bgUV = (input.TexCoord * float2(1280.0, 720.0) - BackgroundOffset) / float2(1900.0, 1200.0);
	float4 background = tex2D(InputSampler, bgUV);
	float lit = tex2D(ShadowMapSampler, input.TexCoord).r;
	float4 lightLayer = tex2D(InLightSampler, input.TexCoord);

	float darkness = lerp(BackgroundDarkness, 1.0, lit);
	float4 result = background * darkness;
	result.rgb = lerp(result.rgb, lightLayer.rgb, lightLayer.a);
	result.a = 1;
	return result;
}

float4 CopyPS(QuadVSOutput input) : COLOR0
{
	float4 tex = tex2D(TextureSampler, input.TexCoord);
	return tex * copyColor * (float)copyMultiplier;
}

float4 ApplyMaskPS(QuadVSOutput input) : COLOR0
{
	float4 mask = tex2D(TextureSampler, input.TexCoord);
	float4 shadow = tex2D(ShadowMapSampler, input.TexCoord);
	return shadow * mask.a;
}

technique ApplyShadowMap
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 ApplyShadowMapPS();
	}
}

technique ComputeDistance
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 ComputeDistancePS();
	}
}

technique Distort
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 DistortPS();
	}
}

technique HorizontalReduction
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 HorizontalReductionPS();
	}
}

technique GetShadowMap
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 GetShadowMapPS();
	}
}

technique BlurHorizontally
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 BlurHorizontallyPS();
	}
}

technique BlurVerticallyAndAttenuate
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 BlurVerticallyAndAttenuatePS();
	}
}

technique Copy
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 CopyPS();
	}
}

technique ApplyMask
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 ApplyMaskPS();
	}
}
