using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using SimpleJSON;
using System.Diagnostics;
using FGUnity.Utils;
using Minigames;

public class MinigameJSONValidator
{
    [MenuItem("Assets/Validate Minigame JSON")]
    static public void ValidateJSON()
    {
        TextAsset textAsset = Selection.activeObject as TextAsset;
        if (textAsset == null)
        {
            EditorUtility.DisplayDialog("Unable to parse.", "Please select a minigame JSON file to parse.", "OK");
            return;
        }

        JSONNode fileJSON;
        try { fileJSON = JSON.Parse(textAsset.text); }
        catch { fileJSON = null; }

        if (fileJSON == null)
        {
            EditorUtility.DisplayDialog("Unable to parse.", "Selected file does not validate as a JSON file.", "OK");
            return;
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        ValidateNode(fileJSON);
        stopwatch.Stop();
        UnityEngine.Debug.LogFormat("Validation took {0} seconds.", stopwatch.Elapsed.TotalSeconds);
    }

    static private bool ValidateNode(JSONNode inNode)
    {
        bool bValidates = true;

        if (inNode.AsArray != null)
        {
            foreach (var node in inNode.AsArray)
                if (!ValidateNode(node))
                    bValidates = false;
        }
        else if (inNode.AsObject != null)
        {
            if (inNode["chickens"] != null)
            {
                if (!ValidateLevel(inNode))
                    bValidates = false;
            }
            else
            {
                foreach(var node in inNode.AsObject)
                {
                    if (!ValidateNode(node.Value))
                        bValidates = false;
                }
            }
        }

        return bValidates;
    }

    static private bool ValidateLevel(JSONNode inLevelJSON)
    {
        ExpressionMinigameSettings level = ExpressionMinigameSettings.FromJSON(inLevelJSON, ExpressionMinigameCategories.Single);
        bool bCanSolve = false;

        // Horrible, brute force solving
        foreach (int[] combination in GeneratePossibleCombinations(level))
        {
            if (CanReachTarget(level, combination))
            {
                bCanSolve = true;
                break;
            }
        }

        if (!bCanSolve)
        {
            UnityEngine.Debug.LogWarningFormat("Unsolvable: {0}", inLevelJSON.ToFormattedString());
            EditorUtility.DisplayDialog("Unsolvable!", "Unable to solve level generated with:\n" + inLevelJSON.ToFormattedString(), "OK");
        }

        return bCanSolve;
    }


    static private bool CanReachTarget(ExpressionMinigameSettings inLevel, int[] inValues)
    {
        if (inLevel.FixedOperator)
        {
            if (inLevel.Operator == OperatorType.Add)
            {
                int current = 0;
                foreach (int chicken in inValues)
                {
                    current += chicken;
                }
                return current == inLevel.Value;
            }
            else
            {
                for (int i = 0; i < inValues.Length; ++i)
                {
                    int current = 0;
                    for (int chickenIndex = 0; chickenIndex < inValues.Length; ++chickenIndex)
                    {
                        if (chickenIndex == i)
                        {
                            current += inValues[chickenIndex];
                        }
                        else
                        {
                            current -= inValues[chickenIndex];
                        }
                    }
                    if (current == inLevel.Value)
                        return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < (int)Math.Max(inValues.Length, 2); ++i)
            {
                int current = 0;
                for (int chickenIndex = 0; chickenIndex < inValues.Length; ++chickenIndex)
                {
                    if (chickenIndex == i)
                    {
                        current -= inValues[chickenIndex];
                    }
                    else
                    {
                        current += inValues[chickenIndex];
                    }
                }
                if (current == inLevel.Value)
                    return true;
            }
        }

        return false;
    }

    static private IEnumerable<int[]> GeneratePossibleCombinations(ExpressionMinigameSettings inLevel)
    {
        // Bitwise subset mask algorithm from:
        // https://www.topcoder.com/community/data-science/data-science-tutorials/a-bit-of-fun-fun-with-bits/
        int[] combination = new int[inLevel.NumValueSlots];

        int mask = (1 << combination.Length) - 1;
        while((mask & (1 << inLevel.Chickens.Length)) == 0)
        {
            int chickenIndex = 0;
            for (int i = 0; i < inLevel.Chickens.Length; ++i )
            {
                int chickenBit = 1 << i;
                if ((chickenBit & mask) > 0)
                    combination[chickenIndex++] = inLevel.Chickens[i];
            }

            yield return combination;

            int lowBit = mask & ~(mask - 1);
            int lowZero = (mask + lowBit) & ~mask;
            mask |= lowZero;
            mask &= ~(lowZero - 1);
            mask |= (lowZero / lowBit / 2) - 1;
        }
    }
}
