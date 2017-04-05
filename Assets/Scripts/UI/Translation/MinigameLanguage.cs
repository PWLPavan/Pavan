using UnityEngine;
using System.Collections;

public class MinigameLanguage : MonoBehaviour {

	// Use this for initialization
	void OnEnable()
    {
        GetComponent<Animator>().SetTrigger(LanguageMgr.instance.Current["title.minigameTrigger"]);
	}
}
