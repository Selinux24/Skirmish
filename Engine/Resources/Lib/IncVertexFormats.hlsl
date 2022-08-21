#ifndef __VERTEXFORMATS_INCLUDED__
#define __VERTEXFORMATS_INCLUDED__

/*
BASIC VS INPUTS
*/
struct VSVertexBillboard
{
	float3 positionWorld : POSITION;
	float2 sizeWorld : SIZE;
};
struct VSVertexDecal
{
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float2 sizeWorld : SIZE;
	float startTime : START_TIME;
	float maxAge : MAX_AGE;
};
struct VSVertexCPUParticle
{
	float3 positionWorld : POSITION;
	float3 velocityWorld : VELOCITY;
	float4 random : RANDOM;
	float maxAge : MAX_AGE;
};
struct VSVertexGPUParticle
{
	float3 position : POSITION;
	float3 velocity : VELOCITY;
	float4 random : RANDOM;
	float maxAge : MAX_AGE;

	uint type : TYPE;
	float emissionTime : EMISSION_TIME;
};
struct VSVertexFont
{
	float3 positionLocal : POSITION;
	float2 tex : TEXCOORD0;
	float4 color : COLOR0;
};
struct VSVertexPosition
{
	float3 positionLocal : POSITION;
};
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
struct VSVertexTerrain
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float3 tangentLocal : TANGENT;
	float2 tex : TEXCOORD0;
	float4 color : COLOR0;
};

/*
SKINNED VS INPUTS
*/
struct VSVertexPositionSkinned
{
	float3 positionLocal : POSITION;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
};
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
struct VSVertexPositionI
{
	float3 positionLocal : POSITION;
	row_major float4x4 localTransform : LOCALTRANSFORM;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
	int materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionColorI
{
	float3 positionLocal : POSITION;
	float4 color : COLOR0;
	row_major float4x4 localTransform : LOCALTRANSFORM;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    int materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionNormalColorI
{
	float3 positionLocal : POSITION;
	float3 normalLocal : NORMAL;
	float4 color : COLOR0;
	row_major float4x4 localTransform : LOCALTRANSFORM;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    int materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionTextureI
{
	float3 positionLocal : POSITION;
	float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : LOCALTRANSFORM;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    int materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
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
    int materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
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
    int materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};

/*
SKINNED INSTANCING VS INPUTS
*/
struct VSVertexPositionSkinnedI
{
	float3 positionLocal : POSITION;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : LOCALTRANSFORM;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    int materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};
struct VSVertexPositionColorSkinnedI
{
	float3 positionLocal : POSITION;
	float4 color : COLOR0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : LOCALTRANSFORM;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
    int materialIndex : MATERIALINDEX;
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
    int materialIndex : MATERIALINDEX;
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
    int materialIndex : MATERIALINDEX;
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
    int materialIndex : MATERIALINDEX;
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
    int materialIndex : MATERIALINDEX;
	uint animationOffset : ANIMATIONOFFSET;
	uint animationOffsetB : ANIMATIONOFFSETB;
	float animationInterpolation : ANIMATIONINTERPOLATION;
	uint instanceId : SV_INSTANCEID;
};

/*
BASIC GS INPUTS
*/
struct GSVertexBillboard
{
	float3 centerWorld : POSITION;
	float2 sizeWorld : SIZE;
};
struct GSDecal
{
	float3 centerWorld : POSITION;
	float3 normalWorld : NORMAL;
	float4 rotationWorld : ROTATION;
	float2 sizeWorld : SIZE;
	float alpha : ALPHA;
};
struct GSCPUParticle
{
	float3 centerWorld : POSITION;
	float2 sizeWorld : SIZE;
	float4 color : COLOR;
	float4 rotationWorld : ROTATION;
};
struct GSGPUParticle
{
	float3 positionWorld : POSITION;
	float2 sizeWorld : SIZE;
	float4 color : COLOR;
	uint type : TYPE;
};

/*
BASIC PS INPUTS
*/
struct PSVertexBillboard
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float3 tangentWorld : TANGENT;
	float2 tex : TEXCOORD0;
	uint primitiveID : SV_PRIMITIVEID;
};
struct PSDecal
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 rotationWorld : ROTATION;
	float alpha : ALPHA;
	float2 tex : TEXCOORD0;
	uint primitiveID : SV_PRIMITIVEID;
};
struct PSCPUParticle
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 rotationWorld : ROTATION;
	float2 tex : TEXCOORD0;
	float4 color : COLOR0;
	uint primitiveID : SV_PRIMITIVEID;
};
struct PSGPUParticle
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 color : COLOR0;
	float2 tex : TEXCOORD0;
	uint primitiveID : SV_PRIMITIVEID;
};
struct PSVertexFont
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float2 tex : TEXCOORD0;
	float4 color : COLOR0;
};
struct PSVertexPosition
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
};
struct PSVertexPositionColor
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 color : COLOR0;
	uint materialIndex : MATERIALINDEX;
};
struct PSVertexPositionNormalColor
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float4 color : COLOR0;
	uint materialIndex : MATERIALINDEX;
};
struct PSVertexPositionTexture
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float2 tex : TEXCOORD0;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
	uint materialIndex : MATERIALINDEX;
};
struct PSVertexPositionNormalTexture
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float2 tex : TEXCOORD0;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
	uint materialIndex : MATERIALINDEX;
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
	uint materialIndex : MATERIALINDEX;
};
struct PSVertexTerrain
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float3 tangentWorld : TANGENT;
	float2 tex0 : TEXCOORD0;
	float2 tex1 : TEXCOORD1;
	float4 color : COLOR0;
};
struct PSVertexSkyScattering
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 direction : DIRECTION;
	float4 colorR : COLOR0;
	float4 colorM : COLOR1;
};

/*
SHADOW MAPPING
*/
struct PSShadowMapPosition
{
	float4 positionHomogeneous : SV_POSITION;
};
struct PSShadowMapPositionTexture
{
	float4 positionHomogeneous : SV_POSITION;
	float4 depth : TEXCOORD0;
	float2 tex : TEXCOORD1;
	uint textureIndex : TEXTUREINDEX;
};
struct PSShadowMapBillboard
{
	float4 positionHomogeneous : SV_POSITION;
	float4 depth : TEXCOORD0;
	float2 tex : TEXCOORD1;
	uint primitiveID : SV_PRIMITIVEID;
};

/*
DEFERRED LIGHTNING
*/
struct GBufferVSColorOutput
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float4 color : COLOR0;
	float2 depth : TEXCOORD0;
};
struct GBufferVSTextureOutput
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float2 tex : TEXCOORD0;
	float2 depth : TEXCOORD1;
	uint textureIndex : TEXTUREINDEX;
};
struct GBufferPSOutput
{
	float4 color : SV_TARGET0;
	float4 normal : SV_TARGET1;
	float4 depth : SV_TARGET2;
};

#endif
