using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;

public class Level
{
    #region Controls

    public bool tensQueueEnabled { get; private set; }
    public bool onesQueueEnabled { get; private set; }

    public bool tensColumnEnabled
    {
        get { return isDoubleDigitProblem; }
    }

    public bool seatbelts
    {
        get { return HasFlag(Flags.Seatbelts); }
    }

    public bool usesQueue
    {
        get { return !usesSubZone; }
    }

    public bool usesSubZone
    {
        get { return isSubtractionProblem; }
    }

    public bool useNumberPad = false;

    #endregion

    #region Level flow

    #region Starting Value

    public int startingValue { get; private set; }
    public int startingTens { get; private set; }
    public int startingOnes { get; private set; }

    #endregion

    #region Ending Value

    public int value { get; private set; }
    public int valueTens { get; private set; }
    public int valueOnes { get; private set; }

    #endregion

    public int deltaValue { get; private set; }
    public int deltaTens { get; private set; }
    public int deltaOnes { get; private set; }

    public bool twoPartProblem
    {
        get { return HasFlag(Flags.TwoPart); }
    }

    public MinigameTiming showNestMinigame { get; private set; }
    public MinigameTiming showExpressionMinigame { get; private set; }

    public float countSpeed { get; private set; }

    #endregion

    #region Level classifications

    public string qid { get; private set; }
    public string expression { get; private set; }
    public bool fromEnlearn { get { return HasFlag(Flags.Enlearn); } }

    private Flags m_Flags = 0;

    public bool isTargetNumber
    {
        get { return HasFlag(Flags.TargetNumber); }
    }

    public bool isExpression
    {
        get { return !HasFlag(Flags.TargetNumber); }
    }

    public bool isAdditionProblem
    {
        get { return !HasFlag(Flags.Subtraction); }
    }

    public bool isSubtractionProblem
    {
        get { return HasFlag(Flags.Subtraction); }
    }

    public bool isSingleDigitProblem
    {
        get { return !HasFlag(Flags.DoubleDigit); }
    }

    public bool isDoubleDigitProblem
    {
        get { return HasFlag(Flags.DoubleDigit); }
    }

    public bool requiresRegrouping
    {
        get { return HasFlag(Flags.Regrouping); }
    }

    public bool requiresMultipleCarryover
    {
        get { return deltaTens > 0 && !tensQueueEnabled && usesQueue; }
    }

    public bool isValid
    {
        get { return !HasFlag(Flags.Invalid); }
    }

    public bool useBrownQueue
    {
        get { return HasFlag(Flags.BrownQueue); }
    }

    public bool fillWithBrown
    {
        get { return HasFlag(Flags.BrownStart); }
    }

    public bool showGoldCarryover
    {
        get { return HasFlag(Flags.ShowGoldDigit); }
    }

    #endregion

    #region Tutorials

    private List<SaveData.FlagType> m_Concepts = new List<SaveData.FlagType>();

    public bool startHandhold { get { return HasFlag(Flags.Handhold); } }
    public SaveData.FlagType tutorialPrelude { get; private set; }

    public bool hasTutorial(string key)
    {
        return false;
    }

    public void SubstituteMechanicsTutorials()
    {
        if (NeedsTutorial(SaveData.FlagType.Tutorial_Subtract))
        {
            if (isSubtractionProblem)
            {
                TriggerTutorial(SaveData.FlagType.Tutorial_Subtract);
            }
        }

        if (NeedsTutorial(SaveData.FlagType.Tutorial_Carryover))
        {
            if (isAdditionProblem && (requiresRegrouping || (deltaTens != 0 && !tensQueueEnabled)))
            {
                TriggerTutorial(SaveData.FlagType.Tutorial_Carryover);
            }
        }

        if (NeedsTutorial(SaveData.FlagType.Tutorial_Borrowing))
        {
            if (isSubtractionProblem && startingOnes < valueOnes)
            {
                TriggerTutorial(SaveData.FlagType.Tutorial_Borrowing);
            }
        }

        if (NeedsTutorial(SaveData.FlagType.Tutorial_TensPlane))
        {
            if (tensColumnEnabled)
            {
                TriggerTutorial(SaveData.FlagType.Tutorial_TensPlane);
            }
        }
    }

