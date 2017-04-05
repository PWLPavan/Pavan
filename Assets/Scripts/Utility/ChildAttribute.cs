using System;
using System.Reflection;
using UnityEngine;

namespace FGUnity.Utils
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ChildAttribute : Attribute
    {
        public ChildAttribute()
        {
            Path = String.Empty;
        }

        public ChildAttribute(string inPath)
        {
            Path = inPath;
        }

        public string Path { get; private set; }

        static public void Parse(MonoBehaviour inBehavior)
        {
            Assert.True(inBehavior != null, "Behavior is not null.");
            Type type = inBehavior.GetType();
            foreach(var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                ChildAttribute[] attrs = (ChildAttribute[])field.GetCustomAttributes(typeof(ChildAttribute), true);
                if (attrs.Length > 0)
                {
                    ChildAttribute attr = attrs[0];
                    Transform childTransform = String.IsNullOrEmpty(attr.Path) ? inBehavior.transform : inBehavior.transform.Find(attr.Path);
                    Assert.True(childTransform != null, "Transform exists.", "Unable to find Transform at '{0}' on '{1}'.", attr.Path, inBehavior.gameObject.name);
                    if (field.FieldType == typeof(GameObject))
                    {
                        GameObject obj = childTransform.gameObject;
                        field.SetValue(inBehavior, obj);
                    }
                    else
                    {
                        Assert.True(typeof(Component).IsAssignableFrom(field.FieldType), "Field type inherits from Component.", "Field type of {0} incompatible with Component.", field.FieldType.GetGenericName());
                        Component component = childTransform.GetComponent(field.FieldType);
                        Assert.True(component != null, "Component exists.", "Unable to find Component of type '{0}' at '{1}' on '{1}'.", field.FieldType.GetGenericName(), attr.Path, inBehavior.gameObject.name);
                        field.SetValue(inBehavior, component);
                    }
                }
            }
        }
    }
}
