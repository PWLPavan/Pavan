using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;
using Ekstep;

public class PlaceValueCtrl : MonoBehaviour {

    #region Prefabs
    public Sprite highlightSprite;
    public Sprite tooFewSprite;
    public Sprite tooManySprite;

    public GameObject seatOnesPrefab;
    public GameObject seatTensPrefab;

    public GameObject creatureOnesPrefab;
    public GameObject creatureTensPrefab;

    public GameObject creatureOnesWhitePrefab;
    public GameObject creatureTensWhitePrefab;

    public GameObject creatureCarryPrefab;

    public GameObject dragGroupPrefab;

    public GameObject explosionPrefab;
    #endregion

    #region Gui
    Transform mHighlight;
    public Transform highlight {
        get { return mHighlight; }
    }
    public Bounds bounds {
        get { return mHighlight.GetComponent<SpriteRenderer>().bounds; }
    }
    #endregion

    #region Inspector
    public int creaturesPerColumn = 5;
    public int columnsPerPlaceValue = 4;

    public int creatureMax = 19;
    public int seatMax = 19;

    public int value = 1;
    public float offsetX = 0f;
    public float offsetY = 0f;
    public float spacingX = 1.12f;
    public float spacingY = 1.12f;

    public float conversionSpeed = 2.0f;

    public StretchySeatbeltCtrl stretchySeatbeltCtrl;

    public float defaultCountTimeInitial = 1.25f;
    public float defaultCountTimeCreature = 0.25f;

    [Header("Counting Colors")]
    public Color DefaultSeatCountColor = Color.white;
    public Color BrownSeatCountColor = Color.white;
    public Color WhiteSeatCountColor = Color.white;
    public Color GoldSeatCountColor = Color.white;
    #endregion

    #region Members
    MyScreen _screen;
    public MyScreen screen {
        get { return _screen; }
        set { _screen = value; }
    }

    List<GameObject> _seats;
    //List<GameObject> _seatbelts;

    List<GameObject> _creatures;
    public List<GameObject> creatures {
        get { return _creatures; }
    }
    public int numCreatures {
        get { return _creatures.Count; }
    }
    public Vector3 firstEmptyPosition {
        get { return (_creatures.Count == 0) ? this.transform.TransformPoint(Vector3.zero) : this.transform.TransformPoint(new Vector3((int)(_creatures.Count / creaturesPerColumn) * _creatures[0].GetComponent<CreatureCtrl>().bounds.size.x,
                                                                          (_creatures.Count != 0 ? -(_creatures.Count % creaturesPerColumn) : 0) * _creatures[0].GetComponent<CreatureCtrl>().bounds.size.y,
                                                                          0)); }
    }
    public CreatureCtrl lastCreature {
        get { return _creatures.Count > 0 ? _creatures[_creatures.Count - 1].GetComponent<CreatureCtrl>() : null; }
    }
    public CreatureCtrl firstCreature {
        get { return _creatures.Count > 0 ? _creatures[0].GetComponent<CreatureCtrl>() : null; }
    }

    [HideInInspector]
    public delegate void CreatureCountUpdated(PlaceValueCtrl ctrl);
    [HideInInspector]
    public CreatureCountUpdated onCreatureCountUpdated;

    [HideInInspector]
    public delegate void OnVacuumedDelegate(PlaceValueCtrl ctrl);
    [HideInInspector]
    public OnVacuumedDelegate onVacuumed;

    List<GameObject> mConvertingCreatures;
    GameObject mExplosion;
    [HideInInspector]
    public bool isExploding = false;
    [HideInInspector]
    public delegate void OnExplodedDelegate(PlaceValueCtrl ctrl);
    [HideInInspector]
    public OnExplodedDelegate onExploded;

    public bool isConverting {
        get { return mExplosion != null || isExploding; }
    }
    
    Vector2 mCreatureSpacing;

    [HideInInspector]
    public int numSelected = 0;

    public bool isDraggingEnabled = true;

    [HideInInspector]
    public bool isDraggedOver = false;

    private bool _dragStarted = false;

    const float _sequentialSeatTimeDelay = 1.25f;
    const float _sequentialSeatTimeThreshold = 0.25f;

    public delegate void ColumnHinting(PlaceValueCtrl ctrl, string placement, string suffix = null);
    public ColumnHinting onHintCount;

    [HideInInspector]
    public int targetNumCreatures = 0;

    public Vector2 redirectToConvertControlDistance = new Vector2(15, 45);

    private CoroutineHandle m_HintCountRoutine;
    private CoroutineHandle m_CountingRoutine;
    private CoroutineHandle m_WaitForTenthChickenRoutine;
    private bool m_WasWaitingForTenth = false;

    private float _countTimeDelay;
    private float _countTimeThreshold;

    

    [HideInInspector]
    public delegate void ColumnCounting(PlaceValueCtrl ctrl);
    [HideInInspector]
    public ColumnCounting onFinishedCounting;
    [HideInInspector]
    public ColumnCounting onCount;

    [HideInInspector]
    public delegate void ColumnShifting(PlaceValueCtrl ctrl);
    [HideInInspector]
    public ColumnShifting onShift;

    public bool allowDragConvert { get; private set; }
    #endregion


    #region Hand Hold Tut
    public int currentLevelValueNum {
        get { return (value == 1) ? Session.instance.currentLevel.valueOnes : Session.instance.currentLevel.valueTens; }
    }
    public int currentLevelInitialNum {
        get { return (value == 1) ? Session.instance.currentLevel.startingOnes : Session.instance.currentLevel.startingTens; }
    }
    public bool isCarryoverHinting { get; private set; }
    #endregion


    #region Ctrl
    void Awake() {
        allowDragConvert = true;

        _seats = new List<GameObject>();
        //_seatbelts = new List<GameObject>();

        _creatures = new List<GameObject>();
        mConvertingCreatures = new List<GameObject>();
        
        mHighlight = this.transform.Find("ColumnHighlight");
        
        // x = col spacing, y = row spacing
        mCreatureSpacing.x = bounds.size.y / creaturesPerColumn;
        mCreatureSpacing.y = bounds.size.x / columnsPerPlaceValue;

        _countTimeDelay = defaultCountTimeInitial;
        _countTimeThreshold = defaultCountTimeCreature;
        
        // fill out seats and set states
        int row = 0;
        int col = 0;
        for (int i = 0; i < seatMax; ++i) {
            GameObject seat = (GameObject)Instantiate((value == 1) ? seatOnesPrefab : seatTensPrefab,
                                                      Vector3.zero,
                                                      Quaternion.identity);
            
            row = (int)(i / columnsPerPlaceValue);
            col = (i - (row * columnsPerPlaceValue)); //((i != 0 ? -(i % creaturesPerColumn) : 0)
            float seatX = (col * spacingX) + offsetX;
            float seatY = (-row * spacingY) + offsetY;

            seat.transform.localPosition = new Vector3(seatX,
                                                       seatY,
                                                       0.0f);
            seat.transform.SetParent(this.transform, false);
            seat.GetComponent<Animator>().SetBool("isSeat", (i < 9));
            seat.GetComponent<Animator>().SetBool("isTens", (i == 9));
            seat.GetComponent<Animator>().Update(0.0001f);
            string textToSet = (value * (i + 1)).ToStringLookup();
            seat.GetComponentInChildren<TextMesh>().text = textToSet;
            _seats.Add(seat);
        }

        // find seatbelts
        /*string beltPrefix = (value == 1) ? "ones" : "tens";
        using(PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
        {
            for (int i = 1; i < 10; ++i)
            {
                stringBuilder.Builder.Append("Seatbelts/");
                stringBuilder.Builder.Append(beltPrefix);
                stringBuilder.Builder.Append("Seatbelt ");
                stringBuilder.Builder.Append(i.ToStringLookup());
                _seatbelts.Add(this.transform.FindChild(stringBuilder.Builder.ToString()).gameObject);
                stringBuilder.Builder.Length = 0;
            }
        }*/
    }

    public void SetSeatVisible (int idx, bool visible) {
        //_seats[idx].GetComponent<Animator>().SetBool("isSeat", visible);
        //_seats[idx].GetComponent<Animator>().SetBool("isTens", visible);
    }

    void Start() {

    }

    public void EnableInput(bool enabled) {
        // enable dragging
        isDraggingEnabled = enabled;
    }

    public void SoftReset()
    {
        CancelHintCounting();

        _dragStarted = false;
        isDragging = false;
        isDraggedOver = false;

        ResetHighlight();
        ClearHints();
    }

