using UnityEngine;
using FGUnity.Utils;
using System.Collections;
using System.Collections.Generic;
using Ekstep;

public class ConvertNestCtrl : MonoBehaviour {

    #region Prefabs
    public GameObject nestConvertDragPrefab;
    #endregion

    #region Inspector
    public GameObject convertHolder;
    public GameObject highlight;
    public GameObject kiwi;

    public PlaceValueCtrl onesColumn;

    public Sprite goldNestSprite;
    public Sprite brownNestSprite;
    public Sprite whiteNestSprite;

    float dragThresholdPercent = 0.8f;
    #endregion

    #region Members
    public bool isDraggingEnabled = true;
    public bool isDragging = false;

    public bool isResetting = false;

    GameObject _dragNest;

    Vector3 _dragDist = new Vector3();
    Vector2 _dragOffset = new Vector2();

    Vector2 _animatorOffset = new Vector2(1.05f, 0f);

    float _prevHighlightPercent = 0f;
    float _highlightPercent = 0f;
    float _maxHighlightPercent = 0.0f;
    
    Transform _originalParent;
    Vector3 _originalPosition = new Vector3();
    Vector3 _nestWorldPos;

    SpriteRenderer m_GoldFrame;

    public bool isVisible { get { return _isVisible; } }

    private bool _isVisible = false;

    [HideInInspector]
    public delegate void CreaturesPicked (List<int> creatures);
    [HideInInspector]
    public CreaturesPicked onCreaturesPicked;
    #endregion

    #region Ctrl
	void Start () {
        _originalParent = this.transform.parent;
        _originalPosition.x = this.transform.position.x;
        _originalPosition.y = this.transform.position.y;
        _originalPosition.z = this.transform.position.z;

        m_GoldFrame = _originalParent.FindChild("highlight").GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!isDragging)
            return;

