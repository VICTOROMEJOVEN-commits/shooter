using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

public static class CemeterySetup
{
    const string Art = "Assets/Cemetery";
    [MenuItem("Tools/Shooter/Ambientar cementerio")]
    public static void Apply()
    {
        Directory.CreateDirectory(Art + "/Textures"); Directory.CreateDirectory(Art + "/Materials"); Directory.CreateDirectory(Art + "/Meshes");
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/first person.unity") scene = EditorSceneManager.OpenScene("Assets/Scenes/first person.unity");
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/animations/Enemy.controller");
        var machine = controller.layers[0].stateMachine;
        var death = machine.states.First(s => s.state.name == "Dying").state;
        death.speed = EnemyNavigation.DeathSpeed;
        foreach (var transition in machine.anyStateTransitions.Where(t => t.destinationState == death))
        {
            transition.duration = EnemyNavigation.DeathBlend;
            transition.offset = EnemyNavigation.DeathStart;
            transition.hasFixedDuration = true;
            transition.hasExitTime = false;
            transition.canTransitionToSelf = false;
        }
        EditorUtility.SetDirty(controller);
        var soil = MakeMaterial("Tierra humeda y musgo", MakeTexture("Tierra", 0), new Color(0.92f, 0.94f, 0.9f), 0.05f);
        var stone = MakeMaterial("Mamposteria envejecida", MakeTexture("Mamposteria", 1), Color.white, 0.08f);
        var granite = MakeMaterial("Granito de lapidas", MakeTexture("Granito", 2), new Color(0.86f, 0.9f, 0.87f), 0.12f);
        var iron = MakeMaterial("Hierro oxidado", null, new Color(0.065f, 0.075f, 0.07f), 0.23f);
        iron.SetFloat("_Metallic", 0.7f);
        var graveDirt = MakeMaterial("Tierra removida", soil.mainTexture as Texture2D, new Color(0.48f, 0.43f, 0.32f), 0);
        var inlay = MakeMaterial("Grabado oscuro", null, new Color(0.065f, 0.075f, 0.065f), 0);
        var warm = MakeMaterial("Farol ambar", null, new Color(0.8f, 0.38f, 0.08f), 0.1f);
        warm.EnableKeyword("_EMISSION"); warm.SetColor("_EmissionColor", new Color(1, 0.38f, 0.08f) * 3);
        var environment = GameObject.Find("Environment");
        var report = new List<string>();
        if (!environment) throw new Exception("No se encontro Environment.");
        foreach (var renderer in environment.GetComponentsInChildren<MeshRenderer>())
        {
            var size = renderer.bounds.size;
            bool isGround = renderer.name.ToLowerInvariant().Contains("ground") || (size.y < 0.3f && size.x * size.z > 150);
            var material = new Material(isGround ? soil : stone);
            material.shader = Shader.Find("Shooter/Cemetery World Stone");
            material.SetFloat("_TileSize", isGround ? 2.5f : 3f);
            material.name = "Entorno - " + renderer.name;
            var mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
            Vector2 uvRange = Vector2.one;
            if (mesh && mesh.isReadable && mesh.uv.Length > 0)
            {
                var uv = mesh.uv;
                uvRange = new Vector2(uv.Max(v => v.x) - uv.Min(v => v.x), uv.Max(v => v.y) - uv.Min(v => v.y));
            }
            float width = isGround ? size.x : Mathf.Max(size.x, size.z);
            float height = isGround ? size.z : Mathf.Max(size.y, 1);
            material.mainTextureScale = new Vector2(Mathf.Max(1, width / 3) / Mathf.Max(1, uvRange.x), Mathf.Max(1, height / 3) / Mathf.Max(1, uvRange.y));
            string path = Art + "/Materials/Environment_" + renderer.GetInstanceID().ToString().Replace("-", "n") + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path)) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(material, path);
            Undo.RecordObject(renderer, "Material de cementerio");
            renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
            report.Add(renderer.name + " bounds=" + renderer.bounds + " suelo=" + isGround);
        }
        var old = GameObject.Find("Cementerio - Decoracion");
        if (old) Undo.DestroyObjectImmediate(old);
        var root = new GameObject("Cementerio - Decoracion"); Undo.RegisterCreatedObjectUndo(root, "Crear cementerio");
        var player = GameObject.FindGameObjectWithTag("Player").transform;
        var enemies = Object.FindObjectsByType<EnemyNavigation>(FindObjectsSortMode.None);
        var filter = new NavMeshQueryFilter { agentTypeID = 0, areaMask = NavMesh.AllAreas };
        var navPath = new NavMeshPath();
        NavMesh.SamplePosition(player.position, out var playerHit, 5, filter);
        var gravePositions = new List<Vector3>();
        var random = new System.Random(411);
        var graveMesh = HeadstoneMesh();
        int graves = 0;
        for (int row = 0; row < 7; row++)
        for (int column = 0; column < 9; column++)
        {
            Vector3 candidate = player.position + new Vector3((column - 4) * 4.2f + (row % 2) * 0.6f, 0, -9 + row * 5f);
            if (!NavMesh.SamplePosition(candidate, out var hit, 1.6f, filter)) continue;
            if (Mathf.Abs(hit.position.y - playerHit.position.y) > 0.7f || Vector3.Distance(hit.position, player.position) < 5.5f) continue;
            if (enemies.Any(e => Vector3.Distance(e.transform.position, hit.position) < 2.4f)) continue;
            if (gravePositions.Any(p => Vector3.Distance(p, hit.position) < 3.5f)) continue;
            if (!NavMesh.CalculatePath(hit.position, playerHit.position, filter, navPath) || navPath.status != NavMeshPathStatus.PathComplete) continue;
            var grave = new GameObject("Sepultura " + (++graves).ToString("00")); grave.transform.SetParent(root.transform);
            grave.transform.position = hit.position + Vector3.down * 0.05f;
            grave.transform.rotation = Quaternion.Euler(0, (row % 2 == 0 ? 0 : 180) + (float)random.NextDouble() * 12 - 6, 0);
            gravePositions.Add(hit.position);
            var head = new GameObject("Lapida de granito", typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider));
            head.transform.SetParent(grave.transform, false); head.GetComponent<MeshFilter>().sharedMesh = graveMesh;
            head.GetComponent<MeshRenderer>().sharedMaterial = granite;
            var collider = head.GetComponent<BoxCollider>(); collider.center = new Vector3(0, 0.65f, 0); collider.size = new Vector3(0.9f, 1.3f, 0.26f);
            var obstacle = head.AddComponent<NavMeshObstacle>(); obstacle.shape = NavMeshObstacleShape.Box; obstacle.center = collider.center; obstacle.size = collider.size;
            obstacle.carving = true; obstacle.carveOnlyStationary = false;
            Primitive("Base de piedra", PrimitiveType.Cube, grave.transform, new Vector3(0, 0.06f, 0), new Vector3(1.08f, 0.17f, 0.48f), granite, false);
            Primitive("Tierra de sepultura", PrimitiveType.Sphere, grave.transform, new Vector3(0, 0.02f, -1.1f), new Vector3(0.86f, 0.16f, 1.75f), graveDirt, false);
            Primitive("Cruz grabada vertical", PrimitiveType.Cube, grave.transform, new Vector3(0, 0.81f, -0.141f), new Vector3(0.045f, 0.4f, 0.012f), inlay, false);
            Primitive("Cruz grabada horizontal", PrimitiveType.Cube, grave.transform, new Vector3(0, 0.87f, -0.142f), new Vector3(0.26f, 0.045f, 0.012f), inlay, false);
            if (graves % 5 == 0) Lantern(grave.transform, new Vector3(0.8f, 0, -0.2f), iron, warm);
        }
        // Wrought-iron boundary, kept away from the playable center.
        for (int side = 0; side < 2; side++)
        {
            float z = player.position.z + (side == 0 ? -15 : 31);
            for (int i = -26; i <= 26; i++)
            {
                var p = new Vector3(player.position.x + i, playerHit.position.y, z);
                if (Mathf.Abs(i) < 3 && side == 0) continue; // Entrance gap.
                Primitive("Barrote de hierro", PrimitiveType.Cube, root.transform, p + Vector3.up * 0.85f, new Vector3(0.055f, 1.7f, 0.055f), iron, false);
                if (i % 4 == 0) Primitive("Pilar de cementerio", PrimitiveType.Cube, root.transform, p + Vector3.up * 0.95f, new Vector3(0.32f, 1.9f, 0.32f), stone, true);
            }
            for (int section = 0; section < 2; section++)
            {
                float x = player.position.x + (section == 0 ? -14.5f : 14.5f);
                foreach (float y in new[] { 0.55f, 1.3f }) Primitive("Travesano de reja", PrimitiveType.Cube, root.transform, new Vector3(x, playerHit.position.y + y, z), new Vector3(23, 0.055f, 0.055f), iron, false);
            }
        }
        Lantern(root.transform, playerHit.position + new Vector3(-3.5f, 0, 4), iron, warm);
        Lantern(root.transform, playerHit.position + new Vector3(4, 0, 5.5f), iron, warm);
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l => l.type == LightType.Directional))
        {
            Undo.RecordObject(light, "Luz de luna");
            light.color = new Color(0.62f, 0.73f, 0.94f); light.intensity = 0.75f; light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(32, -35, 0);
        }
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.22f, 0.27f, 0.35f);
        RenderSettings.ambientEquatorColor = new Color(0.13f, 0.17f, 0.18f);
        RenderSettings.ambientGroundColor = new Color(0.09f, 0.1f, 0.09f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.075f, 0.105f, 0.13f); RenderSettings.fogDensity = 0.024f;
        foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(camera, "Cielo nocturno"); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.04f, 0.065f);
        }
        var volumeObject = new GameObject("Atmosfera del cementerio", typeof(Volume)); volumeObject.transform.SetParent(root.transform);
        var volume = volumeObject.GetComponent<Volume>(); volume.isGlobal = true; volume.priority = 20;
        var profile = new VolumeProfile();
        var grading = profile.Add<ColorAdjustments>(); grading.saturation.Override(-22); grading.contrast.Override(12); grading.postExposure.Override(0.1f);
        var vignette = profile.Add<Vignette>(); vignette.intensity.Override(0.22f); vignette.smoothness.Override(0.7f);
        var bloom = profile.Add<Bloom>(); bloom.intensity.Override(0.3f); bloom.threshold.Override(1.1f);
        const string profilePath = Art + "/CemeteryVolume.asset";
        if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath)) AssetDatabase.DeleteAsset(profilePath);
        AssetDatabase.CreateAsset(profile, profilePath); volume.sharedProfile = profile;
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        CemeteryNavMeshBake.Rebuild();
        var clip = death.motion as AnimationClip;
        report.Add("Graves=" + graves + " Enemies=" + enemies.Length);
        report.Add("Death blend=" + EnemyNavigation.DeathBlend + " speed=" + death.speed + " offset=" + EnemyNavigation.DeathStart + " expectedSeconds=" + (clip ? clip.length * (1 - EnemyNavigation.DeathStart) / death.speed + EnemyNavigation.DeathBlend + 0.15f : 0));
        File.WriteAllText("Library/CemeteryReport.txt", string.Join("\n", report)); Debug.Log("CEMETERY_SETUP_OK\n" + report.Last());
    }

    static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material, bool solid)
    {
        var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position; obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        if (!solid) Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }
    static void Lantern(Transform parent, Vector3 position, Material iron, Material glow)
    {
        var obj = new GameObject("Farol de sepultura"); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
        Primitive("Poste", PrimitiveType.Cylinder, obj.transform, new Vector3(0, 0.75f, 0), new Vector3(0.055f, 0.75f, 0.055f), iron, false);
        Primitive("Luz ambar", PrimitiveType.Cube, obj.transform, new Vector3(0, 1.65f, 0), new Vector3(0.2f, 0.3f, 0.2f), glow, false);
        Primitive("Cubierta", PrimitiveType.Cube, obj.transform, new Vector3(0, 1.83f, 0), new Vector3(0.32f, 0.07f, 0.32f), iron, false);
        Primitive("Base farol", PrimitiveType.Cube, obj.transform, new Vector3(0, 1.47f, 0), new Vector3(0.28f, 0.05f, 0.28f), iron, false);
        var lightObject = new GameObject("Iluminacion calida", typeof(Light)); lightObject.transform.SetParent(obj.transform, false); lightObject.transform.localPosition = Vector3.up * 1.65f;
        var light = lightObject.GetComponent<Light>(); light.type = LightType.Point; light.color = new Color(1, 0.53f, 0.2f); light.intensity = 3.4f; light.range = 6.5f;
    }
    static Mesh HeadstoneMesh()
    {
        const string path = Art + "/Meshes/LapidaArco.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (existing) return existing;
        var polygon = new List<Vector2> { new Vector2(-0.45f, 0), new Vector2(0.45f, 0), new Vector2(0.45f, 0.95f) };
        for (int i = 1; i <= 16; i++) { float angle = i / 16f * Mathf.PI; polygon.Add(new Vector2(Mathf.Cos(angle) * 0.45f, 0.95f + Mathf.Sin(angle) * 0.45f)); }
        var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        for (int face = 0; face < 2; face++) foreach (var p in polygon) { vertices.Add(new Vector3(p.x, p.y, face == 0 ? -0.14f : 0.14f)); uv.Add(new Vector2(p.x + 0.5f, p.y)); }
        int n = polygon.Count;
        for (int i = 1; i < n - 1; i++) { tris.AddRange(new[] { 0, i + 1, i, n, n + i, n + i + 1 }); }
        for (int i = 0; i < n; i++) { int j = (i + 1) % n; tris.AddRange(new[] { i, j, n + j, i, n + j, n + i }); }
        var mesh = new Mesh { name = "Lapida con arco" }; mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
    }
    static Material MakeMaterial(string name, Texture2D texture, Color color, float smoothness)
    {
        string path = Art + "/Materials/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); if (AssetDatabase.LoadAssetAtPath<Material>(path)) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(material, path); }
        material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", smoothness); material.SetFloat("_Metallic", 0);
        if (texture) material.SetTexture("_BaseMap", texture);
        EditorUtility.SetDirty(material); return material;
    }
    static Texture2D MakeTexture(string name, int type)
    {
        string path = Art + "/Textures/" + name + ".png";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        const int size = 512;
        var texture = new Texture2D(size, size, TextureFormat.RGB24, true);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float u = x / (float)size, v = y / (float)size;
            float broad = Noise(u, v, 4), middle = Noise(u, v, 16), grain = Noise(u, v, 128);
            Color color;
            if (type == 0)
            {
                color = Color.Lerp(new Color(0.12f, 0.105f, 0.075f), new Color(0.32f, 0.29f, 0.22f), broad * 0.7f + middle * 0.3f);
                color = Color.Lerp(color, new Color(0.17f, 0.23f, 0.11f), Ramp(0.48f, 0.8f, broad) * 0.7f);
                color *= 0.7f + grain * 0.6f;
                if (grain > 0.84f) color += new Color(0.12f, 0.11f, 0.075f);
            }
            else if (type == 1)
            {
                float row = Mathf.Floor(v * 4), bx = Mathf.Repeat(u * 3 + (row % 2) * 0.5f, 1), by = Mathf.Repeat(v * 4, 1);
                float edge = Mathf.Min(Mathf.Min(bx, 1 - bx), Mathf.Min(by, 1 - by));
                float mortar = Ramp(0.012f, 0.045f + middle * 0.02f, edge);
                color = Color.Lerp(new Color(0.055f, 0.065f, 0.06f), new Color(0.35f, 0.37f, 0.34f) * (0.7f + broad * 0.4f + grain * 0.2f), mortar);
                color = Color.Lerp(color, new Color(0.13f, 0.2f, 0.1f), (1 - mortar * 0.65f) * Ramp(0.55f, 0.85f, middle));
            }
            else
            {
                color = Color.Lerp(new Color(0.24f, 0.27f, 0.26f), new Color(0.55f, 0.58f, 0.55f), broad * 0.4f + grain * 0.6f);
                color = Color.Lerp(color, new Color(0.14f, 0.22f, 0.11f), Ramp(0.57f, 0.85f, middle) * Ramp(0.55f, 0.2f, v) * 0.7f);
            }
            pixels[y * size + x] = color;
        }
        texture.SetPixels(pixels); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.wrapMode = TextureWrapMode.Repeat; importer.filterMode = FilterMode.Trilinear; importer.anisoLevel = 4; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static float Ramp(float low, float high, float value) { return Mathf.SmoothStep(0, 1, Mathf.InverseLerp(low, high, value)); }
    static float Noise(float u, float v, int cells)
    {
        float x = u * cells, y = v * cells; int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y); float fx = x - ix, fy = y - iy;
        fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
        return Mathf.Lerp(Mathf.Lerp(Hash(ix % cells, iy % cells), Hash((ix + 1) % cells, iy % cells), fx), Mathf.Lerp(Hash(ix % cells, (iy + 1) % cells), Hash((ix + 1) % cells, (iy + 1) % cells), fx), fy);
    }
    static float Hash(int x, int y)
    {
        unchecked { uint h = (uint)(x * 374761393 + y * 668265263 + 12345); h = (h ^ (h >> 13)) * 1274126177; return (h & 0xffff) / 65535f; }
    }
}