    public void Clear(bool fromTwoPart = false) {
        CancelHintCounting();

        // clear of chickens
        if (!fromTwoPart)
            Remove(creatureMax, false);

        // kill any conversions
        isExploding = false;
        if (mExplosion) {
            mExplosion.GetComponent<MecanimEventHandler>().UnRegisterOnStateBegin(Animator.StringToHash("Base Layer.off"), Explode_onExplosion);
            Destroy(mExplosion);
            mExplosion = null;
        }
        while (mConvertingCreatures.Count > 0) {
            Destroy(mConvertingCreatures[0]);
            mConvertingCreatures.RemoveAt(0);
        }

        // deselect column
        Deselect();

        // remove column highlight
        this.GetComponent<Animator>().SetBool("hoverOver", false);

        // reset column feedback highlight
        ResetHighlight();

        // reset seats
        for (int i = 0; i < _seats.Count; ++i) {
            _seats[i].GetComponent<Animator>().SetBool("isSeat", (i < 9));
            _seats[i].GetComponent<Animator>().SetBool("newFill", false);
            _seats[i].GetComponent<Animator>().SetBool("draggedIn", false);
            _seats[i].GetComponent<Animator>().SetBool("draggedOut", false);
            _seats[i].GetComponent<Animator>().SetBool("correct", false);
            _seats[i].GetComponent<Animator>().SetBool("selected", false);
            _seats[i].GetComponent<Animator>().SetBool("occupied", false);

            if (!_seats[i].GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("Hint.Default"))
                _seats[i].GetComponent<Animator>().SetTrigger("hideHint");
            else
                _seats[i].GetComponent<Animator>().ResetTrigger("hideHint");
        }

        // reset seatbelts
        /*if (_seatbelts != null) {
            for (int j = 0; j < _seatbelts.Count; ++j) {
                _seatbelts[j].GetComponent<Animator>().SetBool("isSeatbelted", false);
            }
        }*/

        targetNumCreatures = 0;
    }

    public void ClearHints()
    {
        for (int i = 0; i < _seats.Count; ++i)
        {
            if (!_seats[i].GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("Hint.Default"))
                _seats[i].GetComponent<Animator>().SetTrigger("hideHint");
            _seats[i].GetComponent<Animator>().ResetTrigger("showHintYellow");
        }
    }

    public void ShowCorrectFeedback() {
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetTrigger("correctSeat");
            _seats[i].GetComponent<Animator>().SetTrigger("correct");
        }

        if (Session.instance.currentLevel.twoPartProblem)
            return;

