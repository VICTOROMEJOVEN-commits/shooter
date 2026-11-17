Shader "Shooter/Cemetery World Stone"
{
    Properties
    {
        [MainTexture] _BaseMap("Textura", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _TileSize("Metros por repeticion", Float) = 2.5
        _Smoothness("Humedad", Range(0,1)) = 0.12
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _Cull("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _TileSize;
                half _Smoothness;
                half _Cutoff;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; half fog : TEXCOORD2; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs pos = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = pos.positionCS; o.positionWS = pos.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS); o.fog = ComputeFogFactor(pos.positionCS.z);
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                half3 n = normalize(i.normalWS);
                half3 weights = pow(abs(n), 4); weights /= max(dot(weights, half3(1,1,1)), 0.0001);
                float3 p = i.positionWS / max(_TileSize, 0.1);
                half3 tex = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.zy).rgb * weights.x
                    + SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xz).rgb * weights.y
                    + SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xy).rgb * weights.z;
                InputData data = (InputData)0;
                data.positionWS = i.positionWS; data.normalWS = n;
                data.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                data.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                data.fogCoord = i.fog; data.bakedGI = SampleSH(n);
                data.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                data.shadowMask = half4(1,1,1,1);
                SurfaceData surface = (SurfaceData)0;
                surface.albedo = tex * _BaseColor.rgb; surface.alpha = 1;
                surface.smoothness = _Smoothness; surface.occlusion = 1;
                surface.normalTS = half3(0,0,1);
                half4 color = UniversalFragmentPBR(data, surface);
                color.rgb = MixFog(color.rgb, i.fog);
                return color;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
    FallBack "Universal Render Pipeline/Lit"
}

