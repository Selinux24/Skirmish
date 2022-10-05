#include "..\Lib\IncGBuffer.hlsl"

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float4 color : COLOR0;
    Material material;
};

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
GBuffer main(PSVertex input)
{
    return Pack(input.positionWorld, normalize(input.normalWorld), input.color, true, input.material);
}
