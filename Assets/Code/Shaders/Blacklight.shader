Shader "Unlit/Blacklight"
{
    Properties
    {
        _Value ("Brightness", float) = 2.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #define _LIGHT_LAYERS
            #define _ADDITIONAL_LIGHTS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "BlacklightLighting.hlsl"
            
            struct Attributes
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float3 posWS : VAR_POSITION_WS;
                float3 normal : NORMAL;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.vertex = TransformWorldToHClip(o.posWS);
                o.normal = TransformObjectToWorldDir(v.normal.xyz);
                return o;
            }

            float _Value;

            float4 frag(Varyings i) : SV_Target
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = i.posWS;
                inputData.normalWS = normalize(i.normal);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = 1.0;
                
                float attenuation = BlacklightLighting(inputData, surfaceData);
                
                return float4(0.0, _Value * attenuation, 0.0, 0.0);
            }
            ENDHLSL
        }
    }
}