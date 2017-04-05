using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;
using Ekstep;

public class EggAwardCtrl : MonoBehaviour
{
    #region Prefabs

    public GameObject EggAwardPrefab;

    #endregion

    #region Gui

    public EggCounterCtrl Counter;
    public float TimeBetweenEggs = 0.5f;
    
    #endregion

    #region Callbacks

    public event Action OnResume;
    public event Action OnFinished;

    #endregion

    private CoroutineHandle m_CurrentRoutine;
    private List<SteerableBehavior> m_Eggs = new List<SteerableBehavior>();
    private List<SteerableBehavior> m_AnimatingEggs = new List<SteerableBehavior>();
    private int m_InitialEggCount = 0;

    public bool TwoPhase { get; private set; }

    private void Awake()
    {
        Counter.OnTopEggAdded += Counter_OnReachTop;
        Counter.OnReadyForEggs += Counter_OnReadyForEggs;
    }

    public void Clear()
    {
        foreach (SteerableBehavior behavior in m_Eggs)
            Destroy(behavior.gameObject);
        foreach (SteerableBehavior behavior in m_AnimatingEggs)
            Destroy(behavior.gameObject);

        m_Eggs.Clear();
        m_AnimatingEggs.Clear();

        this.StopCoroutine(ref m_CurrentRoutine);
    }

    public void SpawnEggs(int inEggCount)
    {
        Assert.True(inEggCount <= EggCounterCtrl.MAX_EGGS && inEggCount > 0, "Max of full counter to add.", "Must add {0} or fewer eggs - {1} provided.", EggCounterCtrl.MAX_EGGS, inEggCount);

        int remainingEggs;
        bool bWillOpenSuitcase = Counter.WillReachTop(inEggCount, out remainingEggs);
        m_InitialEggCount = inEggCount - remainingEggs;

        for(int i = 0; i < inEggCount; ++i)
        {
            Vector3 position = new Vector3(-155f, 165f - 80 * i);
            Quaternion rotation = Quaternion.Euler(0, 0, i % 2 == 0 ? -20f : 20f);
            SteerableBehavior egg = SpawnEgg(position, rotation);
            m_Eggs.Add(egg);
        }

        SoundManager.instance.PlayOneShot(SoundManager.instance.eggSpawn);

        TwoPhase = bWillOpenSuitcase;
    }

    public void UpdateSaveData(int inEggsToAdd)
    {
        if (Session.Exists)
        {
            Session.instance.numEggs += inEggsToAdd;
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "eggsEarned." + Session.instance.numEggs.ToStringLookup()));
        }
        else
        {
            SaveData.instance.Eggs += inEggsToAdd;
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "eggsEarned." + SaveData.instance.Eggs.ToStringLookup()));
        }

        SaveData.instance.SyncAndSave();
        Genie.I.SyncEvents();
    }

    private SteerableBehavior SpawnEgg(Vector3 inPosition, Quaternion inRotation)
    {
        GameObject egg = (GameObject)Instantiate(EggAwardPrefab, inPosition, inRotation);
        SteerableBehavior steering = egg.AddComponent<SteerableBehavior>();
        steering.speed = 2.0f;
        egg.transform.SetParent(Counter.transform.parent, false);
        egg.transform.SetSiblingIndex(Counter.transform.GetSiblingIndex() + 1);
        egg.GetComponent<RectTransform>().anchoredPosition = inPosition;
        return steering;
    }

    private void SendEggToDestination(SteerableBehavior inEgg, Transform inTarget)
    {
        inEgg.GetComponent<Animator>().SetTrigger("moveEggToCounter");
        inEgg.StartMove(inTarget.position, true, 2.0f);
        inEgg.onMoveEnd = Egg_OnReachDestination;

        SoundManager.instance.PlayOneShot(SoundManager.instance.eggFly);
    }

    private void Egg_OnReachDestination(SteerableBehavior inObject)
    {
        Destroy(inObject.gameObject);
        m_AnimatingEggs.Remove(inObject);
        Counter.AddNewEgg();
    }

    private void Counter_OnReadyForEggs()
    {
        if (m_InitialEggCount > 0)
        {
            Counter.SetSuitcaseButton(false);

            this.ReplaceCoroutine(ref m_CurrentRoutine, EggAnimationRoutine(m_InitialEggCount));
            m_InitialEggCount = 0;
        }
        else
        {
            Counter.SetSuitcaseButton(true);

            if (OnResume != null)
                OnResume();

            this.ReplaceCoroutine(ref m_CurrentRoutine, EggAnimationRoutine(m_Eggs.Count));
        }
    }

    private void Counter_OnReachTop()
    {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "stampsEarned." + Session.instance.numStamps.ToStringLookup()));
        Counter.OpenSuitcase();
    }

    private IEnumerator EggAnimationRoutine(int inNumToSend)
    {
        using(PooledList<Transform> targetPositions = PooledList<Transform>.Create())
        {
            for (int i = 0; i < inNumToSend; ++i)
                targetPositions.Add(Counter.GetNextSlot(i + 1));

            SteerableBehavior lastEgg = null;
            for(int i = 0; i < inNumToSend; ++i)
            {
                lastEgg = m_Eggs[m_Eggs.Count - 1];
                m_Eggs.RemoveAt(m_Eggs.Count - 1);
                m_AnimatingEggs.Add(lastEgg);
                SendEggToDestination(lastEgg, targetPositions[i]);
                yield return TimeBetweenEggs;
            }

            if (m_Eggs.Count == 0 && TwoPhase == false)
            {
                while (lastEgg != null && lastEgg.IsMoving)
                    yield return null;

                if (OnFinished != null)
                    OnFinished();
            }

            TwoPhase = false;
        }
    }
}
