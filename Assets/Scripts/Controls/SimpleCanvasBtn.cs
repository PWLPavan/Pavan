using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SimpleCanvasBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	#region Inspector
	#endregion
	
	#region Members
	[HideInInspector]
	public delegate void OnClickedDelegate(GameObject btn);
	[HideInInspector]
	public OnClickedDelegate onClicked;
	
	public bool isEnabled = true;
	#endregion
	
	#region Input
	public void OnMouseEnabled (bool enable) {
		isEnabled = enable;
	}
	
	public void OnPointerDown (PointerEventData eventData) {
		if (!isEnabled)
			return;
		SoundManager.instance.PlayOneShot(SoundManager.instance.buttonClick);
	}
	
	public void OnPointerUp (PointerEventData eventData) {
		if (!isEnabled)
			return;
        if (onClicked == null)
        {
            Debug.LogWarningFormat("Didn't hook up onClicked feedback for SimpleCanvasBtn on {0}.", this.transform.GetNameInHierarchy());
        }
        else
        {
            onClicked(this.gameObject);
        }
	}
	#endregion

}
