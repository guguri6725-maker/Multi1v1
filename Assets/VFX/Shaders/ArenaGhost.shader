Shader "Arena/Dissolving Echo"
{
 Properties { _Tint("Team",Color)=(.1,.65,1,1) _Fade("Fade",Range(0,1))=1 _MainTex("Dissolve detail",2D)="white"{} }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass
  {
   Blend SrcAlpha One ZWrite Off Cull Back
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 p:POSITION;float3 n:NORMAL;};
   struct V {float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;};
   TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   float4 _Tint;float _Fade;float4 _MainTex_ST;
   CBUFFER_END
   V vert(A v){V o;o.w=TransformObjectToWorld(v.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(v.n);return o;}
   half4 frag(V i):SV_Target
   {
    float rim=pow(1-saturate(abs(dot(normalize(i.n),normalize(GetWorldSpaceViewDir(i.w))))),2);
    float noise=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.w.xy*1.4+float2(0,_Time.y*.3)).r;
    float dissolve=smoothstep((1-_Fade)*.7-.18,(1-_Fade)*.7+.1,noise);
    float scan=.7+.3*sin(i.w.y*95-_Time.y*12);
    return half4(_Tint.rgb*(1.3+rim*2),_Fade*(.3+rim*.65)*scan*dissolve);
   }
   ENDHLSL
  }
 }
}
