// Example Shader for Universal RP
// Written by @Cyanilux
// https://cyangamedev.wordpress.com/urp-shader-code/
Shader "Custom/DMETerrainShader_02" {
	Properties {
		_BaseMap ("Base Texture", 2D) = "white" {}
		_TextureInfluence ("Texture Influence", Range(0, 1)) = 0.5
		_Color1 ("Color 1, using red chanel 0", Color) = (0, 0.66, 0.73, 1)
		_Color2 ("Color 2, using red chanel 1", Color) = (0, 0.66, 0.73, 1)
		_Color3 ("Color 3, using green chanel", Color) = (0, 0.66, 0.73, 1)
		_ShadedColor ("Shaded Colour", Color) = (0, 0.66, 0.73, 1)
		_CellShadePower ("Cell Shade Power", Range(0, 1)) = 0.5
		_GrassTexture ("Grass Texture", 2D) = "white" {}
		_GrassSize ("Grass Size", Float) = 0.5
		_GrassSizeVariance ("Grass Size Variance", Float) = 0.3
		_GrassColor ("Grass Color", Color) = (0, 0.66, 0.73, 1)
		_GrassPlanesPerTriangle ("GrassPlanesPerTriangle", Range(1, 10)) = 1
		_Wind ("Wind. xyz = wind direction, w = wind speed", Vector) = (1, 0, 0, 1)
		_WindDeflectionScale ("WindDeflectionScale", Float) = 1
		_Tess("Tessellation", Range(0, 32)) = 20
		_MaxTessDistance("Max Tess Distance", Range(1, 360)) = 20
		
		_Noise ("Noise", 2D) = "white" {}
		_RndNoise ("Random noise", 2D) = "white" {}
	}
	SubShader {
		Tags {"RenderType" = "Opaque""RenderPipeline"="UniversalPipeline" }
		Cull Off
		
		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			float _TextureInfluence;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _ShadedColor;
			float _CellShadePower;
			float _GrassSize;
			float _GrassSizeVariance;
			float4 _GrassColor;
			int _GrassPlanesPerTriangle;
			float4 _Wind;
			float _WindDeflectionScale;
			CBUFFER_END
		ENDHLSL
 
		Pass {
			Name "Example"
			Tags { "LightMode"="UniversalForward" }
			
			//CULL Off
 
			HLSLPROGRAM
			#pragma require tessellation
            #pragma vertex TessellationVertexProgram // This line defines the name of the vertex shader. 
			//#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma hull hull // This line defines the name of the hull shader. 
            #pragma domain domain // This line defines the name of the domain shader. 
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT
			 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "NMGGeometryHelpers.hlsl"
			#include "HlslHelpers.hlsl"
			#include "NoiseSampler.hlsl"
			#include "CustomTessellation.cginc"
 
			struct Attributes {
				float4 vertex	: POSITION; // PositionOS
				float2 uv		: TEXCOORD0;
				float4 color		: COLOR;
				float4 normal     : NORMAL;  // NormalOS
			};
 		
			struct Varyings { // Vertex to geometry
				float4 positionCS 	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				float4 color		: COLOR;
				float3 normalWS     : NORMAL;
				float3 positionWS   : TEXCOORD2;
			};

			struct g2f { // Geometry to fragment
				float4 positionCS 	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				float4 color		: COLOR;
				float3 normalWS     : NORMAL;
				float3 positionWS   : TEXCOORD2;
				float3 positionV1   : TEXCOORD3;
				bool isGrass		: TEXCOORD4;
			};
 
			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			TEXTURE2D(_GrassTexture);
			SAMPLER(sampler_GrassTexture);

			// pre tesselation vertex program
            ControlPoint TessellationVertexProgram(Attributes v)
            {
                ControlPoint p;

                p.vertex = v.vertex;
                p.uv = v.uv;
                p.normal = v.normal.xyz;
                p.color = v.color;
         
                return p;
            }
 
			Varyings vert(Attributes IN) {
				Varyings OUT;
 
				VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.vertex.xyz);
			    VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normal.xyz);
				
				OUT.positionCS = positionInputs.positionCS;
				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
				OUT.color = IN.color;
			    OUT.positionWS = positionInputs.positionWS;
			    OUT.normalWS = normalInputs.normalWS;
				return OUT;
			}

			g2f SetupVertex(float3 positionWS, float3 normalWS, float2 uv, float4 color, float3 v1, bool isGrass) {
				
			    // Setup an output struct
			    g2f OUT = (g2f)0;
			    OUT.positionWS = positionWS;
			    OUT.normalWS = normalWS;
			    OUT.uv = uv;
				OUT.color = color;
				OUT.isGrass = isGrass;
				
			    // This function calculates clip space position, taking the shadow caster pass into account
			    OUT.positionCS = CalculatePositionCSWithShadowCasterLogic(positionWS, normalWS);

            	OUT.positionV1 = v1;
			    return OUT;
			}

			void SetupAndOutputTriangle(inout TriangleStream<g2f> outputStream, Varyings a, Varyings b, Varyings c, bool isGrass) {
			    // Restart the triangle strip, signaling the next appends are disconnected from the last
			    outputStream.RestartStrip();
			    // Since we extrude the center face, the normal must be recalculated
				
			    //float3 normalWS = GetNormalFromTriangle(a.positionWS, b.positionWS, c.positionWS);
			    // Add the output data to the output stream, creating a triangle
			    outputStream.Append(SetupVertex(a.positionWS, a.normalWS, a.uv, a.color, a.positionWS,  isGrass));
			    outputStream.Append(SetupVertex(b.positionWS, b.normalWS, b.uv, b.color, a.positionWS, isGrass));
			    outputStream.Append(SetupVertex(c.positionWS, c.normalWS, c.uv, c.color, a.positionWS, isGrass));
			}
			
			[maxvertexcount(51)]
			void geom(triangle Varyings IN[3], inout TriangleStream<g2f> outputStream) {
				
				const float3 centerPoint = GetTriangleCenter(IN[0].positionWS, IN[1].positionWS, IN[2].positionWS);         	
				const float4 centerColor = (IN[0].color + IN[1].color + IN[2].color) / 3;

				SetupAndOutputTriangle(outputStream, IN[0], IN[1], IN[2], false); // "Real" ground triangle

				float3 dir_v0_v1 = normalize(IN[1].positionWS - IN[0].positionWS);
            	
            	// Create grass planes
            	if (FloatEqualCompare(centerColor.a, 0.125, 0.02))
            	{
            		for (int i = 1; i <= _GrassPlanesPerTriangle; i++)
            		{		
            			float sizeVariance = TexRndRange(float2(IN[0].positionWS.x, IN[0].positionWS.z) * (i * 543), -_GrassSizeVariance / 2, _GrassSizeVariance / 2);
      
            			float3 triNormal = (IN[0].normalWS + IN[1].normalWS + IN[2].normalWS) / 3;
            			float3 dir_h = RotateAroundAxis(dir_v0_v1, triNormal, TexRndRange(float2(centerPoint.x * 10, centerPoint.z * 10) * i + (i * 100), 0, 6.28));
            			dir_h *= (_GrassSize + sizeVariance);

            			float3 positionOffset = RotateAroundAxis(dir_v0_v1, triNormal, TexRndRange(float2(centerPoint.x * 30, centerPoint.z * 30) * i + (i * 44), 0, 360)) * 0.6;
            			
            			// Vertical grass plane dir
						float3 dir_v = cross(float3(0, 1, 0), normalize(dir_h));
						dir_v = normalize(lerp(float3(0, 1, 0), dir_v, TexRndRange(float2(IN[0].positionWS.x, IN[0].positionWS.z) * (i * 3000), 0, 0.5))) * (_GrassSize + sizeVariance);

            			float3 windDeflection = TexNoise(float2(IN[0].positionWS.x + _Time.x * 0.05 * _Wind.w, IN[0].positionWS.z + _Time.x * 0.05 * _Wind.w)) * (normalize(_Wind.xyz) * _Wind.w * _WindDeflectionScale);
            			
						// Create grass plane vertices
						Varyings b1 = (Varyings)0;
						Varyings b2 = (Varyings)0;
						Varyings t1 = (Varyings)0;
						Varyings t2 = (Varyings)0;

						b1.positionWS = centerPoint + dir_h + positionOffset;
						b2.positionWS = centerPoint + -dir_h + positionOffset;
            			t1.positionWS = centerPoint + dir_h + positionOffset + dir_v * 2 + windDeflection;
						t2.positionWS = centerPoint + -dir_h + positionOffset + dir_v * 2 + windDeflection;

						b1.normalWS = triNormal;
						b2.normalWS = triNormal;
						t1.normalWS = triNormal;
						t2.normalWS = triNormal;

						b1.color = centerColor;
						b2.color = centerColor;
						t1.color = centerColor;
						t2.color = centerColor;

						b1.uv = float2(0, 0);
						b2.uv = float2(1, 0);
						t1.uv = float2(0, 1);
						t2.uv = float2(1, 1);
            		
            			SetupAndOutputTriangle(outputStream, b1, t1, t2, true);
            			SetupAndOutputTriangle(outputStream, b1, t2, b2, true);
            		}
            	}
			}

			[UNITY_domain("tri")]
            Varyings domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                Attributes v;
         
                #define DomainPos(fieldName) v.fieldName = \
                patch[0].fieldName * barycentricCoordinates.x + \
                patch[1].fieldName * barycentricCoordinates.y + \
                patch[2].fieldName * barycentricCoordinates.z;
         
                DomainPos(vertex.xyz)
                DomainPos(uv)
                DomainPos(color)
                DomainPos(normal.xyz)
         
                return vert(v);
            }
			
			float Angle(float3 a, float3 b)
			{
				float angle = acos((a.x * b.x + a.y * b.y + a.z * b.z) / (sqrt(pow(a.x, 2) + pow(a.y, 2) + pow(a.z, 2)) * sqrt(pow(b.x, 2) + pow(b.y, 2) + pow(b.z, 2))));
				return angle;
			}
			
			half4 frag(g2f IN) : SV_Target {
		 
				// Initialize some information for the lighting function
				InputData lightingInput = (InputData)0;
				lightingInput.positionWS = IN.positionWS;
				lightingInput.normalWS = IN.normalWS; // No need to renormalize, since triangles all share normals
				lightingInput.viewDirectionWS = GetViewDirectionFromPosition(IN.positionWS);
				lightingInput.shadowCoord = CalculateShadowCoord(IN.positionWS, IN.positionCS);

				half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
				float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
				Light mainLight = GetMainLight(shadowCoord);
				half3 lightDirection = mainLight.direction;

				// Calculate color
				float3 colorSmooth = lerp(_Color1.rgb, _Color2.rgb, IN.color.r); // Use red chanel to lerp between base and grass ground color
				//colorSmooth = lerp(colorSmooth, _Color3, IN.color.g);
				
				float colorStep = floor((IN.color.r - 0.01f) * 3) / 3;
				half3 colorCel = lerp(_Color1.rgb, _Color2.rgb, colorStep);
				float3 color = lerp(colorSmooth, colorCel, 0.5);

				float3 colorSmooth2 = lerp(color, _Color3.rgb, IN.color.g);
				float3 colorStep2 = floor((IN.color.g) * 3) / 3;
				half3 colorCel2 = lerp(color, _Color3.rgb, colorStep2);
				color = lerp(colorSmooth2, colorCel2, 0.5);

				// Grass stuff
				if (IN.isGrass) {
					if (SAMPLE_TEXTURE2D(_GrassTexture, sampler_BaseMap, IN.uv).a < 0.5f)
						discard;

					color = lerp(color, _GrassColor.rgb, IN.uv.y * IN.uv.y);
					baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv / 8);
				}

				// Calculate shading
				float4 lit = UniversalFragmentBlinnPhong(lightingInput, float3(0.5, 0.5, 0.5), 1, 0, 0, 1);
				float lightMod = (lit.x + lit.y + lit.z) / 3;
				lightMod = lerp(1, lightMod, 0.96f);
				
				float brightness = (1 - Angle(lightDirection, IN.normalWS) / PI);// * lightMod;
				brightness *= lightMod;
				float3 rgbSmooth = lerp(_ShadedColor.rgb, color, brightness);
				brightness = floor(brightness * 10) / 10;
				float3 rgbCel = lerp(_ShadedColor.rgb, color, brightness);
				float3 rgb = lerp(rgbSmooth, rgbCel, _CellShadePower);

				// Apply texture
				baseMap.rgb = lerp(float3(1, 1, 1), baseMap.rgb, _TextureInfluence);
				rgb *= baseMap.rgb;
				
				return float4(rgb, 1);
				return float4(rgb * lightMod, 1);
			}
			
			// half4 frag(g2f IN) : SV_Target {
		 //
			// 	// Initialize some information for the lighting function
			// 	InputData lightingInput = (InputData)0;
			// 	lightingInput.positionWS = IN.positionWS;
			// 	lightingInput.normalWS = IN.normalWS; // No need to renormalize, since triangles all share normals
			// 	lightingInput.viewDirectionWS = GetViewDirectionFromPosition(IN.positionWS);
			// 	lightingInput.shadowCoord = CalculateShadowCoord(IN.positionWS, IN.positionCS);
			//
			// 	half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
			//
			// 	// Calculate color
			// 	float3 colorSmooth = lerp(_Color1.rgb, _Color2.rgb, IN.color.r); // Use red chanel to lerp between base and grass ground color
			// 	//colorSmooth = lerp(colorSmooth, _Color3, IN.color.g);
			// 	
			// 	float colorStep = floor((IN.color.r - 0.01f) * 3) / 3;
			// 	half3 colorCel = lerp(_Color1.rgb, _Color2.rgb, colorStep);
			// 	float3 color = lerp(colorSmooth, colorCel, 0.5);
			//
			// 	float3 colorSmooth2 = lerp(color, _Color3.rgb, IN.color.g);
			// 	float3 colorStep2 = floor((IN.color.g) * 3) / 3;
			// 	half3 colorCel2 = lerp(color, _Color3.rgb, colorStep2);
			// 	color = lerp(colorSmooth2, colorCel2, 0.5);
			//
			// 	// Grass stuff
			// 	if (IN.isGrass) {
			// 		if (SAMPLE_TEXTURE2D(_GrassTexture, sampler_BaseMap, IN.uv).a < 0.5f)
			// 			discard;
			//
			// 		color = lerp(color, _GrassColor.rgb, IN.uv.y * IN.uv.y);
			// 		baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv / 8);
			// 	}
			//
			// 	// Calculate shading
			// 	float4 lit = UniversalFragmentBlinnPhong(lightingInput, float3(0.5, 0.5, 0.5), 1, 0, 0, 1);
			// 	float brightness = (lit.x + lit.y + lit.z) / 3; // * lerp(TexNoise(IN.positionWS.xz * 0.02), 1, 0.95); // Calculate brightness	
			// 	float3 rgbSmooth = lerp(_ShadedColor.rgb, color, brightness);
			// 	brightness = floor(brightness * 10) / 10;
			// 	float3 rgbCel = lerp(_ShadedColor.rgb, color, brightness);
			// 	float3 rgb = lerp(rgbSmooth, rgbCel, _CellShadePower);
			//
			// 	// Apply texture
			// 	baseMap.rgb = lerp(float3(1, 1, 1), baseMap.rgb, _TextureInfluence);
			// 	rgb *= baseMap.rgb;
			// 	
			// 	return float4(rgb, 1);
			// }
			ENDHLSL
		}
	}
}