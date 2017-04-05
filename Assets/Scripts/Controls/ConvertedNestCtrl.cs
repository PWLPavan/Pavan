using UnityEngine;
using System.Collections;
using FGUnity.Utils;

public class ConvertedNestCtrl : CreatureCtrl {

    #region Inspector
    public Sprite whiteChicken;
    public Sprite brownChicken;
    #endregion

    #region Members
    public string[] colors = null;
    #endregion


    #region Ctrl
    override public void Awake () {
        base.Awake();
    }

    override public void Start () {
	
	}

    override public void Update () {
        base.Update();
	}
    #endregion

    
    #region Methods
    public void SetChickenColor(string[] colors) {
        this.colors = colors;

        Transform spriteObj;
        for (int i = 0; i < 10; ++i) {
            spriteObj = inner.transform.FindChild("nestsChicken_" + (i + 1).ToStringLookup());
            if (spriteObj)
                spriteObj.GetComponent<SpriteRenderer>().sprite = (colors[i] == CreatureCtrl.COLOR_BROWN) ? brownChicken : whiteChicken;
        }
    }
    #endregion

}
