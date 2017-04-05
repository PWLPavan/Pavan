using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;

public class IntroTransition : MonoBehaviour {

    #region Inspector
	public Transform loadingScreen;
    #endregion


    #region Members
    bool isAnimating = true;
    bool isTransitioning = false;
    #endregion

	#region Ctrl
	void Start () {
		this.GetComponent<Button>().onClick.AddListener(skipBtn_onClick);
	}

    IEnumerator Transition () {
		if (isTransitioning){}else{
        
			//loadingScreen.gameObject.SetActive (true);

			//yield return new WaitForSeconds(.2f);

            if (SaveData.instance.WatchedIntro)
            {
                SceneMgr.instance.LoadScene(SceneMgr.MAIN_MENU);
            }
            else
            {
                SceneMgr.instance.LoadGameScene();
                SaveData.instance.WatchedIntro = true;
            }
	        isTransitioning = true;
	        isAnimating = false;
		}
		yield break;
    }

	void skipBtn_onClick(){
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "intro.skip"));
		StartCoroutine(Transition());
			this.enabled = false;
    }
    #endregion
}
