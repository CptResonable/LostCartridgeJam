// Make sure this file is not included twice
#ifndef NOISE_SAMPLER_INCLUDED
#define NOISE_SAMPLER_INCLUDED

TEXTURE2D(_Noise);
SAMPLER(sampler_Noise);

TEXTURE2D(_RndNoise);
SAMPLER(sampler_RndNoise);

float TexNoise(float2 xy)
{
    xy.x = mod(xy.x, 1);
    xy.y = mod(xy.y, 1);
    float output = SAMPLE_TEXTURE2D_LOD(_Noise, sampler_Noise, xy, 0).x;
    return output;
}

float TexRndRange(float2 xy, float min, float max)
{
    float rangeSize = max - min;
    xy.x = mod(xy.x, 1);
    xy.y = mod(xy.y, 1);
    float output = SAMPLE_TEXTURE2D_LOD(_RndNoise, sampler_RndNoise, xy, 0).x;
    output *= rangeSize;
    output += min;
    return output.x;
}

// float TexRndRange2(float min, float max)
// {
//     float rangeSize = max - min;
//     xy.x = mod(xy.x, 1);
//     xy.y = mod(xy.y, 1);
//     float output = SAMPLE_TEXTURE2D_LOD(_Noise, sampler_Noise, xy, 0).x;
//     output *= rangeSize;
//     output += min;
//     i++;
//     return output.x;
// }


#endif