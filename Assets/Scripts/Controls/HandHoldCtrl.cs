using UnityEngine;
using System.Collections;
using FGUnity.Utils;

public class HandHoldCtrl : MonoBehaviour {

	#region Members
	MyScreen _screen;
	PlaceValueCtrl _ones;
	PlaceValueCtrl _tens;

	bool _isActive = false;
	public bool isActive {
		get { return _isActive; }
	}

    bool isRecallEnabled = false;
    float _recallTimer = 0f;
    float _recallTrigger = 7f;
    string _recallKey = "";
    Vector3 _recallPosition = Vector3.zero;
    CoroutineHandle _waitCountRoutine;

    bool _isBorrowing = false;
    bool _initializedCarryoverSeats;
    CoroutineHandle _carryoverConsumeNestRoutine;

    PlaceValueCtrl _currentFocus;

	#endregion

	#region Ctrl
	void Awake () {
	}

	void Start () {
	
	}

	public void Init (MyScreen screen, PlaceValueCtrl ones, PlaceValueCtrl tens) {
		_screen = screen;
		_ones = ones;
		_tens = tens;
	}

	void Update () {
		if (!_isActive)
			return;

        UpdateRecall();
	}
	#endregion

	#region Methods
    public bool IsFocusedOn(PlaceValueCtrl inColumn)
    {
        return _currentFocus == inColumn;
    }

	public void SetActive (bool active) {
		_isActive = active;
		if (_isActive) {
			_screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", false);
			_screen.expression.GetComponent<Animator>().SetBool("hintFadeOnes", false);
            _screen.expression.GetComponent<Animator>().SetBool("isCounting", true);
		}
        else
        {
            Reset();
        }
	}
    
    public void Begin(bool inbLevelStart) {
        _isActive = true;

        _screen.input.EnableAllInput(false);

        _screen.ResetLevel(true, inbLevelStart);

        _waitCountRoutine.Stop();

        Reset();

        SetActive(true);

        _waitCountRoutine = this.WaitSecondsThen(inbLevelStart ? (Session.instance.currentLevel.isDoubleDigitProblem ? 3.5f : 1.2f) : 0.75f, StartCounting);
    }

    private void StartCounting()
    {
        if (!_isActive)
            return;


        CheckSolution();

        if (!_isActive)
            return;

        if (Session.instance.currentLevel.isSubtractionProblem) {
            _isBorrowing = CheckBorrowing();
            if (!_isBorrowing) {
                _ones.StartHintCounting(_screen.EnableOnesDragOut);
                _currentFocus = _ones;
                _screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", true);
                _screen.tutorial.HideAll();
                _tens.ClearHints();
            }
        }
        else if (Session.instance.currentLevel.isAdditionProblem)
        {
            _isBorrowing = false;
            if (CheckCarryover(_screen.onesColumn, true))
                return;
            bool bIsCarryover = Session.instance.currentLevel.isTargetNumber ? _ones.targetNumCreatures == 10 : _ones.targetNumCreatures == _ones.numCreatures + 10;
            if (bIsCarryover && Session.instance.currentLevel.requiresMultipleCarryover)
            {
                if (Session.instance.currentLevel.isTargetNumber)
                {
                    StartMultipleCarryover();
                    return;
                }
                _ones.targetNumCreatures = _ones.numCreatures;
            }
            
            _ones.StartHintCounting(_screen.EnableOnesDragIn);
            _currentFocus = _ones;
            _screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", true);
            _tens.ClearHints();
            _screen.tutorial.HideAll();
        }
    }

    public void WaitForOnesDragging()
    {
        if (!_initializedCarryoverSeats)
            _screen.tensColumn.ClearHints();
        if (_screen.onesColumn.numCreatures == _screen.onesColumn.targetNumCreatures)
        {
            _screen.queue.EndlessExit(true, false);

            if (_screen.onesColumn.numCreatures > 9)
                CheckCarryover(_screen.onesColumn);
            else
                CheckAdditionalCarryover();
        }
        else
        {
            string action = (Session.instance.currentLevel.isAdditionProblem) ? "Add" : "Sub";
            _screen.tutorial.Show((Session.instance.currentLevel.tensColumnEnabled) ? "showHintDrag" + action + "One" : "showHintDrag" + action, true);
            BeginRecall((Session.instance.currentLevel.tensColumnEnabled) ? "showHintDrag" + action + "One" : "showHintDrag" + action);
            
            // disable dragging from 10s column
            if (Session.instance.currentLevel.isAdditionProblem)
                _screen.input.EnableAllInput(false, GameplayInput.ONES_QUEUE);
            else if(Session.instance.currentLevel.isSubtractionProblem)
                _screen.input.EnableAllInput(false, GameplayInput.ONES_COLUMN, GameplayInput.ONES_SUB);

            _screen.input.EnableCountingAndPause(true, true, false);
        }
    }

