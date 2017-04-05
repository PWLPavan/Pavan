using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FGUnity.Utils;

namespace FGUnity.Utils
{
    static public class RNG
    {
        static private Random s_CurrentRandom = new Random();

        static public Random Instance
        {
            get { return s_CurrentRandom; }
            set
            {
                Assert.True(value != null, "Randomizer is not null.");
                s_CurrentRandom = value;
            }
        }

        static public float NextFloat(this Random inRandom)
        {
            return (float)inRandom.NextDouble();
        }

        static public float NextFloat(this Random inRandom, float inMax)
        {
            return NextFloat(inRandom) * inMax;
        }

        static public float NextFloat(this Random inRandom, float inMin, float inMax)
        {
            return inMin + NextFloat(inRandom) * (inMax - inMin);
        }

        static public bool NextBool(this Random inRandom)
        {
            return NextFloat(inRandom) < 0.5f;
        }

        static public bool Chance(this Random inRandom, float inPercent)
        {
            return NextFloat(inRandom) < inPercent;
        }

        #region Collections

        static public T Choose<T>(this Random inRandom, T inFirstChoice, T inSecondChoice, params T[] inMoreChoices)
        {
            int index = inRandom.Next(inMoreChoices.Length + 2);
            if (index == 0)
                return inFirstChoice;
            else if (index == 1)
                return inSecondChoice;
            else
                return inMoreChoices[index - 2];
        }

        static public T Choose<T>(this Random inRandom, IList<T> inChoices)
        {
            Assert.True(inChoices.Count > 0);
            return inChoices[inRandom.Next(inChoices.Count)];
        }

        static public T Choose<T>(this Random inRandom, ICollection<T> inChoices)
        {
            Assert.True(inChoices.Count > 0);
            return inChoices.ElementAt(inRandom.Next(inChoices.Count));
        }

        static public void Shuffle<T>(this IList<T> inList)
        {
            Shuffle(inList, Instance);
        }

        static public void Shuffle<T>(this IList<T> inList, Random inRandom)
        {
            int i = inList.Count;
            int j;
            while(--i > 0)
            {
                T old = inList[i];
                inList[i] = inList[j = inRandom.Next(i + 1)];
                inList[j] = old;
            }
        }

        #endregion
    }
}
