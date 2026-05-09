Shader "Custom/Tileable"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TilingScale ("Tiling Scale", Float) = 1
        _BlendSharpness ("Blend Sharpness", Float) = 4
        _Color ("Tint", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _MainTex_ST;

        float _TilingScale;
        float _BlendSharpness;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        fixed4 TriplanarSample(float3 worldPos, float3 worldNormal)
        {
            worldNormal = normalize(worldNormal);

            float3 blend = abs(worldNormal);
            blend = pow(blend, _BlendSharpness);
            blend /= blend.x + blend.y + blend.z;

            float2 uvX = worldPos.zy * _TilingScale;
            float2 uvY = worldPos.xz * _TilingScale;
            float2 uvZ = worldPos.xy * _TilingScale;

            uvX = TRANSFORM_TEX(uvX, _MainTex);
            uvY = TRANSFORM_TEX(uvY, _MainTex);
            uvZ = TRANSFORM_TEX(uvZ, _MainTex);

            fixed4 texX = tex2D(_MainTex, uvX);
            fixed4 texY = tex2D(_MainTex, uvY);
            fixed4 texZ = tex2D(_MainTex, uvZ);

            return texX * blend.x + texY * blend.y + texZ * blend.z;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 col = TriplanarSample(IN.worldPos, IN.worldNormal) * _Color;

            o.Albedo = col.rgb;
            o.Alpha = col.a;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }

        ENDCG
    }

    FallBack "Standard"
}
