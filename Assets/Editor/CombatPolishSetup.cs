using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using StarterAssets;
using Object = UnityEngine.Object;

public static class CombatPolishSetup
{
    [MenuItem("Tools/Shooter/Configurar sonido y mira")]
    public static void Apply()
    {
        foreach(var name in new[]{"Shot","PlayerHurt","EnemyHit","EnemyDeath"})
        {
            string path="Assets/Resources/CombatAudio/"+name+".wav";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);
            var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;
            importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
        }
        var scene=EditorSceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/first person.unity")scene=EditorSceneManager.OpenScene("Assets/Scenes/first person.unity");
        var input=Object.FindFirstObjectByType<StarterAssetsInputs>();
        var mount=GameObject.Find("weapon").transform;var view=mount.GetComponent<BoneWeaponView>();var spawn=input.ProjectileSpawnPoint;
        Undo.RecordObjects(new Object[]{mount,view,view.visual,spawn,input},"Alinear arma y mira");
        mount.localPosition=new Vector3(0.145f,-0.18f,0.72f);mount.localRotation=Quaternion.identity;mount.localScale=Vector3.one*0.68f;
        view.visual.localPosition=Vector3.zero;view.visual.localRotation=Quaternion.identity;
        spawn.SetParent(view.visual,false);spawn.localPosition=new Vector3(0,0.055f,0.56f);spawn.localRotation=Quaternion.Euler(90,0,0);spawn.localScale=Vector3.one;view.muzzle=spawn;
        input.aimCamera=mount.GetComponentInParent<Camera>();input.aimMask=~0;input.aimDistance=150;input.projectileUsesGravity=false;
        view.visual.rotation=Quaternion.LookRotation(input.GetAimPoint()-view.visual.TransformPoint(Vector3.up*0.055f),input.aimCamera.transform.up);
        if(view.muzzleFlash)
        {
            var flash=view.muzzleFlash.transform;Undo.RecordObject(flash,"Alinear destello");flash.SetParent(spawn,false);flash.localPosition=Vector3.up*0.025f;flash.localRotation=Quaternion.identity;flash.localScale=new Vector3(0.06f,0.13f,0.06f);
        }
        EditorUtility.SetDirty(input);EditorUtility.SetDirty(view);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("COMBAT_AUDIO_AND_AIM_READY");
    }
}

