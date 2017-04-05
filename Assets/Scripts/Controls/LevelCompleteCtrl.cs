using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;
using Minigames;
using Ekstep;

public class LevelCompleteCtrl : MonoBehaviour
{
    #region Gui
    public Button ContinueButton;

    public Transform EggScreen;
    public EggCounterCtrl EggCounter;
    public EggAwardCtrl EggAward;

    public Transform LoadingScreen;

    Transform _background;
    Transform _foreground;

    public Transform[] ChickenSlots;

    #endregion

    #region Inspector
    public PolaroidConfig[] configurations;

    public PolaroidConfig firstLevelConfiguration;

    public PilotBackgroundConfig[] PeacockAndKiwi;

    public NestTest nestMinigame;
    public ExpressionMinigame expressionMinigame;

    public List<Sprite> ExcludedOnLowEnd;

    public int ExpressionMinigameInterval = 6;
    public int NestMinigameInterval = 3;

    public bool PlayStingerMusic = false;
	public bool ForceNestMinigame = false;

    #endregion

    #region Members

    public Action onCtrlOff;
    public Action onCtrlOn;

    private PolaroidConfig m_LastPolaroid;
    private MinigameCtrl m_NextMinigame;

    private GroupHider m_PolaroidGroup;
    private MaskOptimizer m_Masks;

    private CoroutineHandle m_HideRoutine;
    private bool m_LevelLoaded;

    #endregion

    #region Ctrl
    void Awake () {
        _background = this.transform.Find ("Polaroid/DestinationScene");
        _foreground = this.transform.Find ("Polaroid/DestinationScene/DestinationForeground");
        m_PolaroidGroup = this.transform.Find("Polaroid").GetComponent<GroupHider>();

        m_Masks = gameObject.AddComponent<MaskOptimizer>();
    }

    void Start ()
    {
        EggAward.OnResume += Award_Resuming;
        EggAward.OnFinished += Award_Finished;
        ContinueButton.GetComponent<Button>().onClick.AddListener(ContinueBtn_onClick);
    }

    private void CheckMasks()
    {
        _background.GetComponent<Mask>().enabled = !Optimizer.instance.DisableMasks;
    }

