using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonPressFix : MonoBehaviour, IPointerExitHandler {

	public void OnPointerExit(PointerEventData eventData){
		GetComponent<Animator>().SetTrigger("Normal");
	}
}
