using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResetPlayerPromptCtrl : MonoBehaviour {

	#region Gui
	private Transform closeBtn;
	private Transform confirmBtn;
	#endregion

	void Awake () {
		closeBtn = transform.Find("PopUpHolder/CloseBtn");
		confirmBtn = transform.Find("PopUpHolder/Confirm");
	}
	
	void Start () {
		closeBtn.GetComponent<Button>().onClick.AddListener(closeBtn_onClick);
		confirmBtn.GetComponent<Button>().onClick.AddListener(confirmBtn_onClick);
	}
	
	void closeBtn_onClick(){
		GetComponent<Animator>().SetTrigger("hidePopup");
	}
	
	void confirmBtn_onClick () {
		GetComponent<Animator>().SetTrigger("hidePopup");
	}
}
