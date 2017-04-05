using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;

public class HudCtrl : MonoBehaviour
{
	#region Gui
    public LevelCompleteCtrl LevelCompleteScreen;
	#endregion

	#region Inspector
	public bool isReady = false;
	#endregion

	#region Members
	Transform _eggSlots;
	Transform _egg1;
	Transform _egg2;

	Transform _launchBtn;

    Transform _numberPadPanel;
    Transform _numberPadBtn;

	Transform _pauseBtn;

    bool _showingNumberPad = false;
	#endregion

	#region Delegates
	public delegate void OnLaunch();
    public delegate void OnNumberPad();
	public delegate void OnPause();

	[HideInInspector]
	public OnLaunch onLaunched;
    [HideInInspector]
    public OnLaunch onNumberPad;
	[HideInInspector]
	public OnPause onPause;

    [HideInInspector]
	public delegate void OnLevelComplete();
	[HideInInspector]
	public OnLevelComplete onLevelCompleteOff;
    public OnLevelComplete onLevelCompleteOn;
	#endregion

	#region Getters
	public Canvas canvas {
		get { return this.GetComponent<Canvas>(); }
	}

	public Transform launchBtn {
		get { return _launchBtn; }
	}

    public Transform numberPadBtn {
        get { return _numberPadBtn; }
    }

    public Transform numberPadPanel
    {
        get { return _numberPadPanel; }
    }

	public Transform pauseBtn {
		get { return _pauseBtn; }
	}
    #endregion


    #region Ctrl
    void Awake () {
		_eggSlots = this.transform.Find ("PauseBtnHolder/EggSlots");
		_egg1 = _eggSlots.transform.Find ("egg1");
		_egg2 = _eggSlots.transform.Find ("egg2");
		
		_launchBtn = this.transform.Find("LaunchBtnHolder/LaunchBtn");
        _numberPadBtn = this.transform.Find("NumpadHolder/NumberPad/BtnOpenClose");
        _numberPadPanel = this.transform.Find("NumpadHolder/NumberPad");

		_pauseBtn = this.transform.Find("PauseBtnHolder/PauseBtn");
    }

	void Start () {
		// load & setup
        LevelCompleteScreen.transform.SetParent(this.gameObject.transform, false);
        LevelCompleteScreen.onCtrlOn = LevelComplete_onCtrlOn;
        LevelCompleteScreen.onCtrlOff = LevelComplete_onCtrlOff;


		// input
		_launchBtn.GetComponent<Button>().onClick.AddListener(LaunchBtn_onClick);
        _numberPadBtn.GetComponent<Button>().onClick.AddListener(NumberPadBtn_onClick);
		_pauseBtn.GetComponent<Button>().onClick.AddListener(PauseBtn_onClick);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _pauseBtn.GetComponent<Button>().IsInteractable())
        {
            PauseBtn_onClick();
        }
    }

	public void FinalInit () {
        LevelCompleteScreen.gameObject.SetActive(false);
		isReady = true;
	}
	#endregion

	#region Methods
	public void EnableInput (bool enabled) {
		_launchBtn.gameObject.SetActive(!_showingNumberPad && enabled);
        _numberPadPanel.gameObject.SetActive(_showingNumberPad && enabled);
		_pauseBtn.gameObject.SetActive(enabled);
    }
	#endregion

    public void UseSubmitButton()
    {
        if (_showingNumberPad)
            _numberPadPanel.GetComponent<NumberPadCtrl>().PauseInputs(false);
        _showingNumberPad = false;
        _numberPadPanel.GetComponent<NumberPadCtrl>().Reset();
        _numberPadPanel.gameObject.SetActive(false);
        _launchBtn.gameObject.SetActive(true);
        _launchBtn.gameObject.GetComponent<Animator>().SetBool("isHinting", false);
        GetComponent<Animator>().SetBool("isNumpad", false);
    }

    public void UseNumberPad(bool inbIsTens)
    {
        _showingNumberPad = true;
        _numberPadPanel.gameObject.SetActive(true);
        _numberPadPanel.GetComponent<Animator>().SetBool("isTensPlane", inbIsTens);
        _launchBtn.gameObject.SetActive(false);
        GetComponent<Animator>().SetBool("isNumpad", true);
    }

	#region Level Complete
	public void ShowLevelComplete () {
        LevelCompleteScreen.gameObject.SetActive(true);
        LevelCompleteScreen.ToOn();
	}

	void LevelComplete_onCtrlOn ()
    {
        if (onLevelCompleteOn != null)
            onLevelCompleteOn();
	}

	void LevelComplete_onCtrlOff () {
		//_levelComplete.GetComponent<LevelCompleteCtrl>().onCtrlOff = null;
        LevelCompleteScreen.gameObject.SetActive(false);

		// let the game play screen know
		onLevelCompleteOff();
	}
	#endregion


	#region Eggs Reward Feedback
	public void EggsReset () {
		if (Session.instance.eggsEarned > 0 && _egg1.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.eggsOff"))
			_egg1.GetComponent<Animator>().SetTrigger("showEgg");
        if (Session.instance.eggsEarned > 1 && _egg2.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.eggsOff"))
			_egg2.GetComponent<Animator>().SetTrigger("showEgg");
	}

	public void EggsRemove () {
        if (Session.instance.eggsEarned > 1)
            SoundManager.instance.PlayOneShot(SoundManager.instance.eggLose);
		// note: egg1 only, player always earns egg2
		if (Session.instance.eggsEarned != 1) {
			_egg2.GetComponent<Animator>().SetTrigger("destroyEgg");
		}
		Session.instance.eggsEarned = 1;
		//if (_egg2.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.eggsIdle"))
	}

	public void EggsWin () {
		if (Session.instance.eggsEarned > 1) {
			//if (_egg2.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.eggsIdle")) {
				_egg2.GetComponent<Animator>().SetTrigger("winEgg");
			//}
		}
		if (Session.instance.eggsEarned > 0) {
			//if (_egg1.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.eggsIdle")) {
				_egg1.GetComponent<Animator>().SetTrigger("winEgg");
			//}
		}
	}
	#endregion

    public void SimulateLaunchClick()
    {
        LaunchBtn_onClick();
    }

	#region Input
    void LaunchBtn_onClick()
    {
        //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.submit");
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.submit"));
        onLaunched();
    }

    void NumberPadBtn_onClick() {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.numberPad.toggle"));
        onNumberPad();
    }

	void PauseBtn_onClick () {
        //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.reset");
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.pause"));
        onPause();
	}
	#endregion

}
