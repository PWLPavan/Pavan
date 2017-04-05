using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;
using Ekstep;

public class MyScreen : MonoBehaviour
{
    [Serializable]
    public class EnvironmentConfig
    {
        public GameObject prefab;
        public Color bgColor;
		public Color tarmacColor;
		public AudioClip envMusic;
    }

    //{ Prefabs
    [Header("Prefabs")]
    public GameObject splashScreenPrefab;

    public EnvironmentConfig[] environments;
    //}

    //{ Gui
    [Header("GUI")]
    public QueueController queue;
    public PlaceValueCtrl onesColumn;
    public PlaceValueCtrl tensColumn;

    public TutorialCtrl tutorial;
    public NumberPadCtrl numberInput;
    public PauseMenuCtrl pauseMenu;

    [HideInInspector]
    public Transform tinyShip;
    [HideInInspector]
    public Transform ship;

    Transform _poofHolder1;
    Transform _poofHolder2;
    Transform _poofHolder3;
    Transform _poofHolder4;
    Transform _poofHolder5;
    Transform _poofHolder6;

    Transform _nestConvertHolder;

    [HideInInspector]
    public Transform pilotHolder;
    [HideInInspector]
    public Transform pilot;

    public ExpressionCtrl expression;
    public HudCtrl hud;
    public CabinCounterCtrl cabinCounter;
    public SubtractionCtrl subtractionCtrl;
    public ConvertNestCtrl convertNestCtrl;
    public TransitionBackgroundCtrl transitionCtrl;
    //}

    //{ Inspector
    [Header("Settings")]
    public float gameSpeed = 1.0f;
    public int levelsTilSceneChange = 5;

    //public int[] levelTierIndexes;

    // Exposing to inspector for modification
    [Header("Hint Thresholds")]
    public int AttemptsUntilAnswerHighlight = 2;
    public int AttemptsUntilHandHold = 3;

    [Header("Timing")]
    public float NoColumnFlashWait = 1.0f;

    //}

    //{ Members
    GameObject mSplashScreen;
    HandHoldCtrl _handHoldCtrl;
    public HandHoldCtrl handHoldCtrl
    {
        get { return _handHoldCtrl; }
    }

    bool tensValid = true;
    bool onesValid = true;
    public bool launchValid
    {
        get { return (onesValid && tensValid) || Session.instance.currentLevel.useNumberPad; }
    }

    [HideInInspector]
    public int addend = 0;  //TODO: comment

    [HideInInspector]
    public int valueOnesAddend = 0;//TODO: comment
    [HideInInspector]
    public int valueTensAddend = 0;//TODO: comment

    int m_OverflowSubmitCount = 0;

    bool isTransitioning = false;
    CooldownTimer evalTimer;
    CooldownTimer resumePlayTimer;

    [HideInInspector]
    public GameplayInput input;

    bool resumingFromSession = true;
    bool isProblemStarting = false;
    HintingType queuedHint = HintingType.None;
    bool? queuedNumpadState = null;

    CoroutineHandle expressionCoroutine;

    int attemptsOffset = 0;

    private const float BUBBLE_EXPAND_TIME_SINGLE = 0.25f;
    private const float BUBBLE_EXPAND_TIME_DOUBLE = 2.5f;
    //}

    //{ Screen
    void Awake()
    {
        //var spriteDefault = Shader.Find("Sprites/Default");
        //Camera.main.SetReplacementShader(spriteDefault, "Transparent");

        Time.timeScale = gameSpeed;

        // hook up gui getters
        queue.screen = this;    // lol, dependency injection
        //queue.HideAllQueues();

        onesColumn.screen = this;
        tensColumn.screen = this;

        tinyShip = this.transform.Find("Background/TinyShip");
        ship = this.transform.Find("Ship");

        pilotHolder = ship.transform.Find("GoalHolder/PilotHolder");
        pilot = ship.transform.Find("GoalHolder/PilotHolder/Pilot");

        _poofHolder1 = ship.transform.Find("PoofHolder 1");
        _poofHolder2 = ship.transform.Find("PoofHolder 2");
        _poofHolder3 = ship.transform.Find("PoofHolder 3");
        _poofHolder4 = ship.transform.Find("PoofHolder 4");
        _poofHolder5 = ship.transform.Find("PoofHolder 5");
        _poofHolder6 = ship.transform.Find("PoofHolder 6");

        _nestConvertHolder = this.transform.Find("NestConvertHolder");
        convertNestCtrl = _nestConvertHolder.transform.FindChild("nest").GetComponent<ConvertNestCtrl>();

        this.gameObject.AddComponent<HandHoldCtrl>();
        _handHoldCtrl = this.GetComponent<HandHoldCtrl>();
        _handHoldCtrl.Init(this, onesColumn, tensColumn);

        input = new GameplayInput();
        input.Add(GameplayInput.TENS_QUEUE, queue.gameObject);
        input.Add(GameplayInput.ONES_QUEUE, queue.gameObject);
        input.Add(GameplayInput.CONVERT_TO_ONES, onesColumn.gameObject);
        input.Add(GameplayInput.TENS_COLUMN, tensColumn.gameObject);
        input.Add(GameplayInput.ONES_COLUMN, onesColumn.gameObject);
        input.Add(GameplayInput.CONVERT_TO_TENS, _nestConvertHolder.FindChild("nest").gameObject);
        input.Add(GameplayInput.SUBMIT, hud.launchBtn.gameObject);

        input.Add(GameplayInput.TOGGLE_NUMBER_PAD, hud.numberPadBtn.gameObject);
        input.Add(GameplayInput.NUMBER_PAD_ARROWS, numberInput.gameObject);
        input.Add(GameplayInput.SUBMIT_NUMPAD, numberInput.SubmitButton.gameObject);

        subtractionCtrl.gameObject.SetActive(true);
        input.Add(GameplayInput.ONES_SUB, subtractionCtrl.onesZone.gameObject);
        input.Add(GameplayInput.TENS_SUB, subtractionCtrl.tensZone.gameObject);
        input.Add(GameplayInput.ONES_SUB_ADD, subtractionCtrl.onesZone.gameObject);
        input.Add(GameplayInput.TENS_SUB_ADD, subtractionCtrl.tensZone.gameObject);
        subtractionCtrl.gameObject.SetActive(false);

        input.Add(GameplayInput.COUNT_ONES, onesColumn.gameObject);
        input.Add(GameplayInput.COUNT_TENS, tensColumn.gameObject);

        input.Add(GameplayInput.PAUSE, hud.pauseBtn.gameObject);

        input.Add(GameplayInput.PILOT_TAP, pilot.gameObject);
        input.Add(GameplayInput.EGG_TAP, hud.GetComponentInChildren<TapEggs>().gameObject);

        //input.EnableAllInput(true);
        //input.Add(GameplayInput.SKIP, hud.skipLevelBtn.gameObject);
        //input.Add(GameplayInput.PREV, hud.prevLevelBtn.gameObject);
    }

