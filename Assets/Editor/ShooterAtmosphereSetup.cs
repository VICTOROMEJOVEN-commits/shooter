using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public static class ShooterAtmosphereSetup
{
    [MenuItem("Tools/Shooter/Ambiente y seis enemigos")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/first person.unity") scene = EditorSceneManager.OpenScene("Assets/Scenes/first person.unity");
        var player = GameObject.FindGameObjectWithTag("Player");
        var source = UnityEngine.Object.FindObjectsByType<EnemyNavigation>(FindObjectsSortMode.None).First(e => e.GetComponentInChildren<Animator>());
        var filter = new NavMeshQueryFilter { agentTypeID = source.GetComponent<NavMeshAgent>().agentTypeID, areaMask = NavMesh.AllAreas };
        if (!NavMesh.SamplePosition(player.transform.position, out var playerHit, 5, filter)) throw new Exception("No se encontro el NavMesh del jugador.");
        var triangulation = NavMesh.CalculateTriangulation();
        var random = new System.Random(7341);
        var candidates = new List<Vector3>();
        var path = new NavMeshPath();
        for (int attempt = 0; attempt < 6000; attempt++)
        {
            int triangle = random.Next(triangulation.indices.Length / 3) * 3;
            var a = triangulation.vertices[triangulation.indices[triangle]];
            var b = triangulation.vertices[triangulation.indices[triangle + 1]];
            var c = triangulation.vertices[triangulation.indices[triangle + 2]];
            float u = (float)random.NextDouble(), v = (float)random.NextDouble();
            if (u + v > 1) { u = 1 - u; v = 1 - v; }
            var sample = a + (b - a) * u + (c - a) * v;
            float distance = Vector3.Distance(sample, playerHit.position);
            if (distance < 9 || distance > 28 || Mathf.Abs(sample.y - playerHit.position.y) > 3) continue;
            if (!NavMesh.SamplePosition(sample, out var hit, 1, filter)) continue;
            if (!NavMesh.CalculatePath(hit.position, playerHit.position, filter, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            candidates.Add(hit.position);
        }
        var positions = new List<Vector3> { source.transform.position };
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Pre fabs/EnemyPlayable.prefab");
        if (!prefab) throw new Exception("No se encontro EnemyPlayable.");
        var report = new List<string>();
        for (int i = 2; i <= 7; i++)
        {
            string name = "Enemy - " + i.ToString("00");
            var existing = GameObject.Find(name);
            if (existing) { positions.Add(existing.transform.position); report.Add(name + " conservado"); continue; }
            var options = candidates.Where(p => positions.All(other => Vector3.Distance(p, other) > 4)).ToList();
            if (options.Count == 0) throw new Exception("No hay espacio suficiente para distribuir seis enemigos.");
            var point = options.OrderByDescending(p => positions.Min(other => Vector3.Distance(p, other))).First();
            var enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(enemy, "Agregar enemigo");
            enemy.name = name;
            enemy.transform.position = point + Vector3.up * enemy.GetComponent<NavMeshAgent>().baseOffset;
            enemy.transform.rotation = Quaternion.Euler(0, (float)random.NextDouble() * 360, 0);
            positions.Add(point);
            report.Add(name + " " + point + " - ruta completa al jugador");
        }
        const string audioPath = "Assets/Audio/TerrorAmbient.wav";
        AssetDatabase.ImportAsset(audioPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (AudioImporter)AssetImporter.GetAtPath(audioPath);
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.Streaming;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.65f;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = false;
        importer.SaveAndReimport();
        var musicObject = GameObject.Find("Musica - Terror ambiental");
        if (!musicObject) { musicObject = new GameObject("Musica - Terror ambiental"); Undo.RegisterCreatedObjectUndo(musicObject, "Agregar musica de terror"); }
        var audio = musicObject.GetComponent<AudioSource>();
        if (!audio) audio = musicObject.AddComponent<AudioSource>();
        Undo.RecordObject(audio, "Configurar musica ambiental");
        audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
        audio.loop = true;
        audio.playOnAwake = true;
        audio.spatialBlend = 0;
        audio.volume = 0.26f;
        audio.priority = 32;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        report.Add("Enemigos activos=" + UnityEngine.Object.FindObjectsByType<EnemyNavigation>(FindObjectsSortMode.None).Length);
        report.Add("Musica original 48 segundos, stereo, loop, volumen 26%.");
        File.WriteAllText("Library/ShooterAtmosphereReport.txt", string.Join("\n", report));
        Debug.Log("SHOOTER_ATMOSPHERE_OK\n" + string.Join("\n", report));
    }
}


