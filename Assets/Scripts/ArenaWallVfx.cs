using UnityEngine;

namespace FighterArena
{
    // 시각적인 솟음은 셰이더에서 처리하여 벽 충돌체와 가두기 판정이 흔들리지 않습니다.
    [DefaultExecutionOrder(120)]
    public class ArenaWallVfx : MonoBehaviour
    {
        public ArenaStoneWall wall;
        public ParticleSystem dust, debris;
        int previous;
        double expires;
        float raisedAt=-100;
        MaterialPropertyBlock block;
        public int BreaksPlayed {get;private set;}
        void Awake() { block=new MaterialPropertyBlock(); }
        void LateUpdate()
        {
            if(!wall.IsSpawned) return;
            int mask=wall.Standing?wall.Intact.Value:0;
            if(mask!=0 && (previous==0 || expires!=wall.Expires.Value))
            {
                raisedAt=Time.time; expires=wall.Expires.Value;
                for(int i=0;i<wall.links.visuals.Length;i++)
                    if((mask&(1<<i))!=0) Puff(wall.links.visuals[i].transform.position-Vector3.up*1.1f,false);
            }
            for(int i=0;i<wall.links.visuals.Length;i++)
            {
                if((previous&(1<<i))!=0 && (mask&(1<<i))==0 && wall.links.match.Live.Value) { Puff(wall.links.visuals[i].transform.position,true); BreaksPlayed++; }
                block.Clear(); block.SetFloat("_Rise",Mathf.SmoothStep(0,1,(Time.time-raisedAt-i*.025f)/.22f));
                wall.links.visuals[i].SetPropertyBlock(block);
            }
            previous=mask;
        }
        void Puff(Vector3 p,bool broken)
        {
            ArenaCombatVfx.Emit(dust,p,new Color(.45f,.35f,.25f,.7f),broken?30:20,broken?1.8f:1.1f,broken?1.2f:.95f);
            ArenaCombatVfx.Emit(debris,p,new Color(.48f,.39f,.28f),broken?30:9,broken?4.8f:2f,broken?.22f:.12f);
        }
    }
}