    IEnumerator Start()
    {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.START, "gameplay"));
        EnlearnInstance.I.OnLogActions = ProcessEnlearnResponse;

        gameObject.AddComponent<DebugCtrl>();

        Session.instance.onLevelChanged = Session_onLevelChanged;

        // load levels
        Session.instance.SyncWithSave();

        while (Session.instance.currentLevel == null)
            yield return null;

        Splash_onCtrlOff();
    }

    void OnDestroy()
    {
        if (EnlearnInstance.Exists)
        {
            EnlearnInstance.I.OnLogActions = null;
        }

        if (Session.Exists)
        {
            Session.instance.onLevelChanged = null;
        }
    }

    void Splash_onCtrlOff()
    {
        // destroy splash control
        if (mSplashScreen)
        {
            mSplashScreen.GetComponent<SplashScreenCtrl>().onCtrlOff = null;
            Destroy(mSplashScreen);
            mSplashScreen = null;
        }

        // hook up listeners for game screen
        onesColumn.onCreatureCountUpdated = Column_onCreatureCountUpdated;
        tensColumn.onCreatureCountUpdated = Column_onCreatureCountUpdated;

        hud.onLaunched = Hud_onLaunched;
        hud.onNumberPad = Hud_onNumberPad;
        hud.onPause = Hud_onPaused;

        hud.onLevelCompleteOn = Hud_onLevelCompleteCtrlOn;
        hud.onLevelCompleteOff = Hud_onLevelCompleteCtrlOff;

        onesColumn.onShift = Column_onShift;
        //onesColumn.onVacuumed = OnesColumn_onConverted;
        onesColumn.onFinishedCounting = OnesColumn_onFinishedCounting;
        onesColumn.onCount = OnesColumn_onCount;
        onesColumn.onHintCount = Column_onHintCount;

        tensColumn.onShift = Column_onShift;
        tensColumn.onExploded = TensColumn_onConverted;
        tensColumn.onFinishedCounting = TensColumn_onFinishedCounting;
        tensColumn.onCount = TensColumn_onCount;
        tensColumn.onHintCount = Column_onHintCount;

        cabinCounter.onReadyToEval = CabinCounter_onEvaluate;

        // listen for queue spawns
        queue.onContainerEndMove = Queue_onContainerEndMove;

        // setup jit tutorial
        tutorial.Init(hud.canvas, hud.transform.GetSiblingIndex() + 1);

        // turn bubble off
        expression.ToOff();

        // animate ship on screen
        ShipToOn();

        UpdateBackground(Session.instance.numLevelsCompleted, true, true);
    }

    void Queue_onContainerEndMove(QueueContainer container)
    {
        if (handHoldCtrl.isActive)
            return;

        bool hintShown = tutorial.Show((Session.instance.currentLevel.tensColumnEnabled) ? "showHintDragAddOne" : "showHintDragAdd", false);
        if (hintShown)
        {
            input.EnableAllInput(false, GameplayInput.ONES_QUEUE);
            input.EnableCountingAndPause(true);
        }
    }


    void Update()
    {
        if (!hud.isReady)
        {
            hud.FinalInit();
        }

        if (isTransitioning)
        {
            if (tinyShip.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.TinyShipOut") || tinyShip.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.TinyShipOutTransition"))
            {
                if (tinyShip.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
                {
                    isTransitioning = false;
                }
            }
        }

        if (evalTimer != null && evalTimer.AccumulateFireOnce(Time.deltaTime))
        {
            Evaluate();
        }

        if (resumePlayTimer != null && resumePlayTimer.AccumulateFireOnce(Time.deltaTime))
        {
            //TODO: hide any jit tutorial frames and then show again after hints
            // & resume level
            ResumeLevel();
        }

    }
    //}


    //{ Methods
    void Session_onLevelChanged(int levelIdx)
    {
        UpdateBackground(Session.instance.numLevelsCompleted, true);
    }

    /// <summary>
    /// If the next level will 
    /// </summary>
    public bool WillTransition
    {
        get { return ((Session.instance.numLevelsCompleted + 1) % levelsTilSceneChange) == 0; }
    }

    #region Loading Levels

    private CoroutineHandle m_LoadLevelRoutine;

    public void GotoLevel(int levelIdx)
    {
        m_LoadLevelRoutine.Clear();
        m_LoadLevelRoutine = this.SmartCoroutine(GotoLevelRoutine(levelIdx));
    }

    private IEnumerator GotoLevelRoutine(int inLevelIdx)
    {
        PauseLevel();
        ClearLevel(true);
        yield return Session.instance.GotoLevel(inLevelIdx);
        StartLevel(LevelStartState.Skip);
    }

    public void SkipLevel()
    {
        m_LoadLevelRoutine.Clear();
        m_LoadLevelRoutine = this.SmartCoroutine(SkipLevelRoutine());
    }

    private IEnumerator SkipLevelRoutine()
    {
        PauseLevel();
        ClearLevel(true);
        yield return NextLevel(false, true);
        StartLevel(LevelStartState.Skip);
    }

    public void PrevLevel()
    {
        m_LoadLevelRoutine.Clear();
        m_LoadLevelRoutine = this.SmartCoroutine(PrevLevelRoutine());
    }

    private IEnumerator PrevLevelRoutine()
    {
        PauseLevel();
        ClearLevel(true);
        yield return Session.instance.DecrementLevel();
        StartLevel(LevelStartState.Skip);
    }

    private IEnumerator NextLevel(bool justBeat, bool fromSkip)
    {
        yield return Session.instance.IncrementLevel(justBeat, fromSkip);
    }

    #endregion

    public void ResetLevel(bool fromHandhold = false, bool inbLevelStart = false)
    {
        PauseLevel();

        bool bHadChickens = ClearLevel(!fromHandhold) && !inbLevelStart;

        StartLevel(fromHandhold ? LevelStartState.HandHold : LevelStartState.Reset, bHadChickens);
    }

    public bool ClearLevel(bool enableControls, bool fromTwoPart = false)
    {
        bool bDoPoof = (onesColumn.numCreatures != Session.instance.currentLevel.startingOnes || tensColumn.numCreatures != Session.instance.currentLevel.startingTens);

        m_OverflowSubmitCount = 0;

        // empty queue, begin spawning
        queue.DestroyQueue();

        // empty ship
        onesColumn.Clear(fromTwoPart);
        tensColumn.Clear(fromTwoPart);

        onesColumn.ResetCountingTime();
        tensColumn.ResetCountingTime();

        // empty subtraction drop zone
        if (subtractionCtrl)
        {
            subtractionCtrl.Clear();
            subtractionCtrl.gameObject.SetActive(false);
        }

        // reset pilot states
        pilot.GetComponent<Animator>().SetBool("isCorrect", false);
        pilot.GetComponent<Animator>().SetBool("isUnhappy", false);

        numberInput.Reset();
        numberInput.Show(false);

        //TODO: reset all gameplay input
        if (enableControls)
            input.EnableAllInput(true);

        return bDoPoof;
    }

    public void StartLevel(LevelStartState inState = LevelStartState.Default, bool inbDoPoof = false)
    {
        bool fromReset = inState == LevelStartState.Reset || inState == LevelStartState.TwoPart;
        bool fromSkip = inState == LevelStartState.Skip || inState == LevelStartState.TwoPart;
        bool fromTwoPart = inState == LevelStartState.TwoPart;
        bool shouldHint = inState == LevelStartState.Default || inState == LevelStartState.Reset;
        bool fromHandhold = inState == LevelStartState.HandHold;

        // Reset session data
        if (!resumingFromSession)
        {
            bool bReset = !fromReset && inState != LevelStartState.HandHold;
            Session.instance.ResetProgress(bReset);
            if (bReset)
                attemptsOffset = 0;
        }

        // Reset hinting state
        if (fromReset || fromSkip || inState == LevelStartState.Default)
        {
            queuedHint = HintingType.None;
            queuedNumpadState = null;
        }

        if (!handHoldCtrl.isActive && Session.instance.currentLevel.startHandhold && !fromTwoPart)
        {
            // To make sure the bubble animates in if we start the problem fresh
            if (inState == LevelStartState.Default || inState == LevelStartState.Skip)
                cabinCounter.pilotBubble.GetComponent<Animator>().SetTrigger("reset");

            StartHint(HintingType.Handhold, inState == LevelStartState.Default || inState == LevelStartState.Skip);
            return;
        }

        isProblemStarting = true;

        EnableNumpad(Session.instance.currentLevel.useNumberPad);

        // show hud buttons for transition
        hud.EnableInput(true);

        // hide any active tuts
        tutorial.HideAll();

        if (!fromReset)
        {
            tutorial.ResetSeen();       // reset seen and action taken rules each level - as requested by design
            tutorial.ResetActionTaken();
        }
       
         if (!fromHandhold)
            handHoldCtrl.SetActive(false);
        else
            handHoldCtrl.ResetRecall();

        onesColumn.SoftReset();
        tensColumn.SoftReset();

        if (!fromTwoPart && !fromHandhold)
            cabinCounter.pilotBubble.GetComponent<Animator>().SetTrigger("reset");
        cabinCounter.OnLevelStart(Session.instance.currentLevel);

        convertNestCtrl.ShowEmptyTenFrame(false);
        if (!fromReset || fromSkip)
            convertNestCtrl.GetComponent<ConvertNestCtrl>().ToggleVisibility(false, false);

        // set the goal
        expression.Reset(!fromTwoPart && !fromHandhold);
        expression.SetGoal(Session.instance.currentLevel);
        addend = Session.instance.currentLevel.deltaValue;
        if (Session.instance.currentLevel.isSubtractionProblem && addend < 0)
            addend = -addend;

        UpdateAddends();

        bool bFillWithBrown = Session.instance.currentLevel.fillWithBrown;

        // fill default/init values for columns
        if (fromTwoPart)
        {
            Assert.True(tensColumn.numCreatures == Session.instance.currentLevel.startingTens
                && onesColumn.numCreatures == Session.instance.currentLevel.startingOnes, "Two part lines up.");
            tensColumn.UpdateCreatureTriggers();
            onesColumn.UpdateCreatureTriggers();
        }
        else
        {
            if (inbDoPoof)
                ResetPoof();
            tensColumn.Add(Session.instance.currentLevel.startingTens, bFillWithBrown);
            onesColumn.Add(Session.instance.currentLevel.startingOnes, bFillWithBrown);
        }

        // split the expression addend (for hinting)
        UpdateAddends();
        // --------------------------------------------------------------------

        // set the subtractors
        if (Session.instance.currentLevel.usesSubZone)
        {
            // show ctrl
            subtractionCtrl.gameObject.SetActive(true);

            //TODO: hide queues?
            queue.EndlessExit();
        }
        else
        {
            // init the queue
            queue.Init();
        }

        // reset launch btn
        SetLaunchBtn(1, (Session.instance.currentLevel.startingOnes < 10));
        SetLaunchBtn(10, (Session.instance.currentLevel.startingTens < 10));

        // init cabin counter
        if (!fromTwoPart)
            cabinCounter.Reset();

        // set ship exterior based on level needs
        //TODO: set single column plane or two column plane
        tensColumn.SetSeatVisibility(Session.instance.currentLevel.tensColumnEnabled);
        onesColumn.SetSeatVisibility(Session.instance.currentLevel.tensColumnEnabled);

        if (!fromReset || fromSkip)
        {
            // for double digit (and single digit problem with carryover)
            if (Session.instance.currentLevel.isDoubleDigitProblem/* || Session.instance.currentLevel.isDoubleDigitAnswer*/)
            {
                Camera.main.GetComponent<Animator>().SetBool("isOnes", false);
                onesColumn.GetComponent<Animator>().SetBool("onesHighlight", false);
                //cabinCounter.counterHolder.GetComponent<Animator>().SetTrigger("tensBubble");
                ship.GetComponent<Animator>().SetTrigger("tensPlane");
                tinyShip.GetComponent<Animator>().SetTrigger("tinyTens");
                expression.GetComponent<Animator>().SetBool("isSingle", false);

                onesColumn.creatureMax = 19;
                onesColumn.seatMax = 19;
                onesColumn.SetSeatVisible(9, true);

                // for double digit subtraction
                if (Session.instance.currentLevel.usesSubZone)
                {
                    subtractionCtrl.GetComponent<Animator>().SetTrigger("subTens");
                }
            }
            // for single digit problem
            else if (Session.instance.currentLevel.isSingleDigitProblem)
            {  // && isSingleDigitAnswer
                Camera.main.GetComponent<Animator>().SetBool("isOnes", true);
                onesColumn.GetComponent<Animator>().SetBool("onesHighlight", true);
                //cabinCounter.counterHolder.GetComponent<Animator>().SetTrigger("onesBubble");
                ship.GetComponent<Animator>().SetTrigger("onesPlane");
                tinyShip.GetComponent<Animator>().SetTrigger("tinyOnes");
                expression.GetComponent<Animator>().SetBool("isSingle", true);

                onesColumn.creatureMax = 9;
                onesColumn.seatMax = 9;
                onesColumn.SetSeatVisible(9, false);

                // for single digit subtraction
                if (Session.instance.currentLevel.usesSubZone)
                {
                    subtractionCtrl.GetComponent<Animator>().SetTrigger("subOnes");
                }
            }
        }

        // reset egg count
        if (!fromReset || fromSkip)
            hud.EggsReset();

        // allow input
        if (handHoldCtrl.isActive)
        {
            ResumeLevel();
        }
        else if (shouldHint)
        {
            if (Session.instance.currentHint == HintingType.AnswerHighlight || Session.instance.currentHint == HintingType.Handhold)
            {
                StartHint(Session.instance.currentHint, true);

                // If we started a handhold, finishing this function makes no sense
                if (Session.instance.currentHint == HintingType.Handhold)
                {
                    return;
                }
            }
            else
            {
                Session.instance.currentHint = HintingType.None;
            }

            ResumeLevel();
        }

        UpdateShiftLeftBtn();
        
        // Upon completion of ShipOn anim, 
        //	trigger newProblem condition in Bubble (fires BubbleOn state)
        if (!fromReset)
        { // don't play animations when resetting (no 'out' anims played)
            onesColumn.ChirpInitialCreatures();
            tensColumn.ChirpInitialCreatures();

            if (!fromTwoPart && !fromHandhold)
                expression.GetComponent<Animator>().SetTrigger("newProblem");
            if (!fromSkip || fromTwoPart)
                pilot.GetComponent<Animator>().SetTrigger("sayProblem");
            // listen for bubble's newProblem animation to end (or creature to spawn)

            this.StopCoroutine(expressionCoroutine);
            expressionCoroutine = this.WaitSecondsThen(Session.instance.currentLevel.isDoubleDigitProblem ? BUBBLE_EXPAND_TIME_DOUBLE : BUBBLE_EXPAND_TIME_SINGLE,
                OnBubbleOn, !fromHandhold);
            if (WillShowTutorials() && !fromHandhold)
                input.DisableAllInput();
        }
        else
        {
            //tutorial.BeginLevelTutorials(input, convertNestCtrl);
            //bool readyForCarryover = (onesColumn.numCreatures > 9 && onesColumn.numCreatures == (Session.instance.currentLevel.startingOnes % 10) + onesColumn.addend);
            //tutorial.CarryOverSpecial(readyForCarryover, handHoldCtrl.isActive, input, convertNestCtrl);

            if (fromTwoPart)
            {
                expression.GetComponent<Animator>().SetBool("showingAnswer", false);
                expression.GetComponent<Animator>().SetTrigger("revealProblem");
            }
        }

        if (!fromTwoPart && inState != LevelStartState.HandHold)
            SetCountingMultiplier(Session.instance.currentLevel.countSpeed);

        if (!handHoldCtrl.isActive && Session.instance.currentLevel.startHandhold && fromTwoPart)
        {
            // To make sure the bubble animates in if we start the problem fresh
            if (inState == LevelStartState.Default || inState == LevelStartState.Skip)
                cabinCounter.pilotBubble.GetComponent<Animator>().SetTrigger("reset");

            StartHint(HintingType.Handhold, inState == LevelStartState.Default || inState == LevelStartState.Skip || inState == LevelStartState.TwoPart);
            return;
        }

        isProblemStarting = false;
        resumingFromSession = false;
    }

    void OnBubbleOn(bool inbStartTutorials)
    {
        if (inbStartTutorials)
        {
            //tutorial.BeginLevelTutorials(input, convertNestCtrl);
            //bool readyForCarryover = (onesColumn.numCreatures > 9 && onesColumn.numCreatures == (Session.instance.currentLevel.startingOnes % 10) + onesColumn.addend);
            //tutorial.CarryOverSpecial(readyForCarryover, handHoldCtrl.isActive, input, convertNestCtrl);
        }

        onesColumn.StopChirping();
        tensColumn.StopChirping();

        expressionCoroutine.Clear();
    }

    bool WillShowTutorials()
    {
        //if (tutorial.WillShow((Session.instance.currentLevel.tensColumnEnabled) ? "showHintDragAddOne" : "showHintDragAdd"))
        //    return true;
        //bool readyForCarryover = (onesColumn.numCreatures > 9 && onesColumn.numCreatures == (Session.instance.currentLevel.startingOnes % 10) + onesColumn.addend);
        //return tutorial.WillShowLevelTutorials(readyForCarryover);
        return false;
    }

    public void PauseLevel()
    {
        //TODO: block all input
        tutorial.Pause();

        // stop spawning
        queue.isRunning = false;
    }

    public void ResumeLevel()
    {
        //TODO: remove input blocker
        tutorial.Resume();

        // reset expression column alphas (they update during hinting)
        if (!handHoldCtrl.isActive)
            expression.Reset(false);

        // enable spawning
        queue.isRunning = true;

        //hud.launchBtn.GetComponent<Button>().interactable = true;

        // Since handhold hint input blocking occurs before this
        // we need to make sure we don't override any of its settings
        if (!handHoldCtrl.isActive && !WillShowTutorials())
            input.EnableAllInput(true);
    }
    //}


    //{ Actions
    void EnableShiftLeftBtn(bool enabled)
    {
        //shiftLeftBtn.GetComponent<SimpleBtn>().OnMouseEnabled(enabled);
        convertNestCtrl.GetComponent<ConvertNestCtrl>().ToggleVisibility(enabled, handHoldCtrl.isActive);
    }

    void UpdateShiftLeftBtn()
    {
        EnableShiftLeftBtn(Session.instance.currentLevel.tensColumnEnabled &&
                           onesColumn.numCreatures > 9 &&
                           tensColumn.numCreatures < tensColumn.creatureMax &&
                           (!onesColumn.isConverting && !tensColumn.isConverting)
                           && !(_handHoldCtrl.isActive && Session.instance.currentLevel.isSubtractionProblem));
    }

    void EnableShiftRightBtn(bool enabled)
    {
        //shiftRightBtn.GetComponent<SimpleBtn>().OnMouseEnabled(enabled);
    }

    void EnableNumpad(bool enabled)
    {
        Session.instance.currentLevel.useNumberPad = enabled;
        queuedNumpadState = null;

        if (enabled)
            hud.UseNumberPad(Session.instance.currentLevel.isDoubleDigitProblem);
        else
            hud.UseSubmitButton();
    }

    void Column_onCreatureCountUpdated(PlaceValueCtrl ctrl)
    {
        // update launch btn
        //if (ctrl != null)
        //	SetLaunchBtn(ctrl.value, (ctrl.numCreatures < 10));
        SetLaunchBtn(onesColumn.value, (onesColumn.numCreatures < 10));
        SetLaunchBtn(tensColumn.value, (tensColumn.numCreatures < 10));
        //Debug.Log("CREATURE COUNT UPDATED " + (ctrl != null).ToString());
        //if (ctrl != null)
        //    Debug.Log("ctrl.value = " + ctrl.value.ToStringLookup());
        //shift left
        if (!isProblemStarting)
        {
            UpdateShiftLeftBtn();
        }

        if (onesColumn.numCreatures < 10)
            m_OverflowSubmitCount = 0;

        // shift right
        EnableShiftRightBtn(Session.instance.currentLevel.tensColumnEnabled &&
                            tensColumn.numCreatures > 0 &&
                            onesColumn.numCreatures < 10 &&
                            (!onesColumn.isConverting && !tensColumn.isConverting));

        if (numberInput.Open)
            numberInput.WaitOneFrameThen(numberInput.UpdateSeatHighlights);


        //TODO: fix tutorial stuff
        if (launchValid)
        {
            int value = (onesColumn.numCreatures * onesColumn.value) +
                (tensColumn.numCreatures * tensColumn.value);
            if (launchValid && value == Session.instance.currentLevel.value)
            {
                bool hintShown = tutorial.Show("showHintGo", false, hud.launchBtn.position, hud.launchBtn);
                if (hintShown)
                {
                    input.EnableAllInput(false, GameplayInput.SUBMIT, GameplayInput.SUBMIT_NUMPAD, GameplayInput.NUMBER_PAD_ARROWS, GameplayInput.TOGGLE_NUMBER_PAD);
                    input.EnableCountingAndPause(true);
                    queue.EndlessExit(true, true);
                }
            }
            else
            {
                bool hintHidden = tutorial.Hide("showHintGo", hud.launchBtn) && !handHoldCtrl.isActive;
                if (hintHidden)
                    input.EnableAllInput(true);
            }
        }
        else
        {
            bool hintHidden = tutorial.Hide("showHintGo", hud.launchBtn) && !handHoldCtrl.isActive;
            if (hintHidden)
                input.EnableAllInput(true);
        }

        if (!handHoldCtrl.isActive)
        {
            //bool readyForCarryover = (onesColumn.numCreatures > 9 && onesColumn.numCreatures == (Session.instance.currentLevel.startingOnes % 10) + onesColumn.addend);
            //tutorial.CarryOverSpecial(readyForCarryover, handHoldCtrl.isActive, input, convertNestCtrl);
            /*if (onesColumn.numCreatures > 9 &&
                onesColumn.numCreatures == Session.instance.currentLevel.onesCount + onesColumn.addend) {
                bool hintShown = tutorial.Show("showHintDragCarryover", false, Vector3.zero);
                if (hintShown) {
                    input.EnableAllInput(false, GameplayInput.CONVERT_TO_TENS);
                    convertNestCtrl.GetComponent<ConvertNestCtrl>().ToggleVisibility(true, handHoldCtrl.isActive);
                }
            } else {
                bool beginRecall = tutorial.CarryOverSpecial(input);
                //if (beginRecall)
                //    handHoldCtrl.BeginRecall("showHintDragCarryover");
            }*/
        }


        if (!isProblemStarting && _handHoldCtrl.isActive && ctrl != null)
        {
            if (ctrl != null && !(ctrl == tensColumn && onesColumn.allowDragConvert) && !(ctrl.isCarryoverHinting) && _handHoldCtrl.IsFocusedOn(ctrl))
                Column_onHintCount(ctrl, Session.instance.currentLevel.isTargetNumber ? "Top" : "Bottom");
            _handHoldCtrl.CheckSolution();
            _handHoldCtrl.CheckCarryover(ctrl);
        }
    }

    void Column_onShift(PlaceValueCtrl column)
    {
        onesColumn.Deselect();
        tensColumn.Deselect();

        if (column.value == 1)
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.carryover");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.carryover"));
            EnlearnInstance.I.LogActions(EnlearnInstance.Action.ConvertToTens);

            //SHIFT LEFT        //if (onesColumn.numCreatures < 10 || (tensColumn.numCreatures + 1) > tensColumn.creatureMax)
            bool hintHidden = tutorial.Hide("showHintDragCarryover");
            tutorial.ActionTaken("showHintDragCarryover");
            //if (hintHidden)
            //    input.EnableAllInput(true);

            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenConvertToTens);
            EnableShiftLeftBtn(false);

            //tensColumn <--- onesColumn (10 to 1)
            string[] colors = onesColumn.GetFirstTenColors();
            onesColumn.VacuumUp(10, tensColumn.firstEmptyPosition); // remove ten from 1s column
            string resultingColor = tensColumn.SpitOut(1, colors);  // add one to 10s column

            if (resultingColor == CreatureCtrl.COLOR_GOLD)
                ship.GetComponent<Animator>().SetTrigger("showTenNum");

            // add explosion effects
            _poofHolder1.GetComponent<Animator>().enabled = true;
            _poofHolder1.GetComponent<Animator>().SetTrigger("explode");

            if (_handHoldCtrl.isActive)
            {
                if (!Session.instance.currentLevel.isTargetNumber && Session.instance.currentLevel.showGoldCarryover)
                {
                    expression.GetComponent<Animator>().SetBool("hintShowCarry", true);
                }
            }
            _handHoldCtrl.CheckAdditionalCarryover();

            m_OverflowSubmitCount = 0;
        }
        else
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.borrow");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.borrow"));
            EnlearnInstance.I.LogActions(EnlearnInstance.Action.ConvertToOnes);

            //SHIFT RIGHT       //if (tensColumn.numCreatures == 0 || (onesColumn.numCreatures + 10) > onesColumn.creatureMax)
            bool hintHidden = tutorial.Hide("showHintDragBorrow");
            tutorial.ActionTaken("showHintDragBorrow");
            if (hintHidden)
                input.EnableAllInput(true);

            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenConvertToOnes);
            EnableShiftLeftBtn(false);

            // tensColumn ---> onesColumn (1 to 10)
            tensColumn.Explode(10); // add ten converters in 10s column, move to 1s column

            // add explosion effects
            Poof();


            if (_handHoldCtrl.isActive)
            {
                tutorial.Hide("showHintDragBorrow");
                if (!Session.instance.currentLevel.isTargetNumber)
                {
                    expression.GetComponent<Animator>().SetBool("hintShowBorrow", true);
                    expression.GetComponent<Animator>().SetBool("hintFadeTens", true);
                    this.WaitSecondsThen(1.5f, () => { expression.GetComponent<Animator>().SetBool("hintFadeTensExtra", true); });
                    expression.SetBorrowValue(Session.instance.currentLevel);
                }

                onesColumn.StartHintCounting(EnableOnesDragOut);
                tensColumn.ClearHints();
                tutorial.HideAll();
                input.EnableAllInput(false);
            }
        }
    }

    public void Poof()
    {
        // add explosion effects
        _poofHolder1.GetComponent<Animator>().enabled = true;
        _poofHolder1.GetComponent<Animator>().SetTrigger("explode");
        _poofHolder2.GetComponent<Animator>().enabled = true;
        _poofHolder2.GetComponent<Animator>().SetTrigger("explode");
        _poofHolder3.GetComponent<Animator>().enabled = true;
        _poofHolder3.GetComponent<Animator>().SetTrigger("explode");
        //_poofHolder4.GetComponent<Animator>().SetTrigger("explode");
    }

    public void ResetPoof()
    {
        using (PooledList<Transform> poofs = PooledList<Transform>.Create())
        {
            if (Session.instance.currentLevel.isDoubleDigitProblem)
            {
                poofs.Add(_poofHolder1);
                poofs.Add(_poofHolder2);
                poofs.Add(_poofHolder3);
                poofs.Add(_poofHolder4);
                poofs.Add(_poofHolder5);
                poofs.Add(_poofHolder6);
            }
            else
            {
                poofs.Add(_poofHolder1);
                poofs.Add(_poofHolder4);
            }

            foreach (Transform poof in poofs)
            {
                poof.GetComponent<Animator>().enabled = true;
                poof.GetComponent<Animator>().SetTrigger("explode");
            }

            // TODO: Replace with better "poof" sound
            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenConvertToOnes);
        }
    }

    public int CalculateCurrentValue(bool inbForceChickens = false)
    {
        if (!inbForceChickens && Session.instance.currentLevel.useNumberPad)
            return numberInput.CalculateTotalValue();
        else
        {
            return (onesColumn.numCreatures * onesColumn.value) +
                (tensColumn.numCreatures * tensColumn.value);
        }
    }

    void TensColumn_onConverted(PlaceValueCtrl ctrl)
    {
        //TODO: refresh creature positioning, creature triggers
        Column_onCreatureCountUpdated(null);

        onesColumn.RefreshCreaturePositions(false, true);
        onesColumn.UpdateCreatureTriggers();

        tensColumn.RefreshCreaturePositions(false, true);
        tensColumn.UpdateCreatureTriggers();
    }

    public void ForceColumnUpdate()
    {
        Column_onCreatureCountUpdated(null);
        onesColumn.RefreshCreaturePositions();
        onesColumn.UpdateCreatureTriggers();
    }
    //}


    //{ Animation Events
    public void ShipToOn()
    {

        // Upon completion of CameraSceneIn anim, 
        // for single digit problem
        if (Session.instance.currentLevel.isSingleDigitProblem)
        {
            ship.GetComponent<Animator>().SetTrigger("onesPlane");
        }
        // for double digit (and single digit problem with carryover)
        else if (Session.instance.currentLevel.isDoubleDigitProblem)
        {
            ship.GetComponent<Animator>().SetTrigger("tensPlane");
        }
        //	trigger newProblem condition in Main Camera (fires CameraSceneIn state)
        Camera.main.GetComponent<Animator>().SetTrigger("newProblem");
        //	trigger newProblem condition in Ship (fires ShipOn state)
        ship.GetComponent<Animator>().SetTrigger("newProblem");

        if (Session.instance.currentLevel.usesSubZone && !subtractionCtrl.gameObject.activeSelf)
        {
            subtractionCtrl.gameObject.SetActive(true);
            subtractionCtrl.GetComponent<Animator>().SetTrigger(Session.instance.currentLevel.isDoubleDigitProblem ? "subTens" : "subOnes");
        }

        //SoundManager.instance.PlayOneShot(SoundManager.instance.planeEnter, 0.5f);
    }

    public void OnShipOff()
    {
        ship.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.ShipOff"), OnShipOff);

        //isTransitioning = false;

        //TODO: where should this live? within animation chain? level init?
        // clear the current level (cam has panned away)
        //this.WaitSecondsThen(1.0f, () => { ClearLevel(true); } );

        // Upon completion of ShipOff anim, 
        //  trigger shipOff condition in TinyShip (fires TinyShipOut state)
        tinyShip.GetComponent<Animator>().SetTrigger("shipOff");
        //tinyShip.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.TinyShipOut"), OnTinyShipOff);
    }

    public void ProcessEnlearnResponse(JSONNode inResponseData)
    {
        if (inResponseData == null)
            return;

        Logger.Log(inResponseData.ToString());

        if (inResponseData["nextHint"] != null)
        {
            bool bStartNow = inResponseData["startNow"].AsBool;

            string nextHint = inResponseData["nextHint"].Value;
            if (nextHint == "stopHinting")
            {
                Session.instance.overrideHints = false;
                queuedHint = HintingType.None;
            }
            else if (nextHint == "highlightAnswer")
            {
                Session.instance.overrideHints = false;
                queuedHint = HintingType.AnswerHighlight;
            }
            else if (nextHint == "handHold")
            {
                Session.instance.overrideHints = false;
                queuedHint = HintingType.Handhold;
            }
            else if (nextHint == "UseFilamentHints")
            {
                Session.instance.overrideHints = true;
                queuedHint = HintingType.None;
            }
            else if (nextHint == "none")
            {
                Session.instance.overrideHints = false;
                queuedHint = HintingType.None;
            }

            if (bStartNow && queuedHint != HintingType.None)
            {
                StartHint(queuedHint, false);
            }
        }

        if (inResponseData["countSpeed"] != null)
        {
            float countSpeed = inResponseData["countSpeed"].AsFloat;
            SetCountingMultiplier(countSpeed);
        }

        if (inResponseData["useNumberPad"] != null)
        {
            queuedNumpadState = inResponseData["useNumberPad"].AsBool;
        }
    }

    public void SetCountingMultiplier(float inTime)
    {
        onesColumn.SetCountingMultiplier(inTime);
        tensColumn.SetCountingMultiplier(inTime);
        cabinCounter.SetCountMultiplier(inTime);
    }

    void Hud_onLevelCompleteCtrlOn()
    {
        transitionCtrl.StartTransition();
        UpdateBackground(Session.instance.numLevelsCompleted + 1, false);
    }

    void Hud_onLevelCompleteCtrlOff()
    {
        m_LoadLevelRoutine.Clear();
        m_LoadLevelRoutine = this.SmartCoroutine(OnLevelCompleteOffRoutine());

        transitionCtrl.ForceFinish();
        this.WaitSecondsThen(3.0f, transitionCtrl.SetVisible, false);
    }

    private IEnumerator OnLevelCompleteOffRoutine()
    {
        ClearLevel(false);
        yield return NextLevel(true, false);

        if (Session.instance.currentLevelIndex == 0)
        {
            Genie.I.SyncEvents();

            // if we reset the levels list, display the splash screen
            mSplashScreen = (GameObject)Instantiate(splashScreenPrefab,
                                                    Vector3.zero,
                                                    Quaternion.identity);
            mSplashScreen.transform.SetParent(hud.transform, false);
            mSplashScreen.GetComponent<SplashScreenCtrl>().onCtrlOff = GameResetSplash_onCtrlOff;
        }
        else
        {
            UpdateBackground(Session.instance.numLevelsCompleted, true, true);
            // Upon completion of PolaroidToOff anim, 
            ShipToOn();
        }
    }
    
    void GameResetSplash_onCtrlOff()
    {
        // destroy splash control
        mSplashScreen.GetComponent<SplashScreenCtrl>().onCtrlOff = null;
        Destroy(mSplashScreen);
        mSplashScreen = null;

        SceneMgr.instance.LoadScene(SceneMgr.MAIN_MENU);
        //ShipToOn();
    }
    //}


    //{ Counting
    void TensColumn_onCount(PlaceValueCtrl ctrl)
    {
        cabinCounter.IncrementTensCount(tensColumn.numCreatures > 0);
    }

    void TensColumn_onFinishedCounting(PlaceValueCtrl ctrl)
    {
        //cabinCounter.kiwiHolder.GetComponent<Animator>().SetTrigger("flipRight");
        // move counter to pilot check
        cabinCounter.TransitionToTotal();
    }

    void OnesColumn_onCount(PlaceValueCtrl ctrl)
    {
        cabinCounter.IncrementOnesCount(onesColumn.numCreatures > 0);
    }

    void OnesColumn_onFinishedCounting(PlaceValueCtrl ctrl)
    {
        if (Session.instance.currentLevel.tensColumnEnabled)
        {
            cabinCounter.ToTensColumn();
            tensColumn.StartCounting();
        }
        else
        {
            // move counter to pilot check
            cabinCounter.TransitionToTotal();
        }
    }

    void CabinCounter_onEvaluate()
    {
        onesColumn.StopChirping();
        tensColumn.StopChirping();

        SoundManager.instance.DuckMusic();

        // evaluate problem space
        int value = CalculateCurrentValue();

        if (value == Session.instance.currentLevel.value && launchValid)
        {
            cabinCounter.Correct();
            //ForceColumnsToAnswer();

            if (!Session.instance.currentLevel.twoPartProblem)
                expression.GetComponent<Animator>().SetTrigger("correct");
            pilot.GetComponent<Animator>().SetBool("isCorrect", true);
            SoundManager.instance.PlayOneShot(SoundManager.instance.correctAnswer, 0.5f);

        }
        else
        {
            cabinCounter.Incorrect();

            //expression.GetComponent<Animator>().SetTrigger("incorrect");
            pilot.GetComponent<Animator>().SetTrigger("isIncorrect");
            pilot.GetComponent<Animator>().SetBool("isListening", false);
            SoundManager.instance.PlayOneShot(SoundManager.instance.incorrectAnswer);

            this.WaitSecondsThen(0.10f, hud.EggsRemove);
        }

        //wait 1 second, 
        //trigger attendantToOff in "kiwiHolder" and 
        //	counterToOff in "CounterHolder"
        if (evalTimer == null)
            evalTimer = new CooldownTimer(1f);
        evalTimer.Reset();
    }

    void Evaluate()
    {
        // evaluate problem space
        int value = CalculateCurrentValue();
        if (value == Session.instance.currentLevel.value && launchValid)
        {
            // correct!
            
            ForceColumnsToAnswer();

            // show place value feedback
            if (!Session.instance.currentLevel.twoPartProblem)
                this.WaitOneFrameThen(ShowColumnFeedback);
            
            // send cars off screen
            if (Session.instance.currentLevel.usesSubZone)
            {
                subtractionCtrl.ShowCorrectFeedback();
            }

            if (Session.instance.currentLevel.twoPartProblem)
            {
                m_LoadLevelRoutine.Clear();
                m_LoadLevelRoutine = this.SmartCoroutine(TwoPartProblemAdvanceRoutine());
            }
            else
            {
                isTransitioning = true;

                // reset expression hinting
                expression.Reset(false);

                // give egg(s)
                hud.EggsWin();

                // Trigger correct condition simultaneously in Ship (fires ShipOff state), 
                //	Bubble (fires BubbleOff state), Main Camera (fires CameraSceneOut state)
                Camera.main.GetComponent<Animator>().SetTrigger("correct");
                ship.GetComponent<Animator>().SetTrigger("correct");
                ship.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.ShipOff"), OnShipOff);

                onesColumn.SetSeatbeltAll(true);
                tensColumn.SetSeatbeltAll(true);

                if (WillTransition)
                {
                    int index = (int)((Session.instance.numLevelsCompleted + 1) / levelsTilSceneChange) % environments.Length;
                    int oldIndex = (int)((Session.instance.numLevelsCompleted) / levelsTilSceneChange) % environments.Length;
                    transitionCtrl.PrepTransition(environments[oldIndex].bgColor, environments[index].bgColor);
                }
            }
        }
        else
        {
            // wrong!
            onesColumn.Deselect();
            tensColumn.Deselect();

            // reset column highlights to default sprite and bools, before we trigger 'incorrect'
            onesColumn.ResetHighlight();
            tensColumn.ResetHighlight();

            if (Session.instance.currentLevel.useNumberPad)
            {
                this.SmartCoroutine(NegativeFeedbackNoColumnFlash());
            }
            else
            {
                if ((value % 10) != Session.instance.currentLevel.valueOnes)
                {
                    onesColumn.ShowIncorrectFeedback();
                    onesColumn.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.ColumnIncorrect"), Column_onNegativeFeedbackGiven);
                }

                if (((int)(value / 10)) != Session.instance.currentLevel.valueTens)
                {
                    tensColumn.ShowIncorrectFeedback();
                    tensColumn.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.ColumnIncorrect"), Column_onNegativeFeedbackGiven);
                }
            }

            // show pilot, ship, and column incorrect anims
            //ship.GetComponent<Animator>().SetTrigger("incorrect");
        }
    }

    IEnumerator TwoPartProblemAdvanceRoutine()
    {
        PauseLevel();
        ClearLevel(true, true);
        yield return NextLevel(false, false);
        StartLevel(LevelStartState.TwoPart);

        onesColumn.ChirpInitialCreatures();
        tensColumn.ChirpInitialCreatures();

        cabinCounter.ToOff();

        if (Session.instance.currentLevel.seatbelts)
            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSeatbeltBuckle);

        //expression.GetComponent<Animator>().SetTrigger("newProblem");
        pilot.GetComponent<Animator>().SetTrigger("sayProblem");
        // listen for bubble's newProblem animation to end (or creature to spawn)
        if (expressionCoroutine != null)
            this.StopCoroutine(expressionCoroutine);
        expressionCoroutine = this.WaitSecondsThen(Session.instance.currentLevel.isDoubleDigitProblem ? BUBBLE_EXPAND_TIME_DOUBLE : BUBBLE_EXPAND_TIME_SINGLE,
            OnBubbleOn, false);
    }

    IEnumerator NegativeFeedbackNoColumnFlash()
    {
        yield return 0.25f;
        if (!numberInput.gameObject.activeSelf)
            yield break;
        bool bShow = !(queuedNumpadState.HasValue && !queuedNumpadState.Value) && (queuedHint != HintingType.Handhold);
        if (bShow)
        {
            numberInput.Show(true);
            yield return NoColumnFlashWait;
            numberInput.ShowIncorrect();
            yield return 0.5f;
        }
        Column_onNegativeFeedbackGiven();
    }

    void ShowColumnFeedback()
    {
        onesColumn.ShowCorrectFeedback();
        tensColumn.ShowCorrectFeedback();
    }

    void Column_onNegativeFeedbackGiven()
    {
        Logger.Log("Column_onNegativeFeedbackGiven()");
        onesColumn.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.ColumnIncorrect"), Column_onNegativeFeedbackGiven);
        tensColumn.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.ColumnIncorrect"), Column_onNegativeFeedbackGiven);

        // hide attendant
        cabinCounter.ToOff();
        // remove an egg
        
        // reset expression hinting
        if (Session.instance.currentLevel.usesQueue)
        {
            queue.EndlessEnter();
        }

        // don't increment attempts and dont show hinting if overflow was the issue
        if (!launchValid)
        {
            bool bProceed = true;

            if (Session.instance.currentLevel.fromEnlearn)
            {
                UpdateNumpad();
                bProceed = UpdateHints() != HintingType.Handhold;
            }

            if (bProceed)
            {
                ResumeLevel();
                EnableShiftLeftBtn(Session.instance.currentLevel.tensColumnEnabled &&
                       onesColumn.numCreatures > 9 &&
                       tensColumn.numCreatures < tensColumn.creatureMax &&
                       (!onesColumn.isConverting && !tensColumn.isConverting));
            }
        }
        else
        {
            m_OverflowSubmitCount = 0;
            // hint feedback
            Session.instance.numAttempts++;
            UpdateNumpad();
            if (UpdateHints() != HintingType.Handhold)
            {
                ResumeLevel();
            }
        }
    }

    void StartHint(HintingType inType, bool inbLevelStart)
    {
        Session.instance.currentHint = inType;

        if (inType == HintingType.Handhold)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "hint.handHold"));
            EnableNumpad(false);
            _handHoldCtrl.Begin(inbLevelStart);

            onesColumn.ClearHints();
            tensColumn.ClearHints();

            return;
        }
        
        _handHoldCtrl.SetActive(false);

        if (inType == HintingType.AnswerHighlight)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "hint.highlightAnswer"));
            // show player the correct placeValue configuration
            onesColumn.ShowAnswerHighlight(Session.instance.currentLevel.valueOnes);
            tensColumn.ShowAnswerHighlight(Session.instance.currentLevel.valueTens);
            
            return;
        }

        onesColumn.ClearHints();
        tensColumn.ClearHints();

        // We're cutting out the TooManyTooFew red arrows hint.
        /*
        if (inType == HintingType.TooManyTooFew)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "hint.tooManyTooFew"));
            // show player if they added too many or too few
            int desiredOnes = Session.instance.currentLevel.valueOnes;
            int desiredTens = Session.instance.currentLevel.valueTens;

            bool bShowTens = tensColumn.numCreatures != Session.instance.currentLevel.valueTens;
            bool bShowOnes = onesColumn.numCreatures != Session.instance.currentLevel.valueOnes;
            bool bDisableOnes = (Session.instance.currentLevel.isAdditionProblem && onesColumn.numCreatures < Session.instance.currentLevel.valueOnes && !Session.instance.currentLevel.onesQueueEnabled);
            bool bDisableTens = (Session.instance.currentLevel.isAdditionProblem && tensColumn.numCreatures < Session.instance.currentLevel.valueTens && !Session.instance.currentLevel.tensQueueEnabled);
            
            if (bShowOnes && bDisableOnes)
            {
                bShowTens = true;
                bShowOnes = false;
                desiredTens += 1;
            }

            if (bShowTens && bDisableTens)
            {
                bShowTens = false;
                bShowOnes = true;
                desiredOnes += 10;
            }

            if (bShowOnes)
            {
                onesColumn.GetComponent<Animator>().SetTrigger("incorrect");
                onesColumn.ShowTooManyTooFewHighlight(desiredOnes);
            }
            if (bShowTens)
            {
                tensColumn.GetComponent<Animator>().SetTrigger("incorrect");
                tensColumn.ShowTooManyTooFewHighlight(desiredTens);
            }

            return;
        }*/

        onesColumn.ResetHighlight();
        tensColumn.ResetHighlight();

        tutorial.HideAll();
        tutorial.ResetActionTaken();
        tutorial.ResetSeen();
        input.EnableAllInput(true);
    }

    void UpdateNumpad()
    {
        if (queuedNumpadState != null)
        {
            EnableNumpad(queuedNumpadState.Value);
            queuedNumpadState = null;
        }
    }

    HintingType UpdateHints()
    {
        HintingType nextHint = HintingType.None;
        if (Session.instance.currentLevel.fromEnlearn && !Session.instance.overrideHints)
        {
            nextHint = queuedHint;
            queuedHint = HintingType.None;
        }
        else
        {
            if (Session.instance.numAttempts - attemptsOffset >= AttemptsUntilHandHold)
                nextHint = HintingType.Handhold;
            else if (Session.instance.numAttempts - attemptsOffset == AttemptsUntilAnswerHighlight)
                nextHint = HintingType.AnswerHighlight;
        }

        if (nextHint != HintingType.None)
            StartHint(nextHint, false);

        switch(nextHint)
        {
            case HintingType.Handhold:
                {
                    Session.instance.numAttempts = AttemptsUntilHandHold;  // mad hacks
                    // wait a bit and resume play
                    if (resumePlayTimer == null)
                        resumePlayTimer = new CooldownTimer(1f);
                    resumePlayTimer.Reset();
                    break;
                }
            case HintingType.AnswerHighlight:
                {
                    // wait a bit and resume play
                    if (resumePlayTimer == null)
                        resumePlayTimer = new CooldownTimer(1f);
                    resumePlayTimer.Reset();
                    break;
                }
            /*
            case HintingType.TooManyTooFew:
                {
                    // wait a bit and resume play
                    if (resumePlayTimer == null)
                        resumePlayTimer = new CooldownTimer(2f);
                    resumePlayTimer.Reset();
                    break;
                }*/
        }

        return nextHint;
    }

    //}


    //{ Input
    void Hud_onLaunched()
    {
        if (isTransitioning)
            return;

        resumePlayTimer = null;

        onesColumn.GetComponent<RealtimeCountCtrl>().CancelCount();
        tensColumn.GetComponent<RealtimeCountCtrl>().CancelCount();

        onesColumn.Deselect();
        tensColumn.Deselect();

        //tutorial hinting
        int value = CalculateCurrentValue();

        //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.expression." + Session.instance.currentLevel.expression);
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.expression." + Session.instance.currentLevel.expression));
        //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.submit." + value.ToString());
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.submit." + value.ToString()));

        EnlearnInstance.I.LogActions(EnlearnInstance.Action.Submit,
                                     "expression", Session.instance.currentLevel.expression,
                                     "onesCount", onesColumn.numCreatures.ToStringLookup(),
                                     "tensCount", tensColumn.numCreatures.ToStringLookup(),
                                     "submittedValue", value.ToString(),
                                     "validGrouping", launchValid.ToString(),
                                     "correct", (value == Session.instance.currentLevel.value).ToString());
        
        if (value == Session.instance.currentLevel.value && launchValid)
        {
            // correct!
            bool hintHidden = tutorial.Hide("showHintGo", hud.launchBtn);
            tutorial.ActionTaken("showHintGo");
            if (hintHidden)
                input.EnableAllInput(true);

            tutorial.HideAll();

            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.feedback.correct");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.feedback.correct"));
            //Genie.I.LogEvent(new OE_LEARN());
            Genie.I.LogEvent(new OE_ASSESS(Session.instance.currentLevelIndex, Session.instance.currentLevel, value, Session.instance.numAttempts + 1, Session.instance.timeTaken, true));

            attemptsOffset = Session.instance.numAttempts;
        }
        else
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.feedback.incorrect");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.feedback.incorrect"));
            Genie.I.LogEvent(new OE_ASSESS(Session.instance.currentLevelIndex, Session.instance.currentLevel, value, Session.instance.numAttempts + 1, Session.instance.timeTaken, false));
        }

        hud.launchBtn.GetComponent<Button>().interactable = false;
        input.DisableAllInput();

        numberInput.Show(false);
        PauseLevel();

        if (launchValid)
        {
            queue.EndlessExit();

            // setup counter feedback and kickoff column counting
            cabinCounter.Show(value);

            if (Session.instance.currentLevel.useNumberPad)
            {
                cabinCounter.ForceAnswerText(value);
                cabinCounter.ToNumpadCount();
                this.WaitSecondsThen(1.0f, cabinCounter.TransitionToTotal);
            }
            else
            {
                cabinCounter.ToOnesColumn();
                onesColumn.StartCounting();
            }

        }
        else
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.feedback.overflow");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "level.feedback.overflow"));

            //TODO: new overflow feedback
            EnableShiftLeftBtn(false);

            // handle overflow feedback
            if (!onesValid)
            {
                onesColumn.ResetHighlight();
                onesColumn.ShowSeatOverflow(Session.instance.currentLevel.valueOnes);
            }
            if (!tensValid)
            {
                tensColumn.ResetHighlight();
                tensColumn.ShowSeatOverflow(Session.instance.currentLevel.valueTens);
            }

            if (onesColumn.numCreatures != Session.instance.currentLevel.valueOnes)
            {
                onesColumn.GetComponent<Animator>().SetTrigger("incorrect");
                onesColumn.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.ColumnIncorrect"), Column_onOverflowGiven);
            }

            // remove an egg
            hud.EggsRemove();
        }
    }

    void Hud_onNumberPad()
    {
        // The button takes care of itself now.
        //Animator numberAnimator = hud.numberPadPanel.GetComponent<Animator>();
        //numberAnimator.SetBool("isOpen", !numberAnimator.GetBool("isOpen"));
    }

    void Column_onOverflowGiven()
    {
        onesColumn.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.ColumnIncorrect"), Column_onOverflowGiven);

        ResumeLevel();
        EnableShiftLeftBtn(Session.instance.currentLevel.tensColumnEnabled &&
                       onesColumn.numCreatures > 9 &&
                       tensColumn.numCreatures < tensColumn.creatureMax &&
                       (!onesColumn.isConverting && !tensColumn.isConverting));

        if (++m_OverflowSubmitCount >= 2)
        {
            if (tensColumn.numCreatures < Session.instance.currentLevel.valueTens)
                tutorial.Show(TutorialCtrl.HINT_CARRY, true, Vector3.zero);
            else
                tutorial.Show(TutorialCtrl.HINT_SUB_ONE, true, Vector3.zero);
        }

    }

    void Hud_onPaused()
    {
        input.FreezeAllInputs();
        pauseMenu.gameObject.SetActive(true);
        pauseMenu.GetComponent<Animator>().SetTrigger("showPopup");
        pauseMenu.OnClosed = Hud_onResumed;
        Session.instance.MarkPauseStart();
        // kickoff level reset
    }

    void Hud_onResumed()
    {
        input.UnfreezeAllInputs();
        Session.instance.MarkPauseEnd();
    }

    public void SetLaunchBtn(int placeValue, bool enabled)
    {
        if (placeValue == 1)
            onesValid = enabled;
        if (placeValue == 10)
            tensValid = enabled;

        pilot.GetComponent<Animator>().SetBool("isUnhappy", (!onesValid || !tensValid));
        //launchBtn.GetComponent<Button>().interactable = onesValid && tensValid;
        //ship.GetComponent<Animator>().SetBool("hasOverflow", (!onesValid || !tensValid));
    }
    //}

    //{ Hinting
    public void UpdateAddends()
    {
        bool bCanAddTens = Session.instance.currentLevel.tensQueueEnabled;
        bool bIsSubtraction = Session.instance.currentLevel.isSubtractionProblem;

        valueTensAddend = (int)(Session.instance.currentLevel.value / 10);
        valueOnesAddend = (Session.instance.currentLevel.value % 10);

        int valueRemaining = Session.instance.currentLevel.value - (onesColumn.numCreatures + tensColumn.numCreatures * 10);

        if (bIsSubtraction)
        {
            onesColumn.targetNumCreatures = valueOnesAddend;
            tensColumn.targetNumCreatures = valueTensAddend;
        }
        else
        {
            bool bRequiresMoreCarryover = Session.instance.currentLevel.requiresMultipleCarryover
                && (tensColumn.numCreatures < valueTensAddend);

            if (!bCanAddTens)
                tensColumn.targetNumCreatures = tensColumn.numCreatures;
            else
                tensColumn.targetNumCreatures = valueTensAddend;

            if (bRequiresMoreCarryover)
            {
                if (onesColumn.numCreatures > 9)
                    onesColumn.targetNumCreatures = onesColumn.numCreatures;
                else if (Session.instance.currentLevel.isTargetNumber)
                {
                    if (valueRemaining > 10)
                        onesColumn.targetNumCreatures = 10;
                    else
                        onesColumn.targetNumCreatures = valueRemaining;
                }
                else
                {
                    if ((valueRemaining % 10) != 0)
                        onesColumn.targetNumCreatures = onesColumn.numCreatures + (valueRemaining % 10);
                    else if (valueRemaining > 10)
                        onesColumn.targetNumCreatures = onesColumn.numCreatures + 10;
                    else
                        onesColumn.targetNumCreatures = onesColumn.numCreatures + valueRemaining;
                }
            }
            else
            {
                if (onesColumn.numCreatures > 9)
                    onesColumn.targetNumCreatures = onesColumn.numCreatures;
                else if (valueOnesAddend == 0 && onesColumn.numCreatures > 0 || (Session.instance.currentLevel.isTargetNumber && onesColumn.numCreatures > valueOnesAddend))
                    onesColumn.targetNumCreatures = 10;
                else if (valueOnesAddend > onesColumn.numCreatures)
                    onesColumn.targetNumCreatures = valueOnesAddend;
                else
                    onesColumn.targetNumCreatures = onesColumn.numCreatures + valueRemaining % 10;
            }
        }

        int maxOnes = (Session.instance.currentLevel.isDoubleDigitProblem ? 19 : 9);
        Assert.True(onesColumn.targetNumCreatures <= maxOnes, "Target num on ones column is valid.", "Invalid target on ones column: {0} ({1} max)", onesColumn.targetNumCreatures, maxOnes);
        Assert.True(tensColumn.targetNumCreatures <= 9, "Target num on tens column is valid.", "Invalid target on tens column: {0} ({1} max)", tensColumn.targetNumCreatures, tensColumn.creatureMax);
    }

    private void ForceColumnsToAnswer()
    {
        // Update the chickens on the plane
        int desiredOnes = Session.instance.currentLevel.valueOnes;
        int desiredTens = Session.instance.currentLevel.valueTens;

        bool bDoPoof = false;

        if (onesColumn.numCreatures > desiredOnes)
        {
            onesColumn.Remove(onesColumn.numCreatures - desiredOnes);
            bDoPoof = true;
        }
        else if (onesColumn.numCreatures < desiredOnes)
        {
            onesColumn.Add(desiredOnes - onesColumn.numCreatures, Session.instance.currentLevel.useBrownQueue);
            bDoPoof = true;
        }

        if (tensColumn.numCreatures > desiredTens)
        {
            tensColumn.Remove(tensColumn.numCreatures - desiredTens);
            bDoPoof = true;
        }
        else if (tensColumn.numCreatures < desiredTens)
        {
            tensColumn.Add(desiredTens - tensColumn.numCreatures, Session.instance.currentLevel.useBrownQueue);
            bDoPoof = true;
        }

        if (bDoPoof)
        {
            ResetPoof();
            tutorial.HideAll();
        }
    }

    private void UpdateBackground(int inLevelIndex, bool inbPlayMusic = true, bool inbForceUpdate = false)
    {
        // figure which environment to flip to
        int index = (int)(inLevelIndex / levelsTilSceneChange) % environments.Length;

        if (inbPlayMusic)
            SoundManager.instance.PlayMusicTransition(environments[index].envMusic, SoundManager.instance.TransitionTime);

        if (!inbForceUpdate && inLevelIndex % levelsTilSceneChange != 0)
            return;

        // swap environments
        Transform background = this.transform.Find("Background");
        Transform currentEnv = background.FindChild("Env");
        Vector3 environmentPos = currentEnv.position;
        Destroy(currentEnv.gameObject);
        GameObject env = (GameObject)Instantiate(environments[index].prefab, environmentPos, Quaternion.identity);
        env.transform.SetParent(background, true);
        env.name = "Env";
        subtractionCtrl.SetTarmacColor(environments[index].tarmacColor);
    }

    void Column_onHintCount(PlaceValueCtrl ctrl, string placement, string suffix = null)
    {
        string trigger = "count";
        trigger += placement;
        trigger += (ctrl.value == 1) ? "One" : "Ten";
        if (suffix != null)
            trigger += suffix;
        expression.GetComponent<Animator>().SetTrigger(trigger);
    }
    //}

    #region Callbacks

    public void EnableOnesDragOut(PlaceValueCtrl inCtrl)
    {
        input.EnableAllInput(false, GameplayInput.ONES_COLUMN, GameplayInput.ONES_SUB);
        input.EnableCountingAndPause(true, true, false);
    }

    public void EnableOnesDragIn(PlaceValueCtrl inCtrl)
    {
        input.EnableAllInput(false, GameplayInput.ONES_QUEUE);
        input.EnableCountingAndPause(true, true, false);
    }

    public void EnableTensDragIn(PlaceValueCtrl inCtrl)
    {
        input.EnableAllInput(false, GameplayInput.TENS_QUEUE);
        input.EnableCountingAndPause(true, false, true);
    }

    public void EnableTensDragOut(PlaceValueCtrl inCtrl)
    {
        input.EnableAllInput(false, GameplayInput.TENS_COLUMN, GameplayInput.TENS_SUB);
        input.EnableCountingAndPause(true, false, true);
    }

    #endregion
}

public enum LevelStartState
{
    Default,
    Reset,
    Skip,
    TwoPart,
    HandHold
}

public enum HintingType
{
    None,
    AnswerHighlight,
    Handhold
}