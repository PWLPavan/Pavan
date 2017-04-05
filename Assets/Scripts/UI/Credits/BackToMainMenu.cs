using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackToMainMenu : MonoBehaviour {

	public GameObject loadingScreen;
	
	void Start () {
		GetComponent<Button>().onClick.AddListener(returnBtn_onClick);
	}
	
	void returnBtn_onClick(){
		//loadingScreen.SetActive(true);
		SceneMgr.instance.LoadScene(SceneMgr.MAIN_MENU);
	}
}
