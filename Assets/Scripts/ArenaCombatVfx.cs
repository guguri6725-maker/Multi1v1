using UnityEngine;

namespace FighterArena
{
    // Hierarchy의 고정 풀을 재사용합니다. 효과는 로컬 예측을 따르며 전투 판정에는 관여하지 않습니다.
    [DefaultExecutionOrder(100)]
    public class ArenaCombatVfx : MonoBehaviour
    {
        public ArenaFighter fighter;
        public MeshRenderer[] ghosts;
        public MeshFilter ribbon;
        public MeshRenderer ribbonRenderer;
        public ParticleSystem sparks;
        public Color teamColor = new Color(.12f, .65f, 1f);
        [Range(.1f,.7f)] public float ghostLifetime = .38f;
        [Range(.04f,.3f)] public float trailLifetime = .13f;
        public int GhostsEmitted { get; private set; }
        public int RibbonSamples { get; private set; }
        readonly Vector3[] roots = new Vector3[32], tips = new Vector3[32];
        readonly float[] times = new float[32];
        readonly Vector3[] vertices = new Vector3[64];
        readonly Vector2[] uv = new Vector2[64];
        readonly Color[] colors = new Color[64];
        readonly int[] triangles = new int[186];
        float[] born;
        int head, count, ghostIndex, epoch = -1;
        float nextGhost, nextSpark;
        bool wasSwing;
        double seenFeedback=-100,seenParry=-100;
        Vector3 previous;
        Mesh mesh;
        MaterialPropertyBlock block;
        public bool PreviewSwing, PreviewDash;

