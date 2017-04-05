using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using FGUnity.Utils;
using SimpleJSON;

namespace Minigames
{
    [Serializable]
    public class ExpressionMinigameSettings
    {
        /// <summary>
        /// Idenfifier for the question.
        /// </summary>
        public int ID = 0;

        /// <summary>
        /// Desired value of the expression.
        /// </summary>
        public int Value = 5;

        /// <summary>
        /// Whether the operator is fixed or
        /// able to be input by the player.
        /// </summary>
        public bool FixedOperator = true;

        /// <summary>
        /// The operator to use, if FixedOperator is true.
        /// </summary>
        public OperatorType Operator = OperatorType.Add;
        
        /// <summary>
        /// The number of value slots for chickens.
        /// </summary>
        public int NumValueSlots = 2;

        /// <summary>
        /// The value chickens available.
        /// </summary>
        public int[] Chickens = new int[] { 2, 3, 4 };

        /// <summary>
        /// How long to run the problem for.
        /// </summary>
        public float TimeLimit = 60;

        /// <summary>
        /// Problem classifications.
        /// </summary>
        [EnumFlag] public ExpressionMinigameCategories Categories;

        static public ExpressionMinigameSettings FromJSON(JSONNode inJson, ExpressionMinigameCategories inCategories)
        {
            ExpressionMinigameSettings settings = new ExpressionMinigameSettings();

            settings.Categories = inCategories;

            settings.ID = inJson["id"].AsInt;

            settings.Value = inJson["value"].AsInt;

            if (inJson["operator"] != null)
            {
                settings.Operator = (inJson["operator"].Value == "+" ? OperatorType.Add : OperatorType.Subtract);
                settings.FixedOperator = true;
            }
            else
            {
                settings.FixedOperator = false;
            }

            settings.NumValueSlots = inJson["valueSlots"].AsInt;
            settings.TimeLimit = inJson["timeLimit"].AsFloat;

            var chickens = inJson["chickens"].AsArray;
            settings.Chickens = new int[chickens.Count];
            for (int i = 0; i < chickens.Count; ++i)
                settings.Chickens[i] = chickens[i].AsInt;

            return settings;
        }
    }

    [System.Flags]
    public enum ExpressionMinigameCategories
    {
        Single = 0x1,
        DoubleLow = 0x2,
        Double = 0x4,

        Addition = 0x8,
        Subtraction = 0x10,

        Regroup = 0x20
    }
}
