using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Object = UnityEngine.Object;

public static class CemeteryNavMeshBake
{
    [MenuItem("Tools/Shooter/Rehacer bake del cementerio")]
    public static void Rebuild()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/first person.unity") scene = EditorSceneManager.OpenScene("Assets/Scenes/first person.unity");
        var player = GameObject.FindGameObjectWithTag("Player");
        Ignore(player);
        foreach (var projectile in GameObject.FindGameObjectsWithTag("Projectile")) Ignore(projectile);
        var decor = GameObject.Find("Cementerio - Decoracion");
        foreach (var obstacle in decor.GetComponentsInChildren<NavMeshObstacle>())
        {
            Undo.RecordObject(obstacle, "Obstaculo fijo del cementerio");
            obstacle.carving = false; obstacle.enabled = false;
        }
        foreach (var renderer in decor.GetComponentsInChildren<MeshRenderer>().Where(r => r.name == "Barrote de hierro" || r.name == "Travesano de reja"))
            if (!renderer.GetComponent<BoxCollider>()) Undo.AddComponent<BoxCollider>(renderer.gameObject);
        var surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
        Directory.CreateDirectory("Assets/Cemetery/Navigation");
        var report = new List<string>();
        foreach (var surface in surfaces)
        {
            Undo.RecordObject(surface, "Bake de navegacion del cementerio");
            // The old all-objects bake included a 2 km background plane.
            // Include the entire arena and all seven spawn points, with margins.
            surface.collectObjects = CollectObjects.Volume;
            surface.center = surface.transform.InverseTransformPoint(new Vector3(2, 5, 3));
            surface.size = new Vector3(76, 20, 82);
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.ignoreNavMeshAgent = true;
            surface.ignoreNavMeshObstacle = false;
            surface.overrideVoxelSize = true; surface.voxelSize = 0.08f;
            surface.overrideTileSize = true; surface.tileSize = 128;
            surface.BuildNavMesh();
            if (!surface.navMeshData) throw new Exception("Bake vacio: " + surface.name);
            string asset = "Assets/Cemetery/Navigation/NavMesh_" + surface.agentTypeID + ".asset";
            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(asset)) AssetDatabase.DeleteAsset(asset);
            AssetDatabase.CreateAsset(surface.navMeshData, asset);
            EditorUtility.SetDirty(surface);
            report.Add(surface.name + " agent=" + surface.agentTypeID + " data=" + asset + " voxel=" + surface.voxelSize);
        }
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        File.WriteAllText("Library/CemeteryBakeReport.txt", string.Join("\n", report));
        Debug.Log("CEMETERY_BAKE_SAVED\n" + string.Join("\n", report));
    }
    static void Ignore(GameObject obj)
    {
        var modifier = obj.GetComponent<NavMeshModifier>();
        if (!modifier) modifier = Undo.AddComponent<NavMeshModifier>(obj);
        Undo.RecordObject(modifier, "Excluir personaje del bake"); modifier.ignoreFromBuild = true; modifier.applyToChildren = true;
    }
}

