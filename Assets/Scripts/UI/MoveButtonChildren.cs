using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class MoveButtonChildren : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler {

    private float distance = 28f;
    private bool isDown = false;
    private int pointerDowns = 0;

    void Start(){

    }
    
    public void OnPointerDown(PointerEventData eventData){
        moveChildren();
        ++pointerDowns;
    }

    public void OnPointerUp(PointerEventData eventData){
        --pointerDowns;
        if(pointerDowns < 1) resetChildren();
    }

    public void OnPointerExit(PointerEventData eventData){
        resetChildren();
    }

    public void OnPointerEnter(PointerEventData eventData){
        if(pointerDowns > 0) moveChildren();
    }

    void moveChildren(){
        if(isDown) return;
        else isDown = true;

        foreach(Transform child in transform){
            child.position = new Vector3(child.position.x, child.position.y-distance, child.position.z);
        }
    }

    void resetChildren(){
        if(!isDown) return;
        else isDown = false;
        foreach(Transform child in transform){
            child.position = new Vector3(child.position.x, child.position.y+distance, child.position.z);
        }
    }
}
