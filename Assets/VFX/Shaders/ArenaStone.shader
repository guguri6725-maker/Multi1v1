Shader "Arena/Fractured Basalt"
{
 Properties {_BaseColor("Stone",Color)=(.32,.28,.23,1) _MainTex("Rock detail",2D)="white"{} _Rise("Emerge",Range(0,1))=1 _Metallic("Metal",Float)=0 _Smoothness("Roughness",Float)=.15}
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A {float4 p:POSITION;float3 n:NORMAL;};
   struct V {float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;};
   TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseColor;float4 _MainTex_ST;float _Rise,_Metallic,_Smoothness;
   CBUFFER_END
   V vert(A v){V o;v.p.y=(v.p.y+.5)*_Rise-.5;o.w=TransformObjectToWorld(v.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(v.n);return o;}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.n),w=pow(abs(n),4);w/=max(.001,w.x+w.y+w.z);
    float3 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.w.zy*.7).rgb*w.x+SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.w.xz*.7).rgb*w.y+SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.w.xy*.7).rgb*w.z;
    Light l=GetMainLight(TransformWorldToShadowCoord(i.w));
    float3 lit=SampleSH(n)+l.color*(saturate(dot(n,l.direction))*.8+.08)*l.shadowAttenuation;
    return half4(_BaseColor.rgb*tex*2.2*max(lit,.18),1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
