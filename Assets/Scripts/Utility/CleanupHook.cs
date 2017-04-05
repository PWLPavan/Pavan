using System;
using System.Collections;
using UnityEngine;

public class CleanupHook : LazySingletonBehavior<CleanupHook>
{
    protected override void Awake()
    {
        base.Awake();

        KeepAlive.Apply(this);

        //StartCoroutine(CleanupLoop());
    }

    private IEnumerator CleanupLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(15.0f);
            Cleanup();
        }
    }

    public void Cleanup()
    {
        GC.Collect();
        Resources.UnloadUnusedAssets();
    }
}
