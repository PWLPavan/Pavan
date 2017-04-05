using System;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;

namespace FGUnity.Utils
{
    /// <summary>
    /// Contains logging methods.
    /// Only used when the DEVELOPMENT define is provided.
    /// </summary>
    static public class Logger
    {
        /// <summary>
        /// Logs out to the console.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        static public void Log(string inMessage)
        {
            UnityEngine.Debug.Log(inMessage);
        }

        /// <summary>
        /// Logs out to the console.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        static public void Log(string inMessage, params object[] inMessageParams)
        {
            using(PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
            {
                stringBuilder.Builder.AppendFormat(inMessage, inMessageParams);
                UnityEngine.Debug.Log(stringBuilder.Builder.ToString());
            }
        }

        /// <summary>
        /// Logs out to the console.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        static public void Warn(string inMessage)
        {
            UnityEngine.Debug.LogWarning(inMessage);
        }

        /// <summary>
        /// Logs out to the console.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        static public void Warn(string inMessage, params object[] inMessageParams)
        {
            using (PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
            {
                stringBuilder.Builder.AppendFormat(inMessage, inMessageParams);
                UnityEngine.Debug.LogWarning(stringBuilder.Builder.ToString());
            }
        }
    }
}