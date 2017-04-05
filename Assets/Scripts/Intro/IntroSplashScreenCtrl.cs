using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroSplashScreenCtrl : MonoBehaviour {

    #region Inspector
    public GameObject introTransitionCtrl;
	public Transform music;
    #endregion

	#region Gui
	[HideInInspector]
	public Transform startBtn;
	#endregion
	
	// Ctrl
	void Awake () {
		//startBtn = this.transform.Find("StartBtn");

        //introTransitionCtrl.SetActive(false);
    }
	
	void Start () {
		StartBtn_onClick();
		//startBtn.GetComponent<Button>().onClick.AddListener(() => StartBtn_onClick());
	}

	void StartBtn_onClick () {
		this.GetComponent<Animator>().SetTrigger("startGame");
		Camera.main.GetComponent<Animator>().SetBool ("isPlaying", true);

        introTransitionCtrl.SetActive(true);

        //if (!SaveData.instance.MuteMusic)
		    //music.GetComponent<AudioSource>().Play();
    }
}
