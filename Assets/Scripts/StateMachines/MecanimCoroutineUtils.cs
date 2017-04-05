using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

/// <summary>
/// Helper functions for dealing with coroutines and Mecanim
/// </summary>
static public class MecanimCoroutineUtils
{
    /// <summary>
    /// Waits until the given transition starts.
    /// </summary>
    static public IEnumerator WaitForTransitionBegin(this MecanimEventHandler inMecanim, int inTransitionHash)
    {
        Assert.True(inMecanim != null, "Mecanim is not null.");

        bool bContinue = false;
        Action allowContinue = () => { bContinue = true; };
        inMecanim.RegisterOnTransitionBegin(inTransitionHash, allowContinue);
        while (!bContinue && inMecanim != null)
            yield return null;
        if (inMecanim != null)
            inMecanim.UnRegisterOnTransitionBegin(inTransitionHash, allowContinue);
        else
            Logger.Warn("Mecanim was destroyed before waiting was finished.");
    }

    /// <summary>
    /// Waits until the given transition starts.
    /// </summary>
    static public IEnumerator WaitForTransitionBegin(this MecanimEventHandler inMecanim, string inStateA, string inStateB, string inLayerName = MecanimEventHandler.DEFAULT_LAYER_NAME)
    {
        return WaitForTransitionBegin(inMecanim, MecanimEventHandler.GetTransitionHash(inStateA, inStateB, inLayerName));
    }

    /// <summary>
    /// Waits until the given transition ends.
    /// </summary>
    static public IEnumerator WaitForTransitionEnd(this MecanimEventHandler inMecanim, int inTransitionHash)
    {
        Assert.True(inMecanim != null, "Mecanim is not null.");

        bool bContinue = false;
        Action allowContinue = () => { bContinue = true; };
        inMecanim.RegisterOnTransitionEnd(inTransitionHash, allowContinue);
        while (!bContinue && inMecanim != null)
            yield return null;
        if (inMecanim != null)
            inMecanim.UnRegisterOnTransitionEnd(inTransitionHash, allowContinue);
        else
            Logger.Warn("Mecanim was destroyed before waiting was finished.");
    }

    /// <summary>
    /// Waits until the given transition ends.
    /// </summary>
    static public IEnumerator WaitForTransitionEnd(this MecanimEventHandler inMecanim, string inStateA, string inStateB, string inLayerName = MecanimEventHandler.DEFAULT_LAYER_NAME)
    {
        return WaitForTransitionEnd(inMecanim, MecanimEventHandler.GetTransitionHash(inStateA, inStateB, inLayerName));
    }

    /// <summary>
    /// Waits until the given state starts.
    /// </summary>
    static public IEnumerator WaitForStateBegin(this MecanimEventHandler inMecanim, int inStateHash)
    {
        Assert.True(inMecanim != null, "Mecanim is not null.");

        bool bContinue = false;
        Action allowContinue = () => { bContinue = true; };
        inMecanim.RegisterOnStateBegin(inStateHash, allowContinue);
        while (!bContinue && inMecanim != null)
            yield return null;
        if (inMecanim != null)
            inMecanim.UnRegisterOnStateBegin(inStateHash, allowContinue);
        else
            Logger.Warn("Mecanim was destroyed before waiting was finished.");
    }

    /// <summary>
    /// Waits until the given state starts.
    /// </summary>
    static public IEnumerator WaitForStateBegin(this MecanimEventHandler inMecanim, string inStateName, string inLayerName = MecanimEventHandler.DEFAULT_LAYER_NAME)
    {
        return WaitForStateBegin(inMecanim, MecanimEventHandler.GetStateHash(inStateName, inLayerName));
    }

    /// <summary>
    /// Waits until the given state ends.
    /// </summary>
    static public IEnumerator WaitForStateEnd(this MecanimEventHandler inMecanim, int inStateHash)
    {
        Assert.True(inMecanim != null, "Mecanim is not null.");

        bool bContinue = false;
        Action allowContinue = () => { bContinue = true; };
        inMecanim.RegisterOnStateEnd(inStateHash, allowContinue);
        while (!bContinue && inMecanim != null)
            yield return null;
        if (inMecanim != null)
            inMecanim.UnRegisterOnStateEnd(inStateHash, allowContinue);
        else
            Logger.Warn("Mecanim was destroyed before waiting was finished.");
    }

    /// <summary>
    /// Waits until the given state ends.
    /// </summary>
    static public IEnumerator WaitForStateEnd(this MecanimEventHandler inMecanim, string inStateName, string inLayerName = MecanimEventHandler.DEFAULT_LAYER_NAME)
    {
        return WaitForStateEnd(inMecanim, MecanimEventHandler.GetStateHash(inStateName, inLayerName));
    }
}