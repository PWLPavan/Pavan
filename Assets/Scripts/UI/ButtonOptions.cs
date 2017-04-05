using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;

public class ButtonOptions : MonoBehaviour {

	public GameObject optionsPopup;
	
	void Start () {
		GetComponent<Button>().onClick.AddListener(()=> OpenOptions());
	}

	void OpenOptions () {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.options"));
		optionsPopup.SetActive(true);
		optionsPopup.GetComponent<Animator>().SetTrigger("showPopup");
	}
}
