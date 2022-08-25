#ifndef __HELPERS_INCLUDED__
#define __HELPERS_INCLUDED__

#ifndef FLOAT_ZEROTOLERANCE
#define FLOAT_ZEROTOLERANCE 1E-06f
#endif

#ifndef FLOAT_MAX
#define FLOAT_MAX 3.402823466e+38F
#endif

#ifndef PI
#define PI 3.14159265
#endif

#ifndef TWO_PI
#define TWO_PI 6.28318530
#endif

inline float2 EncodeColor(float3 rgb24)
{
	// scale up to 8-bit
	rgb24 *= 255.0f;

	// remove the 3 LSB of red and blue, and the 2 LSB of green
	int3 rgb16 = rgb24 / int3(8, 4, 8);

	// split the green at bit 3 (we'll keep the 6 bits around the split)
	float greenSplit = rgb16.g / 8.0f;

	// pack it up (capital G's are MSB, the rest are LSB)
	float2 packed;
	packed.x = rgb16.r * 8 + floor(greenSplit); // rrrrrGGG
	packed.y = frac(greenSplit) * 256 + rgb16.b; // gggbbbbb

	// scale down and return
	packed /= 255.0f;

	return packed;
}
inline float3 DecodeColor(float2 packed)
{
	// scale up to 8-bit
	packed *= 255.0f;

	// round and split the packed bits
	float2 split = round(packed) / 8; // first component at bit 3
	split.y /= 4; // second component at bit 5

	// unpack (obfuscated yet optimized crap follows)
	float3 rgb16 = 0.0f.rrr;
	rgb16.gb = frac(split) * 256;
	rgb16.rg += floor(split) * 4;
	rgb16.r *= 2;

	// scale down and return
	rgb16 /= 255.0f;

	return rgb16;
}

inline float4 HDR(float4 color, float exposure)
{
	float4 hdrColor = float4(color.rgb * exposure, color.a);

	hdrColor.r = hdrColor.r < 1.413f ? pow(abs(hdrColor.r) * 0.38317f, 1.0f / 2.2f) : 1.0f - exp(-hdrColor.r);
	hdrColor.g = hdrColor.g < 1.413f ? pow(abs(hdrColor.g) * 0.38317f, 1.0f / 2.2f) : 1.0f - exp(-hdrColor.g);
	hdrColor.b = hdrColor.b < 1.413f ? pow(abs(hdrColor.b) * 0.38317f, 1.0f / 2.2f) : 1.0f - exp(-hdrColor.b);

	return hdrColor;
}

inline float roll(float rnd, float min, float max)
{
	return min + (rnd * (max - min));
}

static const float2 BillboardTexCoords[8] =
{
	float2(0.0f, 1.0f),
	float2(0.0f, 0.0f),
	float2(1.0f, 1.0f),
	float2(1.0f, 0.0f),
	float2(1.0f, 1.0f),
	float2(1.0f, 0.0f),
	float2(0.0f, 1.0f),
	float2(0.0f, 0.0f)
};

void BuildQuad(float3 centerWorld, float halfWidth, float halfHeight, float3 up, float3 right, float3 displacement, inout float4 vertices[4])
{
	vertices[0] = float4(centerWorld + halfWidth * right - halfHeight * up, 1.0f) + float4(displacement, 0.0f);
	vertices[1] = float4(centerWorld + halfWidth * right + halfHeight * up, 1.0f) + float4(displacement, 0.0f);
	vertices[2] = float4(centerWorld - halfWidth * right - halfHeight * up, 1.0f) + float4(displacement, 0.0f);
	vertices[3] = float4(centerWorld - halfWidth * right + halfHeight * up, 1.0f) + float4(displacement, 0.0f);
}

float3 CalcWindTranslation(float totalTime, float random, float3 pos, float3 windDirection, float windStrength)
{
	float3 vWind = sin(totalTime + (pos.x + pos.y + pos.z) * 0.1f) + (windDirection * windStrength);

	return pos + (vWind * min(1, random));
}

float Hash(float2 p)
{
	float h = dot(p, float2(127.1, 311.7));
	return frac(sin(h) * 43758.5453123);
}
float Noise(in float2 p)
{
	float2 i = floor(p);
	float2 f = frac(p);
	float2 u = f * f * (3.0 - 2.0 * f);
	return -1.0 + 2.0 * lerp(lerp(Hash(i + float2(0.0, 0.0)), Hash(i + float2(1.0, 0.0)), u.x), lerp(Hash(i + float2(0.0, 1.0)), Hash(i + float2(1.0, 1.0)), u.x), u.y);
}

inline float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 normalW, float3 tangentW)
{
	//Uncompress each component from [0,1] to [-1,1].
	float3 normalT = (2.0f * normalMapSample) - 1.0f;

	float3 binormalW = cross(normalW, tangentW);

	return normalize((normalT.x * tangentW) + (normalT.y * binormalW) + (normalT.z * normalW));
}

inline float2 MapUVToScreenPixel(float2 uv, float2 screen)
{
	float2 p = 0.5 * float2(uv.x, -uv.y) + 0.5f;

	return float2(screen.x * p.x, screen.y * p.y);
}

inline bool PixelIntoRectangle(float2 pixel, float4 rectangle)
{
	return (
		pixel.x >= rectangle.x &&
		pixel.x <= rectangle.x + rectangle.z &&
		pixel.y >= rectangle.y &&
		pixel.y <= rectangle.y + rectangle.w);
}

#endif
