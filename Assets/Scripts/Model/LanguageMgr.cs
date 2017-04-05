using UnityEngine;
using System.Collections.Generic;
using System;
using SimpleJSON;
using Ekstep;
using FGUnity.Utils;

[Prefab("LanguageManager")]
public class LanguageMgr : SingletonBehavior<LanguageMgr>
{
    public List<LanguageConfig> Languages;

    public LanguageConfig Current { get; private set; }

    public void NextLanguage()
    {
        int languageIndex = Languages.IndexOf(Current);
        int nextIndex = (languageIndex + 1) % Languages.Count;
        Current = Languages[nextIndex];
        SaveData.instance.LanguageCode = Current.Code;

        if (languageIndex != nextIndex && OnLanguageChanged != null)
            OnLanguageChanged(Current);
    }

    private void Start()
    {
        LanguageConfig userConfig = FindLanguage(SaveData.instance.LanguageCode);
        if (userConfig == null)
            userConfig = FindLanguage(Genie.I.UserLanguage);
        if (userConfig == null)
            userConfig = Languages[0];

        Current = userConfig;
        SaveData.instance.LanguageCode = Current.Code;
    }

    public LanguageConfig FindLanguage(string inLanguageName)
    {
        foreach(var language in Languages)
        {
            if (language.Code == inLanguageName)
                return language;
        }

        return null;
    }

    public event Action<LanguageConfig> OnLanguageChanged;
}
