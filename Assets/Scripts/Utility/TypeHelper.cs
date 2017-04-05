using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FGUnity.Utils
{
    /// <summary>
    /// Contains helper functions for types.
    /// </summary>
    static public class TypeHelper
    {
        /// <summary>
        /// Retrieves generic type name in a readable format.
        /// </summary>
        static public string GetGenericName(this Type inType)
        {
            if (!inType.IsGenericType)
                return inType.Name;

            using(PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
            {
                StringBuilder builder = stringBuilder.Builder;
                builder.Append(inType.Name.Substring(0, inType.Name.IndexOf('`')));

                Type[] genericArguments = inType.GetGenericArguments();
                builder.Append('<');
                for(int i = 0; i < genericArguments.Length; ++i)
                {
                    builder.Append(GetGenericName(genericArguments[i]));

                    if (i < genericArguments.Length - 1)
                        builder.Append(", ");
                }
                builder.Append('>');

                return builder.ToString();
            }
        }

        static public bool IsBaseType<T_Base, T_Child>()
        {
            return typeof(T_Base).IsAssignableFrom(typeof(T_Child));
        }

        static public bool IsBaseType<T_Base, T_Child>(T_Base inObjectA, T_Child inObjectB)
        {
            return inObjectA.GetType().IsAssignableFrom(inObjectB.GetType());
        }

        static public bool IsDerivedFrom<T_Base, T_Child>(T_Child inObject)
        {
            return typeof(T_Base).IsAssignableFrom(typeof(T_Child));
        }

        static public bool FindAllNonAbstractSubclasses(this Type inBaseType, ICollection<Type> ioSubclasses)
        {
            Assert.True(inBaseType != null, "Base type is not null.");

            return FindAllNonAbstractSubclasses(inBaseType, Assembly.GetAssembly(inBaseType), ioSubclasses);
        }

        static public bool FindAllNonAbstractSubclasses(this Type inBaseType, Assembly inAssembly, ICollection<Type> ioSubclasses)
        {
            Assert.True(inBaseType != null, "Base type is not null.");
            Assert.True(ioSubclasses != null, "Subclass collection is not null.");

            bool bAdded = false;

            if (!inBaseType.IsAbstract && inBaseType.IsPublic)
            {
                ioSubclasses.Add(inBaseType);
                bAdded = true;
            }

            foreach (Type type in inAssembly.GetTypes())
            {
                if (!type.IsAbstract && type.IsPublic && inBaseType.IsAssignableFrom(type))
                {
                    ioSubclasses.Add(type);
                    bAdded = true;
                }
            }

            return bAdded;
        }

        static public AttributeType[] GetCustomAttributes<AttributeType>(MemberInfo inInfo) where AttributeType : Attribute
        {
            return (AttributeType[])Attribute.GetCustomAttributes(inInfo, typeof(AttributeType));
        }

        static public AttributeType[] GetCustomAttributes<AttributeType>(Type inType) where AttributeType : Attribute
        {
            return (AttributeType[])Attribute.GetCustomAttributes(inType, typeof(AttributeType));
        }

        static public bool HasAttribute<AttributeType>(Type inType) where AttributeType : Attribute
        {
            return Attribute.IsDefined(inType, typeof(AttributeType));
        }

        static public bool HasAttribute<AttributeType>(MemberInfo inInfo) where AttributeType : Attribute
        {
            return Attribute.IsDefined(inInfo, typeof(AttributeType));
        }
    }
}
