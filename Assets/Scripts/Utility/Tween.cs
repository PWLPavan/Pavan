using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FGUnity.Utils;

using UnityEngine;
using UnityEngine.UI;

static public class Tween
{
    #region Values

    static public IEnumerator ValueTo(float inStart, float inEnd, float inTime, Action<float> inSetter, AnimationCurve inCurve = null)
    {
        float delta = inEnd - inStart;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        inSetter(inStart);
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);
            float nextValue = inStart + delta * curvedPercentage;
            inSetter(nextValue);
        }
    }

    static public IEnumerator OverTime(float inTime, Action<float> inSetter, AnimationCurve inCurve = null)
    {
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        inSetter(0);
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);
            inSetter(curvedPercentage);
        }
    }

    static public IEnumerator ColorTo(Color inStart, Color inEnd, float inTime, Action<Color> inSetter, AnimationCurve inCurve = null)
    {
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        inSetter(inStart);
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);
            Color nextValue = Color.LerpUnclamped(inStart, inEnd, curvedPercentage);
            inSetter(nextValue);
        }
    }

    #endregion

    #region Transforms

    static public IEnumerator MoveTo(this Transform inTransform, Vector3 inTarget, float inTime, AnimationCurve inCurve = null)
    {
        Vector3 currentPosition = inTransform.position;
        Vector3 deltaPosition = inTarget - currentPosition;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);
            Vector3 nextPosition = new Vector3(currentPosition.x + deltaPosition.x * curvedPercentage,
                currentPosition.y + deltaPosition.y * curvedPercentage,
                currentPosition.z + deltaPosition.z * curvedPercentage);
            inTransform.position = nextPosition;
        }
    }

    static public IEnumerator ScaleTo(this Transform inTransform, Vector3 inScale, float inTime, AnimationCurve inCurve = null)
    {
        Vector3 currentScale = inTransform.localScale;
        Vector3 deltaScale = inScale - currentScale;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);
            Vector3 nextScale = new Vector3(currentScale.x + deltaScale.x * curvedPercentage,
                currentScale.y + deltaScale.y * curvedPercentage,
                currentScale.z + deltaScale.z * curvedPercentage);
            inTransform.localScale = nextScale;
        }
    }

    #endregion

    #region Sprites

    static public IEnumerator ColorTo(this SpriteRenderer inRenderer, Color inColor, float inTime, bool inbPreserveAlpha = true, AnimationCurve inCurve = null)
    {
        Color currentColor = inRenderer.color;
        Color deltaColor = inColor - currentColor;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);
            Color nextColor = new Color(currentColor.r + deltaColor.r * curvedPercentage,
                currentColor.g + deltaColor.g * curvedPercentage,
                currentColor.b + deltaColor.b * curvedPercentage,
                inbPreserveAlpha ? currentColor.a : currentColor.a + deltaColor.a * curvedPercentage);
            inRenderer.color = nextColor;
        }
    }

    static public IEnumerator FadeTo(this SpriteRenderer inRenderer, float inAlpha, float inTime, AnimationCurve inCurve = null)
    {
        Color currentColor = inRenderer.color;
        float deltaAlpha = inAlpha - currentColor.a;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);

            Color nextColor = currentColor;
            nextColor.a += deltaAlpha * curvedPercentage;
            inRenderer.color = nextColor;
        }
    }

    #endregion

    #region Canvas

    static public IEnumerator ColorTo(this Graphic inRenderer, Color inColor, float inTime, bool inbPreserveAlpha = true, AnimationCurve inCurve = null)
    {
        Color currentColor = inRenderer.color;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);
            Color nextColor = Color.LerpUnclamped(currentColor, inColor, curvedPercentage);
            if (inbPreserveAlpha)
                nextColor.a = currentColor.a;
            inRenderer.color = nextColor;
        }
    }

    static public IEnumerator FadeTo(this Graphic inRenderer, float inAlpha, float inTime, AnimationCurve inCurve = null)
    {
        Color currentColor = inRenderer.color;
        float deltaAlpha = inAlpha - currentColor.a;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);

            Color nextColor = currentColor;
            nextColor.a += deltaAlpha * curvedPercentage;
            inRenderer.color = nextColor;
        }
    }

    static public IEnumerator ColorTo(this CanvasRenderer inRenderer, Color inColor, float inTime, bool inbPreserveAlpha = true, AnimationCurve inCurve = null)
    {
        Color currentColor = inRenderer.GetColor();
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);
            Color nextColor = Color.LerpUnclamped(currentColor, inColor, curvedPercentage);
            if (inbPreserveAlpha)
                nextColor.a = currentColor.a;
            inRenderer.SetColor(nextColor);
        }
    }

    static public IEnumerator FadeTo(this CanvasRenderer inRenderer, float inAlpha, float inTime, AnimationCurve inCurve = null)
    {
        float currentAlpha = inRenderer.GetAlpha();
        float deltaAlpha = inAlpha - currentAlpha;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);

            float nextAlpha = currentAlpha + deltaAlpha * curvedPercentage;
            inRenderer.SetAlpha(nextAlpha);
        }
    }

    static public IEnumerator FadeTo(this CanvasGroup inGroup, float inAlpha, float inTime, AnimationCurve inCurve = null)
    {
        float currentAlpha = inGroup.alpha;
        float deltaAlpha = inAlpha - currentAlpha;
        float currentPercentage = 0.0f;
        float percentageIncrement = 1.0f / inTime;
        while (currentPercentage < 1)
        {
            yield return null;
            currentPercentage += Time.deltaTime * percentageIncrement;
            if (currentPercentage > 1)
                currentPercentage = 1;
            float curvedPercentage = currentPercentage;
            if (inCurve != null)
                curvedPercentage = inCurve.Evaluate(curvedPercentage);

            float nextAlpha = currentAlpha + deltaAlpha * curvedPercentage;
            inGroup.alpha = nextAlpha;
        }
    }

    #endregion
}
