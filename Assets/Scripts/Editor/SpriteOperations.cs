using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Sprites;
using System.Collections.Generic;
using System.IO;

static public class SpriteOperations
{
    [MenuItem("Textures/Set Max Size/1024")]
    static public void SetTextureSize1024()
    {
        SetTextureSize(1024);
    }

    [MenuItem("Textures/Mip Maps/Enable")]
    static public void SetTextureMipMappingOn()
    {
        SetTextureMipmap(true);
    }

    [MenuItem("Textures/Mip Maps/Disable")]
    static public void SetTextureMipMappingOff()
    {
        SetTextureMipmap(false);
    }

    static private string[] sTextureFileExts = new string[]
    {
        ".png", ".jpg", ".psd", ".bmp"
    };

    static private void SetTextureSize(int inTextureSize)
    {
        var texturePaths = AssetDatabase.GetAllAssetPaths().Where((x) => sTextureFileExts.Contains(Path.GetExtension(x)));
        foreach (string path in texturePaths)
            ReimportTextureWithSize(path, inTextureSize);
    }

    static private void SetTextureMipmap(bool inbEnable)
    {
        var texturePaths = AssetDatabase.GetAllAssetPaths().Where((x) => sTextureFileExts.Contains(Path.GetExtension(x)));
        foreach (string path in texturePaths)
            ReimportTextureWithMipmap(path, inbEnable);
    }

    static private void ReimportTextureWithSize(string inPath, int inTextureSize)
    {
        TextureImporter tImporter = AssetImporter.GetAtPath(inPath) as TextureImporter;
        if (tImporter == null)
            return;

        if (tImporter.maxTextureSize > inTextureSize)
        {
            tImporter.maxTextureSize = inTextureSize;
            Debug.LogFormat("Reimporting {0}...", inPath);
            AssetDatabase.ImportAsset(inPath, ImportAssetOptions.ForceUpdate);
        }
    }

    static private void ReimportTextureWithMipmap(string inPath, bool inbEnabled)
    {
        TextureImporter tImporter = AssetImporter.GetAtPath(inPath) as TextureImporter;
        if (tImporter == null)
            return;

        if (tImporter.mipmapEnabled != inbEnabled)
        {
            tImporter.mipmapEnabled = inbEnabled;
            Debug.LogFormat("Reimporting {0}...", inPath);
            AssetDatabase.ImportAsset(inPath, ImportAssetOptions.ForceUpdate);
        }
    }
}