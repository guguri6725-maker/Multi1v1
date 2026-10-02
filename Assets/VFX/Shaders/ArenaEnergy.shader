Shader "Arena/Layered Energy"
{
 Properties { _Tint("Tint",Color)=(1,1,1,1) _Fade("Fade",Range(0,1))=1 _Mode("0 Ribbon 1 Spark 2 Ring 3 Dust",Float)=0 _MainTex("Detail",2D)="white"{} }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass
  {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 p:POSITION; float2 uv:TEXCOORD0; float4 c:COLOR;};
   struct V {float4 p:SV_POSITION; float2 uv:TEXCOORD0; float4 c:COLOR;};
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   float4 _Tint; float _Fade,_Mode; float4 _MainTex_ST;
   CBUFFER_END
   V vert(A v) { V o; o.p=TransformObjectToHClip(v.p.xyz);o.uv=v.uv;o.c=v.c;return o; }
   half4 frag(V i):SV_Target
   {
    float2 p=i.uv*2-1; float a=1,bright=1;
    float noise=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv*float2(2,1)+float2(-_Time.y*.7,0)).r;
    if(_Mode<.5) {float edge=exp(-pow((i.uv.y-.96)*38,2));a=(pow(saturate(sin(i.uv.y*3.14159)),.6)*.6+edge*.8)*(.65+.35*noise); bright=1.3+edge*2;}
    else if(_Mode<1.5) {float r=length(p);a=pow(saturate(1-r),2);bright=1.5+pow(saturate(1-r*3),3)*3;}
    else if(_Mode<2.5) {float r=length(p);a=exp(-pow((r-.86)*38,2))+exp(-pow((r-.72)*65,2))*.3; a*=.65+.35*noise;bright=2.2;}
    else {a=pow(saturate(1-length(p)),1.2)*(.3+noise*.7);bright=1;}
    float3 color=_Tint.rgb*i.c.rgb;
    if(_Mode<.5)color=lerp(color,float3(.8,.9,1),exp(-pow((i.uv.y-.96)*38,2))*.5);
    return half4(color*bright,saturate(a*_Tint.a*i.c.a*_Fade));
   }
   ENDHLSL
  }
 }
}
