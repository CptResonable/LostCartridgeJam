// Make sure this file is not included twice
#ifndef HLSL_HELPERS_INCLUDED
#define HLSL_HELPERS_INCLUDED

float mod(float x, float y)
{
    return x - y * floor(x/y);
}

bool FloatEqualCompare(float f, float compareWith, float margin)
{
    return (f >= compareWith - margin && f <= compareWith + margin);
}

float3 RotateAroundAxis(float3 vec, float3 rotateAround, float angle)
{
    float3 axis = normalize(rotateAround);
    float3 OUT = vec * cos(angle) + cross(axis, vec) * sin(angle) + axis * (axis * vec) * (1 - cos(angle));
    return OUT;
}

// float3 ToBarycentricCoords(float3 p1, float3 p2, float3 p3)
// {
//     float det = (p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z);
//
//     float l1 = ((p2.z - p3.z) * (x - p3.x) + (p3.x - p2.x) * (z - p3.z)) / det;
//     float l2 = ((p3.z - p1.z) * (x - p3.x) + (p1.x - p3.x) * (z - p3.z)) / det;
//     float l3 = 1.0f - l1 - l2;
// }
#endif