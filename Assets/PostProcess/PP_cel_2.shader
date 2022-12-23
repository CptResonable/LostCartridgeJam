Shader "PP_cel_2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CelLevels ("Cel Levels", Int) = 10
        _CelPower ("Celshade Power", Range(0, 1)) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag        

            #include "UnityCG.cginc"



            CBUFFER_START(UnityPerMaterial)
            int _CelLevels;
            float _CelPower;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                
                float gamma = 0.3;

                float4 fragColor = tex2D(_MainTex, i.uv);

                //if (enabled.x != 1) { return; }

                //fragColor.rgb += float3(0.02, 0.02, 0.02);

                //float greyscale = max(fragColor.r, max(fragColor.g, fragColor.b));
                float greyscale = 0.3086 * fragColor.r + 0.6094 * fragColor.g + 0.0820 * fragColor.b;
                greyscale = pow(greyscale, gamma);

                float lower = floor(greyscale * _CelLevels) / _CelLevels;
                float lowerDiff = abs(greyscale - lower);

                float upper = ceil(greyscale * _CelLevels) / _CelLevels;
                float upperDiff = abs(upper - greyscale);

                float level = lowerDiff <= upperDiff ? lower : upper;
                float adjustment = level / (greyscale + 0.001);

                fragColor.rgb = lerp(float3(0,0,0), tex2D(_MainTex, i.uv).rgb, 0.5);
                //fragColor.rgb = pow(fragColor.rgb, float3(1 /gamma, 1/gamma, 1/gamma));
                //fragColor.rgb += float3(0.2, 0.2, 0.2);

                //fragColor.rgb = pow(fragColor.rgb, float3(gamma.y, 1, 1));

                //return float4(1, 1, 1, 1);
                //return(tex2D(_MainTex, i.uv));
                //fragColor.rgb = lerp(tex2D(_MainTex, i.uv).rgb, fragColor.rgb, _CelPower);

                //fragColor.rgb = float3(clamp(0, 1, fragColor.rgb.r), clamp(0, 1, fragColor.rgb.g), clamp(0, 1, fragColor.rgb.b));
                return fragColor;
            }


            // fixed4 frag(v2f i) : SV_Target
            // {
                
            //     float gamma = 1;

            //     float4 fragColor = tex2D(_MainTex, i.uv);

            //     //if (enabled.x != 1) { return; }

            //     fragColor.rgb = pow(fragColor.rgb, float3(gamma, gamma, gamma));
            //     //fragColor.rgb += float3(0.02, 0.02, 0.02);

            //     //float greyscale = max(fragColor.r, max(fragColor.g, fragColor.b));
            //     float greyscale = 0.3086 * fragColor.r + 0.6094 * fragColor.g + 0.0820 * fragColor.b;

            //     float lower = floor(greyscale * _CelLevels) / _CelLevels;
            //     float lowerDiff = abs(greyscale - lower);

            //     float upper = ceil(greyscale * _CelLevels) / _CelLevels;
            //     float upperDiff = abs(upper - greyscale);

            //     float level = lowerDiff <= upperDiff ? lower : upper;
            //     float adjustment = level / greyscale;

            //     fragColor.rgb = fragColor.rgb * adjustment;
            //     fragColor.rgb = pow(fragColor.rgb, float3(1 /gamma, 1/gamma, 1/gamma));
            //     //fragColor.rgb += float3(0.2, 0.2, 0.2);

            //     //fragColor.rgb = pow(fragColor.rgb, float3(gamma.y, 1, 1));

            //     //return float4(1, 1, 1, 1);
            //     //return(tex2D(_MainTex, i.uv));
            //     fragColor.rgb = lerp(tex2D(_MainTex, i.uv).rgb, fragColor.rgb, _CelPower);

            //     //fragColor.rgb = float3(clamp(0, 1, fragColor.rgb.r), clamp(0, 1, fragColor.rgb.g), clamp(0, 1, fragColor.rgb.b));
            //     return fragColor;
            // }

            // fixed4 frag(v2f i) : SV_Target
            // {
                
            //     float2 gamma = float2(1, 1);

            //     float4 fragColor = tex2D(_MainTex, i.uv);

            //     //if (enabled.x != 1) { return; }

            //     fragColor.rgb = pow(fragColor.rgb, float3(0.85, 0.85, 0.85));

            //     //float greyscale = max(fragColor.r, max(fragColor.g, fragColor.b));
            //     float greyscale = 0.3086 * fragColor.r + 0.6094 * fragColor.g + 0.0820 * fragColor.b;

            //     float lower = floor(greyscale * _CelLevels) / _CelLevels;
            //     float lowerDiff = abs(greyscale - lower);

            //     float upper = ceil(greyscale * _CelLevels) / _CelLevels;
            //     float upperDiff = abs(upper - greyscale);

            //     float level = lowerDiff <= upperDiff ? lower : upper;
            //     float adjustment = level / greyscale;

            //     fragColor.rgb = fragColor.rgb * adjustment;
            //     //fragColor.rgb += float3(0.2, 0.2, 0.2);

            //     fragColor.rgb = pow(fragColor.rgb, float3(gamma.y, 1, 1));

            //     //return float4(1, 1, 1, 1);
            //     fragColor.rgb = lerp(tex2D(_MainTex, i.uv).rgb, fragColor.rgb, _CelPower);

            //     fragColor.rgb = float3(clamp(0, 1, fragColor.rgb.r), clamp(0, 1, fragColor.rgb.g), clamp(0, 1, fragColor.rgb.b));
            //     return fragColor;
            // }
            ENDCG
        }
    }
}
