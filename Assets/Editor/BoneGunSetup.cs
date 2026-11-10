using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class BoneGunSetup
{
    const string Folder = "Assets/Weapons/Osario";
    static readonly List<GameObject> pieces = new List<GameObject>();
    [MenuItem("Tools/Shooter/Crear arma Osario")]
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/first person.unity") scene = EditorSceneManager.OpenScene("Assets/Scenes/first person.unity");
        pieces.Clear();
        var bone = Material("Marfil envejecido", new Color(0.86f, 0.79f, 0.61f), 0.16f, 0, new Color(0.24f, 0.21f, 0.15f));
        var iron = Material("Hierro ennegrecido", new Color(0.065f, 0.078f, 0.08f), 0.32f, 0.8f, Color.black);
        var black = Material("Cavidades oscuras", new Color(0.012f, 0.018f, 0.015f), 0.05f, 0, Color.black);
        var brass = Material("Bronce antiguo", new Color(0.31f, 0.21f, 0.085f), 0.25f, 0.75f, Color.black);
        var soul = Material("Alma verde", new Color(0.16f, 0.7f, 0.43f), 0.3f, 0.15f, new Color(0.12f, 1, 0.48f) * 2.4f);
        var texture = new Texture2D(256, 256, TextureFormat.RGB24, true);
        var colors = new Color[256*256];
        for (int y=0;y<256;y++) for(int x=0;x<256;x++)
        {
            float n = Mathf.PerlinNoise(x/20f,y/49f)*0.14f + Mathf.PerlinNoise(x/3f,y/9f)*0.06f;
            colors[y*256+x] = new Color(0.82f+n,0.80f+n,0.72f+n);
        }
        texture.SetPixels(colors); texture.Apply(); string texturePath=Folder+"/Textures/Marfil.png";
        File.WriteAllBytes(texturePath,texture.EncodeToPNG()); Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(texturePath);
        bone.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        var model = new GameObject("Osario - Arma de huesos");
        // Black iron receiver, chamber and open barrel.
        Part(model, "Cuerpo de hierro", PrimitiveType.Cube, new Vector3(0,0,0.08f), new Vector3(0.15f,0.13f,0.32f), iron);
        Part(model, "Tambor", PrimitiveType.Cylinder, new Vector3(0,0.035f,0.13f), new Vector3(0.19f,0.075f,0.19f), iron, Quaternion.Euler(90,0,0));
        Tube(model,"Canon hueco",new Vector3(0,0.055f,0.36f),0.046f,0.029f,0.3f,iron);
        Tube(model,"Anillo frontal",new Vector3(0,0.055f,0.505f),0.063f,0.032f,0.065f,brass);
        Tube(model,"Hueso frontal",new Vector3(0,0.055f,0.465f),0.069f,0.045f,0.025f,bone);
        for(int i=0;i<6;i++)
        {
            float a=i*Mathf.PI/3;
            Part(model,"Sello del tambor",PrimitiveType.Sphere,new Vector3(Mathf.Sin(a)*0.096f,0.035f+Mathf.Cos(a)*0.096f,0.13f),new Vector3(0.017f,0.017f,0.05f),soul);
        }
        // The skull faces the player, with separate sockets, nose, teeth and jaw.
        Part(model,"Craneo",PrimitiveType.Sphere,new Vector3(0,0.075f,-0.105f),new Vector3(0.225f,0.218f,0.18f),bone);
        Part(model,"Maxilar",PrimitiveType.Sphere,new Vector3(0,-0.002f,-0.15f),new Vector3(0.15f,0.10f,0.105f),bone);
        for(int side=-1;side<=1;side+=2)
        {
            Part(model,"Cuenca ocular",PrimitiveType.Sphere,new Vector3(side*0.052f,0.072f,-0.190f),new Vector3(0.080f,0.061f,0.037f),black,Quaternion.Euler(0,side*9,side*11));
            Part(model,"Brillo ocular",PrimitiveType.Sphere,new Vector3(side*0.052f,0.071f,-0.210f),new Vector3(0.030f,0.024f,0.014f),soul);
            Bone(model,new Vector3(side*0.019f,0.107f,-0.198f),new Vector3(side*0.089f,0.115f,-0.171f),0.013f,bone);
            Part(model,"Pomulo",PrimitiveType.Sphere,new Vector3(side*0.083f,0.007f,-0.172f),new Vector3(0.065f,0.052f,0.048f),bone);
            Bone(model,new Vector3(side*0.082f,-0.012f,-0.151f),new Vector3(side*0.063f,-0.071f,-0.151f),0.013f,bone);
        }
        var nose = new GameObject("Nariz triangular",typeof(MeshFilter),typeof(MeshRenderer)); nose.transform.SetParent(model.transform,false);
        var nm=new Mesh { vertices=new[]{new Vector3(0,0.046f,-0.208f),new Vector3(-0.018f,0.007f,-0.208f),new Vector3(0.018f,0.007f,-0.208f)}, triangles=new[]{0,2,1,0,1,2} };nm.RecalculateNormals();nose.GetComponent<MeshFilter>().sharedMesh=nm;nose.GetComponent<MeshRenderer>().sharedMaterial=black;pieces.Add(nose);
        for(int i=0;i<6;i++) Part(model,"Diente",PrimitiveType.Cube,new Vector3((i-2.5f)*0.019f,-0.035f,-0.196f),new Vector3(0.016f,0.029f+(i%2)*0.004f,0.021f),bone,Quaternion.Euler(0,(i-2.5f)*4,(i-2.5f)*-3));
        Bone(model,new Vector3(-0.062f,-0.075f,-0.16f),new Vector3(0.062f,-0.075f,-0.16f),0.016f,bone);
        // Vertebrae and paired ribs around the green core.
        for(int i=0;i<5;i++)
        {
            float z=0.015f+i*0.082f;
            Part(model,"Vertebra dorsal",PrimitiveType.Sphere,new Vector3(0,0.135f,z),new Vector3(0.075f,0.04f,0.055f),bone);
            Part(model,"Apofisis",PrimitiveType.Cube,new Vector3(0,0.161f,z),new Vector3(0.024f,0.032f,0.03f),bone,Quaternion.Euler(-18,0,0));
            for(int side=-1;side<=1;side+=2)
            {
                var curve=new List<Vector3>();
                for(int j=0;j<=10;j++){float a=j/10f*Mathf.PI;curve.Add(new Vector3(side*Mathf.Sin(a)*0.095f,0.05f+Mathf.Cos(a)*0.085f,z+Mathf.Sin(a)*0.016f));}
                Curve(model,"Costilla",curve,0.012f,bone);
            }
        }
        // Spine-like grip and bone trigger guard.
        Part(model,"Empunadura",PrimitiveType.Cube,new Vector3(0,-0.17f,-0.02f),new Vector3(0.105f,0.26f,0.10f),iron,Quaternion.Euler(18,0,0));
        for(int i=0;i<5;i++)
        {
            float y=-0.07f-i*0.047f; float z=-0.016f-(i-2)*0.012f;
            Part(model,"Vertebra del mango",PrimitiveType.Sphere,new Vector3(0,y,z),new Vector3(0.135f,0.042f,0.115f),bone);
            Part(model,"Ligadura de bronce",PrimitiveType.Cube,new Vector3(0,y-0.02f,z-0.045f),new Vector3(0.095f,0.012f,0.025f),brass);
        }
        var guard=new List<Vector3>();
        for(int i=0;i<=20;i++){float a=i/20f*Mathf.PI;guard.Add(new Vector3(0,-0.07f-Mathf.Sin(a)*0.11f,0.065f-Mathf.Cos(a)*0.09f));}
        Curve(model,"Guardamonte de hueso",guard,0.012f,bone);
        Part(model,"Gatillo",PrimitiveType.Cube,new Vector3(0,-0.105f,0.085f),new Vector3(0.02f,0.065f,0.022f),brass,Quaternion.Euler(20,0,0));
        Combine(model,new[]{bone,iron,black,brass,soul});
        var prefabPath=Folder+"/Osario.prefab";
        var prefab=PrefabUtility.SaveAsPrefabAsset(model,prefabPath);Object.DestroyImmediate(model);
        var mount=GameObject.Find("weapon");
        Undo.RecordObject(mount.transform,"Montar arma de huesos");
        mount.transform.localPosition=new Vector3(0.17f,-0.145f,0.74f);mount.transform.localRotation=Quaternion.identity;mount.transform.localScale=Vector3.one * 0.68f;
        var oldMesh=mount.GetComponent<MeshRenderer>(); if(oldMesh){Undo.RecordObject(oldMesh,"Ocultar arma provisional");oldMesh.enabled=false;}
        foreach(var c in mount.GetComponentsInChildren<Collider>()) {Undo.RecordObject(c,"Desactivar colision de arma");c.enabled=false;}
        foreach(var child in mount.transform.Cast<Transform>().Where(c=>c.name.StartsWith("Osario")).ToArray()) Undo.DestroyObjectImmediate(child.gameObject);
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,mount.transform);instance.name="Osario - Arma de huesos";instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=Quaternion.Euler(-4,-25,0);
        Undo.RegisterCreatedObjectUndo(instance,"Arma Osario");
        var spawn=mount.transform.Find("SpawnPoint"); Undo.RecordObject(spawn,"Salida del canon");spawn.localPosition=Quaternion.Euler(-4,-25,0)*new Vector3(0,0.055f,0.56f);spawn.localRotation=Quaternion.Euler(90,0,0);spawn.localScale=Vector3.one;
        var view=mount.GetComponent<BoneWeaponView>();if(!view)view=Undo.AddComponent<BoneWeaponView>(mount);view.visual=instance.transform;
        var oldFlash=mount.transform.Find("Destello Osario");if(oldFlash)Undo.DestroyObjectImmediate(oldFlash.gameObject);
        var flash=GameObject.CreatePrimitive(PrimitiveType.Sphere);flash.name="Destello Osario";flash.transform.SetParent(mount.transform,false);flash.transform.localPosition=Quaternion.Euler(-4,-25,0)*new Vector3(0,0.055f,0.57f);flash.transform.localRotation=Quaternion.Euler(-4,-25,0);flash.transform.localScale=new Vector3(0.06f,0.06f,0.13f);Object.DestroyImmediate(flash.GetComponent<Collider>());flash.GetComponent<Renderer>().sharedMaterial=soul;
        var light=flash.AddComponent<Light>();light.color=new Color(0.3f,1,0.5f);light.intensity=1.2f;light.range=2;flash.SetActive(false);view.muzzleFlash=flash;view.flashLight=light;
        foreach(var tr in mount.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=mount.layer;
        EditorUtility.SetDirty(view);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);EditorSceneManager.OpenScene(scene.path);Debug.Log("OSARIO_WEAPON_READY");
    }
    static Material Material(string name,Color color,float smooth,float metallic,Color emission)
    {
        string path=Folder+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);if(emission.maxColorComponent>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);}EditorUtility.SetDirty(m);return m;
    }
    static GameObject Part(GameObject root,string name,PrimitiveType type,Vector3 position,Vector3 scale,Material m,Quaternion? rotation=null)
    {
        var p=GameObject.CreatePrimitive(type);p.name=name;p.transform.SetParent(root.transform,false);p.transform.localPosition=position;p.transform.localScale=scale;p.transform.localRotation=rotation??Quaternion.identity;Object.DestroyImmediate(p.GetComponent<Collider>());p.GetComponent<Renderer>().sharedMaterial=m;pieces.Add(p);return p;
    }
    static void Bone(GameObject root,Vector3 a,Vector3 b,float radius,Material m)
    {
        var direction=b-a;Part(root,"Hueso",PrimitiveType.Cylinder,(a+b)*0.5f,new Vector3(radius*1.5f,direction.magnitude/2,radius*1.5f),m,Quaternion.FromToRotation(Vector3.up,direction));
        Part(root,"Articulacion",PrimitiveType.Sphere,a,Vector3.one*radius*2,m);Part(root,"Articulacion",PrimitiveType.Sphere,b,Vector3.one*radius*2,m);
    }
    static void Tube(GameObject root,string name,Vector3 position,float outer,float inner,float length,Material material)
    {
        const int n=16;var verts=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
        for(int ring=0;ring<4;ring++)for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;float r=ring<2?outer:inner;verts.Add(new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,(ring%2==0?-1:1)*length/2));uv.Add(new Vector2(i/(float)n,ring%2));}
        for(int i=0;i<n;i++){int j=(i+1)%n;tris.AddRange(new[]{i,j,n+j,i,n+j,n+i,2*n+i,3*n+j,2*n+j,2*n+i,3*n+i,3*n+j,n+i,n+j,3*n+j,n+i,3*n+j,3*n+i,i,2*n+j,j,i,2*n+i,2*n+j});}
        var mesh=new Mesh();mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();AddMesh(root,name,mesh,position,material);
    }
    static void Curve(GameObject root,string name,List<Vector3> points,float radius,Material material)
    {
        const int sides=8;var vertices=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
        for(int i=0;i<points.Count;i++)
        {
            Vector3 forward=(points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(0,i-1)]).normalized;
            Vector3 side=Vector3.Cross(forward,Mathf.Abs(forward.x)<0.9f?Vector3.right:Vector3.up).normalized;Vector3 normal=Vector3.Cross(side,forward).normalized;
            for(int j=0;j<sides;j++){float a=j*Mathf.PI*2/sides;vertices.Add(points[i]+radius*(side*Mathf.Cos(a)+normal*Mathf.Sin(a)));uv.Add(new Vector2(j/(float)sides,i/(float)points.Count));}
            if(i==0)continue;for(int j=0;j<sides;j++){int a=(i-1)*sides+j,b=(i-1)*sides+(j+1)%sides,c=i*sides+j,d=i*sides+(j+1)%sides;tris.AddRange(new[]{a,b,d,a,d,c});}
        }
        var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();AddMesh(root,name,mesh,Vector3.zero,material);
    }
    static void AddMesh(GameObject root,string name,Mesh mesh,Vector3 position,Material material)
    {
        var p=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));p.transform.SetParent(root.transform,false);p.transform.localPosition=position;p.GetComponent<MeshFilter>().sharedMesh=mesh;p.GetComponent<Renderer>().sharedMaterial=material;pieces.Add(p);
    }
    static void Combine(GameObject root,Material[] materials)
    {
        for(int i=0;i<materials.Length;i++)
        {
            var sources=pieces.Where(p=>p.GetComponent<Renderer>().sharedMaterial==materials[i]).ToArray();
            var combine=sources.Select(p=>new CombineInstance{mesh=p.GetComponent<MeshFilter>().sharedMesh,transform=root.transform.worldToLocalMatrix*p.transform.localToWorldMatrix}).ToArray();
            var mesh=new Mesh{name="Osario - "+materials[i].name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combine,true,true);mesh.RecalculateBounds();
                        string path=Folder+"/Meshes/Osario_"+i+".obj";
            ExportObj(mesh,path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
            var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.globalScale=1;importer.useFileScale=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
            var imported=AssetDatabase.LoadAssetAtPath<GameObject>(path);var importedMesh=imported.GetComponentInChildren<MeshFilter>().sharedMesh;
            if(!importedMesh)throw new Exception("No se pudo importar la geometria del arma.");
            Object.DestroyImmediate(mesh);mesh=importedMesh;
            var obj=new GameObject(materials[i].name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(root.transform,false);obj.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.GetComponent<MeshRenderer>();renderer.sharedMaterial=materials[i];renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        }
        foreach(var p in pieces)Object.DestroyImmediate(p);pieces.Clear();
    }
    static void ExportObj(Mesh mesh,string path)
    {
        var culture=System.Globalization.CultureInfo.InvariantCulture;
        var text=new System.Text.StringBuilder("# Osario original skeletal weapon\no Osario\n");
        foreach(var v in mesh.vertices)text.Append("v ").Append((-v.x).ToString("R",culture)).Append(' ').Append(v.y.ToString("R",culture)).Append(' ').Append(v.z.ToString("R",culture)).Append('\n');
        foreach(var v in mesh.uv)text.Append("vt ").Append(v.x.ToString("R",culture)).Append(' ').Append(v.y.ToString("R",culture)).Append('\n');
        foreach(var n in mesh.normals)text.Append("vn ").Append((-n.x).ToString("R",culture)).Append(' ').Append(n.y.ToString("R",culture)).Append(' ').Append(n.z.ToString("R",culture)).Append('\n');
        var indices=mesh.triangles;
        for(int i=0;i<indices.Length;i+=3)
        {
            text.Append("f");foreach(int k in new[]{indices[i],indices[i+2],indices[i+1]}){int v=k+1;text.Append(' ').Append(v).Append('/').Append(v).Append('/').Append(v);}text.Append('\n');
        }
        File.WriteAllText(path,text.ToString());
    }
}



