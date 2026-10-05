using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class ArenaInput { public string kind = "input-beach-v1"; public float x, z, yaw, power, lift; public int restartId, characterId; public bool jump, slide, shot, ready; public string name; }
[Serializable] public class ArenaState {
    public string kind = "state-beach-v1"; public int you, phase, arenaId; public string notice;
    public Vector3 ball; public Vector3[] positions = new Vector3[4]; public float[] angles = new float[4];
    public int[] lives = new int[4]; public string[] names = new string[4]; public bool[] ready = new bool[4]; public bool[] present = new bool[4];
    public int restartId; public float restartCountdown;
    public int[] characters = new int[4], kits = new int[4];
    public bool holding; public float[] slidePose = new float[4]; public bool[] kicking = new bool[4];
    public float[] fallProgress = {-1,-1,-1,-1};
}

public class ArenaGame : MonoBehaviour {
    const int Port = 7777;
    const float Extent = 14f, Corner = 12f, GoalWidth = 2.8f, GoalHeight = 2.2f, GoalDepth = 1.8f;
    const float SlideDuration = .72f, ChargeSeconds = 1.25f;
    const float FallDuration = 1.4f;
    readonly float[] downUntil = new float[4], protectedUntil = new float[4];
    readonly Vector3[] knockDirection = new Vector3[4];
    readonly int[] slideHits = new int[4];
    readonly Color[] colors = {new Color(.1f,.82f,.76f),new Color(1f,.38f,.27f),new Color(.98f,.77f,.23f),new Color(.48f,.43f,.95f)};
    readonly Vector3[] corners = {new Vector3(-Corner,0,-Corner),new Vector3(Corner,0,-Corner),new Vector3(Corner,0,Corner),new Vector3(-Corner,0,Corner)};
    readonly Dictionary<int,int> slots = new Dictionary<int,int>();
    readonly ArenaInput[] inputs = {new ArenaInput(),new ArenaInput(),new ArenaInput(),new ArenaInput()};
    readonly Transform[] avatars = new Transform[4], leftLeg = new Transform[4], rightLeg = new Transform[4], leftArm = new Transform[4], rightArm = new Transform[4];
    readonly Transform[] bodies = new Transform[4];
    readonly Transform[] leftKnee=new Transform[4],rightKnee=new Transform[4],leftElbow=new Transform[4],rightElbow=new Transform[4];
    readonly int[] shownCharacter={-1,-1,-1,-1},shownKit={-1,-1,-1,-1};
    readonly float[] stride=new float[4],smoothMotion=new float[4];
    int selectedCharacter,previewCharacter=-1,previewKit=-1;string kitSignature="";
    GameObject previewModel;Camera previewCamera;RenderTexture previewTexture;
    readonly Vector3[] slideDirection = new Vector3[4];
    readonly CharacterController[] controllers = new CharacterController[4];
    readonly GameObject[] shutters = new GameObject[4];
    readonly TextMesh[] labels = new TextMesh[4];
    readonly float[] vertical = new float[4], slideUntil = new float[4], slideCooldown = new float[4], shotCooldown = new float[4];
    ArenaState state = new ArenaState(); ArenaWire wire; Rigidbody ball; Camera cam; Material white, dark, concrete;
    bool host, practice, menu = true, paused, charging; int localSlot = -1, watched; float yaw, pitch = 24, nextSend, charge, shotLift, resetAt, matchTime;
    string address = "127.0.0.1", playerName = "Oyuncu", status = "Ilk saha / Windows test surumu";
    GUIStyle titleStyle, bodyStyle, buttonStyle; AudioSource audioSource;

