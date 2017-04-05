using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FGUnity.Utils;

public class MainMenuTitleSprite : MonoBehaviour {

	// Use this for initialization
	void Start () {
        UpdateLanguageText();
#if UNITY_EDITOR
        // Just in case
        this.WaitOneFrameThen(UpdateLanguageText);
#endif
	}

	private void UpdateLanguageText()
	{
		GetComponent<Image>().sprite = LanguageMgr.instance.Current.TitleImage;
	}
}
