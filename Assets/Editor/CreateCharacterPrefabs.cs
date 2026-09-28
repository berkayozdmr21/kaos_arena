using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateCharacterPrefabs
{
    // Explicit menu command; importing the models does not modify gameplay scenes.
    [MenuItem("Kaos Arena/Create Character Prefabs")]
    public static void Create()
    {
        var names = new[] { "Turquoise", "Coral", "Gold", "Purple" };
        Directory.CreateDirectory("Assets/Characters/Prefabs");
        Directory.CreateDirectory("Assets/Characters/Materials");
        AssetDatabase.Refresh();
        foreach (var name in names)
        {
            var path = "Assets/Characters/Models/Player_" + name + ".fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!model) throw new FileNotFoundException("Import the FBX model first", path);
            var root = new GameObject("Player_" + name);
            try
            {
                var visual = Object.Instantiate(model, root.transform);
                visual.name = "Visual";
                foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
                {
                    var materials = renderer.sharedMaterials;
                    for (var i = 0; i < materials.Length; i++)
                    {
                        if (!materials[i]) continue;
                        var materialPath = "Assets/Characters/Materials/" + name + "_" + materials[i].name.Replace("/", "_") + ".mat";
                        var saved = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                        if (!saved) { saved = new Material(materials[i]); AssetDatabase.CreateAsset(saved, materialPath); }
                        materials[i] = saved;
                    }
                    renderer.sharedMaterials = materials;
                }
                var controller = root.AddComponent<CharacterController>();
                controller.height = 1.85f; controller.radius = .32f;
                controller.center = new Vector3(0, .94f, 0); controller.stepOffset = .25f;
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Characters/Prefabs/Player_" + name + ".prefab");
                if (!prefab) throw new System.Exception("Could not save prefab: " + name);
            }
            finally { Object.DestroyImmediate(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Four character prefabs and reusable materials created in Assets/Characters.");
    }
}
