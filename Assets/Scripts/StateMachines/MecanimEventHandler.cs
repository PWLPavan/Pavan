//*
// See our blog http://blog.camposanto.com/ for a longer discussion of the motivation for this solution
// No rights reserved. 
//*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

// This is an example MonoBehavior that allows you to get callbacks whenever a Mecanim animator begins or ends a state or transition
// This is very useful for syncing a gameplay state machine to your animation states
public class MecanimEventHandler : MonoBehaviour
{
    [Tooltip("Animator to watch. If none added, will search in children")]
    public Animator animator;
    
    // store all transitions and states so we know when they have changed
    private AnimatorTransitionInfo[] previousTransitions;
    private AnimatorStateInfo[] previousStates;
    
    // declare a set of dictionaries that map Mecanim Hash Id's to our various callbacks
    private Dictionary<int, Action> transitionBeginCallbackMap;
    private Dictionary<int, Action> transitionEndCallbackMap;

    private Dictionary<int, Action> stateBeginCallbackMap;
    private Dictionary<int, Action> stateEndCallbackMap;
    
    // do basic set up
    void Awake()
    {
        // It is very often smart to keep your gameplay logic on a game object that is the parent of your actual mesh and its animator.
        // So, we look for an animator in our children. This lets us easily swap in new rigs and models.
        
        if (animator == null)
        {
            animator = gameObject.GetComponentInChildren<Animator>();
        }
        
        if( animator == null )
        {
            // Nothing will work, everything is ruined, I guess post an error.
            Debug.LogError("Mecanim Event Handler has no animator. " + gameObject.name);
            return;
        }
        
        // allocate some memory. 
        transitionBeginCallbackMap = new Dictionary<int, Action>();
        transitionEndCallbackMap = new Dictionary<int, Action>();
        stateBeginCallbackMap = new Dictionary<int, Action>();
        stateEndCallbackMap = new Dictionary<int, Action>();
        
        previousTransitions = new AnimatorTransitionInfo[animator.layerCount];
        previousStates = new AnimatorStateInfo[animator.layerCount];
    }
    
    // Every frame we need to check our animations for changes. 
    void Update()
    {
        if( animator != null )
        {
            UpdateMecanimCallbacks();
        }
    }
    
    private void UpdateMecanimCallbacks()
    {
        for( int i = 0; i < animator.layerCount; ++i )
        {
            // pull our current transition and state into temporary variables, since we may not need to do anything with them
            AnimatorTransitionInfo tempTransition = animator.GetAnimatorTransitionInfo(i);
            AnimatorStateInfo tempState = animator.GetCurrentAnimatorStateInfo(i);
            
            // if we have a new transition...
            int previousTransition = previousTransitions[i].nameHash;
            if( tempTransition.nameHash != previousTransition )
            {
                // fire off our end callback, if any, for our previous transition...
                Action endCallback;
                if( transitionEndCallbackMap.TryGetValue(previousTransition, out endCallback) )
                {
                    if (endCallback != null)
                        endCallback();
                }
                
                // fire off our begin call back for our new transition...
                Action beginCallback;
                if( transitionBeginCallbackMap.TryGetValue(tempTransition.nameHash, out beginCallback) )
                {
                    if (beginCallback != null)
                        beginCallback();
                }
                
                // and remember that we are now in this transition.
                previousTransitions[i] = tempTransition;
            }
            
            // if we have a new state, things go similarly.
            int previousState = previousStates[i].fullPathHash; //.nameHash
            if( tempState.fullPathHash != previousState )
            {
                // out with the old
                Action endCallback;
                if( stateEndCallbackMap.TryGetValue(previousState, out endCallback) )
                {
                    if (endCallback != null)
                        endCallback();
                }
                
                // in with the new
                Action beginCallback;
                if( stateBeginCallbackMap.TryGetValue(tempState.fullPathHash, out beginCallback) )
                {
                    if (beginCallback != null)
                        beginCallback();
                }
                
                // recall what state we were in
                previousStates[i] = tempState;
            }
        }
    }
    
    // Given a callback and a hash, register it with the given dictionary
    private void RegisterCallback(Dictionary<int, Action> dictionary, int animHash, Action callback)
    {
        if( dictionary.ContainsKey(animHash) )
        {
            dictionary[animHash] = (Action)dictionary[animHash] + callback;
        }
        else
        {
            dictionary.Add(animHash, callback);
        }
    }
    
    // as above, but in reverse
    private void UnRegisterCallback(Dictionary<int, Action> dictionary, int animHash, Action callback)
    {
        if( dictionary.ContainsKey(animHash) )
        {
            dictionary[animHash] = (Action)dictionary[animHash] - callback;
        }
    }
    
    // a set of convenience functions to make calling code look better
    public void RegisterOnTransitionBegin(int animTranisitionHash, Action callback)
    {
        RegisterCallback(transitionBeginCallbackMap, animTranisitionHash, callback);
    }
    
    public void UnRegisterOnTransitionBegin(int animTransitionHash, Action callback)
    {
        UnRegisterCallback(transitionBeginCallbackMap, animTransitionHash, callback);
    }
    
    public void RegisterOnTransitionEnd(int animTransitionHash, Action callback)
    {
        RegisterCallback(transitionEndCallbackMap, animTransitionHash, callback);
    }
    
    public void UnRegisterOnTransitionEnd(int animTransitionHash, Action callback)
    {
        UnRegisterCallback(transitionEndCallbackMap, animTransitionHash, callback);
    }
    
    public void RegisterOnStateBegin(int animStateHash, Action callback)
    {
        RegisterCallback(stateBeginCallbackMap, animStateHash, callback);
    }
    
    public void UnRegisterOnStateBegin(int animStateHash, Action callback)
    {
        UnRegisterCallback(stateBeginCallbackMap, animStateHash, callback);
    }
    
    public void RegisterOnStateEnd(int animStateHash, Action callback)
    {
        RegisterCallback(stateEndCallbackMap, animStateHash, callback);
    }
    
    public void UnRegisterOnStateEnd(int animStateHash, Action callback)
    {
        UnRegisterCallback(stateEndCallbackMap, animStateHash, callback);
    }

    public const string DEFAULT_LAYER_NAME = "Base Layer";

    static public int GetStateHash(string inStateName, string inLayerName = DEFAULT_LAYER_NAME)
    {
        using (PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
        {
            stringBuilder.Builder.Append(inLayerName).Append('.').Append(inStateName);
            return Animator.StringToHash(stringBuilder.Builder.ToString());
        }
    }

    static public int GetTransitionHash(string inStateA, string inStateB, string inLayerName = DEFAULT_LAYER_NAME)
    {
        using (PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
        {
            stringBuilder.Builder.Append(inLayerName).Append('.').Append(inStateA);
            stringBuilder.Builder.Append(" -> ");
            stringBuilder.Builder.Append(inLayerName).Append('.').Append(inStateB);

            return Animator.StringToHash(stringBuilder.Builder.ToString());
        }
    }
    
    // Example calling code:
    //
    // eventHandler.RegisterOnStateEnd( vgPlayerAnimState.Examine, OnExamineAnimationComplete );
    //
}