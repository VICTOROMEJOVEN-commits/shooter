using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
[InitializeOnLoad]
public static class ItchWindowsBuild
{
 const string Root=@"C:\Users\tradi\Documents\Codex\2026-11-10\el-trabajo-pr-ctico-consiste-en\outputs";
 const string Request="Library/ItchWindowsRequest.txt";
 static ItchWindowsBuild(){EditorApplication.update+=Tick;}
 static void Tick(){
 if(!File.Exists(Request)||BuildPipeline.isBuildingPlayer||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlaying)return;
 if(!File.Exists(Root+"/the-cementery-of-the-dead-webgl/index.html"))return;
 if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64){EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64);return;}
 File.Delete(Request);
 try{
 PlayerSettings.productName="The Cementery of the Dead";
 PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
 PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
 AssetDatabase.SaveAssets();
 File.WriteAllText(Root+"/windows-status.txt","Generando version Windows...");
 var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/first person.unity"},locationPathName=Root+"/the-cementery-of-the-dead-windows/The Cementery of the Dead.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
 File.WriteAllText(Root+"/windows-status.txt",report.summary.result+"; errores="+report.summary.totalErrors+"; bytes="+report.summary.totalSize+"; duracion="+report.summary.totalTime);
 }catch(Exception e){File.WriteAllText(Root+"/windows-status.txt",e.ToString());Debug.LogException(e);}
 }
}