        void Awake()
        {
            born = new float[ghosts.Length];
            for(int i=0;i<born.Length;i++) { born[i]=-100; ghosts[i].enabled=false; }
            block = new MaterialPropertyBlock();
            mesh = new Mesh { name="Runtime sword ribbon" }; mesh.MarkDynamic();
            ribbon.sharedMesh = mesh;
            for(int i=0;i<31;i++) { int j=i*6,k=i*2; triangles[j]=k; triangles[j+1]=k+1; triangles[j+2]=k+2; triangles[j+3]=k+1; triangles[j+4]=k+3; triangles[j+5]=k+2; }
            previous = fighter.transform.position;
        }
        void OnDestroy() { if(mesh!=null) Destroy(mesh); }
        void LateUpdate()
        {
            if(!fighter.IsSpawned) { Clear(); return; }
            if(epoch!=fighter.RoundEpoch.Value) { epoch=fighter.RoundEpoch.Value; Clear(); previous=fighter.transform.position; }
            bool live = fighter.match.Live.Value && fighter.Health.Value>0;
            var a=fighter.VisualAction;
            float age=(float)fighter.VisualActionAge;
            float moved=Vector3.Distance(previous,fighter.transform.position);
            bool dash= live && (PreviewDash || fighter.VisualDashActive && moved<2f);
            previous=fighter.transform.position;
            if(dash && Time.time>=nextGhost)
            {
                nextGhost=Time.time+.045f;
                var g=ghosts[ghostIndex];
                g.transform.SetPositionAndRotation(fighter.transform.position,fighter.transform.rotation);
                born[ghostIndex]=Time.time; ghostIndex=(ghostIndex+1)%ghosts.Length; GhostsEmitted++;
                Emit(sparks,fighter.transform.position+Vector3.up*.25f,teamColor,5,1.1f,.065f);
            }
            for(int i=0;i<ghosts.Length;i++)
            {
                float t=(Time.time-born[i])/ghostLifetime;
                ghosts[i].enabled=live && t<1;
                if(t<1) { block.Clear(); block.SetColor("_Tint",teamColor); block.SetFloat("_Fade",Mathf.Clamp01(1-t)); ghosts[i].SetPropertyBlock(block); }
            }
            bool swing=live && fighter.Equipped.Value==1 && (PreviewSwing || a==CombatAction.Whirlwind || ArenaCombat.IsLight(a) && age>=ArenaCombat.Windup(a)-.035f && age<ArenaCombat.Windup(a)+.18f || (a==CombatAction.Heavy || a==CombatAction.Riposte) && age>.14f && age<.62f);
            if(swing && !wasSwing) { count=0; head=0; }
            if(swing)
            {
                roots[head]=fighter.swordPivot.TransformPoint(Vector3.up*.1f);
                tips[head]=fighter.swordPivot.TransformPoint(Vector3.up*1.34f);
                times[head]=Time.time; head=(head+1)%32; count=Mathf.Min(32,count+1); RibbonSamples++;
                if(Time.time>=nextSpark) { nextSpark=Time.time+.035f; Emit(sparks,tips[(head+31)%32],teamColor,2,.45f,.045f); }
            }
            wasSwing=swing;
            while(count>0 && Time.time-times[(head-count+32)%32]>trailLifetime) count--;
            ribbonRenderer.enabled=count>1 && live;
            if(count>1)
            {
                for(int i=0;i<32;i++)
                {
                    int n=(head-count+Mathf.Min(i,count-1)+32)%32;
                    vertices[i*2]=ribbon.transform.InverseTransformPoint(roots[n]); vertices[i*2+1]=ribbon.transform.InverseTransformPoint(tips[n]);
                    float fade=Mathf.Clamp01(1-(Time.time-times[n])/trailLifetime);
                    Color c=a==CombatAction.Riposte?new Color(1,.68f,.16f):teamColor; c.a=fade*fade*(fighter.Local?.38f:.75f);
                    colors[i*2]=colors[i*2+1]=c; uv[i*2]=new Vector2(i/31f,0); uv[i*2+1]=new Vector2(i/31f,1);
                }
                mesh.vertices=vertices; mesh.colors=colors; mesh.uv=uv; mesh.triangles=triangles; mesh.RecalculateBounds();
            }
            // 피격/방어/패링은 새로 도착한 사건마다 한 번만 재생합니다.
            if(live && seenFeedback!=fighter.FeedbackAt.Value)
            {
                seenFeedback=fighter.FeedbackAt.Value;
                if(fighter.Now-seenFeedback<.25)
                {
                    bool broken=fighter.Feedback.Value==5;
                    Emit(sparks,fighter.transform.position+Vector3.up*1.3f+fighter.transform.forward*.55f,broken?new Color(1,.6f,.15f):fighter.Feedback.Value==2?new Color(.5f,.8f,1):new Color(1,.35f,.16f),broken?32:18,broken?3.4f:2.3f,broken?.12f:.09f);
                }
            }
            if(live && seenParry!=fighter.ParryAt.Value)
            {
                seenParry=fighter.ParryAt.Value;
                if(fighter.Now-seenParry<.25) Emit(sparks,fighter.swordPivot.TransformPoint(Vector3.up*.8f),new Color(1,.75f,.25f),35,3,.11f);
            }
            if(live && a==CombatAction.Charging && age>.7f && Time.time>nextSpark)
            {
                nextSpark=Time.time+.07f; Emit(sparks,fighter.swordPivot.TransformPoint(Vector3.up*1.3f),teamColor,age>=ArenaCombat.ChargeTime?3:1,.2f,.05f);
            }
        }
        void Clear()
        {
            count=0; wasSwing=false;
            if(ribbonRenderer!=null) ribbonRenderer.enabled=false;
            for(int i=0;i<ghosts.Length;i++) { ghosts[i].enabled=false; if(born!=null) born[i]=-100; }
            if(sparks!=null) sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        public static void Emit(ParticleSystem ps,Vector3 p,Color color,int number,float speed,float size)
        {
            if(ps==null) return;
            // EmitParams만 바꾸므로 Inspector에 저장된 입자 설정과 머티리얼은 유지됩니다.
            for(int i=0;i<number;i++)
            {
                var e=new ParticleSystem.EmitParams { position=p,velocity=Random.onUnitSphere*Random.Range(speed*.3f,speed)+Vector3.up*.4f,startColor=color,startSize=Random.Range(size*.5f,size),startLifetime=Random.Range(.25f,.7f),rotation3D=Random.rotation.eulerAngles };
                ps.Emit(e,1);
            }
        }
    }
}
