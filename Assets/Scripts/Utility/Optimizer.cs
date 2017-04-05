using FGUnity.Utils;
using System;
using System.Collections;
using System.Text;
using UnityEngine;

[Prefab("Optimizer")]
public class Optimizer : SingletonBehavior<Optimizer>
{
    public float FPS { get { return FPSCounter.Exists ? FPSCounter.instance.Framerate : 0; } }
    public bool Low { get { return m_OptimizeLevel > 0; } }
    public bool SuperLow { get { return m_OptimizeLevel > 1; } }

    public bool DisableMasks { get { return Low || m_DisableMasks; } }

    public void SetMaskingEnabled(bool inbEnabled)
    {
        m_DisableMasks = !inbEnabled;
    }

    private int m_OptimizeLevel = 0;
    private int m_InitialResolution;
    private int m_ScaledResolution;

    private bool m_DisableMasks = false;

    private const int MIN_RESOLUTION = 320;
    private const int MAX_RESOLUTION = 720;

    // Exceptions to the normal rules,
    // since some devices can pass all the other criteria
    // and still run poorly
    private readonly string[] LOW_END_DEVICES = new string[]
    {
        "Adreno (TM) 305"
    };

    // I'm not a fan of this hack
    private readonly string[] SUPER_LOW_DEVICES = new string[]
    {

    };

    private IEnumerator Start()
    {
        yield return null;

        CalculateOptimizeLevel();
        CalculateResolution();

        FirstOptimize();
        Optimize();
    }

    private void CalculateOptimizeLevel()
    {
#if UNITY_ANDROID
        LogSystemInfo();
        if (IsSuperLow())
        {
            m_OptimizeLevel = 2;
        }
        else if (IsLowEnd())
        {
            m_OptimizeLevel = 1;
        }
#else
        m_OptimizeLevel = 0;
#endif
    }

    private bool IsLowEnd()
    {
        if (SystemInfo.systemMemorySize <= 1024 || SystemInfo.processorCount < 4 || SystemInfo.graphicsMemorySize < 128)
            return true;

        string deviceName = SystemInfo.graphicsDeviceName;
        foreach (var name in LOW_END_DEVICES)
            if (deviceName == name)
                return true;

        return false;
    }

    private bool IsSuperLow()
    {
        if (SystemInfo.graphicsMemorySize < 64 || SystemInfo.systemMemorySize <= 512)
            return true;

        string deviceName = SystemInfo.graphicsDeviceName;
        foreach (var name in SUPER_LOW_DEVICES)
            if (deviceName == name)
                return true;

        return false;
    }

    private void LogSystemInfo()
    {
        using(PooledStringBuilder builder = PooledStringBuilder.Create())
        {
            StringBuilder b = builder.Builder;

            b.Append("---- DEVICE SPECS (ACCORDING TO UNITY)---\n");
            b.Append("Device Name: ").Append(SystemInfo.graphicsDeviceName).Append('\n');
            b.Append("VRAM: ").Append(SystemInfo.graphicsMemorySize).Append('\n');
            b.Append("Shader Level: ").Append(SystemInfo.graphicsShaderLevel).Append('\n');
            b.Append("Processors: ").Append(SystemInfo.processorCount).Append('\n');
            b.Append("Render Targets: ").Append(SystemInfo.supportedRenderTargetCount).Append('\n');
            b.Append("3D Textures: ").Append(SystemInfo.supports3DTextures).Append('\n');
            b.Append("Instancing: ").Append(SystemInfo.supportsInstancing).Append('\n');
            b.Append("RAM: ").Append(SystemInfo.systemMemorySize).Append('\n');
            b.Append("Screen: ").Append(Screen.width).Append('x').Append(Screen.height).Append('\n');

            Logger.Log(b.ToString());
        }
    }

    private void CalculateResolution()
    {
        m_InitialResolution = Screen.height;

        if (m_InitialResolution > MIN_RESOLUTION)
        {
            if (m_OptimizeLevel == 1)
                m_ScaledResolution = Math.Max(MIN_RESOLUTION, (int)(m_InitialResolution * 0.75f));
            else if (m_OptimizeLevel == 2)
                m_ScaledResolution = Math.Max(MIN_RESOLUTION, m_InitialResolution / 2);
            else
                m_ScaledResolution = m_InitialResolution;
        }
        if (m_ScaledResolution > MAX_RESOLUTION)
            m_ScaledResolution = MAX_RESOLUTION;
    }

    private void FirstOptimize()
    {
        FixResolution();

        if (m_OptimizeLevel > 0)
        {
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 0;
        }

        if (m_OptimizeLevel == 2)
        {
            Debug.Log("Optimizing at: SuperLow");
            QualitySettings.SetQualityLevel(0, true);
        }
        else if (m_OptimizeLevel == 1)
        {
            Debug.Log("Optimizing at: Low");
        }
    }

    private void Optimize()
    {
        FixCamera();
        FixLoading();

        if (m_OptimizeLevel < 1)
            return;

        FixCanvas();

        if (m_OptimizeLevel < 2)
            return;
    }

    private void FixCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }
    }

    private void FixCanvas()
    {
    }

    private void FixResolution()
    {
        ScaleScreen(m_ScaledResolution);
    }

    private void ScaleScreen(int inTargetHeight)
    {
        if (Screen.height == inTargetHeight)
            return;

        float aspectRatio = (float)Screen.width / Screen.height;
        int targetWidth = (int)(aspectRatio * inTargetHeight);
        Screen.SetResolution(targetWidth, inTargetHeight, Screen.fullScreen);
    }

    private void FixLoading()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    private IEnumerator OnLevelWasLoaded(int levelIndex)
    {
        yield return null;
        Optimize();
    }
}
