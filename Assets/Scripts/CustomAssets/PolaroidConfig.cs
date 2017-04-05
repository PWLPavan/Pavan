using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Serialization;

public class PolaroidConfig : ScriptableObject
{
    public Sprite Background;
    public Sprite Foreground;

    [FormerlySerializedAs("ThemedChickens")]
    public Sprite[] BrownChickens;
    public Sprite[] WhiteChickens;

    public AudioClip Stinger;
}
