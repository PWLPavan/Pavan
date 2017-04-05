using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using FGUnity.Utils;
using SimpleJSON;

namespace Minigames
{
    public class ExpressionMinigameGenerator : MonoBehaviour
    {
        public const int LOW_DOUBLE_THRESHOLD = 25;

        public TextAsset LevelJSON;

        private ExpressionMinigame m_Minigame;

        private Dictionary<ExpressionMinigameCategories, List<ExpressionMinigameSettings>> m_Levels;

        public void GenerateLevel()
        {
            if (m_Minigame == null)
            {
                ParseJSON();
                m_Minigame = GetComponent<ExpressionMinigame>();
            }

            m_Minigame.Level = SelectLevel();
            m_Minigame.Level.Chickens.Shuffle();
        }

        private ExpressionMinigameSettings SelectLevel()
        {
            if (Session.Exists)
                return FindContextualLevel();
            return FindRandomLevel();
        }

        private ExpressionMinigameSettings FindContextualLevel()
        {
            Level currentLevel = Session.instance.currentLevel;
            ExpressionMinigameCategories category;
            
            if (currentLevel.isSingleDigitProblem)
            {
                category = ExpressionMinigameCategories.Single;
            }
            else
            {
                if (currentLevel.value < LOW_DOUBLE_THRESHOLD)
                    category = ExpressionMinigameCategories.DoubleLow;
                else
                {
                    category = ExpressionMinigameCategories.Double;
                    if (currentLevel.requiresRegrouping)
                        category |= ExpressionMinigameCategories.Regroup;
                }
            }

            if (currentLevel.isSubtractionProblem)
                category |= ExpressionMinigameCategories.Subtraction;
            else
                category |= ExpressionMinigameCategories.Addition;

            using (PooledList<ExpressionMinigameSettings> listToUse = PooledList<ExpressionMinigameSettings>.Create())
            {
                listToUse.AddRange(m_Levels[category]);
                if ((category | ExpressionMinigameCategories.Regroup) > 0)
                    listToUse.AddRange(m_Levels[category & ~ExpressionMinigameCategories.Regroup]);

                return RNG.Instance.Choose(listToUse);
            }
        }

        private ExpressionMinigameSettings FindRandomLevel()
        {
            List<ExpressionMinigameSettings> list = RNG.Instance.Choose(m_Levels).Value;
            return RNG.Instance.Choose(list);
        }

        private void ParseJSON()
        {
            m_Levels = new Dictionary<ExpressionMinigameCategories,List<ExpressionMinigameSettings>>();

            JSONNode json = JSON.Parse(LevelJSON.text);
            ParseLevels(json);
        }

        private void ParseLevels(JSONNode inRoot)
        {
            // Single digit
            ParseLevelCollection(inRoot["Single"]["Add"].AsArray, ExpressionMinigameCategories.Single | ExpressionMinigameCategories.Addition);
            ParseLevelCollection(inRoot["Single"]["Sub"].AsArray, ExpressionMinigameCategories.Single | ExpressionMinigameCategories.Subtraction);

            // Double digit add
            ParseLevelCollection(inRoot["Double"]["Add"]["Teens"].AsArray, ExpressionMinigameCategories.DoubleLow | ExpressionMinigameCategories.Addition);
            ParseLevelCollection(inRoot["Double"]["Add"]["NoRegrouping"].AsArray, ExpressionMinigameCategories.Double | ExpressionMinigameCategories.Addition);
            ParseLevelCollection(inRoot["Double"]["Add"]["Regrouping"].AsArray, ExpressionMinigameCategories.Double | ExpressionMinigameCategories.Addition | ExpressionMinigameCategories.Regroup);

            // Double digit sub
            ParseLevelCollection(inRoot["Double"]["Sub"]["Teens"].AsArray, ExpressionMinigameCategories.DoubleLow | ExpressionMinigameCategories.Subtraction);
            ParseLevelCollection(inRoot["Double"]["Sub"]["NoRegrouping"].AsArray, ExpressionMinigameCategories.Double | ExpressionMinigameCategories.Subtraction);
            ParseLevelCollection(inRoot["Double"]["Sub"]["Regrouping"].AsArray, ExpressionMinigameCategories.Double | ExpressionMinigameCategories.Subtraction | ExpressionMinigameCategories.Regroup);
        }

        private void ParseLevelCollection(JSONArray inNode, ExpressionMinigameCategories inCategories)
        {
            List<ExpressionMinigameSettings> levels = new List<ExpressionMinigameSettings>();
            m_Levels[inCategories] = levels;
            foreach (JSONNode node in inNode)
            {
                ExpressionMinigameSettings level = ExpressionMinigameSettings.FromJSON(node, inCategories);
                levels.Add(level);
            }
        }
    }
}
