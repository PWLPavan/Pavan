using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUnity.Utils
{
    /// <summary>
    /// Contains a set of timing coroutines,
    /// along with SmartCoroutine.
    /// 
    /// SmartCoroutine allows for a simpler coroutine syntax.
    /// Yielding a float or integer will wait an equivalent number of seconds.
    /// Yielding an IEnumerator will automatically start that coroutine.
    /// </summary>
    static public class CoroutineUtil
    {
        #region Exposed helper functions

        /// <summary>
        /// Waits the given number of frames.
        /// </summary>
        static public IEnumerator WaitForFrames(int inFrames)
        {
            while (inFrames-- > 0)
                yield return null;
        }

        /// <summary>
        /// Waits for the given number of seconds.
        /// </summary>
        /// <param name="inSeconds"></param>
        /// <returns></returns>
        static public IEnumerator WaitForSeconds(float inSeconds)
        {
            float endTime = Time.time + inSeconds;
            while (Time.time < endTime)
                yield return null;
        }

        /// <summary>
        /// Waits for the given number of real seconds,
        /// ignoring Time.timeScale
        /// </summary>
        static public IEnumerator WaitForRealSeconds(float inSeconds)
        {
            float endTime = Time.unscaledTime + inSeconds;
            while (Time.unscaledTime < endTime)
                yield return null;
        }

        /// <summary>
        /// Waits for the given condition to be true.
        /// </summary>
        static public IEnumerator WaitForCondition(Func<bool> inCondition, float inInterval = 0.0f)
        {
            while (!inCondition())
                yield return inInterval;
        }

        #endregion

        #region Internal Routines

        #region Frames

        static private IEnumerator WaitFramesThen_Routine(int inFrames, Action inFunction)
        {
            while (inFrames-- > 0)
                yield return null;
            inFunction();
        }

        static private IEnumerator WaitFramesThen_Routine<T>(int inFrames, Action<T> inFunction, T inParam)
        {
            while (inFrames-- > 0)
                yield return null;
            inFunction(inParam);
        }

        // Only works with SmartRoutine
        static private IEnumerator WaitFramesThenStart_Routine(int inFrames, IEnumerator inFunction)
        {
            while (inFrames-- > 0)
                yield return null;
            yield return inFunction;
        }

        #endregion

        #region Seconds

        static private IEnumerator WaitSecondsThen_Routine(float inSeconds, Action inFunction)
        {
            yield return inSeconds;
            inFunction();
        }

        static private IEnumerator WaitSecondsThen_Routine<T>(float inSeconds, Action<T> inFunction, T inParam)
        {
            yield return inSeconds;
            inFunction(inParam);
        }

        // Only works with SmartRoutine
        static private IEnumerator WaitSecondsThenStart_Routine(float inSeconds, IEnumerator inFunction)
        {
            yield return inSeconds;
            yield return inFunction;
        }

        #endregion

        #region Condition

        static private IEnumerator WaitConditionThen_Routine(Func<bool> inCondition, Action inFunction, float inInterval)
        {
            while (!inCondition())
                yield return inInterval;
            inFunction();
        }

        static private IEnumerator WaitConditionThen_Routine<T>(Func<bool> inCondition, Action<T> inFunction, T inParam, float inInterval)
        {
            while (!inCondition())
                yield return inInterval;
            inFunction(inParam);
        }

        // Only works with SmartRoutine
        static private IEnumerator WaitConditionThenStart_Routine(Func<bool> inCondition, IEnumerator inFunction, float inInterval)
        {
            while (!inCondition())
                yield return inInterval;
            yield return inFunction;
        }

        #endregion

        #endregion

        #region Smart Routines

        /// <summary>
        /// Starts a smart coroutine.
        /// Smart coroutine simplifies the syntax.
        /// If you yield a number, it will wait that number of seconds to resume.
        /// If you yield an IEnumerator from a coroutine function, it will start that coroutine.
        /// This avoids needing to call StartCoroutine for every subroutine.
        /// <code>yield return StartCoroutine(nameOfMyFunction())</code>
        /// becomes <code>yield return nameOfMyFunction()</code>
        /// </summary>
        static public CoroutineHandle SmartCoroutine(this MonoBehaviour inBehavior, IEnumerator inStart)
        {
            return CoroutineInstance.Run(inBehavior, inStart);
        }

        /// <summary>
        /// Stops the executing Coroutine.
        /// </summary>
        static public void StopCoroutine(this MonoBehaviour inBehavior, CoroutineHandle inHandle)
        {
            inHandle.Stop();
        }

        /// <summary>
        /// Stops the executing Coroutine.
        /// </summary>
        static public void StopCoroutine(this MonoBehaviour inBehavior, ref CoroutineHandle inHandle)
        {
            if (inHandle != CoroutineHandle.Null)
            {
                inHandle.Stop();
                inHandle = CoroutineHandle.Null;
            }
        }

        /// <summary>
        /// Replaces the given routine with another routine.
        /// </summary>
        static public void ReplaceCoroutine(this MonoBehaviour inBehavior, ref CoroutineHandle inCurrent, IEnumerator inStart)
        {
            inCurrent.Stop();
            inCurrent = SmartCoroutine(inBehavior, inStart);
        }

        #endregion

        #region Wait a single frame

        /// <summary>
        /// Waits one frame then starts a coroutine.
        /// </summary>
        /// <param name="inFunction">Coroutine to start.</param>
        static public CoroutineHandle WaitOneFrameThen(this MonoBehaviour inBehavior, IEnumerator inFunction)
        {
            return inBehavior.SmartCoroutine(WaitFramesThenStart_Routine(1, inFunction));
        }

        /// <summary>
        /// Waits one frame then calls a function.
        /// </summary>
        /// <param name="inFunction">Function to call.</param>
        static public CoroutineHandle WaitOneFrameThen(this MonoBehaviour inBehavior, Action inFunction)
        {
            return inBehavior.SmartCoroutine(WaitFramesThen_Routine(1, inFunction));
        }

        /// <summary>
        /// Waits one frame then calls a function.
        /// </summary>
        /// <param name="inFunction">Function to call.</param>
        /// <param name="inParam">Parameter for the function.</param>
        static public CoroutineHandle WaitOneFrameThen<T>(this MonoBehaviour inBehavior, Action<T> inFunction, T inParam)
        {
            return inBehavior.SmartCoroutine(WaitFramesThen_Routine(1, inFunction, inParam));
        }

        #endregion

        #region Wait multiple frames

        /// <summary>
        /// Waits a given number of frames then starts a coroutine.
        /// </summary>
        /// <param name="inFrames">Number of frames to wait.</param>
        /// <param name="inFunction">Coroutine to start.</param>
        static public CoroutineHandle WaitFramesThen(this MonoBehaviour inBehavior, int inFrames, IEnumerator inFunction)
        {
            return inBehavior.SmartCoroutine(WaitFramesThenStart_Routine(inFrames, inFunction));
        }

        /// <summary>
        /// Waits a given number of frames then calls a function.
        /// </summary>
        /// <param name="inFrames">Number of frames to wait.</param>
        /// <param name="inFunction">Function to call.</param>
        static public CoroutineHandle WaitFramesThen(this MonoBehaviour inBehavior, int inFrames, Action inFunction)
        {
            return inBehavior.SmartCoroutine(WaitFramesThen_Routine(inFrames, inFunction));
        }

        /// <summary>
        /// Waits a given number of frames then starts a coroutine.
        /// </summary>
        /// <param name="inFrames">Number of frames to wait.</param>
        /// <param name="inFunction">Coroutine to start.</param>
        static public CoroutineHandle WaitFramesThen<T>(this MonoBehaviour inBehavior, int inFrames, Action<T> inFunction, T inParam)
        {
            return inBehavior.SmartCoroutine(WaitFramesThen_Routine(inFrames, inFunction, inParam));
        }

        #endregion

        #region Wait seconds

        /// <summary>
        /// Waits a given number of seconds then starts a coroutine.
        /// </summary>
        /// <param name="inSeconds">Number of seconds to wait.</param>
        /// <param name="inFunction">Coroutine to start.</param>
        static public CoroutineHandle WaitSecondsThen(this MonoBehaviour inBehavior, float inSeconds, IEnumerator inFunction)
        {
            return inBehavior.SmartCoroutine(WaitSecondsThenStart_Routine(inSeconds, inFunction));
        }

        /// <summary>
        /// Waits a given number of seconds then calls a function.
        /// </summary>
        /// <param name="inSeconds">Number of seconds to wait.</param>
        /// <param name="inFunction">Function to call.</param>
        static public CoroutineHandle WaitSecondsThen(this MonoBehaviour inBehavior, float inSeconds, Action inFunction)
        {
            return inBehavior.SmartCoroutine(WaitSecondsThen_Routine(inSeconds, inFunction));
        }

        /// <summary>
        /// Waits a given number of seconds then calls a function.
        /// </summary>
        /// <param name="inSeconds">Number of seconds to wait.</param>
        /// <param name="inFunction">Function to call.</param>
        /// <param name="inParam">Parameter for the function.</param>
        static public CoroutineHandle WaitSecondsThen<T>(this MonoBehaviour inBehavior, float inSeconds, Action<T> inFunction, T inParam)
        {
            return inBehavior.SmartCoroutine(WaitSecondsThen_Routine(inSeconds, inFunction, inParam));
        }

        #endregion

        #region Wait for condition

        /// <summary>
        /// Waits for a condition then starts a coroutine.
        /// </summary>
        /// <param name="inCondition">Function returning if the condition has been satisfied.</param>
        /// <param name="inFunction">Coroutine to start.</param>
        static public CoroutineHandle WaitConditionThenStart(this MonoBehaviour inBehavior, Func<bool> inCondition, IEnumerator inFunction, float inInterval = 0.0f)
        {
            return inBehavior.SmartCoroutine(WaitConditionThenStart_Routine(inCondition, inFunction, inInterval));
        }

        /// <summary>
        /// Waits for a condition then calls a function.
        /// </summary>
        /// <param name="inCondition">Function returning if the condition has been satisfied.</param>
        /// <param name="inFunction">Function to call.</param>
        static public CoroutineHandle WaitConditionThen(this MonoBehaviour inBehavior, Func<bool> inCondition, Action inFunction, float inInterval = 0.0f)
        {
            return inBehavior.SmartCoroutine(WaitConditionThen_Routine(inCondition, inFunction, inInterval));
        }

        /// <summary>
        /// Waits for a condition then calls a function.
        /// </summary>
        /// <param name="inCondition">Function returning if the condition has been satisfied.</param>
        /// <param name="inFunction">Function to call.</param>
        /// <param name="inParam">Parameter for the function.</param>
        static public CoroutineHandle WaitConditionThen<T>(this MonoBehaviour inBehavior, Func<bool> inCondition, Action<T> inFunction, T inParam, float inInterval = 0.0f)
        {
            return inBehavior.SmartCoroutine(WaitConditionThen_Routine(inCondition, inFunction, inParam, inInterval));
        }

        #endregion
    }

}