using System;
using System.Collections.Generic;

namespace Ekstep
{
    public class OE_INTERACT : GenieEvent
    {
        public enum Type
        {
            TOUCH,
            DRAG,
            DROP,
            PINCH,
            ZOOM,
            SHAKE,
            ROTATE,
            SPEAK,
            LISTEN,
            WRITE,
            DRAW,
            START,
            END,
            CHOOSE,
            ACTIVATE,
            OTHER,
        }

        [EKS] public Type type;
        [EKS] public string extype;
        [EKS] public string id;
        [EKS] public string uri;

        [EKS] [NewInVersion(2)]
        public string subtype = string.Empty;

        [EKS] [NewInVersion(2)]
        public UnityEngine.Vector3[] pos = new UnityEngine.Vector3[0];

        [EKS] [NewInVersion(2)]
        public string[] values = new string[0];

        [EXT] public float duration;

        private OE_INTERACT() { }
        static private OE_INTERACT s_Instance = new OE_INTERACT();

        private void Reset(Type type, Type? extype, string id, string uri, float duration)
        {
            this.type = type;
            this.id = id;
            this.extype = extype.HasValue ? extype.Value.ToString() : string.Empty;
            this.uri = string.IsNullOrEmpty(uri) ? string.Empty : uri;
            this.duration = duration;
        }

        static public OE_INTERACT Create(Type type, string id)
        {
            s_Instance.Reset(type, null, id, null, 0.0f);
            return s_Instance;
        }

        static public OE_INTERACT CreateDuration(Type type, string id, float duration)
        {
            s_Instance.Reset(type, null, id, null, duration);
            return s_Instance;
        }

        static public OE_INTERACT Create(Type type, Type? extype, string id, string uri)
        {
            s_Instance.Reset(type, extype, id, uri, 0.0f);
            return s_Instance;
        }

        public override void WriteEDATA(SimpleJSON.JSONNode inNode)
        {
            base.WriteEDATA(inNode);

            if (duration <= 0)
                inNode["ext"].Remove("duration");
        }
    }
}
