using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Ekstep;
using FGUnity.Utils;

public class PauseMenuCtrl : MonoBehaviour {

	#region Inspector
	public GameObject eggScreen;
    public MyScreen parentScreen;
	public GameObject loadingScreen;
	#endregion

	#region Gui
	private Transform closeBtn;
	private Transform musicBtn;
	private Transform soundBtn;
	private Transform resetBtn;
	private Transform homeBtn;
	private Transform stampsBtn;
	private Transform stampCollection;

	private SpriteState musicOffState;
	private SpriteState musicOnState;
	private Sprite musicOn;
	private SpriteState soundOffState;
	private SpriteState soundOnState;
	private Sprite soundOn;

    private GroupHider m_GroupHider;
	#endregion

    public Action OnClosed;

	public Sprite musicOffpressed;
	public Sprite musicOff;
	public Sprite soundOffpressed;
	public Sprite soundOff;

    private CoroutineHandle m_HideRoutine;
	
	void Awake(){
		closeBtn = transform.Find("PopUpHolder/BtnClose");
		musicBtn = transform.Find("PopUpHolder/BtnMusic");
		soundBtn = transform.Find("PopUpHolder/BtnSound");
		resetBtn = transform.Find("PopUpHolder/BtnReset");
		homeBtn = transform.Find("PopUpHolder/BtnHome");
		stampsBtn = transform.Find("PopUpHolder/BtnStamps");
		stampCollection = eggScreen.transform.FindChild("StampCollection");

		musicOnState.pressedSprite = musicBtn.GetComponent<Button>().spriteState.pressedSprite;
		soundOnState.pressedSprite = soundBtn.GetComponent<Button>().spriteState.pressedSprite;
		musicOn = musicBtn.GetComponent<Image>().sprite;
		soundOn = soundBtn.GetComponent<Image>().sprite;

        m_GroupHider = closeBtn.transform.parent.gameObject.AddComponent<GroupHider>();
	}
	
	void Start () {
		closeBtn.GetComponent<Button>().onClick.AddListener(closeBtn_onClick);
		musicBtn.GetComponent<Button>().onClick.AddListener(musicBtn_onClick);
		soundBtn.GetComponent<Button>().onClick.AddListener(soundBtn_onClick);
		resetBtn.GetComponent<Button>().onClick.AddListener(resetBtn_onClick);
		homeBtn.GetComponent<Button>().onClick.AddListener(homeBtn_onClick);
		stampsBtn.GetComponent<Button>().onClick.AddListener(stampsBtn_onClick);

		musicOffState.pressedSprite = musicOffpressed;
		soundOffState.pressedSprite = soundOffpressed;
		
		UpdateLanguageText();
		UpdateSoundBtns();
	}
	
	void closeBtn_onClick(){
		//close popup
		GetComponent<Animator>().SetTrigger("hidePopup");
        if (OnClosed != null)
            OnClosed();
	}
	
	void musicBtn_onClick(){
        SaveData.instance.MuteMusic = !SaveData.instance.MuteMusic;
		UpdateSoundBtns();
	}
	
	void soundBtn_onClick(){
        SaveData.instance.MuteSound = !SaveData.instance.MuteSound;
        SoundManager.instance.PlayOneShot(SoundManager.instance.buttonClick);
		UpdateSoundBtns();
	}
	
	void resetBtn_onClick()
    {
        parentScreen.ResetLevel();
        EnlearnInstance.I.LogActions(EnlearnInstance.Action.ResetProblem);
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.reset"));
        closeBtn_onClick();
	}
	
	void homeBtn_onClick(){
		//return to the main menu
		//Application.LoadLevel ("MainMenu");
        EnlearnInstance.I.LogActions(EnlearnInstance.Action.ResetProblem);
		StartCoroutine(Transition ());
	}
	
	void stampsBtn_onClick(){
		//bring up the stamps screen
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.suitcase"));
		stampsBtn.GetComponent<Animator>().SetTrigger("hideStampBtn");
		eggScreen.SetActive(true);
		stampCollection.gameObject.SetActive(true);
        stampCollection.GetComponent<SuitcaseCtrl>().Show(true);
        stampCollection.GetComponent<SuitcaseCtrl>().onExited = OnStampClose;

        // override the direction of the animation
        stampCollection.GetComponent<Animator>().SetBool("showCollection", false);
		stampCollection.GetComponent<Animator>().SetBool("showCollectionFromBottom", true);

        m_HideRoutine = this.WaitSecondsThen(0.3f, m_GroupHider.HideAll);
	}

    private void OnStampClose()
    {
        m_HideRoutine.Clear();
        stampCollection.GetComponent<Animator>().SetBool("showCollectionFromBottom", false);
        stampsBtn.GetComponent<Animator>().SetTrigger("showStampBtn");
        m_GroupHider.ShowAll();
    }

    private void UpdateLanguageText()
    {
        LanguageConfig lang = LanguageMgr.instance.Current;

        Image pauseText = transform.Find("PopUpHolder/Text").GetComponent<Image>();
        pauseText.sprite = lang.PausedText;
        pauseText.SetNativeSize();

        Image resetText = transform.Find("PopUpHolder/BtnReset/Text").GetComponent<Image>();
        resetText.sprite = lang.ResetText;
        resetText.SetNativeSize();
    }

	private void UpdateSoundBtns(){
		if(SaveData.instance.MuteSound){
			soundBtn.GetComponent<Image>().sprite = soundOff;
			soundBtn.GetComponent<Button>().spriteState = soundOffState;
		}else{
			soundBtn.GetComponent<Image>().sprite = soundOn;
			soundBtn.GetComponent<Button>().spriteState = soundOnState;
		}
		
		if(SaveData.instance.MuteMusic){
			musicBtn.GetComponent<Image>().sprite = musicOff;
			musicBtn.GetComponent<Button>().spriteState = musicOffState;
		}else{
			musicBtn.GetComponent<Image>().sprite = musicOn;
			musicBtn.GetComponent<Button>().spriteState = musicOnState;
		}
	}

	IEnumerator Transition(){
		//loadingScreen.gameObject.SetActive (true);
		//yield return new WaitForSeconds(.2f);
        SceneMgr.instance.LoadScene(SceneMgr.MAIN_MENU);
		yield break;
	}
}