    public void ToOn ()
    {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.START, "levelComplete"));

        m_PolaroidGroup.HideAll();
        m_PolaroidGroup.ShowAll();

        if (Session.instance.currentLevel.value == 0)
            MakePeacockAndKiwiPhoto();
        else
            MakeChickenPhoto();

        // reset the reward being shown or not
        EggAward.Clear();
        ContinueButton.enabled = false;

        EggScreen.GetComponent<TransformParentMemory>().ChangeTransform(this.transform, false);
        EggScreen.gameObject.SetActive(true);

        // show
        SoundManager.instance.PlayOneShot(SoundManager.instance.polaroidEnter);
        this.GetComponent<Animator>().SetTrigger("tinyShipOut");
        this.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.PolaroidToOn"), Ctrl_on);

        Level levelFrom = Session.instance.currentLevel;
        if (levelFrom.isDoubleDigitProblem)
            SaveData.instance.SeenTens = true;
        levelFrom.CompletedMechanicsTutorials();

        m_Masks.StartChecking();
        m_LevelLoaded = false;

        this.SmartCoroutine(LoadNextLevel());

        CleanupHook.instance.Cleanup();
    }

    private void MakeChickenPhoto()
    {
        m_LastPolaroid = configurations[RNG.Instance.Next(0, configurations.Length)];

        if (Session.instance.numEggs == 0)
        {
            if (firstLevelConfiguration != null)
                m_LastPolaroid = firstLevelConfiguration;
            else
                Logger.Warn("Missing PolaroidConfig for first level!");
        }

        // display foreground
        _background.GetComponent<Image>().sprite = m_LastPolaroid.Background;

        // display foreground
        Image image = _foreground.GetComponent<Image>();
        image.sprite = m_LastPolaroid.Foreground;
        image.enabled = image.sprite != null;

        // clear chicken mounts
        foreach (Transform slot in ChickenSlots)
            slot.GetComponent<Image>().enabled = false;
        foreach (var pilotConfig in PeacockAndKiwi)
            pilotConfig.Slot.gameObject.SetActive(false);

        int numberOfChickensToDisplay = 1;
        if (Session.instance.currentLevel.value > 1 && RNG.Instance.NextBool())
            numberOfChickensToDisplay = 2;

        using (PooledList<Sprite> foregroundChickens = PooledList<Sprite>.Create())
        {
            // HORRIBLE HACK
            GameObject screen = GameObject.FindGameObjectWithTag("GameScreen");
		    MyScreen myScreen = screen.GetComponent<MyScreen>();

            int brownCount = myScreen.onesColumn.GetNumWithColor(CreatureCtrl.COLOR_BROWN) + myScreen.tensColumn.GetNumWithColor(CreatureCtrl.COLOR_BROWN) * 10;
            int goldCount = myScreen.tensColumn.GetNumWithColor(CreatureCtrl.COLOR_GOLD) * 10;
            int whiteCount = myScreen.onesColumn.GetNumWithColor(CreatureCtrl.COLOR_WHITE) + myScreen.tensColumn.GetNumWithColor(CreatureCtrl.COLOR_WHITE) * 10;

            bool bCheckExclusionList = Optimizer.instance.DisableMasks;

            if (brownCount == 1)
            {
                Sprite brownChicken = null;
                while (brownChicken == null)
                {
                    brownChicken = RNG.Instance.Choose(m_LastPolaroid.BrownChickens);
                    if (bCheckExclusionList && ExcludedOnLowEnd.Contains(brownChicken))
                        brownChicken = null;
                }
                string brownChickenPose = brownChicken.name;
                foregroundChickens.Add(brownChicken);

                Sprite whiteChicken = null;
                int tries = m_LastPolaroid.WhiteChickens.Length * 2;
                while(whiteChicken == null && --tries >= 0)
                {
                    whiteChicken = RNG.Instance.Choose(m_LastPolaroid.WhiteChickens);
                    if (whiteChicken.name.Contains(brownChickenPose) || (bCheckExclusionList && ExcludedOnLowEnd.Contains(whiteChicken)))
                        whiteChicken = null;
                }
                if (whiteChicken != null)
                    foregroundChickens.Add(whiteChicken);
                else
                    numberOfChickensToDisplay = 1;
            }
            else if (brownCount >= 2)
            {
                AddValidPoses(foregroundChickens, m_LastPolaroid.BrownChickens);
            }
            else
            {
                AddValidPoses(foregroundChickens, m_LastPolaroid.WhiteChickens);
                if (foregroundChickens.Count < 2)
                    numberOfChickensToDisplay = 1;
            }

            ChickenSlots.Shuffle();

            for (int i = 0; i < numberOfChickensToDisplay; ++i)
            {
                int chickenIndex = (i % foregroundChickens.Count);
                if (chickenIndex == 0)
                    foregroundChickens.Shuffle();

                Image slot = ChickenSlots[i].GetComponent<Image>();
                slot.sprite = foregroundChickens[chickenIndex];
                slot.enabled = true;
                //slot.SetNativeSize();
            }
        }
    }

    private void AddValidPoses(List<Sprite> ioList, Sprite[] inSource)
    {
        bool bCheckExclusionList = Optimizer.instance.DisableMasks;

        if (bCheckExclusionList)
        {
            for (int i = 0; i < inSource.Length; ++i)
            {
                if (!ExcludedOnLowEnd.Contains(inSource[i]))
                    ioList.Add(inSource[i]);
            }
        }
        else
        {
            ioList.AddRange(inSource);
        }
    }

    private void MakePeacockAndKiwiPhoto()
    {
        PilotBackgroundConfig pilotConfig = RNG.Instance.Choose(PeacockAndKiwi);
        m_LastPolaroid = RNG.Instance.Choose(pilotConfig.Backgrounds);

        _background.GetComponent<Image>().sprite = m_LastPolaroid.Background;
        _foreground.GetComponent<Image>().sprite = m_LastPolaroid.Foreground;
        _foreground.GetComponent<Image>().enabled = m_LastPolaroid.Foreground != null;

        // clear chicken mounts
        foreach (Transform slot in ChickenSlots)
            slot.GetComponent<Image>().enabled = false;
        foreach (var pilotSlot in PeacockAndKiwi)
            pilotSlot.Slot.gameObject.SetActive(false);

        pilotConfig.Slot.gameObject.SetActive(true);
        pilotConfig.Slot.GetComponent<Image>().enabled = true;
    }

    void Ctrl_on ()
    {
        this.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.PolaroidToOn"), Ctrl_on);
        if (onCtrlOn != null)
            onCtrlOn();

        EggCounter.Show(true);
        EggAward.SpawnEggs(Session.instance.eggsEarned);
        EggAward.UpdateSaveData(Session.instance.eggsEarned);

        HookEggControl();

        if (!EggAward.TwoPhase)
            ContinueButton.enabled = true;

        Session.instance.MarkLevelStart();
    }

    void Award_Resuming()
    {
        ContinueButton.enabled = true;
    }

    void Award_Finished()
    {
        ContinueButton.enabled = true;
        EggCounter.SetSuitcaseButton(true);
    }

    public void PlayStinger()
    {
        if (!PlayStingerMusic)
            return;

        SoundManager.instance.DuckMusic(SoundManager.instance.DuckVolume / 2, SoundManager.instance.DuckTime * 2);
        SoundManager.instance.PlayMusicOneShot(m_LastPolaroid.Stinger);
    }

    #endregion

    #region Input

    private IEnumerator LoadNextLevel()
    {
        Session.instance.LoadNextLevel();
        while (!Session.instance.IsNextLevelLoaded)
            yield return null;

        CalculateNextMinigame();

        m_LevelLoaded = true;
    }

    void ContinueBtn_onClick()
    {
        Genie.I.LogEvent(OE_INTERACT.CreateDuration(OE_INTERACT.Type.END, "levelComplete", Session.instance.timeTaken));
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "levelComplete.continue"));

        SaveData.instance.SyncAndSave();

        EggAward.Clear();
        EggCounter.Show(false);
        EggCounter.SetSuitcaseButton(false);

        ContinueButton.enabled = false;

        this.SmartCoroutine(ContinueBtn_onClickRoutine());
    }

    private IEnumerator ContinueBtn_onClickRoutine()
    {
        if (!m_LevelLoaded)
        {
            LoadingScreen.gameObject.SetActive(true);

            while (!m_LevelLoaded)
                yield return null;

            LoadingScreen.gameObject.SetActive(false);

            yield return null;
        }

        this.GetComponent<Animator>().SetTrigger("newProblem");
        SoundManager.instance.PlayOneShot(SoundManager.instance.polaroidExit);

        if (m_NextMinigame != null)
        {
            OpenMinigame();
        }
        else
        {
            CloseWindow();
        }
    }

    private void CalculateNextMinigame()
    {
        Level currentLevel = Session.instance.currentLevel;
        Level nextLevel = Session.instance.PeekNextLevel();

        bool bOnMinigameInterval = Session.instance.numLevelsCompleted > 0 && (Session.instance.numLevelsCompleted % ExpressionMinigameInterval) == 0;
        bool bOnNestMinigameInterval = bOnMinigameInterval && (Session.instance.numLevelsCompleted % (ExpressionMinigameInterval * NestMinigameInterval)) == 0;

        bool bShowNestMinigame = currentLevel.showNestMinigame != Level.MinigameTiming.Block && nextLevel.showNestMinigame != Level.MinigameTiming.Block;
        if (bShowNestMinigame)
        {
            if (currentLevel.showNestMinigame == Level.MinigameTiming.After || nextLevel.showNestMinigame == Level.MinigameTiming.Before)
            {
                bShowNestMinigame = true;
            }
            else if (nextLevel.isDoubleDigitProblem && !SaveData.instance.SeenTens)
            {
                bShowNestMinigame = true;
            }
            else if (SaveData.instance.SeenTens && bOnMinigameInterval && bOnNestMinigameInterval)
            {
                bShowNestMinigame = true;
            }
            else
            {
                bShowNestMinigame = false;
            }
        }

        bool bShowExpressionMinigame = currentLevel.showExpressionMinigame != Level.MinigameTiming.Block && nextLevel.showExpressionMinigame != Level.MinigameTiming.Block;
        if (bShowExpressionMinigame)
        {
            if (currentLevel.showExpressionMinigame == Level.MinigameTiming.After || nextLevel.showExpressionMinigame == Level.MinigameTiming.Before)
            {
                bShowExpressionMinigame = true;
            }
            else if (bOnMinigameInterval)
            {
                bShowExpressionMinigame = true;
            }
            else
            {
                bShowExpressionMinigame = false;
            }
        }

        if (bShowNestMinigame || ForceNestMinigame)
            m_NextMinigame = nestMinigame;
        else if (bShowExpressionMinigame)
            m_NextMinigame = expressionMinigame;
        else
            m_NextMinigame = null;
    }

    private void CloseWindow()
    {
        Camera.main.GetComponent<Animator>().SetTrigger("camIn");
        this.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.PolaroidToOff"), Ctrl_off);
    }

    private void OpenMinigame()
    {
        m_NextMinigame.Open();
        m_NextMinigame.OnClose += Minigame_Off;

        this.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.PolaroidToOff"), Minigame_Start);
    }

    void Ctrl_off ()
    {
        this.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.PolaroidToOff"), Ctrl_off);
        if (onCtrlOff != null)
            onCtrlOff();

        EggScreen.GetComponent<TransformParentMemory>().RestoreTransform();
        EggScreen.gameObject.SetActive(false);

        m_Masks.StopChecking();

        UnhookEggControl();
    }

    void Minigame_Start()
    {
        this.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.PolaroidToOff"), Minigame_Start);

        EggScreen.GetComponent<TransformParentMemory>().RestoreTransform();
        EggScreen.gameObject.SetActive(false);

        m_Masks.StopChecking();

        UnhookEggControl();

        gameObject.SetActive(false);
    }

    void Minigame_Off(MinigameCtrl inControl)
    {
        m_NextMinigame.OnClose -= Minigame_Off;

        Camera.main.GetComponent<Animator>().SetTrigger("camIn");

        if (onCtrlOff != null)
            onCtrlOff();
    }

    private void HookEggControl()
    {
        EggCounter.OnSuitcaseOpen += EggCounter_OnSuitcaseOpen;
        EggCounter.OnSuitcaseClose += EggCounter_OnSuitcaseClose;
    }

    private void UnhookEggControl()
    {
        EggCounter.OnSuitcaseOpen -= EggCounter_OnSuitcaseOpen;
        EggCounter.OnSuitcaseClose -= EggCounter_OnSuitcaseClose;
    }

    void EggCounter_OnSuitcaseOpen()
    {
        m_HideRoutine = this.WaitSecondsThen(0.3f, m_PolaroidGroup.HideAll);
    }

    void EggCounter_OnSuitcaseClose()
    {
        m_HideRoutine.Clear();
        m_PolaroidGroup.ShowAll();
    }

    #endregion

    [Serializable]
    public class PilotBackgroundConfig
    {
        public Transform Slot;
        public PolaroidConfig[] Backgrounds;
    }

}
