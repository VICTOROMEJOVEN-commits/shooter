using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
[InitializeOnLoad]
public static class ItchWebGLBuild
{
 const string Root=@"C:\Users\tradi\Documents\Codex\2026-11-10\el-trabajo-pr-ctico-consiste-en\outputs";
 const string Request="Library/ItchWebGLRequest.txt";
 static ItchWebGLBuild(){EditorApplication.update+=Tick;}
 static void Tick(){
 if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlaying)return;
 if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL)){File.WriteAllText(Root+"/webgl-status.txt","Esperando reconocimiento del modulo WebGL. Reiniciar Unity.");return;}
 if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.WebGL){EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.WebGL,BuildTarget.WebGL);return;}
 File.Delete(Request);
 try{
 PlayerSettings.productName="The Cementery of the Dead";
 PlayerSettings.WebGL.template="PROJECT:ItchWebGL";
 PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
 PlayerSettings.WebGL.decompressionFallback=false;
 PlayerSettings.defaultWebScreenWidth=1280;PlayerSettings.defaultWebScreenHeight=720;
 AssetDatabase.SaveAssets();
 File.WriteAllText(Root+"/webgl-status.txt","Generando build WebGL...");
 var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/first person.unity"},locationPathName=Root+"/the-cementery-of-the-dead-webgl",target=BuildTarget.WebGL,options=BuildOptions.None});
 File.WriteAllText(Root+"/webgl-status.txt",(File.Exists(Root+"/the-cementery-of-the-dead-webgl/index.html") ? "Archivos generados; " : "SIN ARCHIVOS; ")+report.summary.result+"; errores="+report.summary.totalErrors+"; bytes="+report.summary.totalSize+"; duracion="+report.summary.totalTime);
 }catch(Exception e){File.WriteAllText(Root+"/webgl-status.txt",e.ToString());Debug.LogException(e);}
 }
}

