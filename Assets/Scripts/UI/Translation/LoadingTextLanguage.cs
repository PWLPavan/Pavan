using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingTextLanguage : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Image>().sprite = LanguageMgr.instance.Current.LoadingText;
		GetComponent<Image>().SetNativeSize();
	}
}
