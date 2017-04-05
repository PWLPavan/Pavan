using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using FGUnity.Utils;
using SimpleJSON;

public class LanguageConfig : ScriptableObject
{
    public string Code;
    public SystemLanguage[] SystemCodes;

    [Header("Images")]
    public Sprite TitleImage;
    public Sprite FlagImage;

    [Header("Text")]
    public Font Font;
    public float LineSpacing = 1.0f;
	public int LanguageFontSize = 60;
	public int ResetFontSize = 60;
	public int GenieFontSize = 60;
    public TextAsset JSONFile;

    public Sprite OptionsText;
    public Sprite PausedText;
    public Sprite LoadingText;
    public Sprite LanguageText;
    public Sprite ResetText;

    public Sprite ReturnToGenieText;

    /// <summary>
    /// Returns the translated text for the given string.
    /// </summary>
    public string this[string inString]
    {
        get
        {
            ParseTranslations();
            Assert.True(m_Translations.ContainsKey(inString), "Contains key.", "Unable to find '{0}' in '{1}'.", inString, this.name);
            return m_Translations[inString];
        }
    }

    private void ParseTranslations()
    {
        if (m_Translations != null)
            return;

        Assert.True(JSONFile != null, "Translation file exists.", "No translations file provided for '{0}'.", this.name);

        m_Translations = new Dictionary<string, string>();
        JSONNode node = JSON.Parse(JSONFile.text);
        Parse(null, node);
    }

    private void Parse(string inName, JSONNode inNode)
    {
        if (inNode.Type == JSONObjectType.Class)
        {
            foreach (KeyValuePair<string, JSONNode> node in inNode.AsObject)
            {
                string name = string.IsNullOrEmpty(inName) ? node.Key : inName + "." + node.Key;
                Parse(name, node.Value);
            }
        }
        else if (inNode.Type == JSONObjectType.Array)
        {
            using (PooledStringBuilder builder = PooledStringBuilder.Create())
            {
                bool bWriteNewLine = false;
                foreach (JSONNode node in inNode.AsArray)
                {
                    if (bWriteNewLine)
                        builder.Builder.Append('\n');
                    Assert.True(node.Type != JSONObjectType.Array && node.Type != JSONObjectType.Class, "Nodes in array are values.", "Cannot embed more key-value pairs or arrays into string array: key is {0}.", inName);
                    builder.Builder.Append(node.Value);
                    bWriteNewLine = true;
                }

                m_Translations.Add(inName, builder.Builder.ToString());
            }
        }
        else
        {
            m_Translations.Add(inName, inNode.Value);
        }
    }

    private Dictionary<string, string> m_Translations;
}
