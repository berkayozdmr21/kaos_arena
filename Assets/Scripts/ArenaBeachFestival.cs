using UnityEngine;
using System.Collections.Generic;

// Visual decoration only. All props and spectators live outside the shared match arena.
public class ArenaBeachFestival : MonoBehaviour {
    readonly List<Transform> cheeringArms=new List<Transform>();
    readonly List<Transform> flags=new List<Transform>();
    readonly Dictionary<Color,Material> palette=new Dictionary<Color,Material>();
    Material Mat(Color c){if(!palette.TryGetValue(c,out var m)){m=new Material(Shader.Find("Standard"));m.color=c;m.SetFloat("_Glossiness",.08f);palette[c]=m;}return m;}
    GameObject Part(string name,Vector3 p,Vector3 size,Color c,Transform parent=null,PrimitiveType type=PrimitiveType.Cube){
        var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent?parent:transform,false);o.transform.localPosition=p;if(type==PrimitiveType.Capsule)size.y*=.5f;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=Mat(c);Destroy(o.GetComponent<Collider>());return o;
    }
    static readonly Color[] kits={new Color(.98f,.36f,.23f),new Color(.05f,.63f,.76f),new Color(.99f,.77f,.16f),new Color(.39f,.23f,.65f)};
    public void Build(){
        Color timber=new Color(.58f,.36f,.18f),ivory=new Color(.95f,.94f,.85f);
        for(int i=0;i<8;i++){
            float z=-31+(i%4)*20,x=-25-(i/4)*9;Color c=kits[i%4];
            Part("Umbrella pole",new Vector3(x,1.3f,z),new Vector3(.07f,2.6f,.07f),ivory);
            var v=new List<Vector3>{new Vector3(0,.5f,0)};for(int n=0;n<16;n++){float a=n*Mathf.PI*2/16;v.Add(new Vector3(Mathf.Cos(a)*1.9f,0,Mathf.Sin(a)*1.9f));}
            var mesh=new Mesh();mesh.vertices=v.ToArray();mesh.subMeshCount=2;
            for(int side=0;side<2;side++){var tris=new List<int>();for(int n=side;n<16;n+=2){tris.Add(0);tris.Add(1+(n+1)%16);tris.Add(1+n);}mesh.SetTriangles(tris,side);}mesh.RecalculateNormals();
            var roof=new GameObject("Striped beach umbrella");roof.transform.SetParent(transform,false);roof.transform.localPosition=new Vector3(x,2.6f,z);roof.AddComponent<MeshFilter>().sharedMesh=mesh;roof.AddComponent<MeshRenderer>().sharedMaterials=new[]{Mat(c),Mat(ivory)};
            var underside=Instantiate(mesh);for(int side=0;side<2;side++){var indices=underside.GetTriangles(side);for(int n=0;n<indices.Length;n+=3){int swap=indices[n];indices[n]=indices[n+2];indices[n+2]=swap;}underside.SetTriangles(indices,side);}underside.RecalculateNormals();var lining=new GameObject("Umbrella underside");lining.transform.SetParent(roof.transform,false);lining.transform.localPosition=Vector3.down*.025f;lining.AddComponent<MeshFilter>().sharedMesh=underside;lining.AddComponent<MeshRenderer>().sharedMaterials=new[]{Mat(c),Mat(ivory)};

            for(int n=0;n<2;n++){
                Vector3 p=new Vector3(x-1+n*2,.38f,z+1.3f);Part("Sun lounger",p,new Vector3(.72f,.12f,1.9f),c);
                var back=Part("Lounger back",p+new Vector3(0,.38f,-.9f),new Vector3(.72f,.95f,.12f),ivory);back.transform.localRotation=Quaternion.Euler(-25,0,0);
                for(int leg=-1;leg<=1;leg+=2)Part("Lounger legs",p+new Vector3(0,-.18f,leg*.65f),new Vector3(.66f,.35f,.08f),timber);
            }
            Part("Beach towel",new Vector3(x+2.6f,.015f,z),new Vector3(1.2f,.025f,2),c);
        }
        // Small wooden spectator stands, clear of diagonal goal exits.
        for(int side=-1;side<=1;side+=2)for(int row=0;row<2;row++){
            Part("Spectator bench",new Vector3(-2,.4f+row*.35f,side*(18+row*1.8f)),new Vector3(13,.2f,.65f),timber);
        }
        for(int i=0;i<28;i++){
            Vector3 p=i<12?new Vector3(-16.6f,0,-9+(i%12)*1.65f):new Vector3(-8+(i%8)*1.7f,0,i<20?-17:17);
            var root=new GameObject("Beach supporter "+i).transform;root.SetParent(transform,false);root.localPosition=p;root.rotation=Quaternion.LookRotation(-new Vector3(p.x,0,p.z));
            Color skin=i%3==0?new Color(.42f,.23f,.12f):i%3==1?new Color(.86f,.59f,.39f):new Color(.66f,.39f,.23f);Color kit=kits[i%4];
            Part("Supporter shirt",new Vector3(0,.97f,0),new Vector3(.43f,.47f,.25f),kit,root,PrimitiveType.Capsule);
            Part("Supporter face",new Vector3(0,1.4f,0),new Vector3(.35f,.39f,.32f),skin,root,PrimitiveType.Sphere);
            Part("Supporter hair",new Vector3(0,1.55f,-.015f),new Vector3(.36f,.12f,.32f),new Color(.10f,.06f,.04f),root,PrimitiveType.Sphere);
            for(int side=-1;side<=1;side+=2){
                Part("Supporter eye",new Vector3(side*.075f,1.43f,.151f),new Vector3(.03f,.045f,.015f),Color.black,root,PrimitiveType.Sphere);
                Part("Supporter shorts",new Vector3(side*.12f,.65f,0),new Vector3(.18f,.25f,.23f),new Color(.1f,.15f,.21f),root);
                Part("Supporter leg",new Vector3(side*.12f,.31f,0),new Vector3(.13f,.46f,.14f),skin,root);
                Part("Supporter sandal",new Vector3(side*.12f,.075f,.03f),new Vector3(.17f,.08f,.24f),ivory,root);
                var arm=new GameObject("Cheering arm").transform;arm.SetParent(root,false);arm.localPosition=new Vector3(side*.25f,1.12f,0);cheeringArms.Add(arm);
                Part("Supporter arm",new Vector3(0,.18f,0),new Vector3(.12f,.4f,.12f),skin,arm,PrimitiveType.Capsule);
                Part("Supporter hand",new Vector3(0,.4f,0),new Vector3(.13f,.13f,.13f),skin,arm,PrimitiveType.Sphere);
            }
        }
        Part("Beach kiosk",new Vector3(-30,1.1f,16),new Vector3(4.5f,2.2f,3),timber);
        Part("Kiosk counter",new Vector3(-27.65f,1.05f,16),new Vector3(.25f,.18f,3.3f),ivory);
        Part("Kiosk awning",new Vector3(-29,2.45f,16),new Vector3(6,.18f,3.8f),kits[0]);
        var sign=new GameObject("Beach cafe sign");sign.transform.SetParent(transform,false);sign.transform.localPosition=new Vector3(-27.45f,2,16);sign.transform.localRotation=Quaternion.Euler(0,-90,0);var text=sign.AddComponent<TextMesh>();text.text="PLAJ KAFE";text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=64;text.characterSize=.065f;text.anchor=TextAnchor.MiddleCenter;sign.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
        for(int i=0;i<6;i++){
            float z=-27+i*10;Part("Festival flagpole",new Vector3(-19,1.8f,z),new Vector3(.065f,3.6f,.065f),ivory);
            var flag=Part("Festival pennant",new Vector3(-18.5f,3.2f,z),new Vector3(1,.45f,.03f),kits[i%4]);flags.Add(flag.transform);
        }
        for(int i=0;i<3;i++)Part("Surfboard",new Vector3(-28+i*.65f,1.1f,19),new Vector3(.46f,2.1f,.12f),kits[i],null,PrimitiveType.Capsule);
    }
    void Update(){for(int i=0;i<cheeringArms.Count;i++)cheeringArms[i].localRotation=Quaternion.Euler(0,0,(i%2==0?-1:1)*(22+Mathf.Sin(Time.time*2.8f+i*.7f)*22));for(int i=0;i<flags.Count;i++)flags[i].localRotation=Quaternion.Euler(0,Mathf.Sin(Time.time*1.8f+i)*12,0);}
}
