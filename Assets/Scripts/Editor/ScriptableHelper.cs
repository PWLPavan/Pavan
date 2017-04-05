using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

static public class ScriptableHelper
{
    static private void CreateAsset<T>() where T : ScriptableObject
    {
        T newScriptableObject = ScriptableObject.CreateInstance<T>();
        ProjectWindowUtil.CreateAsset(newScriptableObject, "New" + typeof(T).Name + ".asset");
    }

    [MenuItem("Assets/Create/Polaroid Config")]
    static public void CreateSoundTrigger()
    {
        CreateAsset<PolaroidConfig>();
    }

    [MenuItem("Assets/Create/Language Config")]
    static public void CreateLanguageConfig()
    {
        CreateAsset<LanguageConfig>();
    }
}