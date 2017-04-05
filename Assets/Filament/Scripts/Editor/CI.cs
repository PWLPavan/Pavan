using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using FGUnity.Editor;
using FGUnity.Utils;

public class CI
{
	static string[] SCENES = FindEnabledEditorScenes();
    static string APP_NAME = "TakeOff";
    static string TARGET_DIR = "dist";
	
	[MenuItem ("Custom/Build/CI/Win 64")]
	static void PerformWin64Build() {
		string target_dir = TARGET_DIR + "/Win64/" + APP_NAME + ".exe";
		GenericBuild(SCENES, target_dir, BuildTarget.StandaloneWindows64, BuildOptions.None);
	}
	
	[MenuItem ("Custom/Build/CI/Win 32")]		
	static void PerformWin32Build() {
		string target_dir = TARGET_DIR + "/Win32/" + APP_NAME + ".exe";
		GenericBuild(SCENES, target_dir, BuildTarget.StandaloneWindows, BuildOptions.None);
	}		
	
	[MenuItem ("Custom/Build/CI/Web Player")]		
	static void PerformWebPlayerBuild() {
		string target_dir = TARGET_DIR + "/Web/" + APP_NAME;
		GenericBuild(SCENES, target_dir, BuildTarget.WebPlayer, BuildOptions.None);
	}	
	
	[MenuItem ("Custom/Build/CI/Web GL")]		
	static void PerformWebGLBuild() {
		string target_dir = TARGET_DIR + "/WebGL/" + APP_NAME;
		GenericBuild(SCENES, target_dir, BuildTarget.WebGL, BuildOptions.None);
	}	
	
	[MenuItem ("Custom/Build/CI/OSX")]		
	static void PerformOSXBuild() {
		string target_dir = TARGET_DIR + "/OSX/" + APP_NAME + ".app";
		GenericBuild(SCENES, target_dir, BuildTarget.StandaloneOSXUniversal, BuildOptions.None);
	}		
	
	[MenuItem ("Custom/Build/CI/Linux")]		
	static void PerformLinuxBuild() {
		string target_dir = TARGET_DIR + "/Linux/" + APP_NAME;
		GenericBuild(SCENES, target_dir, BuildTarget.StandaloneLinuxUniversal, BuildOptions.None);
	}		
	
	[MenuItem ("Custom/Build/CI/iPhone")]		
	static void PerformiPhoneBuild() {
		string target_dir = TARGET_DIR + "/iPhone/" + APP_NAME + ".ipa";
		GenericBuild(SCENES, target_dir, BuildTarget.iOS, BuildOptions.None);
	}			
	
	[MenuItem ("Custom/Build/CI/Android")]		
	static void PerformAndroidBuild() {
		string target_dir = TARGET_DIR + "/Android/" + APP_NAME + ".apk";
		GenericBuild(SCENES, target_dir, BuildTarget.Android, BuildOptions.None);
	}
	
	private static string[] FindEnabledEditorScenes() {
		List<string> EditorScenes = new List<string>();
		foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if (!scene.enabled) continue;
			EditorScenes.Add(scene.path);
		}
		return EditorScenes.ToArray();
	}

    static BuildOptions ReadCommandLineArguments(ref string target_dir)
    {
        var args = Environment.GetCommandLineArgs();
        string currentConfig = "DISTRIBUTE";
        BuildOptions options = BuildOptions.None;

        for(int i = 0; i < args.Length; ++i)
        {
            string current = args[i];
            if (current == "-sign-apk")
            {
                if (i + 2 < args.Length)
                {
                    string keystorePass = args[i + 1];
                    string keyaliasPass = args[i + 2];
                    PlayerSettings.keystorePass = keystorePass; 
                    PlayerSettings.keyaliasPass = keyaliasPass;
                    i += 2;

                    Debug.LogFormat("Detected passwords for keystore and alias.", currentConfig);
                }
                else
                {
                    Debug.LogWarning("Not enough arguments provided to '-sign-apk'.");
                }
            }
            else if (current == "-debug")
            {
                options |= BuildOptions.AllowDebugging | BuildOptions.Development;
                currentConfig = "DEVELOPMENT";
            }
            else if (current == "-with-fps")
            {
                currentConfig = "RELEASE_TEST";
            }
            else if (current == "-filename")
            {
                if (i + 1 < args.Length)
                {
                    string oldTarget = target_dir;
                    string directory = Path.GetDirectoryName(oldTarget);
                    string extension = Path.GetExtension(oldTarget);
                    target_dir = Path.ChangeExtension(Path.Combine(directory, args[i + 1]), extension);
                    Debug.LogFormat("Switched output from '{0}' to '{1}'...", oldTarget, target_dir);
                }
            }

            Debug.LogFormat("Switching to {0} configuration...", currentConfig);
            GlobalDefines.Instance.SwitchConfiguration(currentConfig);
            GlobalDefines.Instance.SaveConfigurations();
        }

        return options;
    }
		
    static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
    {
		try {
			Directory.CreateDirectory(Application.dataPath + "/../" + target_dir);
		} catch (IOException) {}


        build_options |= ReadCommandLineArguments(ref target_dir);

        EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
        string res = BuildPipeline.BuildPlayer(scenes,target_dir,build_target,build_options);
        if (res.Length > 0)
        {
        	throw new Exception("BuildPlayer failure: " + res);
        }
        else
        {
            Debug.LogFormat("Finished building {0} to '{1}'", build_target.ToString(), target_dir);
        }
    }
}

