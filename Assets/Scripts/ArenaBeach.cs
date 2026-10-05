using UnityEngine;

// Cosmetic beach scenery. The shared arena floor, barriers and goals retain their colliders.
public class ArenaBeach : MonoBehaviour {
    Material sandMaterial, beachSky, previousSky;
    Color oldAmbient,oldFog,oldSunColor; float oldFogDensity,oldSunIntensity;Quaternion oldSunRotation; Light sunlight;
    void ApplyAtmosphere(){
        RenderSettings.skybox=beachSky;RenderSettings.ambientLight=new Color(.72f,.81f,.90f);RenderSettings.fogColor=new Color(.72f,.85f,.92f);RenderSettings.fogDensity=.0025f;
        sunlight.color=new Color(1f,.95f,.82f);sunlight.intensity=1.45f;sunlight.transform.rotation=Quaternion.Euler(34,-110,0);RenderSettings.sun=sunlight;
    }
    public void RestoreAtmosphere(){RenderSettings.skybox=previousSky;RenderSettings.ambientLight=oldAmbient;RenderSettings.fogColor=oldFog;RenderSettings.fogDensity=oldFogDensity;if(sunlight){sunlight.color=oldSunColor;sunlight.intensity=oldSunIntensity;sunlight.transform.rotation=oldSunRotation;}}
    void Ellipsoid(string name,Vector3 p,Vector3 size,Material material){var o=GameObject.CreatePrimitive(PrimitiveType.Sphere);o.name=name;o.transform.SetParent(transform);o.transform.position=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=material;Destroy(o.GetComponent<Collider>());}

