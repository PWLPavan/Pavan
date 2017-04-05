using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExitPromptLanguage : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Image text = GetComponent<Image>();
        text.sprite = LanguageMgr.instance.Current.ReturnToGenieText;
        text.SetNativeSize();
	}
}
