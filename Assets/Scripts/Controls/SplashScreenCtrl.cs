using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SplashScreenCtrl : MonoBehaviour, IPointerDownHandler
{
	#region Gui
	#endregion
	
	#region Members
	[HideInInspector]
	public delegate void OnOffDelegate();
	[HideInInspector]
	public OnOffDelegate onCtrlOff;
	#endregion

    private bool m_HasTouched = false;
	
	#region Input

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_HasTouched)
            return;

        m_HasTouched = true;
        this.GetComponent<Animator>().SetTrigger("newGame");
        this.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.SplashToOff"), Ctrl_off);
    }
	
	void Ctrl_off ()
    {
		this.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.SplashToOff"), Ctrl_off);
		onCtrlOff();
	}

	#endregion
}
