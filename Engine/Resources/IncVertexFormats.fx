/*
BASIC VS INPUTS
*/
struct VSVertexBillboard
{
	float3 positionWorld : POSITION;
	float2 sizeWorld : SIZE;
};
struct VSVertexParticle
{
	float3 positionWorld : POSITION;
	float3 velocityWorld : VELOCITY;
	float2 sizeWorld : SIZE;
	float age : AGE;
	uint type : TYPE;
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
    float4 tangentLocal : TANGENT;
    float2 tex : TEXCOORD0;
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
    float4 tangentLocal : TANGENT;
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
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionColorI
{
    float3 positionLocal : POSITION;
    float4 color : COLOR0;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalColorI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float4 color : COLOR0;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionTextureI
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalTextureI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalTextureTangentI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
	float4 tangentLocal : TANGENT;
    float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};

/*
SKINNED INSTANCING VS INPUTS
*/
struct VSVertexPositionSkinnedI
{
	float3 positionLocal : POSITION;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionColorSkinnedI
{
    float3 positionLocal : POSITION;
    float4 color : COLOR0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalColorSkinnedI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float4 color : COLOR0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionTextureSkinnedI
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalTextureSkinnedI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalTextureTangentSkinnedI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
	float4 tangentLocal : TANGENT;
    float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	uint instanceId : SV_InstanceID;
};

/*
BASIC GS INPUTS
*/
struct GSVertexBillboard
{
	float3 centerWorld : POSITION;
	float2 sizeWorld : SIZE;
};
struct GSParticleFire
{
	float3 positionWorld : POSITION;
	float2 sizeWorld : SIZE;
	float4 color : COLOR;
	uint type : TYPE;
};
struct GSParticleRain
{
	float3 positionWorld : POSITION;
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
	float2 tex : TEXCOORD;
	uint primitiveID : SV_PrimitiveID;
};
struct PSParticleFire
{
	float4 positionHomogeneous : SV_POSITION;
	float4 color : COLOR;
	float2 tex : TEXCOORD;
	uint primitiveID : SV_PrimitiveID;
};
struct PSParticleRain
{
	float4 positionHomogeneous : SV_POSITION;
	float2 tex : TEXCOORD;
	uint primitiveID : SV_PrimitiveID;
};
struct PSVertexPosition
{
	float4 positionHomogeneous : SV_POSITION;
    float3 positionLocal : POSITION;
};
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
};
struct PSVertexPositionTexture
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float2 tex : TEXCOORD0;
};
struct PSVertexPositionNormalTexture
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float2 tex : TEXCOORD0;
};
struct PSVertexPositionNormalTextureTangent
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float4 tangentWorld : TANGENT;
    float2 tex : TEXCOORD0;
};
