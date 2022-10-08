#ifndef __MATRIX_INCLUDED__
#define __MATRIX_INCLUDED__

#ifndef IDENTITY_MATRIX
#define IDENTITY_MATRIX float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
#endif
#ifndef ZERO_MATRIX
#define ZERO_MATRIX float4x4(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
#endif

float4 matrixToQuaternion(float4x4 m)
{
    float scale = m._11 + m._22 + m._33;
    float4 q = float4(0, 0, 0, 0);

    if (scale > 0)
    {
        float s = sqrt(scale + 1.0);
        q.w = s * 0.5;
        s = 0.5 / s;
        
        q.x = (m._32 - m._23) * s;
        q.y = (m._13 - m._31) * s;
        q.z = (m._21 - m._12) * s;
    }
    else if ((m._11 >= m._22) && (m._11 >= m._33))
    {
        float s = sqrt(1.0 + m._11 - m._22 - m._33);
        float h = 0.5 / s;
        
        q.x = 0.5 * s;
        q.y = (m._21 + m._12) * h;
        q.z = (m._31 + m._13) * h;
        q.w = (m._23 - m._32) * h;
    }
    else if (m._22 > m._33)
    {
        float s = sqrt(1.0 + m._22 - m._11 - m._33);
        float h = 0.5 / s;

        q.x = (m._12 + m._21) * h;
        q.y = 0.5 * s;
        q.z = (m._23 + m._32) * h;
        q.w = (m._13 - m._31) * h;
    }
    else
    {
        float s = sqrt(1.0 + m._33 - m._11 - m._22);
        float h = 0.5 / s;
        
        q.x = (m._13 + m._31) * h;
        q.y = (m._23 + m._32) * h;
        q.z = 0.5 * s;
        q.w = (m._21 - m._12) * h;
    }

    return q;
}
bool decompose(in float4x4 m, out float3 position, out float4 rotation, out float3 scale)
{
    position = m._14_24_34;

    scale.x = length(m._11_21_31);
    scale.y = length(m._12_22_32);
    scale.z = length(m._13_23_33);

    if (scale.x == 0 || scale.y == 0 || scale.z == 0)
    {
        rotation = float4(0, 0, 0, 1);
        
        return false;
    }
    
    float det = determinant(m);
    if (det < 0)
    {
        scale.x = -scale.x;
    }
    
    float4x4 rotationmatrix = ZERO_MATRIX;
    
    rotationmatrix._11_21_31 = m._11_21_31 / scale.x;
    rotationmatrix._12_22_32 = m._12_22_32 / scale.y;
    rotationmatrix._13_23_33 = m._13_23_33 / scale.z;
    rotationmatrix._44 = 1;
    
    rotation = matrixToQuaternion(rotationmatrix);

    return true;
}
bool decompose2(in float4x4 m, out float3 position, out float4x4 rotation, out float3 scale)
{
    position = m._14_24_34;

    rotation = IDENTITY_MATRIX;

    scale.x = length(m._11_21_31);
    scale.y = length(m._12_22_32);
    scale.z = length(m._13_23_33);

    if (scale.x == 0 || scale.y == 0 || scale.z == 0)
    {
        return false;
    }
    
    float det = determinant(m);
    if (det < 0)
    {
        scale.x = -scale.x;
    }
    
    float4x4 rotationmatrix = ZERO_MATRIX;
    
    //Fix: row major to column major issue
    rotationmatrix._11_12_13 = m._11_21_31 / scale.x;
    rotationmatrix._21_22_23 = m._12_22_32 / scale.y;
    rotationmatrix._31_32_33 = m._13_23_33 / scale.z;
    rotationmatrix._44 = 1;
    
    rotation = rotationmatrix;

    return true;
}

float4x4 scaleToMatrix(float3 v)
{
    float4x4 result = IDENTITY_MATRIX;
    
    result._11 = v.x;
    result._22 = v.y;
    result._33 = v.z;
    
    return result;
}
float4x4 quaternionToMatrix(float4 quat)
{
    float xx = quat.x * quat.x;
    float yy = quat.y * quat.y;
    float zz = quat.z * quat.z;
    float xy = quat.x * quat.y;
    float zw = quat.z * quat.w;
    float zx = quat.z * quat.x;
    float yw = quat.y * quat.w;
    float yz = quat.y * quat.z;
    float xw = quat.x * quat.w;

    float4x4 result = IDENTITY_MATRIX;
    result._11 = 1.0f - (2.0f * (yy + zz));
    result._12 = 2.0f * (xy + zw);
    result._13 = 2.0f * (zx - yw);
    result._21 = 2.0f * (xy - zw);
    result._22 = 1.0f - (2.0f * (zz + xx));
    result._23 = 2.0f * (yz + xw);
    result._31 = 2.0f * (zx + yw);
    result._32 = 2.0f * (yz - xw);
    result._33 = 1.0f - (2.0f * (yy + xx));
    
    return result;
}
float4x4 translateToMatrix(float3 v)
{
    float4x4 result = IDENTITY_MATRIX;
    
    result._41 = v.x;
    result._42 = v.y;
    result._43 = v.z;
    
    return result;
}
float4x4 compose(float3 position, float4 rotationQuaternion, float3 scale)
{
    float4x4 s = scaleToMatrix(scale);
    float4x4 r = quaternionToMatrix(rotationQuaternion);
    float4x4 p = translateToMatrix(position);
    
    return mul(mul(s, r), p);
}
float4x4 compose2(float3 position, float4x4 rotationMatrix, float3 scale)
{
    float4x4 s = scaleToMatrix(scale);
    float4x4 r = rotationMatrix;
    float4x4 p = translateToMatrix(position);
    
    return mul(mul(s, r), p);
}

#endif