        if (!Input.GetMouseButton(0))
            StopDrag();
        else
            UpdateDragging();
    }

    #endregion

    #region Coloring
    bool hasBrownChickens () {
        return hasColoredChickens(CreatureCtrl.COLOR_BROWN);
    }

    bool hasWhiteChickens() {
        return hasColoredChickens(CreatureCtrl.COLOR_WHITE);
    }

    bool hasColoredChickens(string color)
    {
        int maxChickens = onesColumn.creatures.Count;
        if (maxChickens > 10)
            maxChickens = 10;
        for (int i = 0; i < maxChickens; ++i) {
            if (onesColumn.creatures[i].GetComponent<CreatureCtrl>().color == color)
                return true;
        }
        return false;
    }

    Sprite getColoredSprite () {
        bool brown = hasBrownChickens();
        bool white = hasWhiteChickens();
        if (brown && white) {
            return goldNestSprite;
        } else if (brown) {
            return brownNestSprite;
        } else if (white) {
            return whiteNestSprite;
        }
        return goldNestSprite;
    }
    #endregion

    #region Input
    public void ShowEmptyTenFrame(bool inbShowTenFrame)
    {
        _originalParent.GetComponent<Animator>().SetBool("isHighlightingNoArrows", inbShowTenFrame);
    }

    public bool ScreenPositionWithinTenFrame(Vector3 inMousePosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(inMousePosition);
        worldPoint.z = m_GoldFrame.transform.position.z;
        return m_GoldFrame.bounds.Contains(worldPoint);
    }

    public void EnableInput (bool enable) {
        isDraggingEnabled = enable;
    }

    public void ToggleVisibility (bool visible, bool hintingActive) {
        //isDraggingEnabled = visible; //so the player can't drag immediately after dragging a chicken
        _isVisible = visible;
        if (visible && isDraggingEnabled) {

            this.GetComponent<SpriteRenderer>().sprite = getColoredSprite();

            if (_originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderOff") ||
                _originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderToOff_NoNest") ||
                _originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderToOff")) {
                _originalParent.GetComponent<Animator>().SetTrigger("showNest");
                _originalParent.GetComponent<Animator>().SetBool("draggingNest", false);

                if (Session.instance.currentLevel.isSubtractionProblem && !hintingActive) {
                    // show just the frame
                    _originalParent.GetComponent<Animator>().SetBool("isHighlighting", false);
                    _originalParent.GetComponent<Animator>().SetBool("isHighlightingNoArrows", true);
                } else {
                    // show arrows
                    _originalParent.GetComponent<Animator>().SetBool("isHighlighting", true);
                    _originalParent.GetComponent<Animator>().SetBool("isHighlightingNoArrows", false);
                }
            }
        } else {
            if (_originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderToOn") ||
                _originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderIdle") ||
			    _originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderDragging") ||
                _originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderIdleNoPulse")) {

                if (!_originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderOff") &&
                !_originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderToOff_NoNest") &&
                !_originalParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.NestHolderToOff")) {
                    _originalParent.GetComponent<Animator>().SetTrigger("hideNest");
                }
            }
            //_originalParent.GetComponent<Animator>().SetBool("draggingNest", false);
            _originalParent.GetComponent<Animator>().SetBool("isHighlighting", false);
            _originalParent.GetComponent<Animator>().SetBool("isHighlightingNoArrows", false);
        }
    }

    public void StartDrag()
    {
        _dragDist = Camera.main.WorldToScreenPoint(this.transform.position);
        _dragOffset.x = Input.mousePosition.x - _dragDist.x;
        _dragOffset.y = Input.mousePosition.y - _dragDist.y;

        this.transform.SetParent(null, true);
        isDragging = true;

        _dragNest = (GameObject)Instantiate(nestConvertDragPrefab, Vector3.zero, Quaternion.identity);
        _dragNest.transform.position = this.transform.position;
        var spriteRenderer = _dragNest.transform.FindChild("NestConvertDrag/nest").GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = getColoredSprite();

		convertHolder.GetComponent<Animator>().SetBool("draggingNest", true);
        _maxHighlightPercent = 0.0f;


        //TODO: KIWI SFX
        //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.nestConvert");
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.nestConvert"));

        onesColumn.Unbuckle();

        UpdateDragging();
    }

    public void StopDrag()
    {
        if (!isDragging)
            return;

        isDragging = false;
        //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.nestConvert");
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.nestConvert"));

        if (_highlightPercent < dragThresholdPercent) {
            convertHolder.GetComponent<Animator>().SetBool("draggingNest", false);
            SnapBack();
            onesColumn.ConvertSnapBack();
        }else{
			this.WaitOneFrameThen(() => {
				convertHolder.GetComponent<Animator>().SetBool("draggingNest", false); }
			);
		
		}

        onesColumn.Buckle();
    }

    void SnapBack() {
        this.transform.position = _originalPosition;
        this.transform.SetParent(_originalParent, true);
        isDragging = false;

        if (_dragNest) {
            Destroy(_dragNest);
            _dragNest = null;
        }
    }
    
    void OnMouseDown() {
        if (!isDragging && isDraggingEnabled && _isVisible)
        {
            StartDrag();
        }
    }

    void UpdateDragging()
    {
        if (!isDragging || !isDraggingEnabled)
            return;

        Vector3 currPos = new Vector3(Input.mousePosition.x/* - _dragOffset.x*/,
                                      Input.mousePosition.y/* - _dragOffset.y*/,
                                      _dragDist.z);
        _nestWorldPos = Camera.main.ScreenToWorldPoint(currPos);

        // clamp worldPos to stay within the bounds of the convert nest highlight
        _nestWorldPos.x = Mathf.Clamp(_nestWorldPos.x,
                                 -highlight.GetComponent<SpriteRenderer>().bounds.extents.x + highlight.transform.position.x,
                                 highlight.GetComponent<SpriteRenderer>().bounds.extents.x + highlight.transform.position.x);
        _nestWorldPos.y = highlight.transform.position.y; // retain nest x and z, but reparenting causes position issues fixed by _animatorOffset
        _nestWorldPos.z = this.transform.position.z;

        this.transform.position = _nestWorldPos;
        //kiwi.transform.position = _nestWorldPos;
        _dragNest.transform.position = _nestWorldPos;

        _highlightPercent = 0.5f - ((_dragNest.transform.position.x - highlight.transform.position.x) / ((highlight.transform.position.x + highlight.GetComponent<SpriteRenderer>().bounds.extents.x) -
                                                                 (highlight.transform.position.x - highlight.GetComponent<SpriteRenderer>().bounds.extents.x)));
        // grab chickens (starting from the bottom)
        //   groups: 5 & 10, 4 & 9, 3 & 8, 2 & 7, 1 & 6

        // 5 groups
        //      1: 5, 10    (10%)
        //      2: 4, 9     (30%)
        //      3: 3, 8     (50%)
        //      4: 2, 7     (70%)
        //      5: 1, 6     (90%)

        var animator = _dragNest.GetComponentInChildren<Animator>();
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).tagHash, 0, _highlightPercent);

        // if we've dragged far enough to pick up more creatures, find out which ones and get them from the placeValueCtrl
        if (_highlightPercent > _maxHighlightPercent)
        {
            bool bNewCaptured = false;
            // reparent and 'drag' all creatures that fall under _highlightPercent
            if (_highlightPercent > 0.65f)
            {
                onesColumn.ConvertCapture(_dragNest.transform, _dragNest.transform.FindChild("NestConvertDrag"), 4, 9, 3, 8, 2, 7, 1, 6, 0, 5);
                bNewCaptured = _maxHighlightPercent <= 0.65f;
            }
            else if (_highlightPercent > 0.5f)
            {
                onesColumn.ConvertCapture(_dragNest.transform, _dragNest.transform.FindChild("NestConvertDrag"), 4, 9, 3, 8, 2, 7, 1, 6);
                bNewCaptured = _maxHighlightPercent <= 0.5f;
            }
            else if (_highlightPercent > 0.4f)
            {
                onesColumn.ConvertCapture(_dragNest.transform, _dragNest.transform.FindChild("NestConvertDrag"), 4, 9, 3, 8, 2, 7);
                bNewCaptured = _maxHighlightPercent <= 0.4f;
            }
            else if (_highlightPercent > 0.2f)
            {
                onesColumn.ConvertCapture(_dragNest.transform, _dragNest.transform.FindChild("NestConvertDrag"), 4, 9, 3, 8);
                bNewCaptured = _maxHighlightPercent <= 0.2f;
            }
            else if (_highlightPercent > 0.01f)
            {
                onesColumn.ConvertCapture(_dragNest.transform, _dragNest.transform.FindChild("NestConvertDrag"), 4, 9);
                bNewCaptured = _maxHighlightPercent <= 0.01f;
            }

            if (bNewCaptured)
            {
                SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenDrag);
                animator.SetTrigger("collect");
            }

            _maxHighlightPercent = _highlightPercent;
        }

        _prevHighlightPercent = _highlightPercent;

        // if we've passed the rectangle threshold for a successful conversion
        if (_highlightPercent > dragThresholdPercent)
        {
            // continue conversion
            convertHolder.GetComponent<Animator>().SetBool("draggingNest", true);
            onesColumn.onShift(onesColumn);

            SnapBack();
        }
    }
    #endregion

}
