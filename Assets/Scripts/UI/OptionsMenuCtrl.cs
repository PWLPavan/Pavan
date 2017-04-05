using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FGUnity.Utils;
using Ekstep;

public class OptionsMenuCtrl : MonoBehaviour {

	#region Gui
	private Transform closeBtn;
	private Transform musicBtn;
	private Transform soundBtn;
	private Transform languageBtn;
	//private Transform resetPlayerBtn;
	private Transform introBtn;
	private Transform creditsBtn;
    private Transform resetPrompt;
    private Transform resetConfirmButton;
    private Transform resetCloseButton;

	private SpriteState musicOffState;
	private SpriteState musicOnState;
	private Sprite musicOn;
	private SpriteState soundOffState;
	private SpriteState soundOnState;
	private Sprite soundOn;
	#endregion

    public GameObject loadingScreen;
    public string IntroScene = "Intro";

	public Sprite musicOffpressed;
	public Sprite musicOff;
	public Sprite soundOffpressed;
	public Sprite soundOff;

    public MainMenuBtnSuitcase SuitcaseBtn;

	void Awake(){
		closeBtn = transform.Find("PopUpHolder/BtnClose");
		musicBtn = transform.Find("PopUpHolder/BtnMusic");
		soundBtn = transform.Find("PopUpHolder/BtnSound");
		languageBtn = transform.Find("PopUpHolder/BtnLanguage");
		//resetPlayerBtn = transform.Find("PopUpHolder/BtnResetAccount");
		introBtn = transform.Find("PopUpHolder/BtnIntro");
		creditsBtn = transform.Find("PopUpHolder/BtnCredits");
        resetPrompt = transform.Find("ResetProgressPrompt");
        resetConfirmButton = transform.Find("ResetProgressPrompt/PopUpHolder/Confirm");
        resetCloseButton = transform.Find("ResetProgressPrompt/PopUpHolder/CloseBtn");

		musicOnState.pressedSprite = musicBtn.GetComponent<Button>().spriteState.pressedSprite;
		soundOnState.pressedSprite = soundBtn.GetComponent<Button>().spriteState.pressedSprite;
		musicOn = musicBtn.GetComponent<Image>().sprite;
		soundOn = soundBtn.GetComponent<Image>().sprite;
	}

	void Start () {
		closeBtn.GetComponent<Button>().onClick.AddListener(closeBtn_onClick);
		musicBtn.GetComponent<Button>().onClick.AddListener(musicBtn_onClick);
		soundBtn.GetComponent<Button>().onClick.AddListener(soundBtn_onClick);
		languageBtn.GetComponent<Button>().onClick.AddListener(languageBtn_onClick);
		//resetPlayerBtn.GetComponent<Button>().onClick.AddListener(() => resetPlayerBtn_onClick());
		introBtn.GetComponent<Button>().onClick.AddListener(introBtn_onClick);
		creditsBtn.GetComponent<Button>().onClick.AddListener(creditsBtn_onClick);
        resetConfirmButton.GetComponent<Button>().onClick.AddListener(resetConfirmBtn_onClick);
        resetCloseButton.GetComponent<Button>().onClick.AddListener(resetCancelBtn_onClick);

		musicOffState.pressedSprite = musicOffpressed;
		soundOffState.pressedSprite = soundOffpressed;

        UpdateLanguageText();
		UpdateSoundBtns();
	}

#if DEVELOPMENT
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
            resetConfirmBtn_onClick();
    }
#endif
	
	void closeBtn_onClick(){
		//close popup
		GetComponent<Animator>().SetTrigger("hidePopup");
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
	
	void languageBtn_onClick(){
        LanguageMgr.instance.NextLanguage();
        UpdateLanguageText();
	}
	
	void resetPlayerBtn_onClick(){
        //resetPrompt.gameObject.SetActive(true);
        //resetPrompt.GetComponent<Animator>().SetTrigger("showPopup");
	}

    void resetCancelBtn_onClick()
    {
        //resetPrompt.GetComponent<Animator>().SetTrigger("hidePopup");
    }

    void resetConfirmBtn_onClick()
    {
        SaveData.instance.ResetProfile();
        Session.DestroySingleton();
        resetCancelBtn_onClick();
        closeBtn_onClick();
        SuitcaseBtn.Reset();
    }
	
	void introBtn_onClick(){
		//loadingScreen.SetActive(true);
        //start transition
        Ekstep.Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.watchIntro"));
        SceneMgr.instance.LoadScene(SceneMgr.INTRO);
    }
	
	void creditsBtn_onClick(){
		//loadingScreen.SetActive(true);
		//start transition
        Ekstep.Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.credits"));
		SceneMgr.instance.LoadScene(SceneMgr.CREDITS);
	}

    private void UpdateLanguageText()
    {
        LanguageConfig lang = LanguageMgr.instance.Current;

        Image languageBtnText = languageBtn.FindChild("Text").GetComponent<Image>();
        languageBtnText.sprite = lang.LanguageText;
        languageBtnText.SetNativeSize();

		languageBtn.FindChild("Flag").GetComponent<Image>().sprite = lang.FlagImage;

        Image optionsText = transform.Find("PopUpHolder/Text").GetComponent<Image>();
        optionsText.sprite = lang.OptionsText;
        optionsText.SetNativeSize();

        Image exitText = transform.parent.Find("ExitPrompt/PopUpHolder/Text").GetComponent<Image>();
        exitText.sprite = lang.ReturnToGenieText;
        exitText.SetNativeSize();

        transform.parent.Find("img_Title").GetComponent<Image>().sprite = lang.TitleImage;
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
}
