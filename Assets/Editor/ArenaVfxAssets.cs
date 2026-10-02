using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace FighterArena
{
    // 텍스처와 메시 에셋만 제작합니다. 씬 오브젝트와 Inspector 연결은 MCP에서 직접 설정합니다.
    public static class ArenaVfxAssets
    {
        public static string Build()
        {
            const string folder="Assets/VFX/Generated";
            Directory.CreateDirectory(folder);
            for(int mode=0;mode<2;mode++)
            {
                var t=new Texture2D(256,256,TextureFormat.RGBA32,true);
                for(int y=0;y<256;y++) for(int x=0;x<256;x++)
                {
                    float u=x/256f,v=y/256f;
                    float n=Mathf.PerlinNoise(u*9+7,v*9+3)*.6f+Mathf.PerlinNoise(u*31,v*31)*.25f+Mathf.PerlinNoise(u*87,v*87)*.15f;
                    Color c=new Color(n,n,n,1);
                    if(mode==1)
                    {
                        float px=u*7,py=v*7,first=100,second=100;
                        for(int iy=-1;iy<=1;iy++) for(int ix=-1;ix<=1;ix++)
                        {
                            int cx=Mathf.FloorToInt(px)+ix,cy=Mathf.FloorToInt(py)+iy;
                            float hx=Mathf.Repeat(Mathf.Sin(cx*127.1f+cy*311.7f)*43758.54f,1),hy=Mathf.Repeat(Mathf.Sin(cx*269.5f+cy*183.3f)*43758.54f,1);
                            float d=new Vector2(cx+hx-px,cy+hy-py).sqrMagnitude;
                            if(d<first) { second=first;first=d; } else if(d<second) second=d;
                        }
                        float crack=Mathf.SmoothStep(0,1,(second-first)*19);
                        float stone=(.36f+n*.6f)*Mathf.Lerp(.14f,1,crack);
                        c=new Color(stone,stone*.93f,stone*.82f,1);
                    }
                    t.SetPixel(x,y,c);
                }
                t.Apply(); File.WriteAllBytes(folder+(mode==0?"/EnergyNoise.png":"/BasaltCracks.png"),t.EncodeToPNG()); Object.DestroyImmediate(t);
            }
            AssetDatabase.Refresh();
            foreach(var fighter in Object.FindObjectsByType<ArenaFighter>(FindObjectsSortMode.None))
            {
                var combine=new List<CombineInstance>();
                foreach(var r in fighter.links.anatomy)
                {
                    var filter=r.GetComponent<MeshFilter>();
                    if(filter!=null) combine.Add(new CombineInstance { mesh=filter.sharedMesh,transform=fighter.transform.worldToLocalMatrix*r.transform.localToWorldMatrix });
                }
                var mesh=new Mesh { name=fighter.name+" Echo" };mesh.CombineMeshes(combine.ToArray(),true,true);
                Save(mesh,folder+"/"+fighter.name+"Echo.asset");
            }
            // 울퉁불퉁한 팔각형 단면을 쌓고 면별 노멀로 바위의 깨진 모서리를 만듭니다.
            var verts=new List<Vector3>();var tris=new List<int>();
            Vector2[] ring={new Vector2(-.36f,-.5f),new Vector2(.36f,-.5f),new Vector2(.5f,-.36f),new Vector2(.5f,.36f),new Vector2(.36f,.5f),new Vector2(-.36f,.5f),new Vector2(-.5f,.36f),new Vector2(-.5f,-.36f)};
            var grid=new Vector3[5,8];
            for(int y=0;y<5;y++) for(int i=0;i<8;i++) {float width=y==0?.98f:y==4?.8f:1;grid[y,i]=new Vector3(ring[i].x*width,-.5f+y*.25f+(y==0?0:Mathf.Sin(i*11+y*17)*.025f),ring[i].y*width);}
            for(int y=0;y<4;y++) for(int i=0;i<8;i++) {int j=(i+1)%8;Tri(verts,tris,grid[y,i],grid[y+1,i],grid[y,j]);Tri(verts,tris,grid[y,j],grid[y+1,i],grid[y+1,j]);}
            for(int i=0;i<8;i++) {Tri(verts,tris,new Vector3(0,.48f,0),grid[4,(i+1)%8],grid[4,i]);Tri(verts,tris,new Vector3(0,-.5f,0),grid[0,i],grid[0,(i+1)%8]);}
            var rock=new Mesh {name="Faceted basalt column"};rock.SetVertices(verts);rock.SetTriangles(tris,0);rock.RecalculateNormals();rock.RecalculateBounds();Save(rock,folder+"/BasaltColumn.asset");
            AssetDatabase.SaveAssets();return "Created 2 textures, 2 echo meshes and basalt mesh.";
        }
        static void Tri(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c) {int n=v.Count;v.Add(a);v.Add(b);v.Add(c);t.Add(n);t.Add(n+1);t.Add(n+2);}
        static void Save(Mesh mesh,string path) {var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old==null) AssetDatabase.CreateAsset(mesh,path);else {EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);}}
    }
}