    public void CompletedMechanicsTutorials()
    {
        SaveData saveData = SaveData.instance;
        foreach (var concept in m_Concepts)
            saveData.SetFlag(concept, true);

        saveData.CurrentTutorial = 0;
    }

    public void AddMechanicFlag(SaveData.FlagType inFlag)
    {
        m_Concepts.Add(inFlag);
    }

    private bool NeedsTutorial(SaveData.FlagType inFlag)
    {
        return !SaveData.instance.GetFlag(inFlag);
    }

    private void TriggerTutorial(SaveData.FlagType inFlag)
    {
        if (Session.instance.HasTutorialLevel(inFlag))
            tutorialPrelude = inFlag;
        else
            SetFlag(Flags.Handhold);

        m_Concepts.Add(inFlag);
    }

    #endregion

    string[] _splitExpression;
    
    public string expTop
    {
		get { return this._splitExpression[0]; }
	}
	
    public string expOp
    {
		get { return this._splitExpression.Length < 2 ? string.Empty : this._splitExpression[1]; }
	}

	public string expBottom
    {
        get { return this._splitExpression.Length < 3 ? string.Empty : this._splitExpression[2]; }
	}

    public void ParseJSON(JSONNode inJSON, bool inbFromEnlearn)
    {
        Assert.True(inJSON != null, "JSON is not null.", "Level was provided a non-existent JSON element.");

        Reset();

        if (inbFromEnlearn)
            SetFlag(Flags.Enlearn);

        if (inJSON["expression"] != null)
            this.expression = inJSON["expression"].Value;
        if (inJSON["tensCount"] != null)
            this.startingTens = inJSON["tensCount"].AsInt;
        if (inJSON["onesCount"] != null)
            this.startingOnes = inJSON["onesCount"].AsInt;

        if (inJSON["tensQueueEnabled"] != null)
            this.tensQueueEnabled = inJSON["tensQueueEnabled"].AsBool;
        if (inJSON["onesQueueEnabled"] != null)
            this.onesQueueEnabled = inJSON["onesQueueEnabled"].AsBool;

        /*
         * We're ignoring these now, since all level
         * tutorials have been replaced with handholding.
        if (inJSON["tutorials"] != null)
        {
            JSONArray arr = inJSON["tutorials"].AsArray;
            for (int i = 0; i < arr.Count; ++i)
            {
                this.m_Tutorials.Add(arr[i]);
            }
        }*/

        if (inJSON["useNumberPad"] != null)
            this.useNumberPad = inJSON["useNumberPad"].AsBool;

        if (inJSON["twoPartProblem"] != null)
            ToggleFlag(Flags.TwoPart, inJSON["twoPartProblem"].AsBool);
        if (inJSON["seatbelts"] != null)
            ToggleFlag(Flags.Seatbelts, inJSON["seatbelts"].AsBool);

        var showNestMinigameJson = inJSON["showNestMinigame"];
        if (showNestMinigameJson != null)
        {
            string nestMinigame = showNestMinigameJson.Value;
            if (nestMinigame == "block")
                showNestMinigame = MinigameTiming.Block;
            else if (nestMinigame == "before")
                showNestMinigame = MinigameTiming.Before;
            else if (nestMinigame == "after")
                showNestMinigame = MinigameTiming.After;
        }

        var showExpressionMinigameJson = inJSON["showExpressionMinigame"];
        if (showExpressionMinigameJson != null)
        {
            string expressionMinigame = showExpressionMinigameJson.Value;
            if (expressionMinigame == "block")
                showExpressionMinigame = MinigameTiming.Block;
            else if (expressionMinigame == "before")
                showExpressionMinigame = MinigameTiming.Before;
            else if (expressionMinigame == "after")
                showExpressionMinigame = MinigameTiming.After;
        }

        var countSpeedJson = inJSON["countSpeed"];
        if (countSpeedJson != null)
            countSpeed = countSpeedJson.AsFloat;

        var handholdJSON = inJSON["startHandhold"];
        if (handholdJSON != null)
            ToggleFlag(Flags.Handhold, handholdJSON.AsBool);

        CalculateDerivedValues();
        SanityTest();
    }

