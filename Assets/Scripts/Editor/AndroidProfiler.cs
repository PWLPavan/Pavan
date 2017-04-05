using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using UnityEditor;

static public class AndroidProfiler
{
    [MenuItem("Custom/ADB/USB Profiling", priority=-1)]
    static public void ForwardDevice()
    {
        ProcessStartInfo adb = CreateADBProcessInfo("forward tcp:54999 localabstract:unity-" + PlayerSettings.bundleIdentifier);
        if (adb == null)
            return;

        ProcessHelper helper = new ProcessHelper(adb);
        helper.OnFinish = OnProcessFinish;
        helper.Run();

        s_RunningProcesses.Add(helper);
    }

    [MenuItem("Custom/ADB/Dump Memory Usage Info", priority = -1)]
    static public void DumpMemInfo()
    {
        ProcessStartInfo adb = CreateADBProcessInfo("shell dumpsys meminfo " + PlayerSettings.bundleIdentifier);
        if (adb == null)
            return;

        ProcessHelper helper = new ProcessHelper(adb);
        helper.OnFinish = OnProcessFinish;
        helper.Run();

        s_RunningProcesses.Add(helper);
    }

    [MenuItem("Custom/ADB/Clear Logcat", priority = -1)]
    static public void ClearLogcat()
    {
        ProcessStartInfo adb = CreateADBProcessInfo("logcat -c");
        if (adb == null)
            return;

        ProcessHelper helper = new ProcessHelper(adb);
        helper.OnFinish = OnProcessFinish;
        helper.Run();

        s_RunningProcesses.Add(helper);
    }

    [MenuItem("Custom/ADB/Capture Logcat", priority = -1)]
    static public void CaptureLogcat()
    {
        ProcessStartInfo adb = CreateADBProcessInfo("logcat -s Unity AndroidRuntime DEBUG ActivityManager");
        if (adb == null)
            return;

        ProcessHelper helper = new ProcessHelper(adb, "Logcat/logcat_" + GetDateTimeString() + ".log");
        helper.OnFinish = OnProcessFinish;
        helper.OnLog = FilterLogcat;
        helper.Run();

        s_RunningProcesses.Add(helper);
    }

    [MenuItem("Custom/ADB/Stop All")]
    static public void StopAllProcesses()
    {
        foreach (var process in s_RunningProcesses)
            process.Stop();
    }

    static private string GetADBPath()
    {
        string sdkRoot = EditorPrefs.GetString("AndroidSdkRoot");
        if (string.IsNullOrEmpty(sdkRoot))
        {
            UnityEngine.Debug.LogError("Unable to locate the Android SDK.");
            return null;
        }

        string adbPath = Path.GetFullPath(Path.Combine(sdkRoot, "platform-tools/adb.exe"));
        if (!File.Exists(adbPath))
        {
            UnityEngine.Debug.LogError("Unable to locate ADB at '" + adbPath + "'");
            return null;
        }

        return adbPath;
    }

    static private ProcessStartInfo CreateADBProcessInfo(string inArguments)
    {
        string adbPath = GetADBPath();
        if (adbPath == null)
            return null;

        if (s_RunningProcesses.Count > 0)
        {
            UnityEngine.Debug.LogError("ADB processes are already running.");
            return null;
        }

        ProcessStartInfo adb = new ProcessStartInfo();
        adb.FileName = adbPath;
        adb.Arguments = inArguments;
        adb.RedirectStandardOutput = true;
        adb.RedirectStandardError = true;
        adb.UseShellExecute = false;
        adb.CreateNoWindow = true;

        return adb;
    }

    static private string GetDateTimeString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }

    static private void OnProcessFinish(ProcessHelper inHelper)
    {
        if (inHelper.ReceivedErrors)
            UnityEngine.Debug.LogWarningFormat("Command '{0} {1}' finished unsuccessfully.", Path.GetFileName(inHelper.StartInfo.FileName), inHelper.StartInfo.Arguments);
        else
            UnityEngine.Debug.LogFormat("Command '{0} {1}' finished successfully.", Path.GetFileName(inHelper.StartInfo.FileName), inHelper.StartInfo.Arguments);

        s_RunningProcesses.Remove(inHelper);
    }

    static private void FilterLogcat(string inData)
    {
        string actualData = inData.Substring(inData.IndexOf(':') + 1);
        if (!String.IsNullOrEmpty(actualData) && !UNITY_IGNORED_LOGS.Contains(actualData.Trim()))
        {
            char level = Char.ToLowerInvariant(inData[0]);
            switch(level)
            {
                case 'v':
                case 'd':
                case 'i':
                    UnityEngine.Debug.Log(inData);
                    break;
                    
                case 'w':
                    UnityEngine.Debug.LogWarning(inData);
                    break;
                case 'e':
                    UnityEngine.Debug.LogError(inData);
                    break;

                default:
                    UnityEngine.Debug.Log(inData);
                    break;
            }
        }
    }

    static private readonly List<string> UNITY_IGNORED_LOGS = new List<string>(new string[]
        {
            "Unknown event structure (0)",
            "UnityEngine.Debug:Log(Object)",
            "UnityEngine.Debug:Internal_Log(Int32, String, Object)",
            "(Filename: ./artifacts/generated/common/runtime/UnityEngineDebugBindings.gen.cpp Line: 64)"
        }
        );

    static private List<ProcessHelper> s_RunningProcesses = new List<ProcessHelper>();
}
