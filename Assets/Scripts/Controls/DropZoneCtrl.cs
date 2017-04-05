using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;
using Ekstep;

public class DropZoneCtrl : MonoBehaviour {

    #region Prefabs
    public GameObject dragGroupPrefab;
    #endregion

    #region Inspector
    public int value = 1;
    public int maxCreatures = 9;

    public PlaceValueCtrl column;
    public PlaceValueCtrl convertColumn;

    public bool isDraggingEnabled = true;
    public bool isDroppingEnabled = true;
    #endregion

    #region Members
    public Bounds bounds {
        get { return this.GetComponent<BoxCollider2D>().bounds; }
    }

    List<CreatureCtrl> _creatures = new List<CreatureCtrl>();
    public List<CreatureCtrl> creatures {
        get { return _creatures; }
    }

    public CreatureCtrl lastCreature {
        get { return _creatures[_creatures.Count - 1]; }
    }
    public bool isFilled {
        get { return _creatures.Count == maxCreatures; }
    }

    public Transform getMountByIdx(int idx) {
        return this.transform.Find("slot" + idx.ToStringLookup());
    }

    [HideInInspector]
    public bool isDraggedOver = false;
    #endregion


    #region Ctrl
    #endregion

    #region Methods
    public void Clear() {
        for (int i = 0; i < _creatures.Count; ++i) {
            Destroy(_creatures[i].gameObject);
        }
        _creatures.Clear();
    }
    
    public void SetHighlight(bool enabled) {
        this.GetComponent<Animator>().SetBool("isHighlighting", enabled);
    }

