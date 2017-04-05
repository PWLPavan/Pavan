using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ekstep;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FGUnity.Utils
{
    /// <summary>
    /// Contains assertion methods.
    /// Only used when the DEVELOPMENT define is provided.
    /// </summary>
    static public class Assert
    {
        static Assert()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes Assert functionality and exception handling.
        /// </summary>
        static public void Initialize()
        {
            if (!s_Initialized)
            {
                s_Initialized = true;
                Application.logMessageReceived += Application_logMessageReceived;
            }
        }

        static private bool s_Initialized = false;

        static private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                // HACK HACK HORRIBLE HACK
                // This is due to Unity 5.1 onwards throwing EGL_BAD_NATIVE_WINDOW
                // errors when resuming on certain devices, like the Samsung Galaxy S4.
                // These errors affect nothing and the game can keep running all the same.
                // Why Unity throws an error without meaning to interrupt things is beyond me.
                // - Alex B
                if (condition.Contains("[EGL]") || condition.StartsWith("<R.H"))
                    return;

                Assert.Fail("Uncaught Error: {0}\nat {1}", condition, stackTrace);
            }
        }

        #region True

        /// <summary>
        /// Asserts that the given condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        static public void True(bool inbCondition)
        {
            if (!inbCondition)
            {
                OnFailure(GetStackLocation(1), String.Empty, String.Empty);
            }
        }

        /// <summary>
        /// Asserts that the given condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        static public void True(bool inbCondition, string inCheck)
        {
            if (!inbCondition)
            {
                OnFailure(GetStackLocation(1), inCheck, String.Empty);
            }
        }

        /// <summary>
        /// Asserts that the given condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        static public void True(bool inbCondition, string inCheck, string inMessage)
        {
            if (!inbCondition)
            {
                OnFailure(GetStackLocation(1), inCheck, inMessage);
            }
        }

        /// <summary>
        /// Asserts that the given condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        static public void True(bool inbCondition, string inCheck, string inMessage, params object[] inMessageParams)
        {
            if (!inbCondition)
            {
                OnFailure(GetStackLocation(1), inCheck, String.Format(inMessage, inMessageParams));
            }
        }

        #endregion

        #region Fail

        /// <summary>
        /// Fails an assertion.
        /// </summary>
        static public void Fail(string inMessage)
        {
            OnFailure(GetStackLocation(1), "Failure", inMessage);
        }

        /// <summary>
        /// Fails an assertion.
        /// </summary>
        static public void Fail(string inMessage, params object[] inMessageParams)
        {
            OnFailure(GetStackLocation(1), "Failure", String.Format(inMessage, inMessageParams));
        }

        #endregion

        // Hashed locations to ignore
        static private HashSet<int> s_LocationsToIgnore = new HashSet<int>();

        // Shows an error prompt and responds.
        static private void OnFailure(string inLocation, string inCondition, string inMessage)
        {
            string message = String.Format("Location: {0}\nCondition: {1}{2}", inLocation, inCondition, String.IsNullOrEmpty(inMessage) ? string.Empty : "\n\n" + inMessage);

            UnityEngine.Debug.LogWarning(message);

            string location = inCondition == null ? inLocation : inCondition + " @ " + inLocation;
            int locationHash = Animator.StringToHash(location);
            if (s_LocationsToIgnore.Contains(locationHash))
                return;

            ErrorResult result = ShowUnityError(message);
            if (result == ErrorResult.IgnoreAll)
            {
                s_LocationsToIgnore.Add(locationHash);
            }
            else if (result == ErrorResult.Break)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Break();
#else
                CreateCrashScreen(message);
#endif
            }
        }

        // In the editor, display a dialog box.
        // Otherwise, automatically fail.
        static private ErrorResult ShowUnityError(string inMessage)
        {
#if UNITY_EDITOR
            int result = EditorUtility.DisplayDialogComplex("Assert Fail", inMessage, "Ignore", "Ignore All", "Break");
            if (result == 0)
                return ErrorResult.Ignore;
            else if (result == 1)
                return ErrorResult.IgnoreAll;
            else
                return ErrorResult.Break;
#else
            return ErrorResult.Break;
#endif
        }

        static private void CreateCrashScreen(string inMessage)
        {
            // Log a crash to Genie?
            if (Genie.Exists)
            {
                try
                {
                    // Log an event to Genie detailing the crash
                    // OE_MISC? OE_INTERRUPT?
                    Genie.I.LogEvent(new OE_MISC("crash", inMessage));
                }
                catch
                {
                    UnityEngine.Debug.LogWarning("Unable to write crash to Genie.");
                }
            }

            // Create a crash screen of some sort
            CrashScreen.Create(inMessage);

            // Or maybe just quit the application entirely?
            //Application.Quit();
        }

        // Retrieves the call location several stack frames back
        static private string GetStackLocation(int inDepth)
        {
            // Note: this needs to be disabled on certain platforms
            // since IL2CPP doesn't implement some of these methods.
            // WebGL export will crash -hard- on this and tell you
            // practically nothing. - Alex B
#if !DISABLE_STACK_TRACE
            StackTrace stackTrace = new StackTrace();
            StackFrame frame = stackTrace.GetFrame(inDepth + 1);

            string typeName = frame.GetMethod().DeclaringType.Name;
            string methodName = frame.GetMethod().Name;
            int lineNumber = frame.GetFileLineNumber();
            int columnNumber = frame.GetFileColumnNumber();

            if (lineNumber == 0)
                return String.Format("{0}::{1}", typeName, methodName);
            else
                return String.Format("{0}::{1} @ {2}({3})", typeName, methodName, lineNumber, columnNumber);
#else
            return "[Stack Trace Disabled]";
#endif
        }

        /// <summary>
        /// Result of an error prompt.
        /// </summary>
        public enum ErrorResult
        {
            /// <summary>
            /// Program execution should halt.
            /// </summary>
            Break,

            /// <summary>
            /// This assert will be ignored.
            /// </summary>
            Ignore,

            /// <summary>
            /// Any asserts at the same location will be ignored.
            /// </summary>
            IgnoreAll
        }
    }
}