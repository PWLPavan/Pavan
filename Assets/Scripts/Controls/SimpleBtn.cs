using UnityEngine;
using System.Collections;

public class SimpleBtn : MonoBehaviour {

	#region Inspector
	public Sprite up;
	public Sprite down;
	public Sprite disabled;
	#endregion

	#region Members
	[HideInInspector]
	public delegate void OnClickedDelegate(GameObject btn);
	[HideInInspector]
	public OnClickedDelegate onClicked;

	public bool isEnabled = true;
	#endregion

	#region Ctrl
	#endregion

	#region Input
	public void OnMouseEnabled (bool enable) {
		isEnabled = enable;
		this.GetComponent<SpriteRenderer>().sprite = (isEnabled) ? up : disabled;
	}

	void OnMouseDown () {
		if (!isEnabled)
			return;
		this.GetComponent<SpriteRenderer>().sprite = down;
		SoundManager.instance.PlayOneShot(SoundManager.instance.buttonClick);
	}

	void OnMouseUp () {
		if (!isEnabled)
			return;
		this.GetComponent<SpriteRenderer>().sprite = up;
		onClicked(this.gameObject);
	}
	#endregion

}
