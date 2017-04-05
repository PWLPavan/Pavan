using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FGUnity.Utils;
using Ekstep;

public class MainMenuBtnPlay : MonoBehaviour {

    public GameObject loadingScreen;

    void Start () {
        GetComponent<Button>().onClick.AddListener(()=> playBtn_onClick());

        SoundManager.instance.PlayMusicTransition(SoundManager.instance.gameMusic, SoundManager.instance.TransitionTime);
        Genie.I.SyncEvents();
    }

    void playBtn_onClick()
    {
        GetComponent<Image>().enabled = false;

        //loadingScreen.SetActive(true);
        //start transition
        if (SaveData.instance.WatchedIntro)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.play.game"));
            //this.WaitSecondsThen(0.2f, SceneMgr.instance.LoadGameScene);
			SceneMgr.instance.LoadGameScene();
        }
        else
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.play.intro"));
            //this.WaitSecondsThen(0.2f, () => { SceneMgr.instance.LoadScene(SceneMgr.INTRO); });
			SceneMgr.instance.LoadScene(SceneMgr.INTRO);
        }
    }
}
