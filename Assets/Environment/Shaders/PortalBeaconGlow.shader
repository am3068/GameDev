Shader "Custom/Portal Beacon Glow"
{
    Properties
    {
        _Color ("Glow Colour", Color) = (0.3, 1, 0.45, 1)
        _Intensity ("Intensity", Range(0, 3)) = 1.4
        _Range ("Visible Range", Float) = 42
        [Toggle] _Vertical ("Vertical Beacon", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" }
        Blend SrcAlpha One
        Cull Off
        ZWrite Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            half _Intensity;
            float _Range;
            half _Vertical;

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float distance : TEXCOORD1;
            };

            Varyings vert(appdata_base input)
            {
                Varyings output;
                float3 centre = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 view = mul(UNITY_MATRIX_V, float4(centre, 1)).xyz;
                float width = length(mul((float3x3)unity_ObjectToWorld, float3(1, 0, 0)));
                float height = length(mul((float3x3)unity_ObjectToWorld, float3(0, 1, 0)));
                if (_Vertical > .5)
                {
                    float3 towardsCamera = _WorldSpaceCameraPos - centre;
                    float3 right = normalize(float3(towardsCamera.z + .0001, 0, -towardsCamera.x));
                    float3 vertex = centre + right * (input.vertex.x * width) + float3(0, input.vertex.y * height, 0);
                    view = mul(UNITY_MATRIX_V, float4(vertex, 1)).xyz;
                }
                else
                    view.xy += input.vertex.xy * float2(width, height);
                output.position = mul(UNITY_MATRIX_P, float4(view, 1));
                output.uv = input.texcoord.xy;
                output.distance = distance(centre, _WorldSpaceCameraPos);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                if (_Vertical > .5)
                {
                    half side = 1.0 - smoothstep(0.0, 1.0, abs(input.uv.x * 2.0 - 1.0));
                    half top = pow(saturate(1.0 - input.uv.y), 1.25);
                    half bottom = smoothstep(0.0, .075, input.uv.y);
                    half fade = 1.0 - smoothstep(_Range - 28.0, _Range, input.distance);
                    half shimmer = .94 + .06 * sin(_Time.y * 2.4 + input.uv.y * 13.0);
                    half beam = (side * side * side + .26 * side) * top * bottom * fade * shimmer;
                    return half4(_Color.rgb, saturate(beam * _Intensity));
                }

                float radius = length(input.uv * 2.0 - 1.0);
                half core = 1.0 - smoothstep(.02, .65, radius);
                half haze = 1.0 - smoothstep(.08, 1.0, radius);
                half halo = core * core + .30 * haze;
                half rangeFade = 1.0 - smoothstep(_Range - 2.0, _Range, input.distance);
                half boost = lerp(.55, 1.0, smoothstep(8.0, 30.0, input.distance));
                return half4(_Color.rgb, saturate(halo * _Intensity * rangeFade * boost));
            }
            ENDCG
        }
    }
    Fallback Off
}
