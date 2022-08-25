#ifndef __ANIMATION_INCLUDED__
#define __ANIMATION_INCLUDED__

#include "IncMatrix.hlsl"
#include "IncQuaternion.hlsl"

float4x4 DecodeMatrix(float3x4 m)
{
    float4x4 result = float4x4(
		float4(m[0].x, m[1].x, m[2].x, m[0].w),
		float4(m[0].y, m[1].y, m[2].y, m[1].w),
		float4(m[0].z, m[1].z, m[2].z, m[2].w),
		float4(0, 0, 0, 1));
    
    return result;
}

float4x4 LoadBoneMatrix(Texture2D animationTexture, uint animationOffset, uint bone, uint paletteWidth)
{
    uint baseIndex = animationOffset + (4 * bone);
    uint baseU = baseIndex % paletteWidth;
    uint baseV = baseIndex / paletteWidth;
   
    float4 mat1 = animationTexture.Load(uint3(baseU, baseV, 0));
    float4 mat2 = animationTexture.Load(uint3(baseU + 1, baseV, 0));
    float4 mat3 = animationTexture.Load(uint3(baseU + 2, baseV, 0));

    return DecodeMatrix(float3x4(mat1, mat2, mat3));
}

float4 PopulateWeights(float3 inputWeights)
{
    return float4(inputWeights.x, inputWeights.y, inputWeights.z, 1.0f - inputWeights.x - inputWeights.y - inputWeights.z);
}

float4x4 LerpByComponents(float4x4 trn1, float4x4 trn2, float amount)
{
    //Fix: row major to column major issue
    if (amount == 0)
    {
        return transpose(trn1);
    }
    
    if (amount == 1)
    {
        return transpose(trn2);
    }
    
    float3 position1;
    float4x4 rotation1;
    float3 scale1;
    decompose2(trn1, position1, rotation1, scale1);
     
    float3 position2;
    float4x4 rotation2;
    float3 scale2;
    decompose2(trn2, position2, rotation2, scale2);
   
    return compose2(
        lerp(position1, position2, amount),
        lerp(rotation1, rotation2, amount),
        lerp(scale1, scale2, amount));
}

float4x4 ComputeAnimationTransform(
	Texture2D animationTexture,
	uint animationOffset,
	uint paletteWidth,
	float3 inputWeights,
	uint4 inputBoneIndices)
{
    float4 weights = PopulateWeights(inputWeights);
	
    float4x4 finalMatrix = IDENTITY_MATRIX;
    if (weights.x > 0)
    {
        finalMatrix = mul(weights.x, LoadBoneMatrix(animationTexture, animationOffset, inputBoneIndices.x, paletteWidth));
        if (weights.y > 0)
        {
            finalMatrix += mul(weights.y, LoadBoneMatrix(animationTexture, animationOffset, inputBoneIndices.y, paletteWidth));
            if (weights.z > 0)
            {
                finalMatrix += mul(weights.z, LoadBoneMatrix(animationTexture, animationOffset, inputBoneIndices.z, paletteWidth));
                if (weights.w > 0)
                {
                    finalMatrix += mul(weights.w, LoadBoneMatrix(animationTexture, animationOffset, inputBoneIndices.w, paletteWidth));
                }
            }
        }
    }
    
    return finalMatrix;
}

float4x4 ComputeInterpolatedAnimationTransform(
	Texture2D animationTexture,
	uint animationOffset1,
	uint animationOffset2,
    float interpolationValue,
	uint paletteWidth,
	float3 inputWeights,
	uint4 inputBoneIndices)
{
    float4 weights = PopulateWeights(inputWeights);
	
    float4x4 finalMatrix1 = IDENTITY_MATRIX;
    float4x4 finalMatrix2 = IDENTITY_MATRIX;
    if (weights.x > 0)
    {
        finalMatrix1 = mul(weights.x, LoadBoneMatrix(animationTexture, animationOffset1, inputBoneIndices.x, paletteWidth));
        finalMatrix2 = mul(weights.x, LoadBoneMatrix(animationTexture, animationOffset2, inputBoneIndices.x, paletteWidth));
        if (weights.y > 0)
        {
            finalMatrix1 += mul(weights.y, LoadBoneMatrix(animationTexture, animationOffset1, inputBoneIndices.y, paletteWidth));
            finalMatrix2 += mul(weights.y, LoadBoneMatrix(animationTexture, animationOffset2, inputBoneIndices.y, paletteWidth));
            if (weights.z > 0)
            {
                finalMatrix1 += mul(weights.z, LoadBoneMatrix(animationTexture, animationOffset1, inputBoneIndices.z, paletteWidth));
                finalMatrix2 += mul(weights.z, LoadBoneMatrix(animationTexture, animationOffset2, inputBoneIndices.z, paletteWidth));
                if (weights.w > 0)
                {
                    finalMatrix1 += mul(weights.w, LoadBoneMatrix(animationTexture, animationOffset1, inputBoneIndices.w, paletteWidth));
                    finalMatrix2 += mul(weights.w, LoadBoneMatrix(animationTexture, animationOffset2, inputBoneIndices.w, paletteWidth));
                }
            }
        }
    }
    
    return LerpByComponents(finalMatrix1, finalMatrix2, interpolationValue);
}

void ComputePositionWeights(
	Texture2D animationTexture,
	uint animationOffset1,
	uint animationOffset2,
	float interpValue,
	uint paletteWidth,
	float3 inputWeights,
	uint4 inputBoneIndices,
	float3 inputPositionLocal,
	out float4 positionLocal)
{
    float4x4 finalMatrix = ComputeInterpolatedAnimationTransform(animationTexture, animationOffset1, animationOffset2, interpValue, paletteWidth, inputWeights, inputBoneIndices);
    
    positionLocal = mul(float4(inputPositionLocal, 1.0f), finalMatrix);
}

void ComputePositionNormalWeights(
	Texture2D animationTexture,
	uint animationOffset1,
	uint animationOffset2,
	float interpValue,
	uint paletteWidth,
	float3 inputWeights,
	uint4 inputBoneIndices,
	float3 inputPositionLocal,
	float3 inputNormalLocal,
	out float4 positionLocal,
	out float4 normalLocal)
{
    float4x4 finalMatrix = ComputeInterpolatedAnimationTransform(animationTexture, animationOffset1, animationOffset2, interpValue, paletteWidth, inputWeights, inputBoneIndices);

    positionLocal = mul(float4(inputPositionLocal, 1.0f), finalMatrix);
    normalLocal = mul(float4(inputNormalLocal, 0.0f), finalMatrix);
}

void ComputePositionNormalTangentWeights(
	Texture2D animationTexture,
	uint animationOffset1,
	uint animationOffset2,
	float interpValue,
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
    float4x4 finalMatrix = ComputeInterpolatedAnimationTransform(animationTexture, animationOffset1, animationOffset2, interpValue, paletteWidth, inputWeights, inputBoneIndices);

    positionLocal = mul(float4(inputPositionLocal, 1.0f), finalMatrix);
    normalLocal = mul(float4(inputNormalLocal, 0.0f), finalMatrix);
    tangentLocal = mul(float4(inputTangentLocal, 0.0f), finalMatrix);
}

#endif
