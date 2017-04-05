using System.Text;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using SimpleJSON;
using FGUnity.Utils;
using Minigames;

static public class Microconcepts
{
    public enum Code
    {
        M52, // COUNTING - Map quantities to numbers - use real objects, clapts, etc. to count
        M53, // COUNTING - Decode - Decode 0-9, 10-99

        M64, // SEQUENCE - Order numbers - whole part relationship, stable order, order irrelevance up to 100

        M80, // ADD/SUB - Understand symbols

        M85, // ADD/SUB - Add/sub w/out grouping - through objects
        M86, // ADD/SUB - Add/sub w/out grouping - use add/sub symbols
        M87, // ADD/SUB - Add/sub w/out grouping - through representations of 1s and 10s
        M88, // ADD/SUB - Add/sub w/out grouping - Write numbers vertically (w/out grouping)

        M92, // ADD/SUB - Add/sub w/ grouping - Two digit numbers, addition up to 99
    }

    static public string[] EvaluateConcepts(Level inLevel)
    {
        using (PooledList<Code> codes = PooledList<Code>.Create())
        {
            codes.Capacity = 4;

            if (inLevel.isSingleDigitProblem && inLevel.isTargetNumber)
            {
                codes.Add(Code.M52);
                codes.Add(Code.M53);
            }

            if (inLevel.isDoubleDigitProblem && inLevel.isTargetNumber && !inLevel.requiresRegrouping)
                codes.Add(Code.M64);

            if (inLevel.isSingleDigitProblem && !inLevel.isTargetNumber)
            {
                codes.Add(Code.M80);
                codes.Add(Code.M85);
            }
            if (inLevel.isDoubleDigitProblem)
                codes.Add(Code.M86);
            if (inLevel.isDoubleDigitProblem && inLevel.isTargetNumber && inLevel.requiresRegrouping)
                codes.Add(Code.M87);
            if (!inLevel.requiresRegrouping && !inLevel.isTargetNumber)
                codes.Add(Code.M88);
            if (inLevel.requiresRegrouping)
                codes.Add(Code.M92);

            return BuildCodeStrings(codes);
        }
    }

    static public string[] EvaluateConcepts(Minigames.NestTest inNest)
    {
        return s_Make10Microconcepts;
    }

    static public string[] EvaluateConcepts(Minigames.ExpressionMinigame inMinigame)
    {
        using (PooledList<Code> codes = PooledList<Code>.Create())
        {
            ExpressionMinigameCategories category = inMinigame.Level.Categories;

            codes.Capacity = 4;

            bool bIsDouble = HasCategories(category, ExpressionMinigameCategories.Double) || HasCategories(category, ExpressionMinigameCategories.DoubleLow);

            if (HasCategories(category, ExpressionMinigameCategories.Single))
            {
                codes.Add(Code.M52);
                codes.Add(Code.M53);
                codes.Add(Code.M80);
                codes.Add(Code.M85);
            }

            if (bIsDouble && !HasCategories(category, ExpressionMinigameCategories.Regroup))
                codes.Add(Code.M64);

            if (bIsDouble)
                codes.Add(Code.M86);

            if (HasCategories(category, ExpressionMinigameCategories.Double | ExpressionMinigameCategories.Regroup)
                || HasCategories(category, ExpressionMinigameCategories.DoubleLow | ExpressionMinigameCategories.Regroup))
                codes.Add(Code.M87);

            if (HasCategories(category, ExpressionMinigameCategories.Regroup))
                codes.Add(Code.M92);

            return BuildCodeStrings(codes);
        }
    }

    static private bool HasCategories(Minigames.ExpressionMinigameCategories inCategory, Minigames.ExpressionMinigameCategories inCheck)
    {
        return (inCategory & inCheck) == inCheck;
    }

    static private string[] BuildCodeStrings(params Code[] inCodes)
    {
        string[] newArray = new string[inCodes.Length];
        for(int i = 0; i < newArray.Length; ++i)
            newArray[i] = inCodes[i].ToString();
        return newArray;
    }

    static private string[] BuildCodeStrings(IList<Code> inCodes)
    {
        string[] newArray = new string[inCodes.Count];
        for (int i = 0; i < newArray.Length; ++i)
            newArray[i] = inCodes[i].ToString();
        return newArray;
    }

    static private readonly string[] s_Make10Microconcepts = BuildCodeStrings(Code.M53, Code.M52);

    // A lot of different cases with similar codes
    // Might just be easier to build the list dynamically
    // depending on the problem.
    // Single Digit Target Number = Code.M53, Code.M52
    // Single Digit Addition = Code.M88, Code.M85, Code.M80
    // Single Digit Subtraction Code.M88, Code.M85, Code.M80
    // Double Digit TargetCarryover = Code.M87, Code.M92, Code.M86
    // Double Digit TargetNumber = Code.M64, Code.M86
    // Single Digit AdditionCarryover = Code.M92, Code.M86
    // Double Digit AdditionTens = Code.M88, Code.M86
    // Double Digit Addition = Code.M88, Code.M86
    // Small Subtraction Borrow = Code.M92, Code.M86
    // Double Digit Subtraction = Code.M88, Code.M86
    // Double Digit AdditionCarryover = Code.M92, Code.M86
    // Double Digit SubtractionBorrow = Code.M92, Code.M86
}
