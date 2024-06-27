#ifndef __VERTEXFORMATS_INCLUDED__
#define __VERTEXFORMATS_INCLUDED__

#include "IncMaterials.hlsl"

/*
BASIC VS INPUTS
*/
struct VSVertexPositionColor
{
	float3 positionLocal : POSITION;
	float4 color : COLOR0;
};
struct VSVertexPositionNormalColor
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float4 color : COLOR0;
};
struct VSVertexPositionTexture
{
	float3 positionLocal : POSITION;
	float2 tex : TEXCOORD0;
};
struct VSVertexPositionNormalTexture
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float2 tex : TEXCOORD0;
};
struct VSVertexPositionNormalTextureTangent
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float3 tangentLocal : TANGENT;
	float2 tex : TEXCOORD0;
};

/*
SKINNED VS INPUTS
*/
struct VSVertexPositionColorSkinned
{
	float3 positionLocal : POSITION;
	float4 color : COLOR0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
};
struct VSVertexPositionNormalColorSkinned
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float4 color : COLOR0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
};
struct VSVertexPositionTextureSkinned
{
	float3 positionLocal : POSITION;
	float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
};
struct VSVertexPositionNormalTextureSkinned
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
};
struct VSVertexPositionNormalTextureTangentSkinned
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float3 tangentLocal : TANGENT;
	float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
};

/*
INSTANCING VS INPUTS
*/
struct VSVertexPositionColorI
{
	float3 positionLocal : POSITION;
	float4 color : COLOR0;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
    uint materialIndex : MATERIALINDEX;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionNormalColorI
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float4 color : COLOR0;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
    uint materialIndex : MATERIALINDEX;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionTextureI
{
	float3 positionLocal : POSITION;
	float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionNormalTextureI
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionNormalTextureTangentI
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float3 tangentLocal : TANGENT;
	float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
	uint instanceId : SV_INSTANCEID;
};

/*
SKINNED INSTANCING VS INPUTS
*/
struct VSVertexPositionColorSkinnedI
{
	float3 positionLocal : POSITION;
	float4 color : COLOR0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionNormalColorSkinnedI
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float4 color : COLOR0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionTextureSkinnedI
{
	float3 positionLocal : POSITION;
	float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionNormalTextureSkinnedI
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionNormalTextureTangentSkinnedI
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float3 tangentLocal : TANGENT;
	float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};

/*
BASIC PS INPUTS
*/
struct PSVertexPositionColor
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 color : COLOR0;
};
struct PSVertexPositionNormalColor
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float4 color : COLOR0;
	Material material;
};
struct PSVertexPositionTexture
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float2 tex : TEXCOORD0;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
};
struct PSVertexPositionNormalTexture
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float2 tex : TEXCOORD0;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
	Material material;
};
struct PSVertexPositionNormalTextureTangent
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float3 tangentWorld : TANGENT;
	float2 tex : TEXCOORD0;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
	Material material;
};

#endif
