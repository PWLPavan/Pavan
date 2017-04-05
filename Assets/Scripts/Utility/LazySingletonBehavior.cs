using UnityEngine;
using System.Collections;

public abstract class LazySingletonBehavior<T> : MonoBehaviour where T : LazySingletonBehavior<T>
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
            sInstance = null;
            SingletonMgr.RemoveSingleton(this);
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
                sInstance = new GameObject(typeof(T).Name + "::Singleton").AddComponent<T>();
            }
            DontDestroyOnLoad(sInstance);
        }
    }

    static public void DestroySingleton()
    {
        if (sInstance != null)
        {
            SingletonMgr.RemoveSingleton(sInstance);
            Destroy(sInstance.gameObject);
            sInstance = null;
        }
    }
}