    private void CalculateDerivedValues()
    {
        try
        {
            this._splitExpression = expression.Split(' ');
            bool bIsTargetNumber = _splitExpression.Length == 1;
            ToggleFlag(Flags.TargetNumber, bIsTargetNumber);

            bool bUsesSubtractionSymbol = false;

            if (bIsTargetNumber)
            {
                value = int.Parse(expTop);
            }
            else
            {
                if (this.expOp == "+")
                {
                    value = int.Parse(expTop) + int.Parse(expBottom);
                }
                else
                {
                    value = int.Parse(expTop) - int.Parse(expBottom);
                    bUsesSubtractionSymbol = true;
                }
            }

            valueTens = (int)(value / 10);
            valueOnes = value % 10;

            startingValue = startingOnes + startingTens * 10;
            deltaValue = value - startingValue;
            deltaTens = (int)(deltaValue / 10);
            deltaOnes = deltaValue % 10;

            ToggleFlag(Flags.Subtraction, deltaValue < 0 || (deltaValue == 0 && bUsesSubtractionSymbol));
            ToggleFlag(Flags.DoubleDigit, startingValue >= 10 || value >= 10);

            if (startingOnes > 9 && deltaValue >= 0)
            {
                SetFlag(Flags.Regrouping);
            }
            else
            {
                // Since the starting number of chickens/nest is
                // not locked to the expression, we instead rely on
                // starting chickens versus how many chickens the user
                // has to add/remove to get to the expected result.
                // This works for both target number and expression
                // problems.
                int tensA = startingValue / 10;
                int tensB = deltaValue / 10;

                // If the sum of the tens columns of starting and added
                // value is not equal to the tens column of the final result,
                // then we know carryover/borrowing has to occur.
                // If we need to add/subtract ten or more and we don't give
                // them the tens queue, then they'll also need to carryover.
                int combinedAB = tensA + tensB;
                int actualAB = valueTens;
                if (combinedAB != actualAB || (tensB != 0 && !tensQueueEnabled && usesQueue))
                {
                    SetFlag(Flags.Regrouping);
                }
                else
                {
                    ResetFlag(Flags.Regrouping);
                }
            }

            bool bUseBrownQueue = (isTargetNumber && twoPartProblem) || isSubtractionProblem;
            ToggleFlag(Flags.BrownQueue, bUseBrownQueue);

            bool bFillWithBrown = !(isTargetNumber && !twoPartProblem && !isSubtractionProblem);
            ToggleFlag(Flags.BrownStart, bFillWithBrown);

            ToggleFlag(Flags.ShowGoldDigit, bFillWithBrown && !bUseBrownQueue && startingOnes % 10 > 0 && startingOnes + deltaOnes > 9);

            using (PooledStringBuilder builder = PooledStringBuilder.Create())
            {
                if (this.isDoubleDigitProblem)
                    builder.Builder.Append("double-");
                else
                    builder.Builder.Append("single-");

                if (this.isTargetNumber)
                {
                    builder.Builder.Append("target");
                }
                else
                {
                    if (this.isAdditionProblem)
                        builder.Builder.Append("add");
                    else
                        builder.Builder.Append("sub");
                }

                if (this.requiresRegrouping)
                {
                    builder.Builder.Append(this.isAdditionProblem ? "-grouping" : "-borrow");
                }

                this.qid = builder.Builder.ToString();
            }
        }
        catch(Exception e)
        {
            Debug.LogFormat("Unable to parse problem: {0}", e.Message);
            SetFlag(Flags.Invalid);
        }
    }

    void SanityTest()
    {
        if (valueOnes > 0 && valueOnes != startingOnes && isAdditionProblem && !isDoubleDigitProblem)
        {
            if (!onesQueueEnabled)
            {
                Logger.Warn("Invalid level: Final answer is {0} and requires ones dragging, but ones queue is not available.", value);
                SetFlag(Flags.Invalid);
            }
        }

        if (isAdditionProblem && value != startingValue)
        {
            if (!onesQueueEnabled && !tensQueueEnabled)
            {
                Logger.Warn("Invalid level: Problem requires addition, but neither the tens nor the ones queue are available.");
                SetFlag(Flags.Invalid);
            }
            if (deltaOnes != 0 && !onesQueueEnabled)
            {
                Logger.Warn("Invalid level: Problem requires ones addition, but ones queue is not available.");
                SetFlag(Flags.Invalid);
            }
        }

        if (isSingleDigitProblem)
        {
            if (startingTens != 0 || startingOnes > 9)
            {
                Logger.Warn("Single digit problem, but columns have invalid values: {0} tens, {1} ones.", startingTens, startingOnes);
                SetFlag(Flags.Invalid);
            }
        }
        else if (isDoubleDigitProblem)
        {
            if (startingTens > 9 || startingOnes > 19)
            {
                Logger.Warn("Double digit problem, but columns have invalid values: {0} tens, {1} ones.", startingTens, startingOnes);
                SetFlag(Flags.Invalid);
            }
        }
    }

