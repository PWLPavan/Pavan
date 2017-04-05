using System;
using UnityEngine;

namespace FGUnity.Utils
{
    /// <summary>
    /// Allows a MonoBehavior to specify a link to a resource
    /// in the Resources folder for easy instantiation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PrefabAttribute : Attribute
    {
        public PrefabAttribute(string inPrefabPath)
        {
            Path = inPrefabPath;
        }

        public string Path { get; private set; }

        private T Spawn<T>() where T : MonoBehaviour
        {
            T prefab = Resources.Load<T>(Path);
            Assert.True(prefab != null, "Prefab exists.", "Unable to load resource of type {0} at Resources path '{1}'.", typeof(T).GetGenericName(), Path);
            return UnityEngine.Object.Instantiate<T>(prefab);
        }

        static public T Instantiate<T>() where T : MonoBehaviour
        {
            PrefabAttribute[] prefabAttributes = TypeHelper.GetCustomAttributes<PrefabAttribute>(typeof(T));
            if (prefabAttributes.Length > 0)
                return prefabAttributes[0].Spawn<T>();
            return null;
        }
    }
}