    public void WaitForTensDragging()
    {
        _screen.onesColumn.ClearHints();
        string action = (Session.instance.currentLevel.isAdditionProblem) ? "Add" : "Sub";
        _screen.tutorial.Show((Session.instance.currentLevel.tensColumnEnabled) ? "showHintDrag" + action + "Ten" : "showHintDrag" + action, true);
        BeginRecall((Session.instance.currentLevel.tensColumnEnabled) ? "showHintDrag" + action + "Ten" : "showHintDrag" + action);
        
        // disable dragging from 1s column
        if (Session.instance.currentLevel.isAdditionProblem)
            _screen.input.EnableAllInput(false, GameplayInput.TENS_QUEUE);
        else if(Session.instance.currentLevel.isSubtractionProblem)
            _screen.input.EnableAllInput(false, GameplayInput.TENS_COLUMN, GameplayInput.TENS_SUB);

        _screen.input.EnableCountingAndPause(true, false, true);

        //TODO: what if no adding/subtracting is necessary?

    }

    public bool CheckAdditionalCarryover()
    {
        if (!isActive)
            return false;

        _screen.UpdateAddends();
        if (_ones.numCreatures != _ones.targetNumCreatures)
        {
            bool bIsCarryover = Session.instance.currentLevel.requiresMultipleCarryover && (Session.instance.currentLevel.isTargetNumber ? _ones.targetNumCreatures == 10 : _ones.targetNumCreatures == _ones.numCreatures + 10);
            if (bIsCarryover)
            {
                StartMultipleCarryover();
                return true;
            }
            else
            {
                _ones.StartHintCounting(_screen.EnableOnesDragIn);
                _currentFocus = _ones;
                _tens.ClearHints();
                _screen.tutorial.HideAll();
                _screen.queue.EndlessEnter(true, false);
                _screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", true);
                _screen.input.EnableAllInput(false);
                return true;
            }
        }

        return CheckTens();
    }

    public bool CheckTens() {
        CheckSolution();

        if (!isActive)
            return false;

        Assert.True(Session.instance.currentLevel.tensQueueEnabled || Session.instance.currentLevel.usesSubZone, "Tens dragging is available.");

        _screen.UpdateAddends();

        EndRecall();

        PlaceValueCtrl.ColumnCounting callback;
        if (Session.instance.currentLevel.isAdditionProblem)
            callback = _screen.EnableTensDragIn;
        else
            callback = _screen.EnableTensDragOut;
        _screen.onesColumn.ClearHints();
        _screen.tensColumn.StartHintCounting(callback);
        _currentFocus = _tens;

        _screen.input.EnableAllInput(false);
        _screen.tutorial.HideAll();

        _screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", false);
        _screen.expression.GetComponent<Animator>().SetBool("hintFadeOnes", true);
        _screen.expression.GetComponent<Animator>().SetBool("hintFadeTensExtra", false);

        return true;
    }
    
    public void CheckSolution () {
		if (!_isActive)
			return;

        ResetRecall();

        int value = _screen.CalculateCurrentValue();
		if (_screen.launchValid && value == Session.instance.currentLevel.value)
        {
            _screen.queue.EndlessExit(true, true);

            EndRecall();

            _screen.tutorial.Show("showHintGo", true, _screen.hud.launchBtn.position, _screen.hud.launchBtn);
            _screen.expression.ResetHintingFades(); // reset hinting fades
            _currentFocus = null;

            //_screen.input.EnableAllInput(true);
            _screen.input.EnableAllInput(false, GameplayInput.SUBMIT, GameplayInput.SUBMIT_NUMPAD, GameplayInput.TOGGLE_NUMBER_PAD, GameplayInput.NUMBER_PAD_ARROWS);
            _screen.input.EnableCountingAndPause(true);
            _screen.queue.EndlessExit(true, true);

            _ones.ClearHints();
            _tens.ClearHints();

            SetActive(false);
		}
	}

