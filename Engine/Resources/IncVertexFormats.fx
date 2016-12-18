static const float4x4 IDENTITY = {1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1};
static const float PI = 6.28319f;

/*
BASIC VS INPUTS
*/
struct VSVertexBillboard
{
	float3 positionWorld : POSITION;
	float2 sizeWorld : SIZE;
};
struct VSVertexCPUParticle
{
	float3 positionWorld : POSITION;
	float3 velocityWorld : VELOCITY;
	float4 random: RANDOM;
	float maxAge : MAX_AGE;
};
struct VSVertexGPUParticle
{
	float3 position : POSITION;
	float3 velocity : VELOCITY;
	float4 random: RANDOM;
	float maxAge : MAX_AGE;

	uint type : TYPE;
	float emissionTime : EMISSION_TIME;
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
    float2 tex0 : TEXCOORD0;
	float2 tex1 : TEXCOORD1;
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
	row_major float4x4 localTransform : localTransform;
    uint3 animationData : animationData;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionColorI
{
    float3 positionLocal : POSITION;
    float4 color : COLOR0;
	row_major float4x4 localTransform : localTransform;
    uint3 animationData : animationData;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalColorI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float4 color : COLOR0;
	row_major float4x4 localTransform : localTransform;
    uint3 animationData : animationData;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionTextureI
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : localTransform;
    uint3 animationData : animationData;
	float textureIndex : textureIndex;
	uint instanceId : SV_InstanceID;
};
struct VSVertexPositionNormalTextureI
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float2 tex : TEXCOORD0;
	row_major float4x4 localTransform : localTransform;
    uint3 animationData : animationData;
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
    uint3 animationData : animationData;
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
    uint3 animationData : animationData;
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
    uint3 animationData : animationData;
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
    uint3 animationData : animationData;
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
    uint3 animationData : animationData;
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
    uint3 animationData : animationData;
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
    uint3 animationData : animationData;
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
	float2 tex : TEXCOORD0;
	uint primitiveID : SV_PrimitiveID;
};
struct PSCPUParticle
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 rotationWorld : ROTATION;
	float2 tex : TEXCOORD0;
	float4 color : COLOR0;
	uint primitiveID : SV_PrimitiveID;
};
struct PSGPUParticle
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
	float textureIndex : textureIndex;
};
struct PSVertexPositionNormalTextureTangent
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
    float2 tex : TEXCOORD0;
	float textureIndex : textureIndex;
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
struct ShadowMapOutput
{
	float4 positionHomogeneous : SV_POSITION;
};
struct PSShadowMapOutput
{
	float4 positionHomogeneous : SV_POSITION;
	float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
	uint primitiveID : SV_PrimitiveID;
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
float4x4 DecodeMatrix(float3x4 m)
{
	return float4x4(
		float4(m[0].xyz, 0),
		float4(m[1].xyz, 0),
		float4(m[2].xyz, 0),
		float4(m[0].w, m[1].w, m[2].w, 1));
}

float4x4 LoadBoneMatrix(Texture2D animationTexture, uint3 animationData, uint bone, uint paletteWidth)
{
    uint baseIndex = animationData.x + animationData.y + (4*bone);
    uint baseU = baseIndex % paletteWidth;
    uint baseV = baseIndex / paletteWidth;
   
    float4 mat1 = animationTexture.Load(uint3(baseU,baseV,0));
    float4 mat2 = animationTexture.Load(uint3(baseU+1,baseV,0));
    float4 mat3 = animationTexture.Load(uint3(baseU+2,baseV,0));

    return DecodeMatrix(float3x4(mat1,mat2,mat3));
}

void PopulateWeights(float3 inputWeights, out float4 weights)
{
	weights = float4(inputWeights.x, inputWeights.y, inputWeights.z, 1.0f - inputWeights.x - inputWeights.y - inputWeights.z);
}

void ComputePositionWeights(
	Texture2D animationTexture,
	uint3 animationData,
	uint paletteWidth,
	float3 inputWeights, 
	uint4 inputBoneIndices, 
	float3 inputPositionLocal, 
	out float4 positionLocal)
{
	float4 weights;
	PopulateWeights(inputWeights, weights);
	
	float4x4 finalMatrix = IDENTITY;
	if(weights.x > 0)
	{
		finalMatrix = weights.x * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.x, paletteWidth);
		if(weights.y > 0)
		{
			finalMatrix += weights.y * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.y, paletteWidth);
			if(weights.z > 0)
			{
				finalMatrix += weights.z * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.z, paletteWidth);
				if(weights.w > 0)
				{
					finalMatrix += weights.w * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.w, paletteWidth);
				}
			}
		}
	}

	positionLocal = mul(float4(inputPositionLocal, 1.0f), finalMatrix);
}

void ComputePositionNormalWeights(
	Texture2D animationTexture,
	uint3 animationData,
	uint paletteWidth,
	float3 inputWeights, 
	uint4 inputBoneIndices, 
	float3 inputPositionLocal, 
	float3 inputNormalLocal,
	out float4 positionLocal,
	out float4 normalLocal)
{
	float4 weights;
	PopulateWeights(inputWeights, weights);
	
	float4x4 finalMatrix = IDENTITY;
	if(weights.x > 0)
	{
		finalMatrix = weights.x * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.x, paletteWidth);
		if(weights.y > 0)
		{
			finalMatrix += weights.y * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.y, paletteWidth);
			if(weights.z > 0)
			{
				finalMatrix += weights.z * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.z, paletteWidth);
				if(weights.w > 0)
				{
					finalMatrix += weights.w * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.w, paletteWidth);
				}
			}
		}
	}

	positionLocal = mul(float4(inputPositionLocal, 1.0f), finalMatrix);
	normalLocal = mul(float4(inputNormalLocal, 0.0f), finalMatrix);
}

void ComputePositionNormalTangentWeights(
	Texture2D animationTexture,
	uint3 animationData,
	uint paletteWidth,
	float3 inputWeights, 
	uint4 inputBoneIndices, 
	float3 inputPositionLocal, 
	float3 inputNormalLocal,
	float3 inputTangentLocal,
	out float4 positionLocal,
	out float4 normalLocal,
	out float4 tangentLocal)
{
	float4 weights;
	PopulateWeights(inputWeights, weights);
	
	float4x4 finalMatrix = IDENTITY;
	if(weights.x > 0)
	{
		finalMatrix = weights.x * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.x, paletteWidth);
		if(weights.y > 0)
		{
			finalMatrix += weights.y * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.y, paletteWidth);
			if(weights.z > 0)
			{
				finalMatrix += weights.z * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.z, paletteWidth);
				if(weights.w > 0)
				{
					finalMatrix += weights.w * LoadBoneMatrix(animationTexture, animationData, inputBoneIndices.w, paletteWidth);
				}
			}
		}
	}

	positionLocal = mul(float4(inputPositionLocal, 1.0f), finalMatrix);
	normalLocal = mul(float4(inputNormalLocal, 0.0f), finalMatrix);
	tangentLocal = mul(float4(inputTangentLocal, 0.0f), finalMatrix);
}