using UnityEngine;
using System.Collections.Generic;
using System.Collections;

static public class SingletonMgr
{
    static public void Initialize()
    {
        if (s_ExistingSingletons == null)
        {
            s_ExistingSingletons = new HashSet<MonoBehaviour>();
            s_LazyInstantiationAllowed = true;
        }
    }

    static private bool s_LazyInstantiationAllowed = true;
    static public bool LazyInstantiationAllowed { get { return s_LazyInstantiationAllowed; } }
    static private HashSet<MonoBehaviour> s_ExistingSingletons;

    static public void AllowLazyInstantiation()
    {
        s_LazyInstantiationAllowed = true;
    }

    static public void DisableLazyInstantiation()
    {
        s_LazyInstantiationAllowed = false;
    }

    static public void AddSingleton<T>(SingletonBehavior<T> inBehavior) where T : SingletonBehavior<T>
    {
        Initialize();
        s_ExistingSingletons.Add(inBehavior);
    }

    static public void AddSingleton<T>(LazySingletonBehavior<T> inBehavior) where T : LazySingletonBehavior<T>
    {
        Initialize(); 
        s_ExistingSingletons.Add(inBehavior);
    }

    static public void RemoveSingleton<T>(SingletonBehavior<T> inBehavior) where T : SingletonBehavior<T>
    {
        Initialize();
        s_ExistingSingletons.Remove(inBehavior);
    }

    static public void RemoveSingleton<T>(LazySingletonBehavior<T> inBehavior) where T : LazySingletonBehavior<T>
    {
        Initialize();
        s_ExistingSingletons.Remove(inBehavior);
    }

    static public void ClearAll()
    {
        Initialize();

        bool oldLazySetting = s_LazyInstantiationAllowed;
        DisableLazyInstantiation();

        MonoBehaviour[] toDelete = new MonoBehaviour[s_ExistingSingletons.Count];
        s_ExistingSingletons.CopyTo(toDelete);
        s_ExistingSingletons.Clear();
        foreach(MonoBehaviour singleton in toDelete)
        {
            if (singleton != null)
                MonoBehaviour.Destroy(singleton.gameObject);
        }

        if (oldLazySetting)
            AllowLazyInstantiation();
    }
}
