#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_METAL) || defined(SHADER_API_PSSL)
#define UNITY_CAN_COMPILE_TESSELLATION 1
#   define UNITY_domain                 domain
#   define UNITY_partitioning           partitioning
#   define UNITY_outputtopology         outputtopology
#   define UNITY_patchconstantfunc      patchconstantfunc
#   define UNITY_outputcontrolpoints    outputcontrolpoints
#endif

// tessellation data
struct TessellationFactors
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

// Extra vertex struct
struct ControlPoint
{
	float4 vertex : INTERNALTESSPOS;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;
	float3 normal : NORMAL;
};

// tessellation variables, add these to your shader properties
float _Tess;
float _MaxTessDistance;

// info so the GPU knows what to do (triangles) and how to set it up , clockwise, fractional division
// hull takes the original vertices and outputs more
[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
//[UNITY_partitioning("fractional_odd")]
//[UNITY_partitioning("fractional_even")]
[UNITY_partitioning("pow2")]
//[UNITY_partitioning("integer")]
[UNITY_patchconstantfunc("patchConstantFunction")]
ControlPoint hull(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}


// fade tessellation at a distance
float CalcDistanceTessFactor(float4 vertex, float4 color, float minDist, float maxDist, float tess)
{
	if (!FloatEqualCompare(color.a, 0.125, 0.02))
		return 1;
	
    float3 worldPosition = TransformObjectToWorld(vertex.xyz);
    float dist = distance(worldPosition, _WorldSpaceCameraPos);
	float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
	return (f);
}


// tessellation
TessellationFactors patchConstantFunction(InputPatch<ControlPoint, 3> patch)
{
	// values for distance fading the tessellation
	float minDist = 5.0;
	float maxDist = _MaxTessDistance;

	TessellationFactors f;

	float edge0 = CalcDistanceTessFactor(patch[0].vertex, patch[0].color, minDist, maxDist, _Tess);
	float edge1 = CalcDistanceTessFactor(patch[1].vertex, patch[1].color, minDist, maxDist, _Tess);
	float edge2 = CalcDistanceTessFactor(patch[2].vertex, patch[2].color, minDist, maxDist, _Tess);

	// make sure there are no gaps between different tessellated distances, by averaging the edges out.
	f.edge[0] = (edge1 + edge2) / 2;
	f.edge[1] = (edge2 + edge0) / 2;
	f.edge[2] = (edge0 + edge1) / 2;
	f.inside = (edge0 + edge1 + edge2) / 3;
	return f;
}


