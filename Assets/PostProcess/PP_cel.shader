Shader "PP_cel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}

        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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
                float levels = 8.0;

                //float2 texSize = textureSize(colorTexture, 0).xy;
                //float2 texCoord = gl_FragCoord.xy / texSize;

                //// Avoid the background.
                //vec4 position = texture(positionTexture, texCoord);
                //if (position.a <= 0) { fragColor = vec4(0); return; }
                
                float gamma = 0.4;

                float4 fragColor = tex2D(_MainTex, i.uv);

                //if (enabled.x != 1) { return; }

                //fragColor.rgb = pow(fragColor.rgb, float3(0.85, 0.85, 0.85));

                //float greyscale = max(fragColor.r, max(fragColor.g, fragColor.b));
                float greyscale = 0.3086 * fragColor.r + 0.6094 * fragColor.g + 0.0820 * fragColor.b;
                greyscale = pow(greyscale, gamma);

                float lower = floor(greyscale * levels) / levels;
                float lowerDiff = abs(greyscale - lower);

                float upper = ceil(greyscale * levels) / levels;
                float upperDiff = abs(upper - greyscale);

                float level = lowerDiff <= upperDiff ? lower : upper;
                float adjustment = level / greyscale;

                fragColor.rgb = lerp(fragColor.rgb, fragColor.rgb * adjustment, 0.9);
                //fragColor.rgb += float3(0.2, 0.2, 0.2);

                //fragColor.rgb = pow(fragColor.rgb, float3(gamma.y, 1, 1));

                //return float4(1, 1, 1, 1);
                fragColor.rgb = lerp(tex2D(_MainTex, i.uv).rgb, fragColor.rgb, 1);

                fragColor.rgb = float3(clamp(0, 1, fragColor.rgb.r), clamp(0, 1, fragColor.rgb.g), clamp(0, 1, fragColor.rgb.b));
                return fragColor;
            }

            //fixed4 frag(v2f i) : SV_Target
            //{
            //    fixed4 col = tex2D(_MainTex, i.uv);
            //    // just invert the colors
            //    float brightness = (col.rgb.x + col.rgb.y + col.rgb.z) / 3;
            //    //float3 rgbSmooth = lerp(_ShadedColor.rgb, color, brightness);
            //    brightness = round(brightness * 5) / 5 + 0.5;
            //    float3 rgbCel = float3(round(col.rgb.x * 10) / 10, round(col.rgb.y * 10) / 10, round(col.rgb.z * 10) / 10);
            //    //float3 rgb = lerp(rgbSmooth, rgbCel, _CellShadePower);

            //    col.rgb = rgbCel;
            //    return col;
            //}

            //fixed4 frag(v2f i) : SV_Target
            //{
            //    fixed4 col = tex2D(_MainTex, i.uv);
            //    // just invert the colors
            //    float brightness = (col.rgb.x + col.rgb.y + col.rgb.z) / 3;
            //    //float3 rgbSmooth = lerp(_ShadedColor.rgb, color, brightness);
            //    brightness = round(brightness * 5) / 5 + 0.5;
            //    float3 rgbCel = lerp(float3(0, 0, 0), col.rgb, brightness);
            //    //float3 rgb = lerp(rgbSmooth, rgbCel, _CellShadePower);

            //    col.rgb = rgbCel;
            //    return col;
            //}

            //fixed4 frag (v2f i) : SV_Target
            //{
            //    fixed4 col = tex2D(_MainTex, i.uv);
            //    // just invert the colors
            //    float brightness = (col.rgb.x + col.rgb.y + col.rgb.z) / 3;
            //    //float3 rgbSmooth = lerp(_ShadedColor.rgb, color, brightness);
            //    brightness = floor(brightness * 10) / 10;

            //    float3 rgbCel = lerp(_ShadedColor.rgb, color, brightness);
            //    float3 rgb = lerp(rgbSmooth, rgbCel, _CellShadePower);

            //    col.rgb = rgb
            //    return col;
            //}
            ENDCG
        }
    }
}
