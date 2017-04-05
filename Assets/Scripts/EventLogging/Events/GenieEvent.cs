using System.Text;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Collections;
using SimpleJSON;
using FGUnity.Utils;

namespace Ekstep
{
    public abstract class GenieEvent
    {
        public class EKSAttribute : Attribute { }
        public class EXTAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class RenamedInVersionAttribute : Attribute
        {
            public int Version;
            public string Name;

            public RenamedInVersionAttribute(int inVersion, string inName)
            {
                Version = inVersion;
                Name = inName;
            }
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class RemovedInVersionAttribute : Attribute
        {
            public int VersionRemoved;
            public int VersionReinstated;

            public RemovedInVersionAttribute(int inRemovedAt)
            {
                VersionRemoved = inRemovedAt;
                VersionReinstated = int.MaxValue;
            }

            public RemovedInVersionAttribute(int inRemovedAt, int inReinstatedAt)
            {
                VersionRemoved = inRemovedAt;
                VersionReinstated = inReinstatedAt;
            }
        }

        public class NewInVersionAttribute : RemovedInVersionAttribute
        {
            public NewInVersionAttribute(int inNewVersion)
                : base (1, inNewVersion)
            { }
        }

        public virtual string Name
        {
            get { return GetType().Name; }
        }

        public virtual int MinVersion { get { return 1; } }

        public virtual void WriteEDATA(JSONNode inNode)
        {
            int currentVersion = Genie.TELEMETRY_VERSION;

            JSONNode eks = inNode["eks"] = new JSONClass();
            JSONNode ext = inNode["ext"] = new JSONClass();

            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach(var f in fields)
            {
                JSONNode target = null;

                if (Attribute.IsDefined(f, typeof(EKSAttribute)))
                    target = eks;
                else if (Attribute.IsDefined(f, typeof(EXTAttribute)))
                    target = ext;
                else
                    continue;

                object value = f.GetValue(this);
                string name = f.Name;

                bool bWriteOut = value != null;
                if (bWriteOut)
                {
                    RemovedInVersionAttribute[] removedVersions = TypeHelper.GetCustomAttributes<RemovedInVersionAttribute>(f);
                    foreach(var removedAttribute in removedVersions)
                    {
                        if (currentVersion >= removedAttribute.VersionRemoved && currentVersion < removedAttribute.VersionReinstated)
                        {
                            bWriteOut = false;
                            break;
                        }
                    }
                }
                if (bWriteOut)
                {
                    RenamedInVersionAttribute[] renamedVersions = TypeHelper.GetCustomAttributes<RenamedInVersionAttribute>(f);
                    int minProcessed = 0;
                    foreach(var renameAttribute in renamedVersions)
                    {
                        if (minProcessed < renameAttribute.Version)
                        {
                            minProcessed = renameAttribute.Version;
                            if (currentVersion >= renameAttribute.Version)
                            {
                                name = renameAttribute.Name;
                            }
                        }
                    }

                    target.Add(name, JSONUtils.Parse(value));
                }
            }
        }
    }
}
