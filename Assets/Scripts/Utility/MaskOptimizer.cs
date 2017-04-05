using FGUnity.Utils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MaskOptimizer : MonoBehaviour
{
    public Action OnMaskingDisabled;

    private Mask[] m_Masks;

    private const float FPS_THRESHOLD = 24;
    private const float SAMPLE_DELAY = 0.5f;
    private const int CONSECUTIVE_SAMPLES = 6;

    private CoroutineHandle m_CheckRoutine;

    private void Awake()
    {
        m_Masks = GetComponentsInChildren<Mask>(true);
    }

    public void UpdateState()
    {
        bool enabledState = !Optimizer.instance.DisableMasks;
        foreach (var mask in m_Masks)
            mask.enabled = enabledState;
    }

    public void StartChecking()
    {
        UpdateState();
        if (!Optimizer.instance.DisableMasks)
        {
            this.ReplaceCoroutine(ref m_CheckRoutine, Check());
        }
    }

    public void StopChecking()
    {
        m_CheckRoutine.Clear();
    }

    private IEnumerator Check()
    {
        Optimizer opt = Optimizer.instance;
        int positiveSamples = 0;
        while (true)
        {
            yield return SAMPLE_DELAY;
            if (opt.FPS < FPS_THRESHOLD)
            {
                if (++positiveSamples >= CONSECUTIVE_SAMPLES)
                {
                    Debug.Log("Disabling masking!");
                    opt.SetMaskingEnabled(false);
                    UpdateState();
                    if (OnMaskingDisabled != null)
                        OnMaskingDisabled();
                    yield break;
                }
            }
            else
            {
                positiveSamples = 0;
            }
        }
    }
}
