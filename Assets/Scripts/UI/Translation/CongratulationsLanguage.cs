using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CongratulationsLanguage : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Text>().text = LanguageMgr.instance.Current["title.goodJob"];
		GetComponent<Text>().font = LanguageMgr.instance.Current.Font;
		GetComponent<Text>().lineSpacing = LanguageMgr.instance.Current.LineSpacing;
	}
}
