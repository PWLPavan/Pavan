using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;

public class NewUserPopUpCtrl : MonoBehaviour {

	#region Gui
	private Transform closeBtn;
    private Transform confirmBtn;
    #endregion

    public delegate void OnNewUserDelegate(string id);
    public OnNewUserDelegate onNewUser;


    void Awake () {
		closeBtn = this.transform.Find("PopUpHolder/CloseBtn");
        confirmBtn = this.transform.Find("PopUpHolder/Confirm");
    }
	
	void Start () {
		closeBtn.GetComponent<Button>().onClick.AddListener(closeBtn_onClick);
        confirmBtn.GetComponent<Button>().onClick.AddListener(confirmBtn_onClick);
    }

	void closeBtn_onClick(){
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "newUser.close"));
        this.GetComponent<Animator>().SetTrigger("hidePopup");
	}

    void confirmBtn_onClick ()
    {		
		this.GetComponent<Animator>().SetTrigger("hidePopup");
	}
}
