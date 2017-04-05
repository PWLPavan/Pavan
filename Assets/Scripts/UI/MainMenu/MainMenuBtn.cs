using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;

public class MainMenuBtn : MonoBehaviour {

	public GameObject hiddenMenu;

	private Transform menuBtn;

	private float tapCount;
	private float timer;
	
	void Start ()
    {
#if !DEVELOPMENT
        this.gameObject.SetActive(false);
#else
        if (!Debug.isDebugBuild)
            this.gameObject.SetActive(false);
        else
		    this.GetComponent<Button>().onClick.AddListener(tappingCount);
#endif
	}

	void Update (){
		timer += Time.deltaTime;
	}

	void tappingCount (){
		tapCount++;
		//Debug.Log (timer + " " + tapCount);

		if(timer >= .5f){
			tapCount = 0;
		}

		if(tapCount >= 2){
			openMenu();
			tapCount = 0;
		}

		timer = 0;
	}

	void openMenu(){
		hiddenMenu.SetActive(true);
		hiddenMenu.transform.GetComponent<Animator>().SetTrigger("showPopup");
        hiddenMenu.GetComponent<HiddenMenuCtrl>().updateLevelName();
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.open"));
    }
}