        if(Session.instance.currentLevel.value == 1)
            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenCorrect1);
        if(Session.instance.currentLevel.value <= 5 && Session.instance.currentLevel.value > 1)
            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenCorrect2);
        if(Session.instance.currentLevel.value > 5)
            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenCorrect3);
    }

    public void StopShowingCorrectFeedback()
    {
        for (int i = 0; i < _creatures.Count; ++i)
        {
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetTrigger("reset");
        }
    }

    public void ShowIncorrectFeedback () {
        this.GetComponent<Animator>().SetTrigger("incorrect");
    }

    public void ShowSeatOverflow(int finalNumericValue) {
        for (int i = 0; i < _creatures.Count; ++i) {
            //if (mSeats.Count > i) {				// -highlight all-
            //if (i > finalNumericValue - 1) {		// -highlight those over the expected value-
            if (i >= 9) {                           // highlight overflow only
                _seats[i].GetComponent<Animator>().SetTrigger("incorrect");
            }
        }
    }

    public void ResetHighlight() {
        this.GetComponent<Animator>().SetBool("tooFew", false);
        this.GetComponent<Animator>().SetBool("tooMany", false);
        mHighlight.GetComponent<SpriteRenderer>().sprite = highlightSprite;
    }

    public void ShowTooManyTooFewHighlight(int finalNumericValue) {
        if (numCreatures < finalNumericValue) {
            this.GetComponent<Animator>().SetBool("tooFew", true);
            mHighlight.GetComponent<SpriteRenderer>().sprite = tooFewSprite;
        } else if (numCreatures > finalNumericValue) {
            this.GetComponent<Animator>().SetBool("tooMany", true);
            mHighlight.GetComponent<SpriteRenderer>().sprite = tooManySprite;
        }
    }

    public void ShowAnswerHighlight(int finalNumericValue) {
        for (int i = 0; i < finalNumericValue; ++i)
        {
            _seats[i].GetComponent<Animator>().SetTrigger("showHintYellow");
            _seats[i].GetComponent<Animator>().ResetTrigger("hideHint");
        }
        for (int i = finalNumericValue; i < _seats.Count; ++i)
        {
            _seats[i].GetComponent<Animator>().ResetTrigger("showHintYellow");
        }
    }

    public void ShowNumpadHighlight(int numericValue)
    {
        for (int i = 0; i < _seats.Count; ++i)
        {
            bool bHasCreature = i < _creatures.Count;
            bool bShouldHighlight = i < numericValue;
            _seats[i].GetComponent<Animator>().SetBool("selected", bHasCreature && bShouldHighlight);
            _seats[i].GetComponent<Animator>().SetBool("draggedIn", !bHasCreature && bShouldHighlight);
        }
    }

    public void SetSeatbeltAll(bool seatbelted)
    {
        if (_creatures.Count > 0)
            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSeatbeltBuckle);
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].GetComponent<CreatureCtrl>().SetSeatbelt(seatbelted, true);
        }
    }

    public void Unbuckle () {
        for (int i = 0; i < _creatures.Count; ++i) {
            if (i < 10)
                _creatures[i].GetComponent<CreatureCtrl>().SetSeatbelt(false, true);
        }
    }

    public void Buckle ()
    {
        for (int i = 0; i < _creatures.Count; ++i)
        {
            _creatures[i].GetComponent<CreatureCtrl>().SetSeatbelt(Session.instance.currentLevel.seatbelts);
        }
    }

    public void SetSeatVisibility(bool visible)
    {
        int index = value == 1 ? 9 : 0;
        for (; index < _seats.Count; ++index)
        {
            _seats[index].GetComponent<Animator>().enabled = visible;
        }
    }

    public void ChirpInitialCreatures() {
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetBool("isCounting", true);
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetTrigger("countSeat");
        }
        //TODO: set 'isCounting' to false on all creatures after they've been counted
    }

    public void StopChirping() {
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetBool("isCounting", false);
        }
    }
    #endregion


    #region Hinting
    public CoroutineHandle StartHintCounting(ColumnCounting callback)
    {
        m_HintCountRoutine.Clear();
        isCarryoverHinting = false;
        m_HintCountRoutine = this.SmartCoroutine(HintCountRoutine(callback));
        return m_HintCountRoutine;
    }

    public CoroutineHandle StartCarryoverOnesHintCount(ColumnCounting callback)
    {
        m_HintCountRoutine.Clear();
        isCarryoverHinting = true;
        m_HintCountRoutine = this.SmartCoroutine(CarryoverOnesHintCountRoutine(callback));
        return m_HintCountRoutine;
    }

    public CoroutineHandle StartCarryoverTensHintCount()
    {
        m_HintCountRoutine.Clear();
        isCarryoverHinting = true;
        m_HintCountRoutine = this.SmartCoroutine(CarryoverTensHintCountRoutine());
        return m_HintCountRoutine;
    }

    private void CancelHintCounting()
    {
        m_HintCountRoutine.Clear();
    }

    private IEnumerator CarryoverOnesHintCountRoutine(ColumnCounting inCallback)
    {
        int initialCount = numCreatures;
        if (targetNumCreatures == 10)
            initialCount = 0;

        for (int seatIndex = initialCount; seatIndex < targetNumCreatures; ++seatIndex)
        {
            HintCountSeat(seatIndex, false, false);
        }

        yield return null;

        CancelHintCounting();
        SyncConversionAnimationState();

        if (inCallback != null)
            inCallback(this);
        if (value == 1)
            screen.handHoldCtrl.WaitForOnesDragging();
        else if (value == 10)
            screen.handHoldCtrl.WaitForTensDragging();
    }

    private IEnumerator CarryoverTensHintCountRoutine()
    {
        int initialCount = numCreatures;

        bool bIsTarget = Session.instance.currentLevel.isTargetNumber;

        if (_creatures.Count > 0)
        {
            bool bHasGold = _creatures.Count > 0 && _creatures[0].GetComponent<CreatureCtrl>().color == CreatureCtrl.COLOR_GOLD;
            if (bHasGold)
            {
                onHintCount(this, "Top", "Carry");
                yield return _sequentialSeatTimeDelay;
                HintCountSeat(0, false);
                yield return _sequentialSeatTimeDelay + _sequentialSeatTimeThreshold;
            }

            onHintCount(this, "Top", Session.instance.currentLevel.isSubtractionProblem && Session.instance.currentLevel.requiresRegrouping && value == 10 ? "Borrow" : null);
            yield return _sequentialSeatTimeDelay;

            for (int seatIndex = bHasGold ? 1 : 0; seatIndex < initialCount; ++seatIndex)
            {
                if (bHasGold)
                    SetRealtimeCountNumber(seatIndex, value * (seatIndex));
                else
                    SetRealtimeCountNumber(seatIndex, value * (seatIndex + 1));
                HintCountSeat(seatIndex, false);
                yield return _sequentialSeatTimeThreshold;
            }
        }
        else if (!bIsTarget)
        {
            onHintCount(this, "Top", null);
            yield return _sequentialSeatTimeDelay;
        }

        if (!(bIsTarget && _creatures.Count > 0))
        {
            yield return null;
            onHintCount(this, Session.instance.currentLevel.isTargetNumber ? "Top" : "Bottom");
            yield return _sequentialSeatTimeDelay;
        }
        else if (bIsTarget)
        {
            yield return _sequentialSeatTimeThreshold * 3;
        }

        for (int seatIndex = initialCount; seatIndex < _screen.valueTensAddend; ++seatIndex)
        {
            SetRealtimeCountNumber(seatIndex, bIsTarget ? value * (seatIndex + 1) : value * (seatIndex - initialCount + 1));
            HintCountSeat(seatIndex, false, true);
            yield return _sequentialSeatTimeThreshold;
        }

        CancelHintCounting();
        SyncConversionAnimationState();
    }

    private IEnumerator HintCountRoutine(ColumnCounting inCallback)
    {
        int initialCount = numCreatures;
        bool bSubtracting = targetNumCreatures < numCreatures;

        bool bIsTarget = Session.instance.currentLevel.isTargetNumber;

        if (_creatures.Count > 0)
        {
            bool bHasGold = _creatures.Count > 0 && _creatures[0].GetComponent<CreatureCtrl>().color == CreatureCtrl.COLOR_GOLD;
            if (bHasGold)
            {
                onHintCount(this, "Top", "Carry");
                yield return _sequentialSeatTimeDelay;
                HintCountSeat(0, false);
                yield return _sequentialSeatTimeDelay + _sequentialSeatTimeThreshold;
            }

            onHintCount(this, "Top", Session.instance.currentLevel.isSubtractionProblem && Session.instance.currentLevel.requiresRegrouping && value == 10 ? "Borrow" : null);
            yield return _sequentialSeatTimeDelay;

            for (int seatIndex = bHasGold ? 1 : 0; seatIndex < initialCount; ++seatIndex)
            {
                if (bHasGold)
                    SetRealtimeCountNumber(seatIndex, value * (seatIndex));
                else
                    SetRealtimeCountNumber(seatIndex, value * (seatIndex + 1));
                HintCountSeat(seatIndex, false);
                yield return _sequentialSeatTimeThreshold;
            }
        }
        else if (!bIsTarget)
        {
            onHintCount(this, "Top", null);
            yield return _sequentialSeatTimeDelay;
        }

        if (!(bIsTarget && _creatures.Count > 0))
        {
            yield return null;
            onHintCount(this, Session.instance.currentLevel.isTargetNumber ? "Top" : "Bottom");
            yield return _sequentialSeatTimeDelay;
        }
        else if (bIsTarget)
        {
            yield return _sequentialSeatTimeThreshold * 3;
        }

        if (bSubtracting)
        {
            for (int seatIndex = initialCount - 1; seatIndex >= targetNumCreatures; --seatIndex)
            {
                SetRealtimeCountNumber(seatIndex, -((initialCount - seatIndex) * value));
                HintCountSeat(seatIndex, true);
                yield return _sequentialSeatTimeThreshold;
            }
        }
        else
        {
            for (int seatIndex = initialCount; seatIndex < targetNumCreatures; ++seatIndex)
            {
                if (bIsTarget)
                    SetRealtimeCountNumber(seatIndex, (seatIndex + 1) * value);
                else
                    SetRealtimeCountNumber(seatIndex, (seatIndex - initialCount + 1) * value);
                HintCountSeat(seatIndex, false);
                yield return _sequentialSeatTimeThreshold;
            }
        }

        yield return _sequentialSeatTimeDelay;

        CancelHintCounting();
        SyncConversionAnimationState();

        if (inCallback != null)
            inCallback(this);
        if (value == 1)
            screen.handHoldCtrl.WaitForOnesDragging();
        else if (value == 10)
            screen.handHoldCtrl.WaitForTensDragging();
    }

    private void HintCountSeat(int inSeatIndex, bool inbSubtracting, bool inbShouldCount = true)
    {
        bool bHasCreature = !inbSubtracting && inSeatIndex < _creatures.Count;
        bool bIsGoldNest = bHasCreature
            && _creatures[inSeatIndex].GetComponent<CreatureCtrl>().color == CreatureCtrl.COLOR_GOLD;
        bool bIsWhiteCreature = bHasCreature
            && _creatures[inSeatIndex].GetComponent<CreatureCtrl>().color == CreatureCtrl.COLOR_WHITE;
        string seatTrigger = "showHintBlue";
        Color seatColor = BrownSeatCountColor;
        if (bIsGoldNest)
        {
            seatTrigger = "showHintYellowCarryover";
            seatColor = GoldSeatCountColor;
        }
        else if ((inSeatIndex >= _creatures.Count && !Session.instance.currentLevel.useBrownQueue) || inbSubtracting || bIsWhiteCreature)
        {
            seatTrigger = "showHintWhite";
            seatColor = WhiteSeatCountColor;
        }

        _seats[inSeatIndex].GetComponent<Animator>().ResetTrigger("showHintYellow");

        if (inbSubtracting)
            _seats[inSeatIndex].GetComponent<Animator>().SetTrigger("hideHint");
        else
            _seats[inSeatIndex].GetComponent<Animator>().ResetTrigger("hideHint");

        _seats[inSeatIndex].GetComponent<Animator>().SetTrigger(seatTrigger);
		_seats[inSeatIndex].GetComponent<Animator>().ResetTrigger("hideHint");

        if (inbShouldCount)
        {
            SetRealtimeCountColor(inSeatIndex, seatColor);
            _seats[inSeatIndex].GetComponent<Animator>().SetTrigger("countNum");
            if (inSeatIndex < _creatures.Count)
                _creatures[inSeatIndex].GetComponent<CreatureCtrl>().SetTrigger("countSelected");
        }
    }

    public void SetCarryoverHintSeat(int inSeatIndex, bool inbShow)
    {
        _seats[inSeatIndex].GetComponent<Animator>().ResetTrigger("showHintYellow");
        if (!inbShow)
            _seats[inSeatIndex].GetComponent<Animator>().SetTrigger("hideHint");
        else
            _seats[inSeatIndex].GetComponent<Animator>().SetTrigger("showHintWhite");
    }
    #endregion

    #region Counting
    public void StartCounting()
    {
        m_CountingRoutine.Clear();
        m_CountingRoutine = this.SmartCoroutine(CountingRoutine());
    }

    public void ResetCountingTime()
    {
        SetCountingMultiplier(1.0f);
    }

    public void SetCountingMultiplier(float inTimeMultiplier)
    {
        _countTimeDelay = defaultCountTimeInitial / inTimeMultiplier;
        _countTimeThreshold = defaultCountTimeCreature / inTimeMultiplier;
    }

    private IEnumerator CountingRoutine()
    {
        for (int i = 0; i < _creatures.Count; ++i)
        {
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetBool("isCounting", true);
        }

        yield return _countTimeDelay;

        if (_creatures.Count > 0)
        {
            for(int creatureIndex = 0; creatureIndex < _creatures.Count; ++creatureIndex)
            {
                _creatures[creatureIndex].GetComponent<CreatureCtrl>().SetTrigger("countSeat");
                _seats[creatureIndex].GetComponent<Animator>().SetBool("selected", true);
                onCount(this);
                yield return _countTimeThreshold;
            }
        }
        else
        {
            onCount(this);
            yield return _countTimeThreshold;
        }

        onFinishedCounting(this);
        m_CountingRoutine.Clear();
    }

    public void SetRealtimeCount(int inSeatNum)
    {
        if (inSeatNum < _creatures.Count)
        {
            Animator seatAnimator = _seats[inSeatNum].GetComponent<Animator>();
            seatAnimator.SetBool("showCountHighlight", true);
            seatAnimator.SetTrigger("countNum");
            _creatures[inSeatNum].GetComponent<CreatureCtrl>().SetTrigger("countSelected");
        }
    }

    public void ClearRealtimeCount()
    {
        for(int i = 0; i < _seats.Count; ++i)
        {
            _seats[i].GetComponent<Animator>().SetBool("showCountHighlight", false);
        }

        SyncConversionAnimationState();
    }

    public void SetRealtimeCountNumber(int inSeatNum, int inDisplayedNumber)
    {
        string textToSet = inDisplayedNumber.ToStringLookup();

        foreach(TextMesh t in _seats[inSeatNum].GetComponentsInChildren<TextMesh>()){
            t.text = textToSet;
        }
    }

    public void SetRealtimeCountColor(int inSeatNum, Color inColor)
    {
        Transform insideText = _seats[inSeatNum].transform.FindChild("countNum/inside");
        insideText.GetComponent<TextMesh>().color = inColor;
    }

    #endregion


    #region Animation
    public void RefreshCreaturePositions(bool pathToSeat = false, bool retargetMoving = true)
    {
        int row = 0;
        int col = 0;
        //Debug.Log("REFRESH " + _creatures.Count.ToString() + " CREATURE POSITIONS");
        CreatureCtrl creature;
        for (int i = 0; i < _creatures.Count; ++i) {
            // adjust position by amount of creatures
            row = (int)(i / columnsPerPlaceValue);
            col = (i - (row * columnsPerPlaceValue));
            float creatureX = (col * spacingX) + offsetX;
            float creatureY = (-row * spacingY) + offsetY;
            //_creatures[i].transform.localPosition = new Vector3(creatureX, creatureY, _creatures[i].transform.position.z);

            creature = _creatures[i].GetComponent<CreatureCtrl>();

            Vector3 localPosition = new Vector3(creatureX, creatureY, creature.transform.position.z);
            if (pathToSeat) {
                creature.transform.SetParent(this.transform, true);
                creature.prevLocalPosition = localPosition;
                creature.StartMove(localPosition, false, 9f, true);
                creature.onMoveEnd = Creature_onAdded;  // necessary for 'isMoving' condition interrupts
                creature.SetBool("isWalking", true);
                creature.SetTrigger("reset");

                if (creature.transform.localPosition.x - localPosition.x < 0) {
                    creature.transform.localScale = (value == 1) ? new Vector3(-1f, 1f, 1f) : new Vector3(1.28f, 1.28f, 1f);
                }
            }
            else if (retargetMoving && creature.IsMoving)
            {
                creature.prevLocalPosition = localPosition;
                creature.StartMove(localPosition, false, ChickenSettings.instance.snapBackSpeed, true);

                if (creature.transform.localPosition.x - localPosition.x < 0)
                {
                    creature.transform.localScale = (value == 1) ? new Vector3(-1f, 1f, 1f) : new Vector3(1.28f, 1.28f, 1f);
                }
            }
            else
            {
                creature.transform.localPosition = localPosition;
                creature.prevLocalPosition = localPosition;
            }

        }
        //Debug.Log(_creatures.Count.ToString() + " CREATURE POSITIONS REFRESHED");

        // update seat belts
        UpdateSeatbelts();
    }

    public void UpdateCreatureTriggers() {
        // update seats as occupied or unoccupied
        UpdateSeatOccupancy();

        // update seat belts
        UpdateSeatbelts();

        if (value == 1) {   // 10s creature don't have this same Animator
            // sort chickens by color (gold, brown/red, white/gray)
            //_creatures.Sort(CompareChickens);
            SortChickens();
            
            //_creatures.Sort(delegate (GameObject lh, GameObject rh) { return lh.GetComponent<CreatureCtrl>().color.CompareTo(rh.GetComponent<CreatureCtrl>().color); });
            //Debug.Log(_creatures.Count.ToString() + " CREATURE COLORS SORTED");
            RefreshCreaturePositions();

            // update creature animations (happy, unhappy, & ready)
            for (int i = 0; i < _creatures.Count; ++i) {
                bool markReady = Session.instance.currentLevel.isAdditionProblem;
                int creatureThreshold = (markReady) ? 10 : 9;
                markReady &= _creatures.Count >= creatureThreshold;

                if (i < creatureThreshold) {
                    _creatures[i].GetComponent<CreatureCtrl>().SetBool("inTen", true);      // HAPPY
                    _creatures[i].GetComponent<CreatureCtrl>().SetBool("fullTen", markReady); // READY
                } else {
                    _creatures[i].GetComponent<CreatureCtrl>().SetBool("inTen", false);     // UNHAPPY
                    _creatures[i].GetComponent<CreatureCtrl>().SetBool("fullTen", true);

                    _creatures[i].GetComponent<CreatureCtrl>().SetTrigger("reset");
                }
            }

            bool bShouldWaitForTenth = _creatures.Count >= 10;

            if (bShouldWaitForTenth && !m_WasWaitingForTenth && Session.instance.currentLevel.isAdditionProblem)
            {
                m_WaitForTenthChickenRoutine.Clear();
                m_WaitForTenthChickenRoutine = this.SmartCoroutine(WaitForTenthChicken());
            }
            else if (m_WasWaitingForTenth)
            {
                m_WaitForTenthChickenRoutine.Clear();
            }

            m_WasWaitingForTenth = bShouldWaitForTenth;
        }

        // notify the screen of any column creature count changes
        onCreatureCountUpdated(this);
    }

    void UpdateSeatOccupancy () {
        for (int i = 0; i < _seats.Count; ++i) {// condition was: < 10
            _seats[i].GetComponent<Animator>().SetBool("isSeat", (i < 9));
            _seats[i].GetComponent<Animator>().SetBool("occupied", (_creatures.Count > i));
        }
    }

    void UpdateSeatbelts () {
        bool overrideBelts = Session.instance.currentLevel.isAdditionProblem;
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].GetComponent<CreatureCtrl>().SetSeatbelt(Session.instance.currentLevel.seatbelts &&
                                                                   (_creatures[i].GetComponent<CreatureCtrl>().color == CreatureCtrl.COLOR_BROWN ||
                                                                   _creatures[i].GetComponent<CreatureCtrl>().color == CreatureCtrl.COLOR_GOLD) && overrideBelts);
        }
    }

    public void SyncConversionAnimationState()
    {
        if (value == 1 && _creatures.Count > 9 && Session.instance.currentLevel.isAdditionProblem)
        {
            SetConversionAnimationState(false);
            SetConversionAnimationState(true);
        }
    }

    IEnumerator WaitForTenthChicken()
    {
        if (!Session.instance.currentLevel.isAdditionProblem)
            yield break;

        // TODO: Make this not so hacky.
        // Would be nice to have another callback on CreatureCtrl for when
        // it finishes moving (and plopping, in the case of returning to seat)
        while (_creatures[9].GetComponent<CreatureCtrl>().IsMoving)
        {
            yield return null;
            if (_creatures.Count < 10 || isDragging)
                yield break;
        }

        if (_creatures.Count < 10 || isDragging)
            yield break;

        yield return null;
        yield return null;

        SyncConversionAnimationState();
        m_WaitForTenthChickenRoutine.Clear();
    }

    void SetConversionAnimationState(bool inbState)
    {
        for (int i = 0; i < 10; ++i)
        {
            _creatures[i].GetComponent<CreatureCtrl>().SetBool("inTen", inbState);
            _creatures[i].GetComponent<CreatureCtrl>().SetBool("fullTen", inbState);
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().Update(0.001f);
        }
    }

    void SortChickens ()
    {
        using (PooledList<GameObject> temp = PooledList<GameObject>.Create())
        {
            for (int i = 0; i < _creatures.Count; )
            {
                if (_creatures[i].GetComponent<CreatureCtrl>().color == CreatureCtrl.COLOR_WHITE)
                {
                    temp.Add(_creatures[i]);
                    _creatures.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
            for (int j = 0; j < temp.Count; ++j)
            {
                _creatures.Add(temp[j]);
            }
        }
    }

    static int CompareChickens(GameObject x, GameObject y) {
        // sort chickens by color (gold, brown/red, white/gray)
        //return 0;     // equal
        //return -1;    // y is greater
        //return 1;     // x is greater
        //return string.Compare(x.GetComponent<CreatureCtrl>().color, y.GetComponent<CreatureCtrl>().color);
        // Debug.Log(x.transform.position.ToString() + " | " + y.transform.position.ToString());
        // Debug.Log(x.GetComponent<CreatureCtrl>().color + " | " + y.GetComponent<CreatureCtrl>().color);
        if (string.Equals(x.GetComponent<CreatureCtrl>().color, y.GetComponent<CreatureCtrl>().color)) {
            return 0;
        } else if (string.Equals(x.GetComponent<CreatureCtrl>().color, CreatureCtrl.COLOR_BROWN) &&
                   string.Equals(y.GetComponent<CreatureCtrl>().color, CreatureCtrl.COLOR_WHITE)) {
            return -1;
        } else {
            return 1;
        }
    }

    public void UpdateDragOver(bool isOver, int numDragged = 0) {
        //TODO: push into an isEnabled check for PlaceValueCtrls
        if (!Session.instance.currentLevel.tensColumnEnabled) {
            if (value == 1 && _creatures.Count == 9)
                return;
            if (value == 10) {
                return;
            }
        }
        
        if (isDraggedOver && !isOver) {
            // set draggedIn for seats animator (false)
            ToggleDragOver(false, numDragged);
        } else if (!isDraggedOver && isOver) {
            // set draggedIn for seats animator (true)
            ToggleDragOver(true, numDragged);
        }
    }
    
    public void ForceDragOverFalse(int numDragged = 0) {
        // set draggedIn for seats animator (false)
        //ResetDragOver_IsSeat();
        ToggleDragOver(false, numDragged);
    }

    void ToggleDragOver (bool draggedIn, int howManyDraggedOver) {
        isDraggedOver = draggedIn;
        this.GetComponent<Animator>().SetBool("hoverOver", draggedIn);

        for (int i = _creatures.Count; i < _creatures.Count + howManyDraggedOver; ++i) {
            if (_seats.Count > i) {
                //if (draggedIn)
                //    _seats[i].GetComponent<Animator>().SetBool("isSeat", true);
                _seats[i].GetComponent<Animator>().SetBool("draggedIn", draggedIn);
            }
        }
    }

    public void ResetDragOver_IsSeat () {
        for (int i = _creatures.Count; i < _creatures.Count; ++i) {
            if (_seats.Count > i) {
                _seats[i].GetComponent<Animator>().SetBool("isSeat", (i < 9));
                _seats[i].GetComponent<Animator>().SetBool("draggedIn", false);
                
            }
        }
    }
    #endregion


    #region Methods
    public bool SplitDragGroup(DragGroup group) {
        if (numCreatures >= creatureMax)
            return false;

        if (value == 1) {
            if (numCreatures + group.numOnes > creatureMax)
                return false;
        } else if (value == 10) {
            if (numCreatures + group.numTens > creatureMax)
                return false;
        }

        //SoundManager.instance.PlayOneShot(SoundManager.instance.counterUpdates);
        SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenCount);

        // split dragGroup between 1s creatures and 10s creatures
        CreatureCtrl[] children = group.GetComponentsInChildren<CreatureCtrl>();
        for (int i = 0; i < children.Length; ++i) {
            if (children[i].value == value) // value comparison (only take 10s || only take 1s)
                AddCreature(children[i]);
        }

        // update creature animations (happy, unhappy, & ready)
        UpdateCreatureTriggers();

        return true;
    }

    public bool AddDragGroup(DragGroup group) {
        if (numCreatures >= creatureMax)
            return false;

        if (value == 1) {
            if (numCreatures + group.numOnes > creatureMax)
                return false;
        } else if (value == 10) {
            if (numCreatures + group.numTens > creatureMax)
                return false;
        }

        if (value == 1)
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.onesAdd");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.onesAdd"));
        else if (value == 10)
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.tensAdd");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.tensAdd"));

        //SoundManager.instance.PlayOneShot(SoundManager.instance.counterUpdates);
        SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenCount);

        // convert group of creatures to individual creatures (reparent individually)
        CreatureCtrl[] children = group.GetComponentsInChildren<CreatureCtrl>();
        for (int i = 0; i < children.Length; ++i) {
            AddCreature(children[i], false, true);
        }
        
        // update creature animations (happy, unhappy, & ready)
        UpdateCreatureTriggers();

        return true;
    }

    void AddCreature(CreatureCtrl creature, bool defaultSeat = false, bool pathToSeat = false, string color = null) {
        if (color != null)
            Logger.Log("COLOR ======= " + color);

        int row = 0;    // GOLD
        int col = 0;
        int brownIdx = 0;

        if (color == null || color == CreatureCtrl.COLOR_WHITE) {    // WHITE
            row = (int)(_creatures.Count / columnsPerPlaceValue);
            col = (_creatures.Count - (row * columnsPerPlaceValue));
        } else {
            // BROWN
            // search for last brown || first white
            int searchIdx = 0;
            for (int i = 0; i < _creatures.Count; ++i) {
                string peekColor = _creatures[i].GetComponent<CreatureCtrl>().color;
                if (peekColor == CreatureCtrl.COLOR_GOLD || peekColor == CreatureCtrl.COLOR_BROWN) {
                    searchIdx++;
                    continue;
                } else if (peekColor == CreatureCtrl.COLOR_WHITE) {
                    break;
                }
            }
            brownIdx = searchIdx;
        }

        // adjust position by amount of creatures in column
        float creatureX = (col * spacingX) + offsetX;
        float creatureY = (-row * spacingY) + offsetY;

        // mad positioning hack for converted nests
        if (color != null) {
            creature.transform.position = new Vector3(2.0f, 2.2f, creature.transform.position.z);
        }
        
        // reparent creatures
        Vector3 localPosition = new Vector3(creatureX, creatureY, creature.transform.position.z);
        if (pathToSeat) {
            creature.transform.SetParent(this.transform, true);
            creature.prevLocalPosition = localPosition;
            creature.StartMove(localPosition, false, 9f, true);
            creature.onMoveEnd = Creature_onAdded;  // necessary for 'isMoving' condition interrupts
            creature.SetBool("isWalking", true);

            if (creature.transform.localPosition.x - localPosition.x < 0) {
                creature.transform.localScale = (value == 1) ? new Vector3(-1f, 1f, 1f) : new Vector3(1.28f, 1.28f, 1f);
            }
        } else {
            creature.transform.localPosition = localPosition;
            creature.transform.SetParent(this.transform, false);
            creature.SetBool("isWalking", false);
        }

        // add to list
        if (color == null || color == CreatureCtrl.COLOR_WHITE) {
            _creatures.Add(creature.gameObject);    // WHITE
        } else if (color == CreatureCtrl.COLOR_GOLD) {
            _creatures.Insert(0, creature.gameObject);  // GOLD
        } else {
            _creatures.Insert(brownIdx, creature.gameObject);  // BROWN
        }

        creature.SetBool("inCar", false);
        creature.SetBool("inQueue", false);
        creature.SetBool("inColumn", true);
        creature.SetTrigger("reset");

        Assert.True(_creatures.Count <= _seats.Count, "Number of creatures does not exceed number of seats.");
        Assert.True(_creatures.Count > 0, "Number of creatures is not 0.");

        // set the seat state
        if (defaultSeat) {
            _seats[_creatures.Count - 1].GetComponent<Animator>().SetBool("newFill", false);
        } else {
            _seats[_creatures.Count - 1].GetComponent<Animator>().SetBool("newFill", true);
        }
        if (_creatures.Count > 9) {    //TODO: have UX fix this issue in the animator (ie. 'newFill' need is required to transition to 'overflow' state for seat)
            _seats[_creatures.Count - 1].GetComponent<Animator>().SetBool("newFill", true);
        }

        _seats[_creatures.Count - 1].GetComponent<Animator>().SetBool("occupied", true);
    }

    void Creature_onAdded (SteerableBehavior obj) {
        ((CreatureCtrl)obj).SetBool("isWalking", false);
        ((CreatureCtrl)obj).SetTrigger("reset");
        ((CreatureCtrl)obj).transform.localScale = (value == 1) ? new Vector3(1f, 1f, 1f) : new Vector3(1.28f, 1.28f, 1f);
        obj.onMoveEnd = null;
        ((CreatureCtrl)obj).UpdateQueuedSeatbelt();
    }

    public void Remove(int count, bool updateCreatureCount = true) {
        //mCreatures, pop off end & destroy
        GameObject creature;
        while (count != 0 && _creatures.Count > 0) {    // && mCreatures.Count >= count
            _seats[_creatures.Count - 1].GetComponent<Animator>().SetBool("occupied", false);
            creature = _creatures[_creatures.Count - 1];
            _creatures.Remove(creature);
            Destroy(creature.gameObject);
            count--;
        }

        // update creature animations (happy, unhappy, & ready)
        if (updateCreatureCount)
            UpdateCreatureTriggers();
    }

    public void RemoveAt(int index) {
        Destroy(_creatures[index].gameObject);
        _creatures.RemoveAt(index);
        _seats[index].GetComponent<Animator>().SetBool("occupied", false);

        RefreshCreaturePositions();
        UpdateCreatureTriggers();
    }

    public void Add(int count, bool defaultSeat = false, string[] colors = null) {
        GameObject prefab;
        string color;
        if (defaultSeat) {
            prefab = (value == 1) ? creatureOnesPrefab : creatureTensPrefab;
            color = CreatureCtrl.COLOR_BROWN;
        } else {
            prefab = (value == 1) ? creatureOnesWhitePrefab : creatureTensWhitePrefab;
            color = CreatureCtrl.COLOR_WHITE;
        }
        
        for (int i = 0; i < count; ++i) {
            // update color/prefab per spawned chicken/nest if assigned colors are given
            if (colors != null) {
                color = colors[i];
                if (color == CreatureCtrl.COLOR_BROWN)
                    prefab = (value == 1) ? creatureOnesPrefab : creatureTensPrefab;
                else if (color == CreatureCtrl.COLOR_WHITE)
                    prefab = (value == 1) ? creatureOnesWhitePrefab : creatureTensWhitePrefab;
            }

            GameObject creature = (GameObject)Instantiate(prefab,
                                                          Vector3.zero,
                                                          Quaternion.identity);
            creature.GetComponent<CreatureCtrl>().color = color;
            AddCreature(creature.GetComponent<CreatureCtrl>(), defaultSeat);
        }

        // update creature animations (happy, unhappy, & ready)
        UpdateCreatureTriggers();
    }

    public string AddCarryOver(int count, string[] colors) {
        bool brown = hasBrownChickens(colors);
        bool white = hasWhiteChickens(colors);

        GameObject nest = (GameObject)Instantiate(getColoredCarryoverPrefab(brown, white),
                                                  Vector3.zero,
                                                  Quaternion.identity);
        if (nest.GetComponent<ConvertedNestCtrl>() != null) {
            nest.GetComponent<ConvertedNestCtrl>().SetChickenColor(colors);
        }
        
        // determine color
        string color = CreatureCtrl.COLOR_WHITE;
        if (brown && white)
            color = CreatureCtrl.COLOR_GOLD;
        else if (brown)
            color = CreatureCtrl.COLOR_BROWN;
        nest.GetComponent<CreatureCtrl>().color = color;

        nest.GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetTrigger("carryoverJump");
        AddCreature(nest.GetComponent<CreatureCtrl>(), false, true, color);
        
        // update creature animations (happy, unhappy, & ready)
        RefreshCreaturePositions(true, true);
        UpdateCreatureTriggers();

        return color;
    }

    public void ClearSelected() {
        numSelected = 0;
        //numSelectedTxt.text = numSelected.ToString();
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].GetComponent<CreatureCtrl>().Select(false);
        }
    }
    #endregion


    #region Coloring
    bool hasBrownChickens(string[] colors) {
        return hasColor(colors, CreatureCtrl.COLOR_BROWN);
    }

    bool hasWhiteChickens(string[] colors) {
        return hasColor(colors, CreatureCtrl.COLOR_WHITE);
    }

    bool hasColor(string[] colors, string targetColor) {
        for (int i = 0; i < colors.Length; ++i) {
            if (colors[i] == targetColor)
                return true;
        }
        return false;
    }

    public int GetNumWithColor(string inTargetColor)
    {
        int numColor = 0;
        for(int i = 0; i < _creatures.Count; ++i)
        {
            if (_creatures[i].GetComponent<CreatureCtrl>().color == inTargetColor)
                ++numColor;
        }
        return numColor;
    }

    GameObject getColoredCarryoverPrefab(string[] colors) {
        return getColoredCarryoverPrefab(hasBrownChickens(colors), hasWhiteChickens(colors));
    }

    GameObject getColoredCarryoverPrefab(bool brown, bool white) {
        if (brown && white) {
            return creatureCarryPrefab;
        } else if (brown) {
            return creatureTensPrefab;
        } else if (white) {
            return creatureTensWhitePrefab;
        }
        return creatureCarryPrefab;
    }
    #endregion


        #region Conversion
    public string[] GetFirstTenColors () {
        string[] colors = new string[10];
        for (int i = 0; i < 10; ++i) {
            if (_creatures.Count <= i)
                break;
            colors[i] = _creatures[i].GetComponent<CreatureCtrl>().color;
        }
        return colors;
    }

    public void VacuumUp(int count, Vector3 target) {
        // convert Remove (int count) method
        GameObject creature;
        for (int i = 0; i < count; ++i) {
            creature = _creatures[0];
            _creatures.Remove(creature);
            Destroy(creature);

            _seats[i].GetComponent<Animator>().SetBool("occupied", false);
        }
        
        // refresh creature position in THIS column
        RefreshCreaturePositions(false, true);
    }

    public string SpitOut(int count, string[] colors) {
        string resultingColor = AddCarryOver(count, colors);

        screen.ForceColumnUpdate();

        return resultingColor;

        // display explosion animation
        /*mExplosion = (GameObject)Instantiate(explosionPrefab, Vector3.zero, Quaternion.identity);
        mExplosion.transform.position = _creatures[_creatures.Count - 1].transform.position;
        mExplosion.GetComponent<MecanimEventHandler>().RegisterOnStateBegin(Animator.StringToHash("Base Layer.off"), SpitOut_onExplosion);*/
    }

    void SpitOut_onExplosion () {
        mExplosion.GetComponent<MecanimEventHandler>().UnRegisterOnStateBegin(Animator.StringToHash("Base Layer.off"), SpitOut_onExplosion);
        Destroy(mExplosion);
        mExplosion = null;
        
        screen.ForceColumnUpdate();
    }

    public void AllowDragConversion(bool enable)
    {
        allowDragConvert = enable;
    }

    public void Explode (int count)
    {
        CreatureCtrl creature = lastCreature;
        if (creature.color == CreatureCtrl.COLOR_BROWN && firstCreature.GetComponent<ConvertedNestCtrl>())
            creature = firstCreature;

        Explode(count, creature, true);
    }
   
    private void Explode(int inCount, CreatureCtrl inCreatureCtrl, bool inbRemove)
    {
        bool isConvertedNest = inCreatureCtrl.GetComponent<ConvertedNestCtrl>();
        string[] colors = null;
        string color = "";
        if (isConvertedNest)
        {
            colors = new string[inCreatureCtrl.GetComponent<ConvertedNestCtrl>().colors.Length];
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = inCreatureCtrl.GetComponent<ConvertedNestCtrl>().colors[i];
            }
        }
        else
            color = inCreatureCtrl.color;

        // remove the creature we're exploding
        if (inbRemove)
        {
            int creatureIndex = _creatures.IndexOf(inCreatureCtrl.gameObject);
            RemoveAt(creatureIndex);
        }

        // add 10 to the ones column
        if (color == CreatureCtrl.COLOR_WHITE)
            screen.onesColumn.Add(inCount, false);
        else if (color == CreatureCtrl.COLOR_BROWN)
            screen.onesColumn.Add(inCount, true);
        else if (colors != null)
            screen.onesColumn.Add(inCount, false, colors);
    }

    public void Explode(DragGroup inDragGroup)
    {
        CreatureCtrl creature = inDragGroup.GetComponentInChildren<CreatureCtrl>();
        Explode(10, creature, false);
    }

    void Explode_onExplosion () {
        mExplosion.GetComponent<MecanimEventHandler>().UnRegisterOnStateBegin(Animator.StringToHash("Base Layer.off"), Explode_onExplosion);
        Destroy(mExplosion);
        mExplosion = null;

        screen.ForceColumnUpdate();
    }
    #endregion


    #region Select
    void RemoveDraggedFromList () {
        // REMOVE FROM mCREATURES LIST (pop off front)
        /*_seats[_creatures.Count - 1].GetComponent<Animator>().SetBool("occupied", false);
        CreatureCtrl[] children = dragGroup.GetComponentsInChildren<CreatureCtrl>();
        _creatures.Remove(children[0].gameObject);*/
        CreatureCtrl[] children = dragGroup.GetComponentsInChildren<CreatureCtrl>();
        int creatureIdx = _creatures.IndexOf(children[0].gameObject);
        _creatures.RemoveAt(creatureIdx);//Remove(children[0].gameObject);
        _seats[creatureIdx].GetComponent<Animator>().SetBool("occupied", false);
        
        // shift creatures within place value ctrl (update positions)
        RefreshCreaturePositions();

        // update creature animations (happy, unhappy, & ready)
        UpdateCreatureTriggers();
    }

    void RemoveSelectedFromList () {
        // REMOVE FROM mCREATURES LIST (pop off front)
        CreatureCtrl[] children = dragGroup.GetComponentsInChildren<CreatureCtrl>();
        for (int i = 0; i < children.Length; ++i) {
            int creatureIdx = _creatures.IndexOf(children[i].gameObject);
            _creatures.RemoveAt(creatureIdx);//Remove(children[i].gameObject);
            _seats[creatureIdx].GetComponent<Animator>().SetBool("occupied", false);
        }

        // shift creatures within place value ctrl (update positions)
        RefreshCreaturePositions();

        // update creature animations (happy, unhappy, & ready)
        UpdateCreatureTriggers();
    }

    void Select () {
        //if (!Session.instance.currentLevel.selectionEnabled)
        //    return;

        // select a creature, and then select another..
        numSelected = 0;
        for (int i = 0; i < _creatures.Count; ++i) {
            if (_creatures[i].GetComponent<CreatureCtrl>().selected) {
                numSelected++;
                continue;
            }
            _creatures[i].GetComponent<CreatureCtrl>().Select(true);
            numSelected++;
            // select seat
            if (_seats.Count > i)
                _seats[i].GetComponent<Animator>().SetBool("selected", true);
            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSelect);
            break;
        }

        if (numSelected > 0)
            this.GetComponent<Animator>().SetBool("selection", true);
        //numSelectedTxt.text = numSelected.ToString();
    }

    public void Deselect() {
        // deselect place value
        this.GetComponent<Animator>().SetBool("selection", false);

        ClearRealtimeCount();

        // deselect creatures
        numSelected = 0;
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].GetComponent<CreatureCtrl>().Select(false);
        }
        //numSelectedTxt.text = numSelected.ToString();

        // deselect seats
        for (int i = 0; i < _seats.Count; ++i) {
            _seats[i].GetComponent<Animator>().SetBool("selected", false);
            _seats[i].GetComponent<Animator>().SetBool("draggedIn", false);
            _seats[i].GetComponent<Animator>().SetBool("draggedOut", false);
        }
    }
    #endregion


    #region Converting 1s to 10s
    public void ConvertCapture (Transform parent, Transform mounts, params int[] creatureIndexes) {
        int index, mountIndex;
        for (int i = 0; i < creatureIndexes.Length; ++i)
        {
            index = creatureIndexes[i];
            mountIndex = (creatureIndexes.Length - 10) / 2 + index;
            Transform mount = mounts.FindChild("slot" + (mountIndex + 1).ToStringLookup());

            if (_creatures[index].transform.parent == mount)
                continue;

            if (_creatures[index].GetComponent<CreatureCtrl>().IsMoving)
                _creatures[index].GetComponent<CreatureCtrl>().EndMove();

            // reparent creature
            //if (creatures[index].transform.parent == this.transform)
            //    _creatures[index].GetComponent<CreatureCtrl>().prevLocalPosition = _creatures[index].transform.localPosition;

            Vector3 mPos = mount.position;
            mPos = parent.InverseTransformPoint(mPos);

            _creatures[index].transform.localPosition = Vector3.zero;//new Vector3(0.0f, 0.0f, 0.0f);  // localPosition
            _creatures[index].transform.SetParent(mount, false);    //false
            _creatures[index].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetBool("dragged", true);
            _creatures[index].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetTrigger("plop");
            _creatures[index].GetComponent<CreatureCtrl>().SetSortingLayer(Globals.sortOrder);
            // set seat as draggedOut
            _seats[index].GetComponent<Animator>().SetBool("draggedOut", true);
            _creatures[index].GetComponent<CreatureCtrl>().SetBool("fullTen", true);
        }
    }

    public void ConvertSnapBack ()
    {
        bool markReady = Session.instance.currentLevel.isAdditionProblem;
        int creatureThreshold = (markReady) ? 10 : 9;
        markReady &= _creatures.Count >= creatureThreshold;

        // put creatures back into place value column
        for (int i = 0; i < _creatures.Count; ++i) {
            if (_creatures[i].transform.parent == this.transform)
                continue;
            _creatures[i].transform.localPosition = _creatures[i].GetComponent<CreatureCtrl>().prevLocalPosition;
            _creatures[i].transform.SetParent(this.transform, false);
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetBool("dragged", false);
            _creatures[i].GetComponent<CreatureCtrl>().inner.GetComponent<Animator>().SetTrigger("reset");
            // remove draggedOut state from seats
            _seats[i].GetComponent<Animator>().SetBool("draggedOut", false);
            _creatures[i].GetComponent<CreatureCtrl>().SetBool("fullTen", markReady);
            //Logger.Log("SNAPBACK " + i.ToStringLookup());
        }
        
        SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSnapBack);
        if (_creatures.Count > 9)
            this.StartCoroutine(WaitForTenthChicken());
    }
    #endregion


    #region Dragging
    void StartDrag () {
        if (numCreatures <= 0)
            return;
        /*if (firstCreature.GetComponent<ConvertedNestCtrl>() == null && 
            numCreatures <= _seatbelts.Count && 
            _seatbelts[numCreatures - 1].GetComponent<Animator>().GetBool("isSeatbelted")) {
            return;
        }*/
        
        if (value == 1)
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.onesColumn");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.onesColumn"));
        else if (value == 10)
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.tensColumn");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.tensColumn"));

        isDragging = true;

        dragGroup = (GameObject)Instantiate(dragGroupPrefab,
                                            Vector3.zero,
                                            Quaternion.identity);

        // pick the correct creature (use carry over nests first (gold), else use white, then brown)
        CreatureCtrl creature = lastCreature;
        if (lastCreature.color == CreatureCtrl.COLOR_BROWN && firstCreature.GetComponent<ConvertedNestCtrl>() != null) {
            creature = firstCreature;
        }

        creature.UpdateQueuedSeatbelt();

        // if this creature was already moving, kill its movement and set/reset its prevLocalPosition
        if (creature.onMoveEnd != null) {
            creature.onMoveEnd = null;
            creature.transform.localPosition = creature.prevLocalPosition;
            // kill movement
            creature.EndMove();
        }

        if (creature.isSeatbelted)
        {
            stretchySeatbeltCtrl.Begin(value, creature.transform.position);
        }
        
        // reparent last creature in list
        creature.prevLocalPosition = creature.transform.localPosition;
        creature.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        creature.transform.SetParent(dragGroup.transform, false);
        // set seat as draggedOut
        _seats[_creatures.IndexOf(creature.gameObject)].GetComponent<Animator>().SetBool("draggedOut", true);//_seats[_creatures.Count - 1]

        // let the dragGroup know the whole value
        dragGroup.GetComponent<DragGroup>().SetValue(this.value);

        // update creatures drag state
        dragGroup.GetComponent<DragGroup>().SetCreaturesBool("dragged", true);
        dragGroup.GetComponent<DragGroup>().SetCreaturesSortOrder(1000);

        // set dragGroup position
        Vector3 currPos = new Vector3(Input.mousePosition.x,
                                      Input.mousePosition.y,
                                      0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(currPos);
        worldPos.z = 0.0f;
        dragGroup.transform.position = worldPos;

        SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenDrag);
        //SoundManager.instance.PlayOneShot(SoundManager.instance.chickenDrag);
    }

    void StopDrag () {
        isDragging = false;

        stretchySeatbeltCtrl.End();

        // update creatures drag state
        dragGroup.GetComponent<DragGroup>().SetCreaturesBool("dragged", false);
        dragGroup.GetComponent<DragGroup>().SetCreaturesSortOrder(0);
    }

    void SnapBack () {
        if (value == 1)
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn.snapBack");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn.snapBack"));
        else if (value == 10)
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensColumn.snapBack");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensColumn.snapBack"));

        // put creatures back into place value column
        //SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSnapBack);

        CreatureCtrl[] children = dragGroup.GetComponentsInChildren<CreatureCtrl>();

        // remove draggedOut state from seats
        _seats[_creatures.IndexOf(children[0].gameObject)].GetComponent<Animator>().SetBool("draggedOut", false);

        children[0].Drop(this.transform, false);
    }

    void PlayIncorrectSound()
    {
        SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSnapBack);
    }

    #endregion


    #region Input
    void OnMouseDown () {
        if (isDragging)
            return;
        mMouseDown.x = Input.mousePosition.x;
        mMouseDown.y = Input.mousePosition.y;

        _dragStarted = true;
    }

    Vector2 mDragDist = new Vector2();

    Vector2 mMouseDown = new Vector2();
    Vector2 mMouseCurr = new Vector2();
    
    GameObject dragGroup;
    bool isDragging = false;

    void OnMouseDrag()
    {
        if (!_dragStarted)
            return;

        if (!isDragging) {
            mMouseCurr.x = Input.mousePosition.x;
            mMouseCurr.y = Input.mousePosition.y;
            mDragDist = mMouseDown - mMouseCurr;

            if (mDragDist.magnitude > 30)
            {
                var conversionNest = _screen.convertNestCtrl;
                if (value == 1
                    && conversionNest.ScreenPositionWithinTenFrame(mMouseDown)
                    && mDragDist.x > redirectToConvertControlDistance.x
                    && mDragDist.y > -redirectToConvertControlDistance.y
                    && mDragDist.y < redirectToConvertControlDistance.y
                    && !conversionNest.isDragging && conversionNest.isDraggingEnabled && conversionNest.isVisible)
                {
                    _screen.convertNestCtrl.StartDrag();
                    _dragStarted = false;
                }
                else if (isDraggingEnabled)
                {
                    StartDrag();
                }
                else
                {
                    _dragStarted = false;
                }
            }
        } else if (isDragging) {
            // update the drag group position
            Vector3 currPos = new Vector3(Input.mousePosition.x,
                                          Input.mousePosition.y,
                                          0);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(currPos);
            worldPos.z = 0.0f;

            // clamp worldPos to stay within the bounds of the display
            worldPos.x = Mathf.Clamp(worldPos.x,
                                     CameraUtils.cameraRect.xMin + dragGroup.GetComponent<SpriteRenderer>().bounds.extents.x,
                                     CameraUtils.cameraRect.xMax - dragGroup.GetComponent<SpriteRenderer>().bounds.extents.x);
            worldPos.y = Mathf.Clamp(worldPos.y,
                                     CameraUtils.cameraRect.yMin + dragGroup.GetComponent<SpriteRenderer>().bounds.extents.y,
                                     CameraUtils.cameraRect.yMax - dragGroup.GetComponent<SpriteRenderer>().bounds.extents.x);

            if (dragGroup != null)
                dragGroup.transform.position = worldPos;
            
            // check for bounding box intersections between group and place value subtraction car
            // show hover state
            int dragValue = dragGroup.GetComponent<DragGroup>().value;
            Bounds dragGroupBounds = dragGroup.GetComponent<SpriteRenderer>().bounds;
            bool onesColPlacement = dragGroupBounds.Intersects(screen.onesColumn.bounds);
            bool tensColPlacement = dragGroupBounds.Intersects(screen.tensColumn.bounds);
            
            if (Session.instance.currentLevel.usesSubZone)
            {
                bool onesSubPlacement = (dragGroupBounds.Intersects(screen.subtractionCtrl.onesZone.bounds));
                bool tensSubPlacement = (dragGroupBounds.Intersects(screen.subtractionCtrl.tensZone.bounds));

                if (value == 1 && !onesColPlacement && !tensColPlacement && !screen.subtractionCtrl.onesZone.isFilled) {
                    screen.subtractionCtrl.SetHighlight(1, true);
                } else {
                    screen.subtractionCtrl.SetHighlight(1, false);
                }
                if (value == 10 && !tensColPlacement && !onesColPlacement && !screen.subtractionCtrl.tensZone.isFilled) {
                    screen.subtractionCtrl.SetHighlight(10, true);
                } else {
                    screen.subtractionCtrl.SetHighlight(10, false);
                }
            }
            
            // highlight dragging 10s over 1s column if valid
            if (dragValue == 10 && (screen.onesColumn.numCreatures + 10 <= screen.onesColumn.creatureMax) && screen.onesColumn.allowDragConvert) {
                screen.onesColumn.UpdateDragOver(onesColPlacement, dragGroup.GetComponent<DragGroup>().numTens * 10);
            }
            
            if (stretchySeatbeltCtrl.isStretching) {
                stretchySeatbeltCtrl.UpdateBelts(worldPos);
            }
        }
    }

    void OnMouseUp ()
    {
        if (!isDragging || !_dragStarted)
            return;

        _dragStarted = false;
        
        StopDrag();

        // check for bounding box intersections between group and place value subtraction car
        int dragValue = dragGroup.GetComponent<DragGroup>().value;
        Bounds dragGroupBounds = dragGroup.GetComponent<SpriteRenderer>().bounds;

        DropZoneCtrl onesSub = screen.subtractionCtrl.onesZone;
        DropZoneCtrl tensSub = screen.subtractionCtrl.tensZone;
        DropZoneCtrl mySub = (value == 1 ? onesSub : tensSub);

        PlaceValueCtrl onesCol = screen.onesColumn;
        PlaceValueCtrl tensCol = screen.tensColumn;

        bool onesColPlacement = dragGroupBounds.Intersects(onesCol.bounds);
        bool tensColPlacement = dragGroupBounds.Intersects(tensCol.bounds);

        bool bWithinHandhold = screen.handHoldCtrl.isActive;
        bool bSnapBack = false;
        bool bDrop = false;
        bool bOpenAir = !onesColPlacement && !tensColPlacement;
        bool bAllowSubtractZone = !mySub.isFilled && mySub.isDroppingEnabled;

        bool subPlacement = Session.instance.currentLevel.usesSubZone
            && (dragGroupBounds.Intersects(onesSub.bounds) || dragGroupBounds.Intersects(tensSub.bounds) || bOpenAir);

        if (dragValue == 10)
        {
            if (onesColPlacement)
            {
                if (onesCol.allowDragConvert && onesCol.numCreatures + 10 <= onesCol.creatureMax)
                {
                    Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn"));
                    onShift(this);
                }
                else
                {
                    bSnapBack = true;
                }
            }
            else if (tensColPlacement)
            {
                bSnapBack = true;
            }
            else if (subPlacement)
            {
                if (bAllowSubtractZone)
                {
                    Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensSub"));
                    EnlearnInstance.I.LogActions(EnlearnInstance.Action.RemoveFromTens);
                    RemoveSelectedFromList();
                    screen.subtractionCtrl.AddDragGroup(10, dragGroup.GetComponent<DragGroup>());
                    if (!bWithinHandhold)
                    {
                        bool hidden = screen.tutorial.HideHandHold();
                        if (hidden)
                            screen.input.EnableAllInput(true);
                    }
                    Deselect();
                }
                else
                {
                    // TODO: Flash subtraction zone and play sound
                    PlayIncorrectSound();
                    bSnapBack = true;
                }
            }
            else
            {
                bDrop = true;
            }
        }
        else if (dragValue == 1)
        {
            if (onesColPlacement)
            {
                bSnapBack = true;
            }
            else if (tensColPlacement)
            {
                PlayIncorrectSound();
                tensCol.ShowIncorrectFeedback();
                bSnapBack = true;
            }
            else if (subPlacement)
            {
                if (bAllowSubtractZone)
                {
                    Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesSub"));
                    EnlearnInstance.I.LogActions(EnlearnInstance.Action.RemoveFromOnes);
                    RemoveSelectedFromList();
                    screen.subtractionCtrl.AddDragGroup(1, dragGroup.GetComponent<DragGroup>());
                    if (!bWithinHandhold)
                    {
                        bool hidden = screen.tutorial.HideHandHold();
                        if (hidden)
                            screen.input.EnableAllInput(true);
                    }
                    Deselect();
                }
                else
                {
                    // TODO: Flash subtraction zone and play sound
                    PlayIncorrectSound();
                    bSnapBack = true;
                }
            }
            else
            {
                bDrop = true;
            }
        }

        if (bDrop)
        {
            CreatureCtrl[] children = dragGroup.GetComponentsInChildren<CreatureCtrl>();
            children[0].UpdateQueuedSeatbelt();
            if (children[0].isSeatbelted)
            {
                bSnapBack = true;
            }
            else
            {
                if (value == 1)
                    EnlearnInstance.I.LogActions(EnlearnInstance.Action.RemoveFromOnes);
                else if (value == 10)
                    EnlearnInstance.I.LogActions(EnlearnInstance.Action.RemoveFromTens);

                // destroy chicken
                _seats[_creatures.Count - 1].GetComponent<Animator>().SetBool("draggedOut", false);
                RemoveDraggedFromList();

                // have chicken drop to 'ground level'
                children[0].Drop();

                // Hide tutorials
                if (!bWithinHandhold)
                {
                    bool hidden = screen.tutorial.HideHandHold();
                    if (hidden)
                        screen.input.EnableAllInput(true);

                    if (_creatures.Count < 10)
                    {
                        hidden = screen.tutorial.Hide(TutorialCtrl.HINT_CARRY);
                        if (hidden)
                            screen.input.EnableAllInput(true);
                    }
                }
            }
        }

        if (bSnapBack)
        {
            SnapBack();
            if (dragValue == 1 && _creatures.Count == 10)
                this.StartCoroutine(WaitForTenthChicken());
        }
        
        if (Session.instance.currentLevel.usesSubZone)
            screen.subtractionCtrl.ClearHighlight();

        onesCol.ForceDragOverFalse(value);
        tensCol.ForceDragOverFalse(value);

        Destroy(dragGroup);
        dragGroup = null;
    }
    #endregion
    
}
