#include "..\Lib\IncGBuffer.hlsl"

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float4 color : COLOR0;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
GBuffer main(PSVertex input)
{
    return Pack(input.positionWorld, float3(0, 0, 0), input.color, false, (Material) 0);
}
