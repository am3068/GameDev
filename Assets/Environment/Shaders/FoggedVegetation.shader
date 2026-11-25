Shader "Custom/Fogged Scenery"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        [HDR] _EmissionColor ("Glow", Color) = (0, 0, 0, 0)
        _EmissionTextureWeight ("Glow Texture Detail", Range(0, 1)) = 0
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0
        _Wind ("Wind", Range(0, 0.15)) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        Cull [_Cull]
        CGPROGRAM
        #pragma surface surf Standard vertex:Bend addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        half4 _EmissionColor;
        half _EmissionTextureWeight;
        half _Cutoff;
        half _Wind;
        float _SceneryFogEnd;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float4 screenPos;
        };

        void Bend(inout appdata_full vertex)
        {
            float3 world = mul(unity_ObjectToWorld, vertex.vertex).xyz;
            float phase = _Time.y * 1.7 + dot(world.xz, float2(0.34, 0.23));
            vertex.vertex.x += sin(phase) * _Wind * saturate(vertex.vertex.y);
        }

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 color = tex2D(_MainTex, input.uv_MainTex) * _Color;
            clip(color.a - _Cutoff);

            #if !defined(UNITY_PASS_SHADOWCASTER)
                if (_SceneryFogEnd > 0.0)
                {
                    float visibility = saturate((_SceneryFogEnd - distance(_WorldSpaceCameraPos, input.worldPos)) / 4.0);
                    float2 pixel = input.screenPos.xy / input.screenPos.w * _ScreenParams.xy;
                    float noise = frac(52.9829189 * frac(dot(floor(pixel), float2(0.06711056, 0.00583715))));
                    clip(visibility - max(0.0001, noise));
                }
            #endif

            output.Albedo = color.rgb;
            half brightness = dot(color.rgb, half3(0.299, 0.587, 0.114));
            output.Emission = _EmissionColor.rgb * lerp(1.0, brightness, _EmissionTextureWeight);
            output.Smoothness = 0.14;
            output.Alpha = 1;
        }
        ENDCG
    }

    Fallback Off
}
