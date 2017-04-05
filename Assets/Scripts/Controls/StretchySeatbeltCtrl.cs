using UnityEngine;
using System.Collections;

public class StretchySeatbeltCtrl : MonoBehaviour {

    #region Prefab
    #endregion

    #region Gui
    public GameObject seatbeltHolderOnes;  //Stretchy seat belt prefabs are in Screen > Ship > SeatbeltStretchHolder
    public GameObject seatbeltHolderTens;
    #endregion

    #region Members
    public Vector2 xScaleRange;

    GameObject _seatbeltHolderRef;

    Vector3 _beltScale = new Vector3(1, 1, 1);
    Quaternion _beltRot = new Quaternion();

    float _origWidth = 0;
    Vector3 _seatPos;

    public bool isStretching = false;
    public bool isVisible = false;
    #endregion


    #region Ctrl
    void Awake() {
        _beltRot = Quaternion.identity;
        _origWidth = seatbeltHolderOnes.transform.FindChild("beltR/beltSprite").GetComponent<SpriteRenderer>().bounds.size.x;
    }
    #endregion

    #region Methods
    public void Begin (int value, Vector3 pos) {
        _seatbeltHolderRef = (value == 1) ? seatbeltHolderOnes : seatbeltHolderTens;
        _seatPos = pos;
        SetPosition(pos);
        isStretching = true;
        SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSeatbeltStretch);
    }

    public void End () {
        if (!isStretching)
            return;
        ShowBelts(false);
        isStretching = false;
        SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSeatbeltImpact);
    }

    void ShowBelts (bool visible) {
        if (_seatbeltHolderRef == null)
            return;
        _seatbeltHolderRef.transform.FindChild("beltR").gameObject.SetActive(visible);
        _seatbeltHolderRef.transform.FindChild("beltL").gameObject.SetActive(visible);
        isVisible = visible;
    }

    void SetPosition (Vector3 pos) {
        if (_seatbeltHolderRef == null)
            return;
        //position "SeatbeltStretchHolder" on the picked up chicken's seat
        _seatbeltHolderRef.transform.position = pos;
    }

    public void UpdateBelts (Vector3 mousePos) {
        if (_seatbeltHolderRef == null)
            return;

        // wait to show the belts until a mouse move event has occured
        if (!isVisible)
            ShowBelts(true);

        Vector3 mouseV = mousePos - _seatPos;
        float distance = mouseV.magnitude;
        
        // re-position scaled sprites
        SetPosition((mousePos + _seatPos) / 2);

        //Scale "beltR" and "beltL" on the y axis when dragging a seatbelted chicken
        _beltScale.y = (distance / _origWidth);
        //Scale "beltR" and "beltL" on the x axis the farther away, the smaller (narrower) it should get
        _beltScale.x = MathUtilities.Remap(distance, 0, 5, xScaleRange.x, xScaleRange.y);
        if (_beltScale.x < xScaleRange.y)
            _beltScale.x = xScaleRange.y;
        _seatbeltHolderRef.transform.FindChild("beltR").localScale = _beltScale;
        _seatbeltHolderRef.transform.FindChild("beltL").localScale = _beltScale;

        //Rotate "beltR" and "beltL" to follow the dragged chicken
        float degrees = Mathf.Rad2Deg * Mathf.Atan2(mouseV.y, mouseV.x);
        _beltRot = Quaternion.Euler(0f, 0f, 90f + degrees);
        _seatbeltHolderRef.transform.FindChild("beltR").rotation = _beltRot;
        _seatbeltHolderRef.transform.FindChild("beltL").rotation = _beltRot;
    }


    //set isSeatbelted to true when dragging a seatbelted chicken (to prevent swaying)

    //Upon release, quickly slide the chicken back to it's seat and when it has reached its seat, trigger dragFeedback on the seat belt animator

    #endregion

}
