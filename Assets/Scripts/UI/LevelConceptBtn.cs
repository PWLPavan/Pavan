using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FGUnity.Utils;
using Ekstep;

public class LevelConceptBtn : MonoBehaviour {

    #region Inspector
    public MyScreen screen;
    public int gotoLevelIdx;
    #endregion
	
    #region Ctrl
	void Start () {
        this.GetComponent<Button>().onClick.AddListener(btn_onClick);
    }

    void OnDestroy () {
        this.GetComponent<Button>().onClick.RemoveAllListeners();
    }
    
    #endregion


    #region Input
    void btn_onClick() {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "debug.skipToLevel" + gotoLevelIdx.ToStringLookup()));
        if (screen)
            screen.GotoLevel(gotoLevelIdx);

        this.transform.parent.parent.GetComponent<HiddenMenuCtrl>().WaitForLoad();
    }
    #endregion

}
