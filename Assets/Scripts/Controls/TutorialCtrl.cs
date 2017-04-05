using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TutorialCtrl : MonoBehaviour {

	#region Prefabs
	public GameObject jitTutorialHandScenePrefab;
	public GameObject jitTutorialHandCanvasPrefab;
	#endregion

	#region Members
	GameObject jitTutorialHandScene;
	GameObject jitTutorialHandCanvas;

	Dictionary<string, GameObject> jitMap;

	Dictionary<string, bool> seenMap;
	Dictionary<string, bool> actionTakenMap;

    public const string HINT_GO = "showHintGo";
    public const string HINT_BORROW = "showHintDragBorrow";
    public const string HINT_CARRY = "showHintDragCarryover";
    public const string HINT_ADD = "showHintDragAdd";
    public const string HINT_ADD_ONE = "showHintDragAddOne";
    public const string HINT_ADD_TEN = "showHintDragAddTen";
    public const string HINT_SUB = "showHintDragSub";
    public const string HINT_SUB_ONE = "showHintDragSubOne";
    public const string HINT_SUB_TEN = "showHintDragSubTen";
    #endregion


    #region Ctrl
    void Awake () {
		jitTutorialHandScene = (GameObject)Instantiate(jitTutorialHandScenePrefab, Vector3.zero, Quaternion.identity);
		jitTutorialHandCanvas = (GameObject)Instantiate(jitTutorialHandCanvasPrefab, Vector3.zero, Quaternion.identity);

		//jitTutorialHandScene.SetActive (false);
		//jitTutorialHandCanvas.SetActive (false);

		jitMap = new Dictionary<string, GameObject>();
		seenMap = new Dictionary<string, bool>();
		actionTakenMap = new Dictionary<string, bool>();

		// add game objects to global map
		AddMadden(jitTutorialHandScene);
		AddMadden(jitTutorialHandCanvas);
	}

	public void Init (Canvas canvasParent, int siblingIndex)
    {
		jitTutorialHandCanvas.transform.SetParent(canvasParent.transform, false);
        jitTutorialHandCanvas.transform.SetSiblingIndex(siblingIndex);
	}

	public void Pause () {
		//jitTutorialHandScene.SetActive(false);
		//jitTutorialHandCanvas.SetActive(false);
		/*Color temp = jitTutorialHandScene.transform.Find ("TutorialHand").GetComponent<SpriteRenderer>().color;
		temp.a = 0f;
		jitTutorialHandScene.transform.Find ("TutorialHand").GetComponent<SpriteRenderer>().color = temp;
		temp = jitTutorialHandCanvas.transform.Find ("TutorialHand").GetComponent<Image>().color;
		temp.a = 0f;
		jitTutorialHandCanvas.transform.Find ("TutorialHand").GetComponent<Image>().color = temp;*/
	}

	public void Resume () {
		//jitTutorialHandScene.SetActive(true);
		//jitTutorialHandCanvas.SetActive(true);
		/*Color temp = jitTutorialHandScene.transform.Find ("TutorialHand").GetComponent<SpriteRenderer>().color;
		temp.a = 1f;
		jitTutorialHandScene.transform.Find ("TutorialHand").GetComponent<SpriteRenderer>().color = temp;
		temp = jitTutorialHandCanvas.transform.Find ("TutorialHand").GetComponent<Image>().color;
		temp.a = 1f;
		jitTutorialHandCanvas.transform.Find ("TutorialHand").GetComponent<Image>().color = temp;*/
	}
	#endregion

	#region Methods
	void AddMadden (GameObject madden) {
		Transform inner = madden.transform.FindChild("TutorialHand");
		AnimatorControllerParameter[] parameters = inner.gameObject.GetComponent<Animator>().parameters;
		// anim trig (key), gameobject (value)
		for (int i = 0; i < parameters.Length; ++i) {
			jitMap.Add(parameters[i].name, inner.gameObject);
			seenMap.Add(parameters[i].name, false);
			actionTakenMap.Add(parameters[i].name, false);
		}
	}

    public void BeginLevelTutorials(GameplayInput input, ConvertNestCtrl convertNestCtrl)
    {
        FGUnity.Utils.Logger.Log("Beginning level tutorials");
        bool hintShown = false;
        hintShown = Show(HINT_BORROW);
        if (hintShown)
        {
            input.EnableAllInput(false, GameplayInput.CONVERT_TO_ONES, GameplayInput.TENS_COLUMN);
            input.EnableCountingAndPause(true);
            if (convertNestCtrl)
                convertNestCtrl.ShowEmptyTenFrame(true);
        }

        hintShown = Show(HINT_SUB);
        if (hintShown)
        {
            input.EnableAllInput(false, GameplayInput.ONES_COLUMN, GameplayInput.ONES_SUB);
            input.EnableCountingAndPause(true);
        }

        hintShown = Show(HINT_SUB_ONE);
        if (hintShown)
        {
            input.EnableAllInput(false, GameplayInput.ONES_COLUMN, GameplayInput.ONES_SUB);
            input.EnableCountingAndPause(true);
        }

        hintShown = Show(HINT_SUB_TEN);
        if (hintShown)
        {
            input.EnableAllInput(false, GameplayInput.TENS_COLUMN, GameplayInput.TENS_SUB);
            input.EnableCountingAndPause(true);
        }
    }

    public bool WillShowLevelTutorials(bool readyForCarryover)
    {
        bool bShowNormal = WillShow(HINT_BORROW) || WillShow(HINT_SUB) || WillShow(HINT_SUB_ONE) || WillShow(HINT_SUB_TEN);

        if (!bShowNormal)
        {
            if (readyForCarryover)
                bShowNormal = WillShow("showHintDragCarryover");
            else
            {
                if (Session.instance.currentLevel.hasTutorial("showHintDragCarryover") && !seenMap["showHintDragCarryover"])
                    bShowNormal = WillShow(Session.instance.currentLevel.tensColumnEnabled ? "showHintDragAddOne" : "showHintDragAdd", true);
            }
        }

        return bShowNormal;
    }

    public bool CarryOverSpecial (bool readyForCarryover, bool handHoldActive, GameplayInput input, ConvertNestCtrl convertNestCtrl) {
        /*
        if (readyForCarryover) {
            bool hintShown = Show("showHintDragCarryover", false, Vector3.zero);
            if (hintShown) {
                input.EnableAllInput(false, GameplayInput.CONVERT_TO_TENS);
                input.EnableCountingAndPause(true);
                if (convertNestCtrl)
                    convertNestCtrl.ToggleVisibility(true, handHoldActive);
                return true;
            }
        } else {
            if (Session.instance.currentLevel.hasTutorial("showHintDragCarryover") && !seenMap["showHintDragCarryover"]) {
                Show((Session.instance.currentLevel.tensColumnEnabled) ? "showHintDragAddOne" : "showHintDragAdd", true);
                input.EnableAllInput(false, GameplayInput.ONES_QUEUE);
                input.EnableCountingAndPause(true);
                return true;
            }
        }*/
        return false;
    }
    
    public bool Show (string key, bool forceShow = false, Vector3 position = default(Vector3), Transform btn = null) {
        if (!forceShow && !Session.instance.currentLevel.hasTutorial(key))
            return false;
        if (jitMap[key] == null) {
			FGUnity.Utils.Logger.Log("Warning: no madden animator bool with the name '" + key + "'");
			return false;
		}
		if (!forceShow && seenMap[key] == true && actionTakenMap[key] == true) {
			//Debug.Log("Warning: jit with the name '" + key + "' already seen");
			return false;
		}

        GameObject jit = jitMap[key];

        FGUnity.Utils.Logger.Log("Showing tutorial '" + key + "' (forceShow=" + forceShow.ToString() + ")");

        HideAll();
        SetShowHint(jit.GetComponent<Animator>(), true);
        jit.GetComponent<Animator>().SetBool(key, true);
		seenMap[key] = true;

		jitTutorialHandScene.transform.position = position;
        jitTutorialHandCanvas.transform.position = position;

        if (btn)
            btn.GetComponent<Animator>().SetBool("isHinting", true);

        return true;
	}

    public bool WillShow(string key, bool forceShow = false)
    {
        if (!forceShow && !Session.instance.currentLevel.hasTutorial(key))
            return false;
        if (jitMap[key] == null)
            return false;
        if (!forceShow && seenMap[key] == true && actionTakenMap[key] == true)
            return false;

        return true;
    }

	public void ActionTaken (string key) {
		if (actionTakenMap[key] == true) {
			//Debug.Log("Warning: jit with the name '" + key + "' action already taken");
			return;
		}
		actionTakenMap[key] = true;
	}
    
    public bool Hide (string key, Transform btn = null) {
        //if (levelIdx != -1 && currentLevelIdx != levelIdx)
        //	return;
        if (jitMap[key] == null) {
            FGUnity.Utils.Logger.Log("Warning: no madden animator bool with the name '" + key + "'");
			return false;
		}
        jitMap[key].GetComponent<Animator>().SetBool(key, false);
        SetShowHint(jitMap[key].GetComponent<Animator>(), false);
        if (btn)
            btn.GetComponent<Animator>().SetBool("isHinting", false);
        if (!seenMap[key])
            return false;
        return true;
    }

    private void SetShowHint(Animator inAnimator, bool inbState)
    {
        if (inAnimator.transform.parent == jitTutorialHandCanvas.transform)
            inAnimator.SetBool("showHint", inbState);
    }

	public void HideAll () {
		foreach (KeyValuePair<string, GameObject> kvp in jitMap) {
			jitMap[kvp.Key].GetComponent<Animator>().SetBool(kvp.Key, false);
		}
	}

	public void ResetSeen () {
        List<string> keys = new List<string>(seenMap.Keys);
        foreach (string key in keys) {
            seenMap[key] = false;
        }
	}

	public void ResetActionTaken () {
        List<string> keys = new List<string>(actionTakenMap.Keys);
        foreach (string key in keys) {
            actionTakenMap[key] = false;
        }
    }

    public bool HideHandHold () {
        if (!Session.instance.currentLevel.hasTutorial("showHintDragCarryover") || seenMap["showHintDragCarryover"]) {
            Hide(HINT_ADD);
            ActionTaken(HINT_ADD);

            Hide(HINT_ADD_ONE);
            ActionTaken(HINT_ADD_ONE);
        }

        bool hidden = false;

        hidden = Hide(HINT_ADD_TEN) || hidden;
        ActionTaken(HINT_ADD_TEN);

        hidden = Hide(HINT_SUB) || hidden;
        ActionTaken(HINT_SUB);

        hidden = Hide(HINT_SUB_ONE) || hidden;
        ActionTaken(HINT_SUB_ONE);

        hidden = Hide(HINT_SUB_TEN) || hidden;
        ActionTaken(HINT_SUB_TEN);

        return hidden;
    }
	#endregion

}
