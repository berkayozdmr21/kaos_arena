using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class RosterAssets {
    [MenuItem("Kaos Arena/Prepare Character Assets")]
    public static void Prepare(){
        Directory.CreateDirectory("Assets/Characters/RosterPrefabs");
        AssetDatabase.Refresh();
        for(int i=0;i<ArenaRosterRules.Count;i++){
            var c=ArenaRoster.Get(i);string path="Assets/Resources/RosterModels/"+c.id+".fbx";
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(!asset)throw new InvalidOperationException("FBX not imported: "+path);
            foreach(var joint in new[]{"Leg_Left","Leg_Right","Arm_Left","Arm_Right","Knee_Left","Knee_Right","Elbow_Left","Elbow_Right"})ArenaRoster.Find(asset.transform,joint);
            var obj=UnityEngine.Object.Instantiate(asset);obj.name=c.id;
            try {PrefabUtility.SaveAsPrefabAsset(obj,"Assets/Characters/RosterPrefabs/"+c.id+".prefab");}
            finally{UnityEngine.Object.DestroyImmediate(obj);}
        }
        AssetDatabase.SaveAssets();
        Debug.Log("All footballer models and joint hierarchies verified; reusable visual prefabs saved.");
    }
}