	public bool CheckCarryover (PlaceValueCtrl ctrl, bool inbForceCountOnes = false) {
		if (!_isActive)
			return false;
		if (ctrl != _ones)
			return false;

        if (Session.instance.currentLevel.isAdditionProblem)
        {
            if (_ones.numCreatures == _ones.targetNumCreatures)
            {
                _screen.queue.EndlessExit(true, false);
                if (_ones.numCreatures > 9)
                {
                    _ones.ClearHints();
                    _screen.tutorial.Show("showHintDragCarryover", true, Vector3.zero);  //showHintToTens
                    EndRecall();
                    _screen.input.EnableAllInput(false, GameplayInput.CONVERT_TO_TENS);
                    _screen.input.EnableCountingAndPause(true);
                    _screen.convertNestCtrl.GetComponent<ConvertNestCtrl>().ToggleVisibility(true, this.isActive);
                    _screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", false);
                    _screen.expression.GetComponent<Animator>().SetBool("hintFadeOnes", false);
                    _screen.expression.GetComponent<Animator>().SetBool("hintFadeTensExtra", false);
                    _currentFocus = null;
                    return true;
                }
                else if (inbForceCountOnes)
                {
                    return false;
                }
                else if (Session.instance.currentLevel.tensQueueEnabled)
                {
                    return CheckTens();
                }
                else
                {
                    return CheckAdditionalCarryover();
                }
            }
        } else if (Session.instance.currentLevel.isSubtractionProblem) {
            if (_ones.numCreatures == _screen.valueOnesAddend) {
                return CheckTens();
            }
        }

        return false;
    }

    bool CheckBorrowing () {
        if (!_isActive)
            return false;
        //if (_ones.numCreatures < _ones.addend) {
        if (_ones.numCreatures < _screen.valueOnesAddend) {
            _screen.tutorial.Show("showHintDragBorrow", true, Vector3.zero);  //showHintToTens
            _screen.input.EnableAllInput(false, GameplayInput.CONVERT_TO_ONES, GameplayInput.TENS_COLUMN);
            _screen.input.EnableCountingAndPause(true);
            _screen.convertNestCtrl.ShowEmptyTenFrame(true);
            _screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", false);
            _screen.expression.GetComponent<Animator>().SetBool("hintFadeOnes", false);
            _currentFocus = null;
            return true;
        }
        return false;
    }
	#endregion

    #region Recalling JITs
    public void BeginRecall (string key, Vector3 position = default(Vector3)) {
        isRecallEnabled = true;
        _recallTimer = 0f;
        _recallTrigger = 7f;
        _recallKey = key;
        _recallPosition = position;
    }

    public void ResetRecall () {
        if (!isRecallEnabled)
            return;
        _recallTimer = 0f;
    }

    void EndRecall () {
        isRecallEnabled = false;
    }

    void Reset()
    {
        EndRecall();
        _recallTimer = 0.0f;
        _recallTrigger = 0.0f;
        _recallKey = null;
        _recallPosition = default(Vector3);
        _isBorrowing = false;
        _initializedCarryoverSeats = false;

        _waitCountRoutine.Clear();
        _carryoverConsumeNestRoutine.Clear();
    }

    void UpdateRecall () {
        if (!isRecallEnabled)
            return;
        _recallTimer += Time.deltaTime;
        bool bThreshold = (_recallTimer >= _recallTrigger && _recallTimer - Time.deltaTime < _recallTrigger);
        if (bThreshold)
        {
            _screen.tutorial.Show(_recallKey, true, _recallPosition);
        }
    }
    #endregion

    #region Carryover special

    private IEnumerator InitializeCarryoverSeats()
    {
        _screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", false);
        _screen.expression.GetComponent<Animator>().SetBool("hintFadeOnes", false);
        yield return _screen.tensColumn.StartCarryoverTensHintCount();
        _initializedCarryoverSeats = true;
    }

    private IEnumerator ConsumeCarryoverSeat()
    {
        if (!_initializedCarryoverSeats)
        {
            yield return InitializeCarryoverSeats();
        }

        yield return 1.0f;

        _screen.tensColumn.SetCarryoverHintSeat(_screen.tensColumn.numCreatures, false);

        yield return 0.25f;

        _ones.StartCarryoverOnesHintCount(_screen.EnableOnesDragIn);
        _screen.tutorial.HideAll();
        _screen.queue.EndlessEnter(true, false);
        _screen.expression.GetComponent<Animator>().SetBool("hintFadeTens", false);
    }

    private void StartMultipleCarryover()
    {
        _carryoverConsumeNestRoutine.Clear();
        _carryoverConsumeNestRoutine = this.SmartCoroutine(ConsumeCarryoverSeat());
    }

    #endregion

}
