using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;

public class HiddenMenuCtrl : MonoBehaviour {

	#region Inspector
	public GameObject loadingScreen;
	public GameObject myScreen;
	public Transform newUserPopup;
	#endregion

	#region Gui
	private Transform closeBtn;
    private Transform homeBtn;
    private Transform skipBtn;
    private Transform prevBtn;
    private Transform newUserBtn;
    private Transform levelText;
    private Transform userText;
    private Transform nestGameBtn;
    private Transform groupGameBtn;
    #endregion

    void Awake () {
		closeBtn = this.transform.Find("PopUpHolder/CloseBtn");
		homeBtn = this.transform.Find ("PopUpHolder/MainMenuBtn");
		skipBtn = this.transform.Find ("PopUpHolder/SkipBtn");
		prevBtn = this.transform.Find ("PopUpHolder/PrevBtn");
		newUserBtn = this.transform.Find ("PopUpHolder/NewUserBtn");
		levelText = this.transform.Find ("PopUpHolder/Level");
        userText = this.transform.Find("PopUpHolder/UserID");
        nestGameBtn = this.transform.Find("PopUpHolder/NestMinigameBtn");
        groupGameBtn = this.transform.Find("PopUpHolder/GroupMinigameBtn");

        updateLevelName();
        UpdateUser(Genie.I.UserID);
    }

	void Start () {
		closeBtn.GetComponent<Button>().onClick.AddListener(closeBtn_onClick);
		homeBtn.GetComponent<Button>().onClick.AddListener(homeBtn_onClick);
		skipBtn.GetComponent<Button>().onClick.AddListener(skipBtn_onClick);
		prevBtn.GetComponent<Button>().onClick.AddListener(prevBtn_onClick);
		newUserBtn.GetComponent<Button>().onClick.AddListener(newUserBtn_onClick);
        nestGameBtn.GetComponent<Button>().onClick.AddListener(nestGameBtn_onClick);
        groupGameBtn.GetComponent<Button>().onClick.AddListener(groupGameBtn_onClick);
	}

	//Buttons
	void closeBtn_onClick(){
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.close"));
        this.GetComponent<Animator>().SetTrigger("hidePopup");
    }

	void homeBtn_onClick(){
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.home"));
        StartCoroutine(Transition());
	}

	void skipBtn_onClick(){
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.skip"));
        myScreen.transform.GetComponent<MyScreen>().SkipLevel();

        WaitForLoad();
	}

	void prevBtn_onClick(){
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.previous"));
        myScreen.transform.GetComponent<MyScreen>().PrevLevel();

        WaitForLoad();
	}

	void newUserBtn_onClick()
    {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.newUser"));
        newUserPopup.gameObject.SetActive(true);
		newUserPopup.GetComponent<Animator>().SetTrigger("showPopup");
        newUserPopup.GetComponent<NewUserPopUpCtrl>().onNewUser = UpdateUser;
    }

    void nestGameBtn_onClick()
    {
        Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.make10game"));
        Application.LoadLevel("Make10Prototype");
    }

    void groupGameBtn_onClick()
    {
        Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.expressionGame"));
        Application.LoadLevel("ExpressionPrototype");
    }
	
	//Text
    void UpdateUser (string id) {
        userText.GetComponent<Text>().text = "User: #" + id;
    }

	public void updateLevelName(){
		levelText.GetComponent<Text>().text = "Level: " + Session.instance.currentLevel.expression;
	}

	//Transition
	IEnumerator Transition()
    {
		loadingScreen.gameObject.SetActive (true);
		yield return new WaitForSeconds(.2f);
		SceneMgr.instance.LoadScene(SceneMgr.MAIN_MENU);
	}

    IEnumerator WaitToFinishLoading()
    {
        EnableInput(false);
        while (!Session.instance.IsNextLevelLoaded)
            yield return null;
        yield return new WaitForSeconds(0.1f);
        updateLevelName();
        EnableInput(true);
    }

    public void WaitForLoad()
    {
        this.StartCoroutine(WaitToFinishLoading());
    }

    private void EnableInput(bool inbEnable)
    {
        foreach (var btn in GetComponentsInChildren<Button>())
            btn.enabled = inbEnable;
    }
}
