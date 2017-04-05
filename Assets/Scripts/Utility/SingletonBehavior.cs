using UnityEngine;
using System.Collections;
using FGUnity.Utils;

public abstract class SingletonBehavior<T> : MonoBehaviour where T : SingletonBehavior<T>
{
    static private T sInstance = null;
    static public T instance
    {
        get
        {
            if (sInstance.IsNull())
                CreateSingleton();
            return sInstance;
        }
    }

    static public bool Exists
    {
        get { return sInstance != null; }
    }

    protected virtual void Awake()
    {
        if (sInstance == null)
        {
            sInstance = (T)this;
            DontDestroyOnLoad(this);
            SingletonMgr.AddSingleton(this);
        }
        else if (!object.ReferenceEquals(this, sInstance))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (object.ReferenceEquals(sInstance, this))
        {
            SingletonMgr.RemoveSingleton(this);
            sInstance = null;
        }
    }

    static public void CreateSingleton()
    {
        if (sInstance == null)
        {
            SingletonMgr.Initialize();
            sInstance = GameObject.FindObjectOfType<T>();
            if (sInstance == null && SingletonMgr.LazyInstantiationAllowed)
            {
                sInstance = PrefabAttribute.Instantiate<T>();
            }
            if (sInstance != null)
                DontDestroyOnLoad(sInstance.gameObject);
        }
    }

    static public void DestroySingleton()
    {
        if (sInstance != null)
        {
            Destroy(sInstance.gameObject);
            SingletonMgr.RemoveSingleton(sInstance);
            sInstance = null;
        }
    }
}
