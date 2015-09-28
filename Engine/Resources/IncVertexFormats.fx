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
	float4 color: COLOR0;
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
    float3 tangentLocal : TANGENT;
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
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionColorI
{
    float3 positionLocal : POSITION;
    float4 color : COLOR0;
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalColorI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float4 color : COLOR0;
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionTextureI
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalTextureI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalTextureTangentI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
	float3 tangentLocal : TANGENT;
    float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
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
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionColorSkinnedI
{
    float3 positionLocal : POSITION;
    float4 color : COLOR0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
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
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionTextureSkinnedI
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
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
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalTextureTangentSkinnedI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
	float3 tangentLocal : TANGENT;
    float2 tex : TEXCOORD0;
	float3 weights : WEIGHTS;
	uint4 boneIndices : BONEINDICES;
	row_major float4x4 localTransform : localTransform;
	float textureIndex : textureIndex;
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
struct GSParticleSolid
{
	float3 positionWorld : POSITION;
	float2 sizeWorld : SIZE;
	float4 color : COLOR;
	uint type : TYPE;
};
struct GSParticleLine
{
	float3 positionWorld : POSITION;
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
	float2 tex : TEXCOORD0;
	float4 shadowHomogeneous : TEXCOORD1;
	uint primitiveID : SV_PrimitiveID;
};
struct PSParticleSolid
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 color : COLOR0;
	float2 tex : TEXCOORD0;
	uint primitiveID : SV_PrimitiveID;
};
struct PSParticleLine
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 color : COLOR0;
	float2 tex : TEXCOORD0;
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
	float4 shadowHomogeneous : TEXCOORD1;
};
struct PSVertexPositionTexture
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float2 tex : TEXCOORD0;
	float textureIndex : textureIndex;
};
struct PSVertexPositionNormalTexture
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float2 tex : TEXCOORD0;
	float4 shadowHomogeneous : TEXCOORD1;
	float textureIndex : textureIndex;
};
struct PSVertexPositionNormalTextureTangent
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
    float2 tex : TEXCOORD0;
	float4 shadowHomogeneous : TEXCOORD1;
	float textureIndex : textureIndex;
};

/*
SHADOW MAPPING
*/
struct ShadowMapOutput
{
	float4 positionHomogeneous : SV_POSITION;
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
	float textureIndex : textureIndex;
    float2 depth : TEXCOORD1;
};
struct GBufferPSOutput
{
    float4 color : SV_TARGET0;
    float4 normal : SV_TARGET1;
    float4 depth : SV_TARGET2;
};

/*
HELPER FUNCTIONS
*/
static const int MAXBONETRANSFORMS = 96;

void PopulateWeights(float3 inputWeights, out float weights[4])
{
	weights[0] = inputWeights.x;
	weights[1] = inputWeights.y;
	weights[2] = inputWeights.z;
	weights[3] = 1.0f - weights[0] - weights[1] - weights[2];
}

void ComputePositionWeights(
	float4x4 boneTransforms[MAXBONETRANSFORMS],
	float3 inputWeights, 
	uint4 inputBoneIndices, 
	float3 inputPositionLocal, 
	out float4 positionLocal)
{
	float weights[4];
	PopulateWeights(inputWeights, weights);
	
	positionLocal = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	for(int i = 0; i < 4; ++i)
	{
		positionLocal += weights[i] * mul(float4(inputPositionLocal, 1.0f), boneTransforms[inputBoneIndices[i]]);
	}

	positionLocal.w = 1.0f;
}

void ComputePositionNormalWeights(
	float4x4 boneTransforms[MAXBONETRANSFORMS],
	float3 inputWeights, 
	uint4 inputBoneIndices, 
	float3 inputPositionLocal, 
	float3 inputNormalLocal,
	out float4 positionLocal,
	out float4 normalLocal)
{
	float weights[4];
	PopulateWeights(inputWeights, weights);
	
	positionLocal = float4(0.0f, 0.0f, 0.0f, 0.0f);
	normalLocal = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	for(int i = 0; i < 4; ++i)
	{
		positionLocal += weights[i] * mul(float4(inputPositionLocal, 1.0f), boneTransforms[inputBoneIndices[i]]);
		normalLocal += weights[i] * mul(float4(inputNormalLocal, 0.0f), boneTransforms[inputBoneIndices[i]]);
	}

	positionLocal.w = 1.0f;
	normalLocal.w = 0.0f;
}

void ComputePositionNormalTangentWeights(
	float4x4 boneTransforms[MAXBONETRANSFORMS],
	float3 inputWeights, 
	uint4 inputBoneIndices, 
	float3 inputPositionLocal, 
	float3 inputNormalLocal,
	float3 inputTangentLocal,
	out float4 positionLocal,
	out float4 normalLocal,
	out float4 tangentLocal)
{
	float weights[4];
	PopulateWeights(inputWeights, weights);
	
	positionLocal = float4(0.0f, 0.0f, 0.0f, 0.0f);
	normalLocal = float4(0.0f, 0.0f, 0.0f, 0.0f);
	tangentLocal = float4(0.0f, 0.0f, 0.0f, 0.0f);

	for(int i = 0; i < 4; ++i)
	{
		positionLocal += weights[i] * mul(float4(inputPositionLocal, 1.0f), boneTransforms[inputBoneIndices[i]]);
		normalLocal += weights[i] * mul(float4(inputNormalLocal, 0.0f), boneTransforms[inputBoneIndices[i]]);
		tangentLocal += weights[i] * mul(float4(inputTangentLocal, 0.0f), boneTransforms[inputBoneIndices[i]]);
	}

	positionLocal.w = 1.0f;
	normalLocal.w = 0.0f;
	tangentLocal.w = 0.0f;
}