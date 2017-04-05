using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class BundleInfoGenerator
{
    /// <summary>
    /// Class name to use when referencing from code.
    /// </summary>
    const string ClassName = "BundleInfo";
    const string TargetCodeFile = "Assets/Scripts/Config/" + ClassName + ".cs";

    static BundleInfoGenerator()
    {
        if (BundleInfo.Identifier != PlayerSettings.bundleIdentifier ||
            BundleInfo.Version != PlayerSettings.bundleVersion)
        {
            Debug.Log("Updating " + TargetCodeFile);
            CreateNewBuildVersionClassFile(PlayerSettings.bundleIdentifier, PlayerSettings.bundleVersion);
        }
    }

    static string CreateNewBuildVersionClassFile(string identifier, string version)
    {
        using (StreamWriter writer = new StreamWriter(TargetCodeFile, false))
        {
            try
            {
                string code = GenerateCode(identifier, version);
                writer.WriteLine("{0}", code);
            }
            catch (System.Exception ex)
            {
                string msg = " threw:\n" + ex.ToString();
                Debug.LogError(msg);
                EditorUtility.DisplayDialog("Error when trying to regenerate class", msg, "OK");
            }
        }
        return TargetCodeFile;
    }

    /// <summary>
    /// Regenerates (and replaces) the code for ClassName with new bundle version id.
    /// </summary>
    /// <returns>
    /// Code to write to file.
    /// </returns>
    /// <param name='bundleVersion'>
    /// New bundle version.
    /// </param>
    static string GenerateCode(string identifier, string version)
    {
        string code = "public static class " + ClassName + "\n{\n";
        code += System.String.Format("\tpublic static readonly string Identifier = \"{0}\";\n", identifier);
        code += System.String.Format("\tpublic static readonly string Version = \"{0}\";\n", version);
        code += "}";
        return code;
    }
}