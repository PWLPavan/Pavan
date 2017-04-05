using System;
using UnityEngine;

/// <summary>
/// This acts to dispatch Unity's messages as C# events.
/// Important utilities can be hooked up here without needing
/// to create separate GameObjects as hooks.
/// </summary>
public class MessageHook : LazySingletonBehavior<MessageHook>
{
    public event Action OnUpdate;
    public event Action OnFixedUpdate;

    protected override void Awake()
    {
        base.Awake();

        KeepAlive.Apply(this);

        CleanupHook.CreateSingleton();
        FPSCounter.CreateSingleton();
    }

    private void FixedUpdate()
    {
        if (OnFixedUpdate != null)
            OnFixedUpdate();
    }

    private void Update()
    {
        if (OnUpdate != null)
            OnUpdate();
    }
}
