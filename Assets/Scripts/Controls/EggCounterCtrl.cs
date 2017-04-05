using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;
using Ekstep;

public class EggCounterCtrl : MonoBehaviour
{
    #region Prefabs

    public GameObject OldEggPrefab;
    public GameObject NewEggPrefab;

    #endregion

    #region Gui

    public SuitcaseCtrl Suitcase;
    public Button SuitcaseButton;
    
    #endregion

    #region Callbacks

    public event Action OnReadyForEggs;
    public event Action OnEggAdded;
    public event Action OnTopEggAdded;
    public event Action OnClosed;
    public event Action OnSuitcaseOpen;
    public event Action OnSuitcaseClose;

    #endregion

    public const int MAX_EGGS = 10;

    private Transform[] m_EggPositions;
    private Transform[] m_DisplayedEggs;
    private int m_NextEggInsert = 0;

    private Animator m_Animator;
    private MecanimEventHandler m_Mecanim;
    private CoroutineHandle m_CurrentRoutine;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Mecanim = GetComponent<MecanimEventHandler>();
        FindPositions();

        Suitcase.onExited = Suitcase_onExited;
		SuitcaseButton.onClick.AddListener(SuitcaseButton_onClicked);
    }

    private void FindPositions()
    {
        m_EggPositions = new Transform[MAX_EGGS];
        m_DisplayedEggs = new Transform[MAX_EGGS];

        for (int i = 0; i < MAX_EGGS - 1; ++i)
        {
            m_EggPositions[i] = transform.FindChild("CountSlot" + (i + 1).ToStringLookup());
        }
        m_EggPositions[MAX_EGGS - 1] = transform.FindChild("SuitcaseHolder");
    }

    public void PopulateExistingEggs(int inOldEggs)
    {
        Assert.True(inOldEggs <= MAX_EGGS, "Old eggs is valid.");
        for(int i = 0; i < inOldEggs; ++i)
        {
            if (m_DisplayedEggs[i] == null)
            {
                GameObject egg = (GameObject)Instantiate(OldEggPrefab);
                egg.transform.SetParent(m_EggPositions[i], false);
                m_DisplayedEggs[i] = egg.transform;
            }
        }
        for(int i = inOldEggs; i < MAX_EGGS; ++i)
        {
            UnityHelper.SafeDestroy(ref m_DisplayedEggs[i]);
        }
        m_Animator.SetBool("suitcaseFilled", inOldEggs == MAX_EGGS);
        m_Animator.SetBool("suitcaseOpen", false);
        m_NextEggInsert = inOldEggs;
    }

    public void ClearEggs()
    {
        PopulateExistingEggs(0);
    }

    public void Show(bool inbShow = true)
    {
        if (inbShow)
            gameObject.SetActive(true);
        
        if (inbShow)
        {
            ClearEggs();
            int numEggs = Session.Exists ? Session.instance.numEggs : SaveData.instance.Eggs;
            numEggs %= 10;
            PopulateExistingEggs(numEggs);
        }

        this.ReplaceCoroutine(ref m_CurrentRoutine, inbShow ? OpenSequence() : CloseSequence());
    }

    public void AddNewEgg()
    {
        Assert.True(m_NextEggInsert < MAX_EGGS, "Next egg is valid.");

        GameObject egg = (GameObject)Instantiate(NewEggPrefab);
        egg.transform.SetParent(m_EggPositions[m_NextEggInsert], false);
        egg.GetComponent<Animator>().SetTrigger("isNewEgg");
        m_DisplayedEggs[m_NextEggInsert] = egg.transform;
        ++m_NextEggInsert;

        SoundManager.instance.PlayRandomOneShot(SoundManager.instance.eggEarn);

        if (m_NextEggInsert == MAX_EGGS)
            egg.gameObject.SetActive(false);

        if (OnEggAdded != null)
            OnEggAdded();

        if (m_NextEggInsert == MAX_EGGS)
        {
            this.ReplaceCoroutine(ref m_CurrentRoutine, AddTopEggSequence());
        }
    }

    public Transform GetNextSlot(int inAdvanceBy = 1)
    {
        int slotIndex = m_NextEggInsert + inAdvanceBy - 1;
        Assert.True(slotIndex < MAX_EGGS, "Slot index is valid.");
        return m_EggPositions[slotIndex];
    }

    public bool WillReachTop(int inToAdd, out int outRemaining)
    {
        bool bPasses = false;
        int numEggs = m_NextEggInsert + inToAdd;
        if (numEggs >= MAX_EGGS)
        {
            bPasses = true;
            outRemaining = numEggs - MAX_EGGS;
        }
        else
        {
            outRemaining = 0;
        }
        return bPasses;
    }

    #region Sequences

    public bool IsTransitioning { get { return m_CurrentRoutine.IsRunning(); } }

    private IEnumerator OpenSequence()
    {
        SoundManager.instance.PlayOneShot(SoundManager.instance.eggMeterOpen);

        m_Animator.SetTrigger("showCounter");
        yield return m_Mecanim.WaitForStateBegin("eggCounterOn");

        if (OnReadyForEggs != null)
            OnReadyForEggs();
    }

    private IEnumerator CloseSequence()
    {
        SoundManager.instance.PlayOneShot(SoundManager.instance.eggMeterClose);

        m_Animator.SetTrigger("hideCounter");
        yield return m_Mecanim.WaitForStateBegin("eggCounterOff");

        if (OnClosed != null)
            OnClosed();

        gameObject.SetActive(false);
    }

    private IEnumerator ResetSequence()
    {
        SoundManager.instance.PlayOneShot(SoundManager.instance.eggMeterClose);
        m_Animator.SetTrigger("hideCounter");
        yield return m_Mecanim.WaitForStateBegin("eggCounterOff");

        SoundManager.instance.PlayOneShot(SoundManager.instance.eggMeterClear);
        ClearEggs();

        yield return 0.3f;

        SoundManager.instance.PlayOneShot(SoundManager.instance.eggMeterOpen);
        m_Animator.SetTrigger("showCounter");
        yield return m_Mecanim.WaitForStateBegin("eggCounterOn");

        if (OnReadyForEggs != null)
            OnReadyForEggs();
    }

    private IEnumerator AddTopEggSequence()
    {
        SoundManager.instance.PlayOneShot(SoundManager.instance.eggMeterTop);

        m_Animator.SetBool("suitcaseFilled", true);
        yield return m_Mecanim.WaitForStateEnd("suitcaseFilledToOn", "SuitcaseState");

        if (OnTopEggAdded != null)
            OnTopEggAdded();
    }

    #endregion

    #region Suitcase

    public void OpenSuitcase()
    {
        int numStamps = Session.Exists ? Session.instance.numStamps : SaveData.instance.Stamps;
        bool bReveal = m_NextEggInsert >= MAX_EGGS;
        bReveal &= numStamps <= Suitcase.stamps.Length;

        m_Animator.SetBool("suitcaseOpen", true);
        Suitcase.Show(true, bReveal);
        Suitcase.onExited = Suitcase_onExited;
        SetSuitcaseButton(false);

        if (OnSuitcaseOpen != null)
            OnSuitcaseOpen();
    }

    private void Suitcase_onExited()
    {
        m_Animator.SetBool("suitcaseOpen", false);
        if (m_NextEggInsert >= MAX_EGGS)
        {
            this.ReplaceCoroutine(ref m_CurrentRoutine, ResetSequence());
        }
        if (OnSuitcaseClose != null)
            OnSuitcaseClose();
        SetSuitcaseButton(true);
    }

    #endregion

    #region Buttons

    public void SetSuitcaseButton(bool inbActive)
    {
        SuitcaseButton.enabled = inbActive;
    }

    private void SuitcaseButton_onClicked()
    {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "levelComplete.suitcase"));
        OpenSuitcase();
    }

    #endregion
}
