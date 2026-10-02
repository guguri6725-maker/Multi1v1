using UnityEngine;

namespace FighterArena
{
    // 이미 동기화된 투사체와 폭발 시각을 읽어 양쪽 PC에서 효과를 재생합니다.
    [DefaultExecutionOrder(110)]
    public class ArenaThrowableVfx : MonoBehaviour
    {
        public ArenaThrowables source;
        public TrailRenderer flightTrail;
        public LineRenderer[] bolts;
        public MeshRenderer shockRing;
        public ParticleSystem electricSparks, dust;
        public Color electricColor=new Color(.2f,.7f,1f);
        double seenBurst=-100;
        float burstTime=-100,nextFlight,nextSlow;
        Vector3 center;
        int lastKind;
        MaterialPropertyBlock block;
        readonly Vector3[] points=new Vector3[12];
        public int BurstsPlayed {get;private set;}
        void Awake() { block=new MaterialPropertyBlock(); Hide(); }
        void OnDisable() { Hide(); }
        void Hide()
        {
            if(flightTrail!=null) { flightTrail.emitting=false; flightTrail.Clear(); }
            if(shockRing!=null) shockRing.enabled=false;
            foreach(var b in bolts) if(b!=null) b.enabled=false;
            if(electricSparks!=null) electricSparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(dust!=null) dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            burstTime=-100; lastKind=0;
        }
        void LateUpdate()
        {
            if(!source.IsSpawned || !source.links.fighter.match.Live.Value) { Hide(); seenBurst=source.BurstAt.Value; return; }
            int kind=source.FlightKind.Value;
            if(kind!=0)
            {
                flightTrail.transform.position=source.links.projectile.position;
                if(lastKind==0) flightTrail.Clear();
                flightTrail.startColor=kind==2?electricColor:new Color(.7f,.45f,.18f);
                flightTrail.endColor=new Color(0,0,0,0);
                if(Time.time>nextFlight)
                {
                    nextFlight=Time.time+.035f;
                    ArenaCombatVfx.Emit(kind==2?electricSparks:dust,flightTrail.transform.position,kind==2?electricColor:new Color(.4f,.29f,.16f,.4f),2,.4f,kind==2?.075f:.16f);
                }
            }
            flightTrail.emitting=kind!=0; lastKind=kind;
            if(source.BurstAt.Value!=seenBurst)
            {
                seenBurst=source.BurstAt.Value;
                if(source.links.fighter.Now-seenBurst<.7) PreviewBurst(source.ImpactPosition.Value);
            }
            float age=Time.time-burstTime;
            bool visible=age>=0 && age<.65f;
            shockRing.enabled=visible;
            if(visible)
            {
                shockRing.transform.position=center+Vector3.up*.055f;
                float diameter=Mathf.Lerp(.4f,source.thunderRadius*2,Mathf.Clamp01(age/.32f));
                shockRing.transform.localScale=new Vector3(diameter,diameter,diameter);
                block.Clear(); block.SetFloat("_Fade",Mathf.Pow(1-age/.65f,2)); shockRing.SetPropertyBlock(block);
            }
            for(int i=0;i<bolts.Length;i++)
            {
                bool on=visible && age<.4f && ((int)(age*35)+i)%4!=0;
                bolts[i].enabled=on;
                if(!on) continue;
                float angle=i*Mathf.PI*2/bolts.Length;
                Vector3 direction=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                int tick=(int)(age*24);
                for(int j=0;j<points.Length;j++)
                {
                    float t=j/(float)(points.Length-1);
                    float jitter=(Mathf.PerlinNoise(i*13+j*3,tick*.71f)-.5f)*.8f*Mathf.Sin(t*Mathf.PI);
                    points[j]=center+direction*(t*source.thunderRadius*Mathf.Clamp01(age*12+.3f))+Vector3.Cross(direction,Vector3.up)*jitter+Vector3.up*(.12f+Mathf.Abs(jitter)*1.8f);
                }
                bolts[i].SetPositions(points);
                bolts[i].widthMultiplier=Mathf.Lerp(.065f,.012f,age/.4f);
            }
            var f=source.links.fighter;
            if(f.MoveMultiplier<1 && f.Health.Value>0 && Time.time>nextSlow)
            {
                nextSlow=Time.time+.09f;
                ArenaCombatVfx.Emit(electricSparks,f.transform.position+Vector3.up*.25f,electricColor,3,.6f,.06f);
            }
            // 기존의 불투명한 폭발 원판은 새로운 투명 충격파로 대체합니다.
            source.links.burstRenderer.enabled=false;
        }
        public void PreviewBurst(Vector3 position)
        {
            center=position; burstTime=Time.time; BurstsPlayed++;
            ArenaCombatVfx.Emit(electricSparks,center+Vector3.up*.2f,new Color(.45f,.8f,1f),75,5f,.13f);
        }
    }
}
