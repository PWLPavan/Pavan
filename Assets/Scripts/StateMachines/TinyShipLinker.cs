using UnityEngine;
using System.Collections;

public class TinyShipLinker : MonoBehaviour
{
	public HudCtrl hud;

    private void Awake()
    {
        /*
        if (Optimizer.instance.SuperLow)
        {
            Transform effectRoot = this.transform.Find("effects");
            foreach (Transform child in effectRoot)
            {
                child.gameObject.SetActive(false);
            }
        }*/
    }

	public void TriggerPolaroid()
    {
        hud.ShowLevelComplete();
    }
}
