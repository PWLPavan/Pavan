using System;

namespace FGUnity.Utils
{
    /// <summary>
    /// Contains helper functions operating on references.
    /// </summary>
    static public class Ref
    {
        /// <summary>
        /// Safely disposes of an object and sets its reference to null.
        /// </summary>
        static public void Dispose<T>(ref T inObjectToDispose) where T : class, IDisposable
        {
            if (inObjectToDispose != null)
            {
                inObjectToDispose.Dispose();
                inObjectToDispose = null;
            }
        }

        /// <summary>
        /// Safetly disposes and switches a disposable object to another object.
        /// </summary>
        static public void Replace<T>(ref T inObject, T inObjectToReplace) where T : class, IDisposable
        {
            if (inObject != null && inObject != inObjectToReplace)
                inObject.Dispose();
            inObject = inObjectToReplace;
        }
    }
}
