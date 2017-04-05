using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CanvasBtn : MonoBehaviour, IPointerDownHandler {

    public virtual void OnPointerDown (PointerEventData eventData) {
        Button button = GetComponent<Button>();
        if (!button.IsInteractable())
            return;
        SoundManager.instance.PlayOneShot(SoundManager.instance.buttonClick);
    }

}
