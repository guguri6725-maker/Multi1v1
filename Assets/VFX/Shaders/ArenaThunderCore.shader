Shader "Arena/Thunder Core"
{
 Properties {_BaseColor("Casing",Color)=(.018,.065,.1,1) _MainTex("Energy",2D)="white"{} }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
   struct V {float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;};
   TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseColor;float4 _MainTex_ST;
   CBUFFER_END
   V vert(A v){V o;o.w=TransformObjectToWorld(v.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(v.n);o.uv=v.uv;return o;}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.n);float2 uv=i.uv;
    float noise=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv*3+float2(_Time.y*.17,0)).r;
    float bolt=pow(saturate(1-abs(sin(uv.x*30+noise*9+_Time.y*2))*7),2);
    float rim=pow(1-saturate(dot(n,normalize(GetWorldSpaceViewDir(i.w)))),3);
    float band=pow(saturate(1-abs(sin(uv.y*15.7))*8),2);
    Light l=GetMainLight();
    return half4(_BaseColor.rgb*(.4+saturate(dot(n,l.direction)))+float3(.04,.6,1.4)*(bolt*1.8+rim*.6+band*.65),1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 }
}
