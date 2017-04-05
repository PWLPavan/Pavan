using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using SimpleJSON;
using FGUnity.Utils;

public class LocalizedText : MonoBehaviour
{
    public Text Target;
    public string Key;

    private bool m_Hooked = false;

    private void Start()
    {
        this.WaitOneFrameThen(Initialize);
    }

    private void Initialize()
    {
        HookEvents(true);
    }

    private void Localize(LanguageConfig inConfig)
    {
        Target.text = inConfig[Key];
        Target.font = inConfig.Font;
        Target.lineSpacing = inConfig.LineSpacing;
    }

    private void OnDestroy()
    {
        HookEvents(false);
    }

    private void HookEvents(bool inbHook)
    {
        if (!LanguageMgr.Exists)
            return;
        
        if (m_Hooked != inbHook)
        {
            m_Hooked = inbHook;
            if (inbHook)
            {
                LanguageMgr.instance.OnLanguageChanged += Localize;
                Localize(LanguageMgr.instance.Current);
            }
            else
            {
                LanguageMgr.instance.OnLanguageChanged -= Localize;
            }
        }
    }

    private void OnDisable()
    {
        HookEvents(false);
    }

    private void OnEnable()
    {
        HookEvents(true);
    }
}
