using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FGUnity.Utils;

/// <summary>
/// Wrapper around an AndroidJavaObject.
/// The intent is to mimic the API as closely
/// as possible through these subclasses.
/// </summary>
public abstract class JavaWrapper : IDisposable
{
#if UNITY_ANDROID
    protected AndroidJavaObject m_InternalObject;

    public JavaWrapper(AndroidJavaObject inJavaObject)
    {
        m_InternalObject = inJavaObject;
    }
#endif

    public virtual void Dispose()
    {
#if UNITY_ANDROID
        Ref.Dispose(ref m_InternalObject);
#endif
    }
}