    public void ApplySurface(){ApplyAtmosphere();GameObject.Find("Octagonal corner-goal pitch").GetComponent<Renderer>().sharedMaterial=sandMaterial;var f=GameObject.Find("Neighborhood foundation");f.GetComponent<Renderer>().sharedMaterial=sandMaterial;f.transform.localScale=new Vector3(33,.7f,80);}
    Mesh water; Vector3[] points; Color[] tint; ParticleSystem spray; float nextSplash=7, surgeUntil;
    public static float WaveHeight(float x,float z,float time){return -.38f+Mathf.Sin(x*.58f-time*1.8f)*.17f+Mathf.Sin(x*.24f+z*.15f-time*.9f)*.10f;}
    Material Material(Color c){var m=new Material(Shader.Find("Standard"));m.color=c;m.SetFloat("_Glossiness",.6f);return m;}
    GameObject Box(string name,Vector3 p,Vector3 size,Color color){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(transform);o.transform.position=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=Material(color);Destroy(o.GetComponent<Collider>());return o;}
    public void Build(){
        previousSky=RenderSettings.skybox;oldAmbient=RenderSettings.ambientLight;oldFog=RenderSettings.fogColor;oldFogDensity=RenderSettings.fogDensity;
        sunlight=GameObject.Find("Warm evening light").GetComponent<Light>();oldSunColor=sunlight.color;oldSunIntensity=sunlight.intensity;oldSunRotation=sunlight.transform.rotation;
        var skyAsset=Resources.Load<Material>("BeachSky");beachSky=skyAsset?new Material(skyAsset):new Material(Shader.Find("Skybox/Procedural"));
        beachSky.SetFloat("_SunSize",.045f);beachSky.SetFloat("_AtmosphereThickness",.85f);beachSky.SetColor("_SkyTint",new Color(.38f,.58f,.8f));beachSky.SetColor("_GroundColor",new Color(.75f,.72f,.63f));beachSky.SetFloat("_Exposure",1.2f);
        var cloud=Material(new Color(.97f,.98f,1));cloud.SetFloat("_Glossiness",0);
        for(int i=0;i<9;i++){float x=-65+i*17,z=32+(i%3)*14;for(int n=0;n<4;n++)Ellipsoid("Soft coastal cloud",new Vector3(x+n*5,23+Mathf.Sin(n*2+i)*1.3f,z),new Vector3(13,3.2f,7),cloud);}

        var sand=Material(new Color(.78f,.64f,.40f));sand.SetFloat("_Glossiness",0);sandMaterial=sand;
        var texture=new Texture2D(128,128);var random=new System.Random(63);var pixels=new Color[128*128];
        for(int i=0;i<pixels.Length;i++){float v=.83f+(float)random.NextDouble()*.25f;pixels[i]=new Color(v,v*.94f,v*.79f);}
        texture.SetPixels(pixels);texture.Apply();texture.wrapMode=TextureWrapMode.Repeat;sand.mainTexture=texture;sand.mainTextureScale=new Vector2(22,22);
        GameObject.Find("Octagonal corner-goal pitch").GetComponent<Renderer>().sharedMaterial=sand;
        var baseObject=GameObject.Find("Neighborhood foundation");baseObject.GetComponent<Renderer>().sharedMaterial=sand;baseObject.transform.localScale=new Vector3(33,.7f,80);
        Box("Beach beyond court",new Vector3(-24,-.18f,0),new Vector3(20,.2f,95),new Color(.84f,.71f,.48f));
        Box("Distant sea horizon",new Vector3(350,-.65f,0),new Vector3(520,.1f,800),new Color(.08f,.44f,.57f));
        Box("Wet shoreline",new Vector3(18,-.28f,0),new Vector3(7,.1f,95),new Color(.56f,.48f,.33f));
        for(int i=0;i<16;i++){
            float z=-43+(i%8)*12;float palmX=-21-(i/8)*8;var trunk=Box("Palm trunk",new Vector3(palmX,2.5f,z),new Vector3(.38f,5,.38f),new Color(.39f,.25f,.12f));
            for(int n=0;n<6;n++){float angle=n*60;var leaf=Box("Palm frond",new Vector3(palmX,5,z)+Quaternion.Euler(0,angle,0)*Vector3.forward*1.7f,new Vector3(.6f,.12f,4),new Color(.16f,.38f,.16f));leaf.transform.rotation=Quaternion.Euler(12,angle,0);}
            if(i<8)Box("Beach bench",new Vector3(-18,.5f,z),new Vector3(1.1f,.18f,3),new Color(.61f,.39f,.19f));
        }
        var festival=new GameObject("Beach festival");festival.transform.SetParent(transform,false);festival.AddComponent<ArenaBeachFestival>().Build();
        const int nx=64,nz=80;points=new Vector3[(nx+1)*(nz+1)];tint=new Color[points.Length];var triangles=new int[nx*nz*6];int t=0;
        for(int j=0;j<=nz;j++)for(int i=0;i<=nx;i++)points[j*(nx+1)+i]=new Vector3(19+i*1.4f,0,-65+j*1.6f);
        for(int j=0;j<nz;j++)for(int i=0;i<nx;i++){int a=j*(nx+1)+i;triangles[t++]=a;triangles[t++]=a+nx+1;triangles[t++]=a+1;triangles[t++]=a+1;triangles[t++]=a+nx+1;triangles[t++]=a+nx+2;}
        water=new Mesh();water.name="Animated shore water";water.vertices=points;water.triangles=triangles;water.MarkDynamic();
        var ocean=new GameObject("Sea waves");ocean.transform.SetParent(transform);ocean.AddComponent<MeshFilter>().sharedMesh=water;ocean.AddComponent<MeshRenderer>().sharedMaterial=Material(new Color(.055f,.51f,.64f));
        // White travelling crest strips, independently animated along the shore.
        for(int i=0;i<7;i++)Box("Foam crest "+i,new Vector3(20+i*3,.02f,0),new Vector3(.15f,.055f,110),new Color(.85f,.96f,.96f));
        var splash=new GameObject("Wave spray");splash.transform.SetParent(transform);spray=splash.AddComponent<ParticleSystem>();
        var main=spray.main;main.loop=false;main.playOnAwake=false;main.startLifetime=1.8f;main.startSpeed=0;main.startSize=.075f;main.startColor=new Color(.71f,.92f,1);main.gravityModifier=1;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=600;
        var emission=spray.emission;emission.enabled=false;var shape=spray.shape;shape.enabled=false;
        var droplet=GameObject.CreatePrimitive(PrimitiveType.Sphere);droplet.SetActive(false);var particleRenderer=spray.GetComponent<ParticleSystemRenderer>();particleRenderer.renderMode=ParticleSystemRenderMode.Mesh;particleRenderer.mesh=droplet.GetComponent<MeshFilter>().sharedMesh;particleRenderer.sharedMaterial=Material(new Color(.65f,.87f,1));Destroy(droplet);
        Animate(0);
    }
    public void Animate(float time){
        if(!water)return;float surge=Mathf.Clamp01((surgeUntil-Time.time)/2.5f);for(int i=0;i<points.Length;i++)points[i].y=WaveHeight(points[i].x,points[i].z,time)+surge*.38f*Mathf.Sin(points[i].x*.7f-time*4.8f);water.vertices=points;water.RecalculateNormals();water.RecalculateBounds();
        for(int i=0;i<7;i++){var crest=transform.Find("Foam crest "+i);float x=19+Mathf.Repeat(i*3-time*(1.3f+surge*2.8f),21);crest.position=new Vector3(x,WaveHeight(x,0,time)+.08f,0);}
    }
    public void Burst(){
        surgeUntil=Time.time+2.5f;
        // Droplets cross above the barrier and fall onto the sea-side half of the court.
        for(int i=0;i<220;i++){var e=new ParticleSystem.EmitParams();e.position=new Vector3(19,.4f,Random.Range(-13f,13f));e.velocity=new Vector3(Random.Range(-11f,-6f),Random.Range(5f,8f),Random.Range(-2f,2f));e.startLifetime=Random.Range(1.3f,2.1f);spray.Emit(e,1);}
    }
    void Update(){Animate(Time.time);if(Time.time>=nextSplash){Burst();nextSplash=Time.time+Random.Range(10f,18f);}}
}