    public void FlagAsInvalid()
    {
        SetFlag(Flags.Invalid);
    }

    public Level SplitTwoPart()
    {
        Level nextLevel = new Level();
        nextLevel.CopyFrom(this);

        ResetFlag(Flags.Seatbelts);
        useNumberPad = false;
        expression = expTop;

        CalculateDerivedValues();
        SanityTest();

        nextLevel.ResetFlag(Flags.TwoPart);
        nextLevel.startingOnes = valueOnes;
        nextLevel.startingTens = valueTens;

        nextLevel.CalculateDerivedValues();
        nextLevel.SanityTest();

        AlignWith(nextLevel);

        Assert.True(nextLevel.isExpression, "Second half of two-part problem is expression.", "Invalid level: two-part problems must be served as expressions, not as target number problems. Generated level can be found in out_log.txt in the data folder.");

        return nextLevel;
    }

    private void CopyFrom(Level inLevel)
    {
        m_Flags = inLevel.m_Flags;
        qid = inLevel.qid;

        expression = inLevel.expression;
        startingTens = inLevel.startingTens;
        startingOnes = inLevel.startingOnes;

        tensQueueEnabled = inLevel.tensQueueEnabled;
        onesQueueEnabled = inLevel.onesQueueEnabled;

        tutorialPrelude = 0;

        value = inLevel.value;

        useNumberPad = inLevel.useNumberPad;

        showNestMinigame = inLevel.showNestMinigame;
        showExpressionMinigame = inLevel.showExpressionMinigame;

        countSpeed = inLevel.countSpeed;
    }

    private void AlignWith(Level inLevel)
    {
        AlignFlag(Flags.DoubleDigit, inLevel);
        this.qid = inLevel.qid;
    }

    private void AlignFlag(Flags inFlag, Level inLevel)
    {
        ToggleFlag(inFlag, inLevel.HasAllFlags(inFlag));
    }

	private void Reset ()
    {
        m_Flags = 0;

		expression = "";
		startingTens = 0;
		startingOnes = 0;

        tensQueueEnabled = true;
        onesQueueEnabled = true;

        value = 0;

        useNumberPad = false;

        showNestMinigame = MinigameTiming.None;
        showExpressionMinigame = MinigameTiming.None;

        countSpeed = 1.0f;
    }

    [System.Flags]
    private enum Flags
    {
        TargetNumber        = 0x1,
        Subtraction         = 0x2,
        Seatbelts           = 0x4,
        TwoPart             = 0x8,
        DoubleDigit         = 0x10,
        Regrouping          = 0x20,
        Enlearn             = 0x40,
        Handhold            = 0x80,
        BrownQueue          = 0x100,
        BrownStart          = 0x200,
        ShowGoldDigit       = 0x400,

        Invalid             = 0x800
    }

    private bool HasFlag(Flags inFlag)
    {
        return (m_Flags & inFlag) > 0;
    }

    private bool HasAllFlags(Flags inFlag)
    {
        return (m_Flags & inFlag) == inFlag;
    }

    private void SetFlag(Flags inFlag)
    {
        m_Flags |= inFlag;
    }

    private void ResetFlag(Flags inFlag)
    {
        m_Flags &= ~inFlag;
    }

    private void ToggleFlag(Flags inFlag, bool inbState)
    {
        if (inbState)
            SetFlag(inFlag);
        else
            ResetFlag(inFlag);
    }

    public enum MinigameTiming
    {
        /// <summary>
        /// Uses the game's timing for minigames.
        /// </summary>
        None,

        /// <summary>
        /// Shows the minigame before.
        /// </summary>
        Before,

        /// <summary>
        /// Shows the minigame afterwards.
        /// </summary>
        After,

        /// <summary>
        /// Blocks the minigame from being shown.
        /// </summary>
        Block
    }
}
