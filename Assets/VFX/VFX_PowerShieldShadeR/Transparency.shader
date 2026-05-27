Shader "Custom/TransparentShader"
{
    Properties
    {
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _Color ("Color", Color) = (1,1,1,1)
        _Intensity("Intensity", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
        };

        half _Transparency;
        float4 _Color;
        float _Intensity;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float intensity = abs(dot(IN.worldNormal,IN.viewDir)) * (1-_Intensity);
            o.Albedo = _Color.rgb;
            o.Alpha = _Transparency * (1-intensity);
        }

        ENDCG
    }

    FallBack "Diffuse"
}