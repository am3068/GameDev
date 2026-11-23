Shader "Custom/Forest Triplanar Terrain"
{
    Properties
    {
        [HideInInspector] _Control ("Terrain Paint", 2D) = "red" {}
        [HideInInspector] _Splat0 ("Grass", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Path", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Rock", 2D) = "white" {}
        _RockTileSize ("Rock Tile Size (Metres)", Float) = 5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-100" "TerrainCompatible"="True" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _Control, _Splat0, _Splat1, _Splat2;
        float _RockTileSize;

        struct Input
        {
            float2 uv_Control;
            float2 uv_Splat0;
            float2 uv_Splat1;
            float3 worldPos;
            float3 worldNormal;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 normal = abs(normalize(IN.worldNormal));
            float3 blend = pow(normal, 4);
            blend /= max(blend.x + blend.y + blend.z, 0.0001);

            float3 position = IN.worldPos / max(_RockTileSize, 0.1);
            float3 rock =
                tex2D(_Splat2, position.zy).rgb * blend.x +
                tex2D(_Splat2, position.xz).rgb * blend.y +
                tex2D(_Splat2, position.xy).rgb * blend.z;

            float4 paint = tex2D(_Control, IN.uv_Control);
            float3 grass = tex2D(_Splat0, IN.uv_Splat0).rgb;
            float3 path = tex2D(_Splat1, IN.uv_Splat1).rgb;

            o.Albedo = grass * paint.r + path * paint.g + rock * paint.b;
            o.Metallic = 0;
            o.Smoothness = 0.1;
            o.Alpha = 1;
        }
        ENDCG
    }

    Fallback "Diffuse"
}
