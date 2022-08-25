#ifndef __QUATERNION_INCLUDED__
#define __QUATERNION_INCLUDED__

#include "IncHelpers.hlsl"

#ifndef IDENTITY_QUATERNION
#define IDENTITY_QUATERNION float4(0, 0, 0, 1)
#endif

float4 slerp(in float4 start, in float4 end, in float amount)
{
    float opposite;
    float inverse;
    float d = dot(start, end);

    if (abs(d) > 1.0 - FLOAT_ZEROTOLERANCE)
    {
        inverse = 1.0 - amount;
        opposite = amount * sign(d);
    }
    else
    {
        float ac = (float) acos(abs(d));
        float invSin = (float) (1.0 / sin(ac));

        inverse = (float) sin((1.0 - amount) * ac) * invSin;
        opposite = (float) sin(amount * ac) * invSin * sign(d);
    }

    return (inverse * start) + (opposite * end);
}

#endif
