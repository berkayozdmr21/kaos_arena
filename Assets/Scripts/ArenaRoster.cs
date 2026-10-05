using System;
using UnityEngine;

[Serializable] public class Footballer {public string id,name;public float[] skin,hair,jersey,accent;public int style,face;}
[Serializable] public class FootballRoster {public Footballer[] characters;}
public static class ArenaRoster {
    static FootballRoster catalog;
    public static Footballer Get(int index){if(catalog==null){var file=Resources.Load<TextAsset>("Roster");if(!file)throw new InvalidOperationException("Roster.json is missing");catalog=JsonUtility.FromJson<FootballRoster>(file.text);if(catalog.characters.Length!=ArenaRosterRules.Count)throw new InvalidOperationException("Unexpected footballer count");}return catalog.characters[ArenaRosterRules.Clamp(index)];}
    public static Color KitColor(int index){var c=Get(index).jersey;return new Color(c[0],c[1],c[2]);}
    public static GameObject Create(int index,Transform parent,int kit){
        var data=Get(index);var asset=Resources.Load<GameObject>("RosterModels/"+data.id);
        if(!asset)throw new InvalidOperationException("Missing imported footballer FBX: "+data.id);
        var visual=UnityEngine.Object.Instantiate(asset,parent,false);visual.name="Footballer_"+data.id;
        foreach(var c in visual.GetComponentsInChildren<Collider>(true))UnityEngine.Object.Destroy(c);
        foreach(var r in visual.GetComponentsInChildren<Renderer>(true)){
            var materials=r.sharedMaterials;
            for(int i=0;i<materials.Length;i++){
                var source=materials[i];if(!source)continue;
                var material=new Material(Shader.Find("Standard"));material.name=source.name;
                material.color=source.HasProperty("_Color")?source.color:Color.white;material.SetFloat("_Glossiness",.18f);
                if(source.name.Contains("Jersey_Main"))material.color=KitColor(kit);
                materials[i]=material;
            }r.sharedMaterials=materials;
        }return visual;
    }
    public static Transform Find(Transform parent,string name){foreach(var t in parent.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;throw new InvalidOperationException("Missing joint "+name);}
    // Clean Unity-space animation pivots, independent of FBX axis conversion transforms.
    public static Transform Wrap(Transform anchor,Transform parent){var p=new GameObject(anchor.name+"_Animation").transform;p.SetParent(parent,false);p.position=anchor.position;anchor.SetParent(p,true);return p;}
    public static void DisposeVisual(GameObject root){if(!root)return;foreach(var r in root.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)if(m)UnityEngine.Object.Destroy(m);UnityEngine.Object.Destroy(root);}
}