    public void ShowCorrectFeedback() {
        // trigger correctSeat on all chickens & nests in drop zone
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].SetTrigger("correctSeat");
			_creatures[i].SetSortingLayer(999);
        }
    }
    #endregion


    #region Dropping Chickens
    public void AddDragGroup(DragGroup group) {
        SoundManager.instance.PlayOneShot(SoundManager.instance.counterUpdates);
        // add to end
        CreatureCtrl[] children = group.GetComponentsInChildren<CreatureCtrl>();
        ConvertedNestCtrl[] convertedNests = group.GetComponentsInChildren<ConvertedNestCtrl>();
        int startingSeatIdx = _creatures.Count;
        int convertedNestIdxOffset = 0;
        for (int i = 0; i < children.Length; ++i) {
            /*children[i].transform.localPosition = getMountByIdx(++startingSeatIdx).transform.localPosition;
            children[i].transform.SetParent(this.transform, false);
            children[i].prevLocalPosition = children[i].transform.localPosition;*/
            
                // reparent creature
                children[i].transform.SetParent(this.transform, true);
                children[i].prevLocalPosition = getMountByIdx(++startingSeatIdx).transform.localPosition;

                // have chicken drop to 'ground level'
                //children[i].SetBool("dragged", true);
                children[0].transform.localScale = ChickenSettings.instance.dropScale;// new Vector3(0.66f, 2.5f, 1f);
                children[i].StartMove(new Vector3(children[0].transform.position.x, -2.5f, children[0].transform.position.z), true, ChickenSettings.instance.dropSpeed, true);
                children[i].onMoveEnd = Creature_onFellToGround;
            

            // insure dropped creatures have no anim active but 'idle'
            children[i].SetBool("inColumn", false);
            children[i].SetBool("fullTen", false);
            children[i].SetBool("inCar", true);
            children[i].SetTrigger("reset");

            // sort added creatures in reverse order
            children[i].inner.GetComponent<SpriteRenderer>().sortingOrder = Mathf.Abs(startingSeatIdx - maxCreatures) - convertedNestIdxOffset;
            if (children[i].GetComponent<ConvertedNestCtrl>() != null) {
                convertedNestIdxOffset += 2;
                children[i].inner.FindChild("nestsChicken_11").GetComponent<SpriteRenderer>().sortingOrder = Mathf.Abs(startingSeatIdx - maxCreatures) - convertedNestIdxOffset + 2;
                children[i].inner.FindChild("nestBG").GetComponent<SpriteRenderer>().sortingOrder = Mathf.Abs(startingSeatIdx - maxCreatures) - convertedNestIdxOffset;
                for (int j = 1; j <= 10; ++j)
                    children[i].inner.FindChild("nestsChicken_" + j.ToStringLookup()).GetComponent<SpriteRenderer>().sortingOrder = Mathf.Abs(startingSeatIdx - maxCreatures) + 1 - convertedNestIdxOffset;
            }
            _creatures.Add(children[i]);
        }
    }
    
    void Creature_onFellToGround (SteerableBehavior obj) {
        ((CreatureCtrl)obj).SetBool("dragged", false);
        ((CreatureCtrl)obj).SetBool("isWalking", true);
        ((CreatureCtrl)obj).SetTrigger("plop");

        if (((CreatureCtrl)obj).transform.localPosition.x - ((CreatureCtrl)obj).prevLocalPosition.x < 0) {
            ((CreatureCtrl)obj).transform.localScale = new Vector3(-1, 1, 1);
        } else {
            ((CreatureCtrl)obj).transform.localScale = new Vector3(1, 1, 1);
        }
        
        obj.StartMove(obj.transform.position, true, ChickenSettings.instance.plopWait, false);
        obj.onMoveEnd = Creature_onPlopped;
    }

    void Creature_onPlopped (SteerableBehavior obj) {
        obj.StartMove(((CreatureCtrl)obj).prevLocalPosition, false, ChickenSettings.instance.toSubZoneSpeed, true);
        obj.onMoveEnd = Creature_onArrive;
    }

    void Creature_onArrive (SteerableBehavior obj) {
        ((CreatureCtrl)obj).SetBool("isWalking", false);
        ((CreatureCtrl)obj).SetTrigger("reset");
        obj.onMoveEnd = null;

        ((CreatureCtrl)obj).transform.localScale = new Vector3(1, 1, 1);
    }
    #endregion


    #region Dragging
    void StartDrag() {
        if (value == 1)
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.onesSub");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.onesSub"));
        else if (value == 10)
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.tensSub");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.tensSub"));

        isDragging = true;

        dragGroup = (GameObject)Instantiate(dragGroupPrefab,
                                            Vector3.zero,
                                            Quaternion.identity);

        // if this creature was already moving, kill its movement and set/reset its prevLocalPosition
        if (lastCreature.GetComponent<CreatureCtrl>().onMoveEnd != null) {
            lastCreature.GetComponent<CreatureCtrl>().onMoveEnd = null;
            lastCreature.transform.localPosition = lastCreature.GetComponent<CreatureCtrl>().prevLocalPosition;
            // kill movement
            lastCreature.GetComponent<CreatureCtrl>().EndMove();
        }

        // reparent last creature in list
        lastCreature.prevLocalPosition = lastCreature.transform.localPosition;
        lastCreature.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        lastCreature.transform.SetParent(dragGroup.transform, false);

        // let the dragGroup know the whole value
        dragGroup.GetComponent<DragGroup>().SetValue(value);

        // update creatures drag state
        dragGroup.GetComponent<DragGroup>().SetCreaturesBool("dragged", true);
        dragGroup.GetComponent<DragGroup>().SetCreaturesSortOrder(1000);

        // set dragGroup position
        Vector3 currPos = new Vector3(Input.mousePosition.x,
                                      Input.mousePosition.y,
                                      0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(currPos);
        worldPos.z = 0.0f;
        dragGroup.transform.position = worldPos;

        //SoundManager.instance.PlayOneShot(SoundManager.instance.chickenDrag);
		SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenDrag);
    }

    void StopDrag() {
        isDragging = false;

        // update creatures drag state
        dragGroup.GetComponent<DragGroup>().SetCreaturesBool("dragged", false);
        dragGroup.GetComponent<DragGroup>().SetCreaturesSortOrder(0);
    }

    void SnapBack() {
        if (value == 1)
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesSub.snapBack"));
        else if (value == 10)
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensSub.snapBack"));

        // put creatures back into drop zone
        //SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSnapBack);
        CreatureCtrl[] children = dragGroup.GetComponentsInChildren<CreatureCtrl>();
        children[0].Drop(this.transform);
    }
    #endregion


    #region Input
    public void EnableInput(bool enabled) {
        // enable dragging
        isDraggingEnabled = enabled;
    }

    public void EnableDropping(bool enabled)
    {
        // enable dragging
        isDroppingEnabled = enabled;
    }

    void OnMouseDown() {
        mMouseDown.x = Input.mousePosition.x;
        mMouseDown.y = Input.mousePosition.y;
    }

    Vector2 mDragDist = new Vector2();

    Vector2 mMouseDown = new Vector2();
    Vector2 mMouseCurr = new Vector2();

    GameObject dragGroup;
    bool isDragging = false;

    void OnMouseDrag() {
        if (!isDragging && isDraggingEnabled) {
            mMouseCurr.x = Input.mousePosition.x;
            mMouseCurr.y = Input.mousePosition.y;
            mDragDist = mMouseDown - mMouseCurr;

            if (mDragDist.magnitude > 30 && _creatures.Count > 0) {
                StartDrag();
            }
        } else if (isDragging) {
            // update the drag group position
            Vector3 currPos = new Vector3(Input.mousePosition.x,
                                          Input.mousePosition.y,
                                          0);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(currPos);
            worldPos.z = 0.0f;

            // clamp worldPos to stay within the bounds of the display
            worldPos.x = Mathf.Clamp(worldPos.x,
                                     CameraUtils.cameraRect.xMin + dragGroup.GetComponent<SpriteRenderer>().bounds.extents.x,
                                     CameraUtils.cameraRect.xMax - dragGroup.GetComponent<SpriteRenderer>().bounds.extents.x);
            worldPos.y = Mathf.Clamp(worldPos.y,
                                     CameraUtils.cameraRect.yMin + dragGroup.GetComponent<SpriteRenderer>().bounds.extents.y,
                                     CameraUtils.cameraRect.yMax - dragGroup.GetComponent<SpriteRenderer>().bounds.extents.x);

            if (dragGroup != null)
                dragGroup.transform.position = worldPos;


            // check for bounding box intersections between group and place value subtraction car
            // show hover state
            Bounds dragGroupBounds = dragGroup.GetComponent<SpriteRenderer>().bounds;
            int dragValue = dragGroup.GetComponent<DragGroup>().value;

            bool placement = dragGroupBounds.Intersects(column.bounds);
            bool placementConvert = !placement && convertColumn != null && dragGroupBounds.Intersects(convertColumn.bounds) && (convertColumn.numCreatures + value) <= convertColumn.creatureMax && convertColumn.allowDragConvert;

            //if (placement && column.numCreatures < column.creatureMax)
            if (value == 1)
            {
                column.UpdateDragOver(placement, dragGroup.GetComponent<DragGroup>().numOnes);
            }
            else if (value == 10)
            {
                column.UpdateDragOver(placement, dragGroup.GetComponent<DragGroup>().numTens);
                if (convertColumn != null)
                    convertColumn.UpdateDragOver(placementConvert, dragGroup.GetComponent<DragGroup>().numTens * 10);
            }
        }
    }

    void OnMouseUp() {
        if (!isDragging)
            return;

        StopDrag();

        // check for bounding box intersections between group and place value subtraction car
        Bounds dragGroupBounds = dragGroup.GetComponent<SpriteRenderer>().bounds;
        int dragValue = dragGroup.GetComponent<DragGroup>().value;

        PlaceValueCtrl draggedColumn = null;
        if (dragGroupBounds.Intersects(column.bounds))
            draggedColumn = column;
        else if (convertColumn != null && dragGroupBounds.Intersects(convertColumn.bounds) && (value == 1 || convertColumn.allowDragConvert))
            draggedColumn = convertColumn;

        if (draggedColumn != null)
        {
            bool bFlashAlt = false;
            if (value == 1 && draggedColumn == convertColumn)
            {
                draggedColumn = column;
                bFlashAlt = true;
            }

            int creaturesToAdd = (dragValue / draggedColumn.value);
            if (draggedColumn.numCreatures + creaturesToAdd <= draggedColumn.creatureMax)
            {
                if (draggedColumn.value == 1)
                    //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn");
                    Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn"));
                else
                    //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensColumn");
                    Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensColumn"));

                // remove creatures from list
                CreatureCtrl[] children = dragGroup.GetComponentsInChildren<CreatureCtrl>();
                for (int i = 0; i < children.Length; ++i)
                {
                    _creatures.Remove(children[i]);
                }

                // place into column
                if (draggedColumn.numCreatures > 9)
                {
                    dragGroup.GetComponent<DragGroup>().SetCreaturesBool("fullTen", true);
                }
                dragGroup.GetComponent<DragGroup>().SetCreaturesBool("inColumn", true);
                dragGroup.GetComponent<DragGroup>().SetCreaturesBool("inCar", false);
                dragGroup.GetComponent<DragGroup>().SetCreaturesTrigger("reset");

                if (creaturesToAdd > 1)
                {
                    column.Explode(dragGroup.GetComponent<DragGroup>());
                    draggedColumn.screen.Poof();
                    SoundManager.instance.PlayOneShot(SoundManager.instance.chickenConvertToOnes);
                }
                else
                {
                    draggedColumn.AddDragGroup(dragGroup.GetComponent<DragGroup>());
                    if (bFlashAlt)
                    {
                        SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSnapBack);
                        convertColumn.ShowIncorrectFeedback();
                    }
                }
            }
            else
            {
                SnapBack();
            }
        }
        else
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.sub.snapback");
            //Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.sub.snapback"));
            SnapBack();
        }

        column.ForceDragOverFalse();
        if (convertColumn != null)
        {
            convertColumn.UpdateDragOver(false, 10);
            convertColumn.ForceDragOverFalse();
        }

         // dispose of drag group
         Destroy(dragGroup);
         dragGroup = null;
    }
    #endregion

}
