using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class ShooterSetup
{
    [MenuItem("Tools/Shooter/Completar prototipo")]
    public static void Configure()
    {
        const string deathPath = "Assets/animations/Ch10_nonPBR@Dying.fbx";
        var importer = (ModelImporter)AssetImporter.GetAtPath(deathPath);
        var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
        foreach (var clip in clips)
        {
            clip.loopTime = false;
            clip.lockRootHeightY = true;
            clip.keepOriginalPositionY = true;
            clip.heightFromFeet = false;
            clip.lockRootPositionXZ = true;
            clip.keepOriginalPositionXZ = true;
            clip.lockRootRotation = true;
            clip.keepOriginalOrientation = true;
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/first person.unity");
        if (!Object.FindFirstObjectByType<MatchGame>()) new GameObject("Game Manager").AddComponent<MatchGame>();
        foreach (var enemy in Object.FindObjectsByType<EnemyNavigation>(FindObjectsSortMode.None))
        {
            if (!enemy.GetComponent<ReciveDamage>()) enemy.gameObject.AddComponent<ReciveDamage>();
            EditorUtility.SetDirty(enemy.gameObject);
        }
        // This classroom test spawns a loose projectile at the world origin.
        foreach (var spawner in Object.FindObjectsByType<SpawnerTest>(FindObjectsSortMode.None)) spawner.enabled = false;
        var mobile = GameObject.Find("UI_TouchScreenInput");
        if (mobile) mobile.SetActive(false);
        var playable = Object.FindObjectsByType<EnemyNavigation>(FindObjectsSortMode.None).FirstOrDefault(e => e.GetComponentInChildren<Animator>());
        if (playable) PrefabUtility.SaveAsPrefabAsset(playable.gameObject, "Assets/Pre fabs/EnemyPlayable.prefab");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scene.path, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("SHOOTER_SETUP_OK");
    }
}

// Scene integration.


