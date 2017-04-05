using System;
using System.Collections;
using System.Reflection;
using SimpleJSON;

public class JSONUtils
{
    public static JSONNode Parse(object o)
    {
        if (o == null) return null;
        if (o is bool) return new JSONData((bool)o);
        if (o is int) return new JSONData((int)o);
        if (o is float) return new JSONData((float)o);
        if (o is double) return new JSONData((double)o);
        if (o is string) return new JSONData((string)o);
        if (o is Enum) return new JSONData(Enum.GetName(o.GetType(), o));
        if (o is IEnumerable) return Parse((IEnumerable)o);

        var json = new JSONClass();
        var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var f in fields) json.Add(f.Name, Parse(f.GetValue(o)));
        return json;
    }
    public static JSONNode Parse(IEnumerable a)
    {
        var array = new JSONArray();
        foreach (object o in a) array.Add(Parse(o));
        return array;
    }

    public static object Parse(Type type, JSONNode json)
    {
        if (type == null || json == null) return null;
        if (type == typeof(bool)) return json.AsBool;
        if (type == typeof(int)) return json.AsInt;
        if (type == typeof(float)) return json.AsFloat;
        if (type == typeof(double)) return json.AsDouble;
        if (type == typeof(string)) return json.Value;
        if (type == typeof(Enum)) return Enum.Parse(type, json.Value);
        if (type == typeof(IEnumerable)) return Parse(type, json.AsArray);

        var obj = type.GetConstructor(null).Invoke(null);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var f in fields) if (json[f.Name] != null) f.SetValue(obj, Parse(f.FieldType, json[f.Name]));
        return obj;
    }
    public static IEnumerable Parse(Type type, JSONArray json)
    {
        var listtype = type.GetGenericParameterConstraints()[0];
        foreach (var o in json.Childs) yield return Parse(listtype, o);
    }

    //public class Data
    //{
    //    public class JSON : Attribute { }

    //    public string Name
    //    {
    //        get { return GetType().Name; }
    //    }
    //    public JSONNode Data
    //    {
    //        get
    //        {
    //            var json = new JSONClass();

    //            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //            foreach (var f in fields)
    //            {
    //                if (!Attribute.IsDefined(f, typeof(JSON))) continue;
    //                var output = f.GetValue(this);
    //                if (output != null) json.Add(f.Name, JSONUtils.Parse(output));
    //            }

    //            return json;
    //        }
    //        set
    //        {
    //            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //            foreach (var f in fields)
    //            {
    //                if (!Attribute.IsDefined(f, typeof(JSON))) continue;
    //                var input = value[f.Name];
    //                if (input != null) f.SetValue(this, JSONUtils.Read(f.FieldType, input));
    //            }
    //        }
    //    }
    //}
}