    int selectedArena,shownArena=-1; ArenaBeach beach; readonly List<GameObject> neighborhood=new List<GameObject>();
    void ApplyArena(int id){
        id=Mathf.Clamp(id,0,1);if(shownArena==id)return;shownArena=id;
        foreach(var o in neighborhood)if(o)o.SetActive(id==0);
        if(id==1){if(!beach){beach=new GameObject("Beach scenery").AddComponent<ArenaBeach>();beach.Build();}beach.gameObject.SetActive(true);beach.ApplySurface();}
        else {if(beach){beach.RestoreAtmosphere();beach.gameObject.SetActive(false);}GameObject.Find("Octagonal corner-goal pitch").GetComponent<Renderer>().sharedMaterial=Mat(new Color(.12f,.32f,.33f));var f=GameObject.Find("Neighborhood foundation");f.transform.localScale=new Vector3(80,.7f,80);f.GetComponent<Renderer>().sharedMaterial=concrete;}
    }
    void Awake() {
        Application.runInBackground = true; Application.targetFrameRate = 90; Time.fixedDeltaTime = .02f;
        white = Mat(new Color(.88f,.92f,.86f)); dark = Mat(new Color(.055f,.085f,.12f)); concrete = Mat(new Color(.23f,.27f,.3f));
        BuildArena(); BuildPlayers(); BuildBall(); ApplyArena(0);
        cam = new GameObject("Match Camera").AddComponent<Camera>(); cam.cullingMask=~(1<<30);cam.fieldOfView = 65; cam.farClipPlane = 180;
        cam.gameObject.AddComponent<AudioListener>(); audioSource = gameObject.AddComponent<AudioSource>();
        state.notice = "KAOS ARENA";
    }
    System.Collections.IEnumerator Start() {
        var args=Environment.GetCommandLineArgs();
        if(Array.IndexOf(args,"-arenaCapture")>=0){
            if(Array.IndexOf(args,"-beach")>=0)selectedArena=1;Open(true,true);StartMatch();yield return null;
            CaptureView("arena-preview.png",new Vector3(21,23,-25),Vector3.zero);
            if(beach){beach.Animate(4);beach.Burst();yield return new WaitForSeconds(.65f);CaptureView("beach-sea-preview.png",new Vector3(-17,8,-17),new Vector3(17,0,3));}
            CaptureView("goal-preview.png",corners[0]-corners[0].normalized*6+Vector3.up*3,corners[0]+Vector3.up);
            ApplyBodyPose(0,1);CaptureView("slide-preview.png",Spawn(0)+new Vector3(3,1.5f,-1),Spawn(0)+Vector3.up*.6f);
            ApplyFallPose(0,.38f);CaptureView("fall-preview.png",Spawn(0)+new Vector3(3,1.7f,-1),Spawn(0)+Vector3.up*.5f);
            Application.Quit();yield break;
        }
        if(Array.IndexOf(args,"-arenaSmoke")<0)yield break;
        yield return null;
        try {
            for(int id=0;id<ArenaRosterRules.Count;id++){
                var test=ArenaRoster.Create(id,null,id);var renderers=test.GetComponentsInChildren<Renderer>();
                var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                Require(Mathf.Abs(bounds.size.y-1.95f)<.025f,"Equal visual height: "+ArenaRoster.Get(id).name);
                Require(test.GetComponentsInChildren<Collider>().Length==0,"Cosmetic model has no gameplay hitbox: "+id);
                ArenaRoster.Find(test.transform,"Knee_Left");ArenaRoster.Find(test.transform,"Elbow_Right");test.SetActive(false);ArenaRoster.DisposeVisual(test);
            }
            for(int i=0;i<4;i++)Require(Mathf.Approximately(controllers[i].height,1.85f)&&Mathf.Approximately(controllers[i].radius,.32f),"Shared controller dimensions: "+i);
            var same=new[]{0,0,0,0};var active=new[]{true,true,true,true};var kits=ArenaRosterRules.AssignKits(same,active,null,3);
            Require(new System.Collections.Generic.HashSet<int>(kits).Count==4,"Four copies receive four kit colors");
        }catch(Exception e){Debug.LogError("ARENA_SMOKE_FAIL: "+e);Application.Quit(1);yield break;}
        if(Array.IndexOf(args,"-beach")>=0)selectedArena=1;Open(true,true);StartMatch();
        inputs[0].x=1;inputs[0].jump=inputs[0].slide=inputs[0].shot=true;
        yield return new WaitForFixedUpdate();
        Require(state.holding&&ball.isKinematic,"Kickoff held before release");
        Require(Vector3.Distance(avatars[0].position,Spawn(0))<.01f,"Movement locked before release");
        yield return new WaitForSeconds(2.4f);
        try {
            if(selectedArena==1){Require(beach&&beach.GetComponentsInChildren<Collider>().Length==0,"Sea scenery has no gameplay collision");Require(GameObject.Find("Octagonal corner-goal pitch").GetComponent<MeshCollider>()!=null,"Sand retains shared floor collision");}
            Require(ball.position.y<2.5f,"Ball falls under gravity");
            for(int i=0;i<4;i++)Require(avatars[i].position.y>-.2f,"Player stays above floor "+i);
            // Exercise the authoritative tackle path, including protection against repeated hits.
            controllers[0].enabled=false;avatars[0].position=new Vector3(0,.08f,0);controllers[0].enabled=true;
            controllers[1].enabled=false;avatars[1].position=new Vector3(0,.08f,.7f);controllers[1].enabled=true;
            slideUntil[0]=Time.time+SlideDuration;slideDirection[0]=Vector3.forward;slideHits[0]=0;
            CheckSlideHits(0,avatars[0].position,avatars[0].position+Vector3.forward*.1f);
            Require(FallProgress(1)>=0,"Sliding contact knocks opponent down");
            Require(state.lives[1]==3,"Tackle does not cost a life");
            float firstEnd=downUntil[1];CheckSlideHits(0,avatars[0].position,avatars[0].position+Vector3.forward*.1f);
            Require(downUntil[1]==firstEnd,"Same slide cannot repeatedly stun victim");
            ApplyFallPose(1,.4f);Require(Mathf.Abs(Vector3.Dot(bodies[1].up,Vector3.up))<.3f,"Fall pose lies sideways on ground");
            var replicated=JsonUtility.FromJson<ArenaState>(JsonUtility.ToJson(state));Require(replicated.fallProgress[1]>=0,"Fall state survives network serialization");
            downUntil[1]=Time.time-.01f;Require(FallProgress(1)<0,"Fall control lock expires");
            ApplyBodyPose(1,0);Require(Vector3.Dot(bodies[1].up,Vector3.up)>.99f,"Player can return upright");
            ResetPlayers();Require(state.fallProgress[1]<0&&protectedUntil[1]==0,"Restart clears fall and protection");
            state.holding=false;Goal(0);Require(state.lives[0]==2,"Goal costs owner one life");
            Require(Vector3.Distance(ball.position,GoalRestart(0))<.01f,"Normal goal restarts with conceding player");
            for(int i=0;i<4;i++)Require(Vector3.Distance(avatars[i].position,Spawn(i))<.01f,"Goal returns player to corner "+i);
            Goal(0);Require(state.lives[0]==2,"Duplicate goal blocked during restart");
            state.holding=false;Goal(0);state.holding=false;Goal(0);Require(state.lives[0]==0,"Three goals eliminate owner");
            Require(Vector3.Distance(ball.position,new Vector3(0,3,0))<.01f,"Elimination restarts at centre");
            state.holding=false;Goal(0);Require(state.lives[0]==0,"Eliminated goal cannot lose lives");
            Require(state.lives[1]==3,"Other player lives survive elimination");
            Require(ShotVelocity(Vector3.forward,1,0).magnitude>ShotVelocity(Vector3.forward,0,0).magnitude,"Charge increases speed");
            Require(ShotVelocity(Vector3.forward,1,1).y>ShotVelocity(Vector3.forward,1,0).y+15,"Mouse pull increases loft");
            Require(ShotVelocity(Vector3.forward,1,1).y>ShotVelocity(Vector3.forward,.2f,1).y,"Power increases lob height");
            ApplyBodyPose(1,1);Require(Vector3.Dot(bodies[1].up,Vector3.up)<.3f,"Sliding body reclines near horizontal");
            foreach(var renderer in FindObjectsOfType<MeshRenderer>())if(renderer.gameObject.name=="Window")Require(renderer.bounds.size.x<1.2f&&renderer.bounds.size.y<1,"Facade windows stay correctly scaled");
            for(int i=1;i<3;i++)for(int n=0;n<3;n++){state.holding=false;Goal(i);}
            Require(state.phase==2&&state.lives[3]==3,"Last survivor wins");
            Debug.Log("ARENA_SMOKE_PASS");Application.Quit(0);
        }catch(Exception e){Debug.LogError("ARENA_SMOKE_FAIL: "+e);Application.Quit(1);}
    }
    void CaptureView(string name,Vector3 from,Vector3 target){
        cam.transform.position=from;cam.transform.LookAt(target);
        var rt=new RenderTexture(1280,720,24);var previous=RenderTexture.active;
        cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
        var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(Application.dataPath,"..",name),texture.EncodeToPNG());
        cam.targetTexture=null;RenderTexture.active=previous;rt.Release();Destroy(rt);Destroy(texture);
    }
    void Require(bool ok,string message){if(!ok)throw new Exception(message);Debug.Log("ARENA_CHECK: "+message);}
    Material Mat(Color c) { var m = new Material(Shader.Find("Standard")); m.color = c; m.SetFloat("_Glossiness", .12f); return m; }
    GameObject Shape(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat, Transform parent = null, bool solid = true) {
        var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
        go.transform.localPosition = pos; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!solid) Destroy(go.GetComponent<Collider>()); return go;
    }
    void Text(string value, Vector3 pos, float size, Color color, Transform parent = null) {
        var obj = new GameObject(value); obj.transform.SetParent(parent,false); obj.transform.localPosition = pos;
        var t = obj.AddComponent<TextMesh>(); t.text = value; t.fontSize = 64; t.characterSize = size; t.color = color; t.anchor = TextAnchor.MiddleCenter;
        t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");obj.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;
    }
    void BuildArena() {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.65f,.7f,.76f); RenderSettings.fog = true; RenderSettings.fogColor = new Color(.44f,.56f,.65f); RenderSettings.fogDensity = .005f;
        var sun = new GameObject("Warm evening light").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.15f; sun.color = new Color(1,.94f,.84f); sun.transform.rotation = Quaternion.Euler(62,-32,0); sun.shadows = LightShadows.Soft; sun.shadowStrength=.65f;
        Shape("Neighborhood foundation",PrimitiveType.Cube,new Vector3(0,-.45f,0),new Vector3(80,.7f,80),concrete);
        // A chamfer at each corner holds a diagonal goal, facing the centre.
        var vertices = new List<Vector3> {Vector3.zero};
        vertices.AddRange(new [] {new Vector3(-10,0,-14),new Vector3(10,0,-14),new Vector3(14,0,-10),new Vector3(14,0,10),new Vector3(10,0,14),new Vector3(-10,0,14),new Vector3(-14,0,10),new Vector3(-14,0,-10)});
        var tris = new List<int>(); for(int i=0;i<8;i++) {tris.Add(0);tris.Add(1+(i+1)%8);tris.Add(1+i);}
        var mesh = new Mesh {vertices=vertices.ToArray(),triangles=tris.ToArray()}; mesh.RecalculateNormals();var uv=new Vector2[vertices.Count];for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(vertices[i].x/28+.5f,vertices[i].z/28+.5f);mesh.uv=uv;
        var ground = new GameObject("Octagonal corner-goal pitch"); ground.AddComponent<MeshFilter>().sharedMesh=mesh; ground.AddComponent<MeshRenderer>().sharedMaterial=Mat(new Color(.12f,.32f,.33f)); ground.AddComponent<MeshCollider>().sharedMesh=mesh;
        var bounce = new PhysicMaterial("Angle-based wall rebound") {bounciness=.72f,dynamicFriction=.2f,staticFriction=.2f,bounceCombine=PhysicMaterialCombine.Maximum};
        for(int i=0;i<4;i++) {
            Vector3 a = vertices[1+i*2], b = vertices[2+i*2]; Wall(a,b,bounce);
            Vector3 c = vertices[2+i*2], d = vertices[1+(i*2+2)%8], mid=(c+d)*.5f, axis=(d-c).normalized;
            Wall(c,mid-axis*GoalWidth*.5f,bounce); Wall(mid+axis*GoalWidth*.5f,d,bounce);
        }
        for(int i=0;i<4;i++) {
            var goal = new GameObject("Goal "+(i+1)); goal.transform.position=corners[i]; goal.transform.rotation=Quaternion.LookRotation(-corners[i].normalized);
            var color = Mat(colors[i]);
            Shape("Left post",PrimitiveType.Cube,new Vector3(-GoalWidth/2,GoalHeight/2,0),new Vector3(.13f,GoalHeight,.13f),color,goal.transform);
            Shape("Right post",PrimitiveType.Cube,new Vector3(GoalWidth/2,GoalHeight/2,0),new Vector3(.13f,GoalHeight,.13f),color,goal.transform);
            Shape("Crossbar",PrimitiveType.Cube,new Vector3(0,GoalHeight,0),new Vector3(GoalWidth+.13f,.13f,.13f),color,goal.transform);
            BuildGoalNet(goal.transform,color);
            var trigger=Shape("Goal volume",PrimitiveType.Cube,new Vector3(0,GoalHeight/2,-.8f),new Vector3(GoalWidth-.13f,GoalHeight,.6f),white,goal.transform);
            trigger.GetComponent<Renderer>().enabled=false; trigger.GetComponent<BoxCollider>().isTrigger=true; var sensor=trigger.AddComponent<ArenaGoal>(); sensor.game=this;sensor.slot=i;
            shutters[i]=Shape("Elimination shutter",PrimitiveType.Cube,new Vector3(0,GoalHeight/2,0),new Vector3(GoalWidth,GoalHeight,.2f),dark,goal.transform);shutters[i].SetActive(false);
            Text("0"+(i+1),new Vector3(0,2.85f,0),.11f,colors[i],goal.transform);
            goal.transform.Find("0"+(i+1)).localRotation=Quaternion.Euler(0,180,0);
            var marker=Shape("Goal area",PrimitiveType.Cylinder,corners[i]*.77f+Vector3.up*.012f,new Vector3(3.4f,.008f,3.4f),color,null,false);
        }
        for(int i=0;i<64;i++) {float a=i*Mathf.PI*2/64,b=(i+1)*Mathf.PI*2/64;Line(new Vector3(Mathf.Sin(a)*3,.025f,Mathf.Cos(a)*3),new Vector3(Mathf.Sin(b)*3,.025f,Mathf.Cos(b)*3),.045f,white);}
        Line(new Vector3(-13.7f,.025f,0),new Vector3(13.7f,.025f,0),.045f,white);
        var rand=new System.Random(44);
        for(int i=0;i<20;i++) {
            float a=i*Mathf.PI*2/20, h=5+(float)rand.NextDouble()*10; Vector3 p=new Vector3(Mathf.Sin(a)*29,h/2,Mathf.Cos(a)*29);
            // Keep the parent unscaled: facade dimensions must not inherit the building mesh scale.
            var building=new GameObject("Neighborhood building");building.transform.position=p;
            neighborhood.Add(building);building.transform.rotation=Quaternion.LookRotation(p.normalized);
            Shape("Building shell",PrimitiveType.Cube,Vector3.zero,new Vector3(6,h,6),Mat(Color.Lerp(new Color(.22f,.32f,.4f),new Color(.72f,.46f,.32f),(float)rand.NextDouble())),building.transform);
            var glass=Mat(new Color(.64f,.79f,.84f));
            for(int y=0;y<(int)h/2;y++) for(int x=-1;x<=1;x++) Shape("Window",PrimitiveType.Cube,new Vector3(x*1.3f,y*1.8f-h/2+1.2f,-3.025f),new Vector3(.7f,.8f,.035f),glass,building.transform,false);
        }
        for(int i=0;i<4;i++) {
            Vector3 p = Quaternion.Euler(0,i*90,0)*new Vector3(0,0,18);
            Shape("Floodlight mast",PrimitiveType.Cube,p+Vector3.up*4,new Vector3(.16f,8,.16f),dark);
            var light=new GameObject("Floodlight").AddComponent<Light>();light.type=LightType.Point;light.range=22;light.intensity=1.5f;light.transform.position=p+Vector3.up*7;
        }
        Text("KAOS  /  ARENA",new Vector3(0,4,19),.23f,Color.white);
    }
    void BuildGoalNet(Transform goal,Material frame) {
        // Three-dimensional back, side and roof grids; the mouth remains completely open.
        for(int side=-1;side<=1;side+=2){
            NetLine(new Vector3(side*GoalWidth/2,.06f,0),new Vector3(side*GoalWidth/2,.06f,-GoalDepth),.065f,frame,goal);
            NetLine(new Vector3(side*GoalWidth/2,GoalHeight,0),new Vector3(side*GoalWidth/2,GoalHeight,-GoalDepth),.055f,frame,goal);
            NetLine(new Vector3(side*GoalWidth/2,0,-GoalDepth),new Vector3(side*GoalWidth/2,GoalHeight,-GoalDepth),.055f,frame,goal);
            for(int n=0;n<=8;n++){float z=-GoalDepth*n/8;NetLine(new Vector3(side*GoalWidth/2,.05f,z),new Vector3(side*GoalWidth/2,GoalHeight,z),.018f,white,goal);}
            for(int n=0;n<=10;n++){float y=GoalHeight*n/10;NetLine(new Vector3(side*GoalWidth/2,y,0),new Vector3(side*GoalWidth/2,y,-GoalDepth),.018f,white,goal);}
        }
        for(int n=0;n<=14;n++){float x=-GoalWidth/2+GoalWidth*n/14;
            NetLine(new Vector3(x,0,-GoalDepth),new Vector3(x,GoalHeight,-GoalDepth),.018f,white,goal);
            NetLine(new Vector3(x,GoalHeight,0),new Vector3(x,GoalHeight,-GoalDepth),.018f,white,goal);}
        for(int n=0;n<=10;n++){float y=GoalHeight*n/10;NetLine(new Vector3(-GoalWidth/2,y,-GoalDepth),new Vector3(GoalWidth/2,y,-GoalDepth),.018f,white,goal);}
        for(int n=0;n<=8;n++){float z=-GoalDepth*n/8;NetLine(new Vector3(-GoalWidth/2,GoalHeight,z),new Vector3(GoalWidth/2,GoalHeight,z),.018f,white,goal);}
    }
    void NetLine(Vector3 a,Vector3 b,float width,Material mat,Transform parent){
        var line=Shape("Goal net / frame",PrimitiveType.Cube,(a+b)*.5f,new Vector3(width,width,Vector3.Distance(a,b)),mat,parent,false);
        line.transform.localRotation=Quaternion.LookRotation(b-a);
    }
    void Line(Vector3 a,Vector3 b,float width,Material mat) {var obj=Shape("Pitch stripe",PrimitiveType.Cube,(a+b)/2,new Vector3(width,width,Vector3.Distance(a,b)),mat,null,false);obj.transform.rotation=Quaternion.LookRotation(b-a);}
    void Wall(Vector3 a,Vector3 b,PhysicMaterial physics) {
        var wall=Shape("Rebound barrier",PrimitiveType.Cube,(a+b)/2+Vector3.up*GoalHeight/4,new Vector3(.25f,GoalHeight/2,Vector3.Distance(a,b)+.08f),concrete);
        wall.transform.rotation=Quaternion.LookRotation(b-a);wall.GetComponent<Collider>().material=physics;
        Line(a+Vector3.up*GoalHeight/2,b+Vector3.up*GoalHeight/2,.065f,white);
    }
    void BuildPlayers() {
        for(int i=0;i<4;i++) {
            var obj=new GameObject("Player "+(i+1));avatars[i]=obj.transform;obj.transform.position=Spawn(i);
            var cc=obj.AddComponent<CharacterController>();cc.height=1.85f;cc.radius=.32f;cc.center=new Vector3(0,.94f,0);cc.stepOffset=.25f;controllers[i]=cc;
            var label=new GameObject("Player name");label.transform.SetParent(obj.transform,false);label.transform.localPosition=new Vector3(0,2.2f,0);
            labels[i]=label.AddComponent<TextMesh>();labels[i].anchor=TextAnchor.MiddleCenter;labels[i].fontSize=48;labels[i].characterSize=.045f;labels[i].color=colors[i];
            labels[i].font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.GetComponent<MeshRenderer>().sharedMaterial=labels[i].font.material;
            RebuildCharacter(i,i,i);obj.SetActive(false);
        }
    }
    void RebuildCharacter(int slot,int character,int kit){
        if(bodies[slot]){bodies[slot].gameObject.SetActive(false);ArenaRoster.DisposeVisual(bodies[slot].gameObject);}
        bodies[slot]=new GameObject("Body pose").transform;bodies[slot].SetParent(avatars[slot],false);
        var model=ArenaRoster.Create(character,bodies[slot],kit).transform;
        leftLeg[slot]=ArenaRoster.Wrap(ArenaRoster.Find(model,"Leg_Left"),bodies[slot]);
        rightLeg[slot]=ArenaRoster.Wrap(ArenaRoster.Find(model,"Leg_Right"),bodies[slot]);
        leftArm[slot]=ArenaRoster.Wrap(ArenaRoster.Find(model,"Arm_Left"),bodies[slot]);
        rightArm[slot]=ArenaRoster.Wrap(ArenaRoster.Find(model,"Arm_Right"),bodies[slot]);
        leftKnee[slot]=ArenaRoster.Wrap(ArenaRoster.Find(leftLeg[slot],"Knee_Left"),leftLeg[slot]);
        rightKnee[slot]=ArenaRoster.Wrap(ArenaRoster.Find(rightLeg[slot],"Knee_Right"),rightLeg[slot]);
        leftElbow[slot]=ArenaRoster.Wrap(ArenaRoster.Find(leftArm[slot],"Elbow_Left"),leftArm[slot]);
        rightElbow[slot]=ArenaRoster.Wrap(ArenaRoster.Find(rightArm[slot],"Elbow_Right"),rightArm[slot]);
        shownCharacter[slot]=character;shownKit[slot]=kit;
    }
    void ResolveKits(){
        string signature="";for(int i=0;i<4;i++)signature+=state.present[i]+":"+state.characters[i]+";";
        if(signature==kitSignature)return;kitSignature=signature;
        state.kits=ArenaRosterRules.AssignKits(state.characters,state.present,state.kits,UnityEngine.Random.Range(0,int.MaxValue));
    }
    void UpdateRosterPreview(){
        bool show=!menu&&state.phase==0&&localSlot>=0;
        if(previewCamera)previewCamera.enabled=show;
        if(!show)return;
        if(!previewCamera){
            previewTexture=new RenderTexture(512,512,24);previewTexture.name="Character selection";
            previewCamera=new GameObject("Roster preview camera").AddComponent<Camera>();previewCamera.cullingMask=1<<30;
            previewCamera.targetTexture=previewTexture;previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=new Color(.055f,.09f,.13f);
            previewCamera.transform.position=new Vector3(100,1.12f,3.8f);previewCamera.transform.LookAt(new Vector3(100,1.02f,0));previewCamera.fieldOfView=34;
        }
        int kit=state.characters[localSlot]==selectedCharacter?state.kits[localSlot]:selectedCharacter;
        if(previewCharacter!=selectedCharacter||previewKit!=kit){
            if(previewModel){previewModel.SetActive(false);ArenaRoster.DisposeVisual(previewModel);}
            previewModel=ArenaRoster.Create(selectedCharacter,null,kit);previewModel.transform.position=new Vector3(100,0,0);
            foreach(var child in previewModel.GetComponentsInChildren<Transform>(true))child.gameObject.layer=30;
            previewCharacter=selectedCharacter;previewKit=kit;
        }
        previewModel.transform.rotation=Quaternion.Euler(0,Mathf.Sin(Time.unscaledTime*.55f)*22,0);
    }
    void DrawRosterPicker(){
        if(localSlot<0)return;
        Panel(new Rect(550,110,685,560));
        GUI.Label(new Rect(575,120,600,35),"KARAKTERINI SEC  /  "+ArenaRoster.Get(selectedCharacter).name,titleStyle);
        if(previewTexture)GUI.DrawTexture(new Rect(725,162,310,310),previewTexture,ScaleMode.ScaleToFit,false);
        for(int id=0;id<ArenaRosterRules.Count;id++){
            GUI.backgroundColor=selectedCharacter==id?ArenaRoster.KitColor(id):Color.white;
            if(GUI.Button(new Rect(575+(id%5)*128,472+(id/5)*42,120,36),ArenaRoster.Get(id).name)){
                selectedCharacter=id;inputs[localSlot].characterId=id;inputs[localSlot].ready=false;
                if(host){state.characters[localSlot]=id;state.ready[localSlot]=false;ResolveKits();}
            }
        }GUI.backgroundColor=Color.white;
        GUI.Label(new Rect(575,585,640,65),"15 karakter / Ayni boy, hiz ve fizik.\nAyni karakter secilirse forma rengi otomatik ayrilir.",bodyStyle);
    }
    void BuildBall() {
        var obj=Shape("Ball",PrimitiveType.Sphere,new Vector3(0,3,0),Vector3.one*.46f,white);
        ball=obj.AddComponent<Rigidbody>();ball.mass=.43f;ball.drag=.25f;ball.angularDrag=.3f;ball.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;ball.interpolation=RigidbodyInterpolation.Interpolate;ball.isKinematic=true;
        obj.GetComponent<Collider>().material=new PhysicMaterial("Football") {bounciness=.65f,dynamicFriction=.45f,staticFriction=.45f};
        for(int i=0;i<6;i++){Vector3 dir=Quaternion.Euler(i*61,i*137,0)*Vector3.forward;Shape("Ball panel",PrimitiveType.Sphere,dir*.46f,Vector3.one*.25f,dark,obj.transform,false);}
    }
    Vector3 Spawn(int i) {return corners[i]*.71f+Vector3.up*.08f;}
    void Open(bool asHost,bool bots=false) {
        Close();host=asHost;practice=bots;menu=false;state=new ArenaState();state.arenaId=asHost?selectedArena:0;ApplyArena(state.arenaId);state.notice="Oyuncular bekleniyor";kitSignature="";
        for(int i=0;i<4;i++){inputs[i]=new ArenaInput();vertical[i]=slideUntil[i]=slideCooldown[i]=shotCooldown[i]=0;}
        if(asHost) {localSlot=0;state.present[0]=true;state.names[0]=SafeName(playerName);state.lives[0]=3;state.characters[0]=selectedCharacter;inputs[0].characterId=selectedCharacter;
            for(int i=0;i<4;i++) {state.positions[i]=Spawn(i);state.angles[i]=Quaternion.LookRotation(-corners[i]).eulerAngles.y;}
            if(bots){for(int i=1;i<4;i++){state.present[i]=true;state.ready[i]=true;state.names[i]="Bot "+i;state.lives[i]=3;state.characters[i]=i;}}
        }
        if(!bots) {wire=new ArenaWire();try{if(asHost)wire.Host(Port);else wire.Join(address,Port);}catch(Exception e){status=e.Message;Close();}}
        yaw=asHost?state.angles[0]:0;status=asHost?"Lobi acik / TCP 7777":"Baglaniliyor...";
    }
    void Close() {wire?.Dispose();wire=null;slots.Clear();localSlot=-1;menu=true;host=false;practice=false;paused=false;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;if(ball)ball.isKinematic=true;}
    string SafeName(string s) {if(string.IsNullOrWhiteSpace(s))return "Oyuncu";s=s.Replace("\n","").Replace("\r","").Replace("<","").Replace(">","");return s.Substring(0,Math.Min(10,s.Length));}
    void Receive() {
        if(wire==null)return;
        int count=0;while(count++<256 && wire.events.TryDequeue(out var ev)) {
            if(ev.data.StartsWith("!error")){status=ev.data;Close();break;}
            if(ev.data=="!connected") {
                if(host){int slot=-1;for(int i=1;i<4;i++)if(!state.present[i]){slot=i;break;}
                    if(slot<0||state.phase!=0){wire.Send(ev.peer,"!rejected");wire.Disconnect(ev.peer);continue;}
                    slots[ev.peer]=slot;state.present[slot]=true;state.lives[slot]=3;state.names[slot]="Oyuncu "+(slot+1);
                }continue;
            }
            if(ev.data=="!rejected"){status="Lobi dolu veya mac baslamis.";Close();break;}
            if(ev.data=="!disconnected") {
                if(!host){status="Host baglantisi kapandi. Mac iptal edildi.";Close();break;}
                if(slots.TryGetValue(ev.peer,out int s)){slots.Remove(ev.peer);state.present[s]=false;state.ready[s]=false;state.lives[s]=0;
                    if(state.phase==1){state.notice=state.names[s]+" ayrildi";CheckWinner();}}
                continue;
            }
            try {
                if(host && slots.TryGetValue(ev.peer,out int s)) {
                    var input=JsonUtility.FromJson<ArenaInput>(ev.data);if(input==null||input.kind!="input-beach-v1")continue;
                    if(!Finite(input.x)||!Finite(input.z)||!Finite(input.yaw)||!Finite(input.power)||!Finite(input.lift))continue;
                    if(input.characterId<0||input.characterId>=ArenaRosterRules.Count)continue;
                    input.x=Mathf.Clamp(input.x,-1,1);input.z=Mathf.Clamp(input.z,-1,1);input.power=Mathf.Clamp01(input.power);input.lift=Mathf.Clamp01(input.lift);
                    if(state.phase==1&&(state.holding||input.restartId!=state.restartId)){input.x=input.z=0;input.shot=input.jump=input.slide=false;input.yaw=state.angles[s];}
                    else {if(inputs[s].shot&&!input.shot){input.power=inputs[s].power;input.lift=inputs[s].lift;}input.shot|=inputs[s].shot;input.jump|=inputs[s].jump;input.slide|=inputs[s].slide;}
                    inputs[s]=input;
                    if(state.phase==0){bool changed=state.characters[s]!=input.characterId;state.characters[s]=input.characterId;state.ready[s]=input.ready&&!changed;state.names[s]=SafeName(input.name);ResolveKits();}
                } else if(!host) {
                    var update=JsonUtility.FromJson<ArenaState>(ev.data);if(update==null||update.kind!="state-beach-v1"||update.you<0||update.you>3||update.positions.Length!=4)continue;
                    bool fresh=localSlot<0||state.restartId!=update.restartId;state=update;localSlot=state.you;
                    if(fresh){yaw=state.angles[localSlot];CancelCharge();for(int i=0;i<4;i++){controllers[i].enabled=false;avatars[i].position=state.positions[i];}}
                }
            } catch(Exception){status="Gecersiz ag mesaji atlandi";}
        }
    }
    bool Finite(float f) {return !float.IsNaN(f)&&!float.IsInfinity(f);}
    void Update() {
        Receive();
        ApplyArena(menu?selectedArena:state.arenaId);
        if(host&&state.phase==0)ResolveKits();
        UpdateRosterPreview();
        if(!menu&&localSlot>=0) {
            if(Input.GetKeyDown(KeyCode.Escape)){paused=!paused;Cursor.lockState=paused?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=paused;}
            var input=inputs[localSlot];input.name=SafeName(playerName);input.characterId=selectedCharacter;
            bool playing=state.phase==1&&state.lives[localSlot]>0&&!paused&&!state.holding&&(host?Time.time>=downUntil[localSlot]:state.fallProgress[localSlot]<0);
            input.x=playing?Input.GetAxisRaw("Horizontal"):0;input.z=playing?Input.GetAxisRaw("Vertical"):0;
            if(playing){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;yaw+=Input.GetAxis("Mouse X")*2.5f;
                float mouseY=Input.GetAxis("Mouse Y");
                input.jump|=Input.GetKeyDown(KeyCode.Space);input.slide|=Input.GetMouseButtonDown(1);
                if(Input.GetMouseButtonDown(0)){charging=true;charge=shotLift=0;}
                if(charging){shotLift=Mathf.Clamp01(shotLift-mouseY*.065f);if(Input.GetMouseButton(0))charge=Mathf.Min(charge+Time.deltaTime/ChargeSeconds,1);}
                else pitch=Mathf.Clamp(pitch-mouseY*1.8f,8,62);
                if(charging&&Input.GetMouseButtonUp(0)){input.shot=true;input.power=charge;input.lift=shotLift;CancelCharge();}
            }else{Cursor.lockState=CursorLockMode.None;Cursor.visible=true;CancelCharge();input.shot=input.jump=input.slide=false;}
            input.yaw=yaw;input.restartId=state.restartId;
            if(!host&&Time.unscaledTime>=nextSend){nextSend=Time.unscaledTime+.033f;wire?.Send(0,JsonUtility.ToJson(input));input.shot=input.slide=input.jump=false;}
            if(state.phase==1&&state.lives[localSlot]<=0&&Input.GetKeyDown(KeyCode.Tab))watched=(watched+1)%5;
        }
        for(int i=0;i<4;i++) {
            bool visible=!menu&&state.present[i]&&(state.phase!=1||state.lives[i]>0);avatars[i].gameObject.SetActive(visible);
            shutters[i].SetActive(!menu&&state.phase!=0&&state.lives[i]<=0);
            if(!visible)continue;
            if(shownCharacter[i]!=state.characters[i]||shownKit[i]!=state.kits[i])RebuildCharacter(i,state.characters[i],state.kits[i]);
            if(!host){controllers[i].enabled=false;avatars[i].position=Vector3.Lerp(avatars[i].position,state.positions[i],1-Mathf.Exp(-18*Time.deltaTime));avatars[i].rotation=Quaternion.Slerp(avatars[i].rotation,Quaternion.Euler(0,state.angles[i],0),1-Mathf.Exp(-18*Time.deltaTime));}
            float motion=state.holding?0:(host?new Vector2(inputs[i].x,inputs[i].z).magnitude:Mathf.Clamp01(Vector3.Distance(avatars[i].position,state.positions[i])*8));
            smoothMotion[i]=Mathf.MoveTowards(smoothMotion[i],motion,Time.deltaTime*7);stride[i]+=Time.deltaTime*13*smoothMotion[i];
            float cycle=Mathf.Sin(stride[i]);float swing=cycle*32*smoothMotion[i];
            leftKnee[i].localRotation=Quaternion.Euler(Mathf.Max(0,-cycle)*44*smoothMotion[i],0,0);
            rightKnee[i].localRotation=Quaternion.Euler(Mathf.Max(0,cycle)*44*smoothMotion[i],0,0);
            leftElbow[i].localRotation=Quaternion.Euler(-18-15*smoothMotion[i],0,0);rightElbow[i].localRotation=Quaternion.Euler(-18-15*smoothMotion[i],0,0);
            leftLeg[i].localRotation=Quaternion.Euler(swing,0,0);rightLeg[i].localRotation=Quaternion.Euler(-swing,0,0);
            leftArm[i].localRotation=Quaternion.Euler(-swing*.7f,0,-8);rightArm[i].localRotation=Quaternion.Euler(swing*.7f,0,8);
            if(host?Time.time<shotCooldown[i]:state.kicking[i])rightLeg[i].localRotation=Quaternion.Euler(-65,0,0);
            float slide=host?SlidePose(i):state.slidePose[i];ApplyBodyPose(i,slide);
            float fall=host?FallProgress(i):state.fallProgress[i];if(fall>=0)ApplyFallPose(i,fall);
            else if(slide<.01f){bodies[i].localPosition+=Vector3.up*(Mathf.Abs(cycle)*.022f*smoothMotion[i]);bodies[i].localRotation=Quaternion.Euler(5*smoothMotion[i],0,-cycle*2*smoothMotion[i]);}
            if(slide>.01f||fall>=0){leftKnee[i].localRotation=Quaternion.identity;rightKnee[i].localRotation=Quaternion.identity;}
            labels[i].text=state.names[i];labels[i].transform.rotation=cam.transform.rotation;
        }
        if(!host&&!menu)ball.position=Vector3.Lerp(ball.position,state.ball,1-Mathf.Exp(-20*Time.deltaTime));
    }
    void CancelCharge(){charging=false;charge=shotLift=0;}
    float FallProgress(int i){return Time.time<downUntil[i]?Mathf.Clamp01(1-(downUntil[i]-Time.time)/FallDuration):-1;}
    void ApplyFallPose(int i,float progress){
        float pose=progress<.2f?Mathf.SmoothStep(0,1,progress/.2f):1-Mathf.SmoothStep(0,1,(progress-.65f)/.35f);
        float wobble=Mathf.Sin(progress*45)*pose;
        Quaternion rotation=Quaternion.Euler(-12*pose,0,92*pose+wobble*5);
        bodies[i].localRotation=rotation;
        bodies[i].localPosition=Vector3.up*Mathf.Lerp(.8f,.3f,pose)-rotation*(Vector3.up*.8f);
        leftLeg[i].localRotation=Quaternion.Euler(wobble*25,0,-18*pose);rightLeg[i].localRotation=Quaternion.Euler(-wobble*25,0,18*pose);
        leftArm[i].localRotation=Quaternion.Euler(wobble*35,0,-65*pose);rightArm[i].localRotation=Quaternion.Euler(-wobble*35,0,65*pose);
    }
    void CheckSlideHits(int attacker,Vector3 from,Vector3 to){
        if(!host||state.holding||Time.time>=slideUntil[attacker]||Time.time<downUntil[attacker])return;
        Vector3 path=to-from;path.y=0;
        for(int victim=0;victim<4;victim++){
            if(victim==attacker||!state.present[victim]||state.lives[victim]<=0||(slideHits[attacker]&(1<<victim))!=0||Time.time<protectedUntil[victim])continue;
            Vector3 delta=avatars[victim].position-from;
            if(Mathf.Abs(delta.y)>.65f)continue;delta.y=0;
            float t=path.sqrMagnitude>.0001f?Mathf.Clamp01(Vector3.Dot(delta,path)/path.sqrMagnitude):0;
            if((delta-path*t).sqrMagnitude>.85f*.85f)continue;
            slideHits[attacker]|=1<<victim;
            downUntil[victim]=Time.time+FallDuration;protectedUntil[victim]=downUntil[victim]+.65f;
            knockDirection[victim]=slideDirection[attacker];slideUntil[victim]=shotCooldown[victim]=0;
            inputs[victim].x=inputs[victim].z=0;inputs[victim].shot=inputs[victim].jump=inputs[victim].slide=false;
            state.fallProgress[victim]=0;if(victim==localSlot)CancelCharge();
        }
    }
    float SlidePose(int i){float t=1-(slideUntil[i]-Time.time)/SlideDuration;if(t<0||t>=1)return 0;return Mathf.SmoothStep(0,1,Mathf.Min(t/.18f,(1-t)/.25f));}
    void ApplyBodyPose(int i,float amount){
        Quaternion recline=Quaternion.Euler(-78*amount,0,-8*amount);
        bodies[i].localRotation=recline;
        bodies[i].localPosition=Vector3.up*Mathf.Lerp(.8f,.31f,amount)-recline*(Vector3.up*.8f);
        if(amount>.01f){leftLeg[i].localRotation=Quaternion.Euler(-9*amount,0,-6*amount);rightLeg[i].localRotation=Quaternion.Euler(-2*amount,0,8*amount);leftArm[i].localRotation=Quaternion.Euler(0,0,-35*amount);rightArm[i].localRotation=Quaternion.Euler(0,0,35*amount);}
    }
    void FixedUpdate() {
        if(!host||menu)return;
        if(state.phase==1) {
            matchTime+=Time.fixedDeltaTime;
            // Unlock players and release the ball in the same authoritative physics tick.
            if(state.holding){state.restartCountdown=Mathf.Max(0,resetAt-Time.time);if(Time.time>=resetAt){state.holding=false;state.restartCountdown=0;resetAt=0;ball.isKinematic=false;}}
            for(int i=0;i<4;i++)if(state.present[i]&&state.lives[i]>0) {
                if(state.holding){inputs[i].x=inputs[i].z=0;inputs[i].shot=inputs[i].jump=inputs[i].slide=false;continue;}
                if(Time.time<downUntil[i]){
                    var fallen=controllers[i];fallen.enabled=true;fallen.height=.68f;fallen.center=Vector3.up*.35f;
                    float shove=Mathf.Clamp01((.3f-FallProgress(i))/.3f)*2;
                    fallen.Move((knockDirection[i]*shove+Vector3.down*2)*Time.fixedDeltaTime);
                    inputs[i].x=inputs[i].z=0;inputs[i].shot=inputs[i].jump=inputs[i].slide=false;continue;
                }
                if(practice&&i>0)Bot(i);
                var input=inputs[i];var cc=controllers[i];cc.enabled=true;
                bool sliding=Time.time<slideUntil[i];
                avatars[i].rotation=sliding?Quaternion.LookRotation(slideDirection[i]):Quaternion.Euler(0,input.yaw,0);
                var move=Quaternion.Euler(0,input.yaw,0)*Vector3.ClampMagnitude(new Vector3(input.x,0,input.z),1);
                if(cc.isGrounded){vertical[i]=-2;if(input.jump&&!sliding)vertical[i]=7;}
                else vertical[i]-=20*Time.fixedDeltaTime;
                if(input.slide&&cc.isGrounded&&vertical[i]<=0&&Time.time>slideCooldown[i]){slideUntil[i]=Time.time+SlideDuration;slideCooldown[i]=Time.time+1.5f;slideDirection[i]=avatars[i].forward;slideHits[i]=0;sliding=true;}
                cc.height=sliding?.68f:1.85f;cc.center=Vector3.up*(sliding?.35f:.94f);
                if(sliding)move=slideDirection[i]*Mathf.Lerp(.65f,1.9f,Mathf.Clamp01((slideUntil[i]-Time.time)/SlideDuration));
                Vector3 beforeMove=avatars[i].position;
                cc.Move((move*5.8f+Vector3.up*vertical[i])*Time.fixedDeltaTime);
                if(sliding)CheckSlideHits(i,beforeMove,avatars[i].position);
                Vector3 p=avatars[i].position;p.x=Mathf.Clamp(p.x,-13.4f,13.4f);p.z=Mathf.Clamp(p.z,-13.4f,13.4f);
                float diagonal=Mathf.Abs(p.x)+Mathf.Abs(p.z);if(diagonal>23){p.x*=23/diagonal;p.z*=23/diagonal;}
                if(p.y<-.5f)p=Spawn(i);cc.enabled=false;avatars[i].position=p;cc.enabled=true;
                Vector3 delta=ball.position-(p+Vector3.up*.45f);
                if(resetAt<=Time.time&&!ball.isKinematic) {
                    if(input.shot&&!sliding&&Time.time>shotCooldown[i]&&delta.magnitude<1.65f&&Vector3.Dot(avatars[i].forward,delta.normalized)>-.15f){ball.velocity=ShotVelocity(avatars[i].forward,input.power,input.lift);ball.angularVelocity=avatars[i].right*Mathf.Lerp(4,20,input.power);shotCooldown[i]=Time.time+.25f;}
                    if(Time.time<slideUntil[i]&&delta.magnitude<1.15f)ball.velocity=avatars[i].forward*12+Vector3.up*1.2f;
                    if(!sliding&&vertical[i]>0&&Vector3.Distance(ball.position,p+Vector3.up*1.8f)<.65f)ball.velocity=avatars[i].forward*11+Vector3.up*2;
                }
                input.shot=input.jump=input.slide=false;
            }
            Vector3 b=ball.position;
            if(resetAt==0&&(Mathf.Abs(b.x)>15||Mathf.Abs(b.z)>15||Mathf.Abs(b.x)+Mathf.Abs(b.z)>25.5f||b.y<-.8f)){
                int near=-1;float dist=float.MaxValue;
                for(int i=0;i<4;i++)if(state.present[i]&&state.lives[i]>0){float d=Vector3.SqrMagnitude(new Vector3(b.x,0,b.z)-corners[i]);if(d<dist){dist=d;near=i;}}
                if(near>=0){ResetPlayers();ResetBall(GoalRestart(near),state.names[near]+" topuyla devam");}
            }
        }
        for(int i=0;i<4;i++){state.positions[i]=avatars[i].position;state.angles[i]=avatars[i].eulerAngles.y;state.slidePose[i]=SlidePose(i);state.kicking[i]=Time.time<shotCooldown[i];state.fallProgress[i]=FallProgress(i);}state.ball=ball.position;
        if(Time.unscaledTime>=nextSend){nextSend=Time.unscaledTime+.05f;foreach(var pair in slots){state.you=pair.Value;wire?.Send(pair.Key,JsonUtility.ToJson(state));}}
    }
    void Bot(int i) {
        Vector3 diff=ball.position-avatars[i].position;diff.y=0;var input=inputs[i];input.yaw=Quaternion.LookRotation(diff.sqrMagnitude>.01f?diff:Vector3.forward).eulerAngles.y;
        input.x=0;input.z=diff.magnitude>1?1:0;input.shot=diff.magnitude<1.5f;input.power=.45f;input.lift=.12f;input.restartId=state.restartId;
        if(input.shot){int target=(i+1)%4;for(int n=0;n<4;n++){int candidate=(i+1+n)%4;if(candidate!=i&&state.lives[candidate]>0){target=candidate;break;}}input.yaw=Quaternion.LookRotation(corners[target]-avatars[i].position).eulerAngles.y;}
    }
    void StartMatch() {
        ResolveKits();
        state.phase=1;matchTime=0;for(int i=0;i<4;i++)state.lives[i]=3;
        ResetPlayers();ResetBall(new Vector3(0,3,0),"MAC BASLIYOR");
    }
    public static Vector3 ShotVelocity(Vector3 forward,float power,float lift){float speed=Mathf.Lerp(7,27,Mathf.Clamp01(power));float angle=Mathf.Lerp(1,68,Mathf.Clamp01(lift))*Mathf.Deg2Rad;return forward.normalized*(Mathf.Cos(angle)*speed)+Vector3.up*(Mathf.Sin(angle)*speed);}
    Vector3 GoalRestart(int i){return Spawn(i)-corners[i].normalized*1.05f+Vector3.up*.18f;}
    void ResetPlayers(){for(int i=0;i<4;i++){
        controllers[i].enabled=false;controllers[i].height=1.85f;controllers[i].center=Vector3.up*.94f;
        avatars[i].position=Spawn(i);avatars[i].rotation=Quaternion.LookRotation(-corners[i]);controllers[i].enabled=state.present[i]&&state.lives[i]>0;
        vertical[i]=slideUntil[i]=shotCooldown[i]=0;inputs[i].x=inputs[i].z=0;inputs[i].shot=inputs[i].jump=inputs[i].slide=false;inputs[i].yaw=avatars[i].eulerAngles.y;
        downUntil[i]=protectedUntil[i]=0;slideHits[i]=0;state.fallProgress[i]=-1;
        state.positions[i]=avatars[i].position;state.angles[i]=inputs[i].yaw;state.slidePose[i]=0;state.kicking[i]=false;
    }if(localSlot>=0)yaw=inputs[localSlot].yaw;CancelCharge();}
    void ResetBall(Vector3 position,string message) {ball.isKinematic=false;ball.velocity=Vector3.zero;ball.angularVelocity=Vector3.zero;ball.isKinematic=true;ball.position=position;resetAt=Time.time+1.5f;state.holding=true;state.restartCountdown=1.5f;state.restartId++;state.ball=position;state.notice=message;CancelCharge();}
    public void Goal(int i) {
        if(!host||state.phase!=1||state.lives[i]<=0||state.holding)return;
        state.lives[i]--;Tone();ResetPlayers();CheckWinner();if(state.phase==2)return;
        bool eliminated=state.lives[i]==0;
        ResetBall(eliminated?new Vector3(0,3,0):GoalRestart(i),state.names[i]+(eliminated?" ELENDI / ORTADAN BASLANGIC":" gol yedi / TOP ONDA"));
    }
    void CheckWinner(){int alive=0,last=-1;for(int i=0;i<4;i++)if(state.present[i]&&state.lives[i]>0){alive++;last=i;}if(alive<=1){state.phase=2;state.notice=last>=0?state.names[last]+" KAZANDI":"MAC BITTI";ball.isKinematic=true;}}
    void Tone(){int length=8000;var clip=AudioClip.Create("Goal tone",length,1,16000,false);var data=new float[length];for(int n=0;n<length;n++)data[n]=Mathf.Sin(n*2*Mathf.PI*660/16000)*.15f*(1-n/(float)length);clip.SetData(data,0);audioSource.PlayOneShot(clip);Destroy(clip,2);}
    void LateUpdate() {
        Vector3 target, desired;
        if(menu||state.phase==0||localSlot<0){target=Vector3.zero;desired=new Vector3(22,24,-26);}
        else {
            int slot=localSlot;bool spectator=state.lives[slot]<=0;
            if(spectator){slot=watched;if(slot<4&&state.lives[slot]<=0){for(int n=0;n<4;n++)if(state.lives[n]>0){slot=n;break;}}}
            if(spectator&&watched==4){target=Vector3.zero;desired=new Vector3(0,22,-21);}
            else {target=avatars[slot].position+Vector3.up*1.4f;float angle=spectator?avatars[slot].eulerAngles.y:yaw;desired=target+Quaternion.Euler(pitch,angle,0)*new Vector3(0,0,-5.6f);
                if(Physics.Linecast(target,desired,out var hit)&&hit.collider.gameObject!=avatars[slot].gameObject)desired=hit.point+(target-hit.point).normalized*.2f;}
        }
        cam.transform.position=Vector3.Lerp(cam.transform.position,desired,1-Mathf.Exp(-8*Time.deltaTime));cam.transform.LookAt(target);
    }
    void OnGUI() {
        if(titleStyle==null){titleStyle=new GUIStyle(GUI.skin.label){fontSize=38,fontStyle=FontStyle.Bold};bodyStyle=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true};buttonStyle=new GUIStyle(GUI.skin.button){fontSize=19,fixedHeight=44};}
        GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
        GUI.color=new Color(.03f,.06f,.1f,.94f);GUI.DrawTexture(new Rect(0,0,1280,82),Texture2D.whiteTexture);GUI.color=Color.white;
        GUI.Label(new Rect(28,18,480,54),"KAOS / ARENA",titleStyle);GUI.Label(new Rect(935,29,320,35),(shownArena==1?"SAHIL 02":"MAHALLE 01")+"  /  PROTOTIP",bodyStyle);
        if(menu){Panel(new Rect(45,125,470,545));GUILayout.BeginArea(new Rect(72,148,416,500));GUILayout.Label("DORT KALE. TEK KAZANAN.",bodyStyle);GUILayout.Space(20);GUILayout.Label("Oyuncu adi (10 karakter)",bodyStyle);playerName=GUILayout.TextField(playerName,10,GUILayout.Height(34));GUILayout.Space(12);
            GUILayout.Label("SAHA SECIMI",bodyStyle);selectedArena=GUILayout.SelectionGrid(selectedArena,new[]{"MAHALLE","SAHIL / KUM"},2);
            if(GUILayout.Button("LOBI OLUSTUR",buttonStyle))Open(true);
            GUILayout.Space(12);GUILayout.Label("Host IP adresi / TCP 7777",bodyStyle);address=GUILayout.TextField(address,100,GUILayout.Height(34));
            if(GUILayout.Button("LOBIYE KATIL",buttonStyle))Open(false);
            GUILayout.Space(12);if(GUILayout.Button("ANTRENMAN  •  3 BOT",buttonStyle))Open(true,true);
            GUILayout.Space(12);GUILayout.Label(status,bodyStyle);GUILayout.Label("Internet testi: port yonlendirme veya ortak sanal ag gerekir.",bodyStyle);GUILayout.EndArea();
        }else if(state.phase==0){Panel(new Rect(45,125,470,545));GUILayout.BeginArea(new Rect(72,148,416,500));GUILayout.Label(localSlot<0?"BAGLANILIYOR":"MAC LOBISI",titleStyle);
            int ready=0;for(int i=0;i<4;i++){GUI.color=colors[i];GUILayout.Label((i+1)+"  "+(state.present[i]?state.names[i]+" / "+ArenaRoster.Get(state.characters[i]).name+(state.ready[i]?"  / HAZIR":"  / BEKLIYOR"):"Bos yer"),bodyStyle);if(state.present[i]&&state.ready[i])ready++;}GUI.color=Color.white;
            GUILayout.Space(20);if(localSlot>=0&&GUILayout.Button(inputs[localSlot].ready?"HAZIR DEGILIM":"HAZIRIM",buttonStyle)){inputs[localSlot].ready=!inputs[localSlot].ready;if(host)state.ready[0]=inputs[0].ready;}
            if(host){GUI.enabled=ready==4;if(GUILayout.Button("MACI BASLAT",buttonStyle))StartMatch();GUI.enabled=true;}
            GUILayout.Space(16);GUILayout.Label("4 oyuncu hazir olunca host baslatir.\nWASD: hareket / Fare: kamera\nSol tik basili tut: sut gucu\nSag tik: kayma / Space: zipla",bodyStyle);
            if(GUILayout.Button("ANA MENU",buttonStyle)){status="Lobiden ayrildin";Close();}GUILayout.EndArea();DrawRosterPicker();
        }else{
            for(int i=0;i<4;i++){GUI.color=colors[i];GUI.Box(new Rect(25+i*312,94,300,55),state.names[i]+"    "+(state.lives[i]>0?new string('|',state.lives[i]):"ELENDI"));}GUI.color=Color.white;
            GUI.Label(new Rect(32,654,1100,42),state.notice+"   |   WASD / Sol tik: sut / Sag tik: kay / Space: zipla / Esc: menu",bodyStyle);
            if(localSlot>=0&&state.lives[localSlot]<=0)GUI.Label(new Rect(420,164,600,35),"IZLEYICI MODU  /  TAB: kamera degistir",bodyStyle);
            if(state.holding)GUI.Label(new Rect(435,185,500,45),"BASLANGIC  /  "+Mathf.CeilToInt(state.restartCountdown),titleStyle);
            if(state.phase==2||paused){Panel(new Rect(420,230,440,300));GUILayout.BeginArea(new Rect(446,254,388,250));GUILayout.Label(state.phase==2?state.notice:"MAC MENUSU",bodyStyle);
                if(state.phase==2&&host&&GUILayout.Button("LOBIYE DON",buttonStyle)){state.phase=0;for(int i=0;i<4;i++){state.ready[i]=practice&&i>0;inputs[i].ready=state.ready[i];}}
                if(paused&&GUILayout.Button("DEVAM ET",buttonStyle))paused=false;
                if(GUILayout.Button(host?"CIK / MACI IPTAL ET":"CIK",buttonStyle)){status="Mactan ayrildin";Close();}GUILayout.EndArea();}
        }
    }
    void Panel(Rect r){GUI.color=new Color(.035f,.065f,.10f,.95f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    void OnApplicationQuit(){wire?.Dispose();}
    void OnDestroy(){wire?.Dispose();if(previewModel)ArenaRoster.DisposeVisual(previewModel);if(previewCamera)Destroy(previewCamera.gameObject);if(previewTexture){previewTexture.Release();Destroy(previewTexture);}}
}

public class ArenaGoal : MonoBehaviour {
    public ArenaGame game;public int slot;
    void OnTriggerEnter(Collider other){if(other.attachedRigidbody&&other.attachedRigidbody.gameObject.name=="Ball")game.Goal(slot);}
}
