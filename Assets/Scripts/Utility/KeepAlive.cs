using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class KeepAlive : MonoBehaviour
{
    static public void Apply(MonoBehaviour inBehavior)
    {
        if (!inBehavior.gameObject.GetComponent<KeepAlive>())
            inBehavior.gameObject.AddComponent<KeepAlive>();
    }

    static public void DestroyEverything()
    {
        foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
        {
            try { if (!obj.GetComponent<KeepAlive>()) GameObject.DestroyImmediate(obj); }
            catch { Debug.Log("Unable to destroy " + obj.ToString()); }
        }
    }
}
