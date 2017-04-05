using UnityEngine;
using System.Collections;
using FGUnity.Utils;
using Ekstep;

public class DragGroup : MonoBehaviour {

	#region Inspector
	public int value = 0;	// total value of the creatures within group
	#endregion

	#region Gui
	#endregion

	#region Members
	[HideInInspector]
	public bool isDragging = false;
	//[HideInInspector]
	//public bool draggingAllowed = true;

	QueueContainer mContainer;
	public QueueContainer container {
		get { return mContainer; }
		set { mContainer = value; }
	}

	Vector3 mDragDist = new Vector3();
	Vector2 mDragOffset = new Vector2();

	[HideInInspector]
	public int numTens = 0;
	[HideInInspector]
	public int numOnes = 0;

	bool isGreaterThanNine = false;
	bool isMultipleOfTen = false;
	bool isLessThanTen = false;

    public bool isDraggingEnabled = true;
    #endregion

    #region Gui
    #endregion


    #region Ctrl
    void Awake () {
		//mNumFlag = this.transform.Find ("NumFlag");
		//mTotalTextHolder = this.transform.Find ("NumFlag/TotalText");
	}

    public void Cleanup () {
        if (container) {
            container.controller.EndlessDropped(value);
            container.controller.screen.tutorial.HideHandHold();
        }

        // this DragGroup has served its purpose, destroy it
        Dispose();
    }

	void Dispose () {
		if (container) {
			container.RemoveFromQueue();
			container = null;
		}
		Destroy (this.gameObject);
	}
	#endregion


	#region Methods
	public void SetValue (int val) {
		value = val;

		numTens = (int)(value / 10);
		numOnes = (value % 10);

		//Debug.Log("DRAG GROUP: " + numTens + " TENS & " + numOnes + " ONES [" + value + "]");

		// determine placement viability
		isGreaterThanNine = (value > 10);
		isMultipleOfTen = ((value % 10) == 0);
		isLessThanTen = (value < 10);
	}

	void RemoveFromContainerAddToPlaceValue (PlaceValueCtrl placeValueColumn, bool correctedColumn = false) {
		bool valid = false;
		if (placeValueColumn)
			valid = placeValueColumn.AddDragGroup(this);

		if (valid) {
            if (placeValueColumn.value == 1) {
                if (correctedColumn)
                    EnlearnInstance.I.LogActions(EnlearnInstance.Action.AddChickenToTens);
                else
                    EnlearnInstance.I.LogActions(EnlearnInstance.Action.AddToOnes);
            } else if (placeValueColumn.value == 10)
                EnlearnInstance.I.LogActions(EnlearnInstance.Action.AddToTens);

            Cleanup();
		} else {
            CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
            children[0].Drop();
            Cleanup();
            //SnapBack();
		}
	}

	void RemoveFromContainerDistributeToPlaceValue () {
		bool valid = container.controller.screen.GetComponent<MyScreen>().onesColumn.SplitDragGroup(this);
		if (valid) {
			container.controller.screen.GetComponent<MyScreen>().tensColumn.SplitDragGroup(this);

            Cleanup();
		} else {
            CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
            children[0].Drop();
            Cleanup();
            //SnapBack();
		}
	}

	void SnapBack () {
		//SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSnapBack);

		this.transform.position = Vector3.zero;
		this.transform.SetParent(container.transform, false);

		container.controller.EndlessReturn(value);

		// animate creatures back to waiting
		//SetCreaturesTrigger("isWaiting");
		//if (Session.instance.currentLevel.isEndless)
			SetCreaturesAlpha(0.0f);
	}
	#endregion


	#region Messages
	/*void OnCollisionEnter (Collision collision) {
		Debug.Log ("DragGroup.OnCollisionEnter()");
	}*/
	#endregion


	#region Animation
	public void SetCreaturesBool (string param, bool val) {
		CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
		for (int i = 0; i < children.Length; ++i) {
			children[i].SetBool(param, val);
		}
	}

	public void SetCreaturesTrigger (string trigger) {
		CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
		for (int i = 0; i < children.Length; ++i) {
			children[i].SetTrigger(trigger);
		}
	}

	public void SetCreaturesSortOrder (int order = 0) {
        int convertChildSortOrder = order + 1;

        //TODO: MAD RELEASE HACKS, PLZ FIX
		CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
		for (int i = 0; i < children.Length; ++i) {
            if (children[i].GetComponent<ConvertedNestCtrl>() != null) {
                children[i].inner.FindChild("nestBG").GetComponent<SpriteRenderer>().sortingOrder = order;
                //TODO: fixme, only for CreatureTenHolder_Convert
                if (children[i].value == 10) {
                    for (int j = 1; j <= 11; ++j)
                    {
                        Transform child = children[i].inner.transform.Find("nestsChicken_" + j.ToStringLookup());
                        if (child) {
                            child.GetComponent<SpriteRenderer>().sortingOrder = convertChildSortOrder++;
                        }
                    }
                }
                children[i].inner.FindChild("nestsChicken_11").GetComponent<SpriteRenderer>().sortingOrder = convertChildSortOrder++;
            } else {
                children[i].inner.GetComponent<SpriteRenderer>().sortingOrder = order;

                //TODO: fixme, only for CreatureTenHolder_Convert
                if (children[i].value == 10) {
                    for (int j = 1; j <= 11; ++j) {
                        Transform child = children[i].inner.transform.Find("nestsChicken_" + j.ToStringLookup());
                        if (child)
                        {
                            child.GetComponent<SpriteRenderer>().sortingOrder = convertChildSortOrder++;
                        }
                    }
                }
            }
            children[i].seatbelt.transform.FindChild("belt 1").GetComponent<SpriteRenderer>().sortingOrder = convertChildSortOrder;
            children[i].seatbelt.transform.FindChild("belt 2").GetComponent<SpriteRenderer>().sortingOrder = convertChildSortOrder;
            children[i].seatbelt.transform.FindChild("belt 3").GetComponent<SpriteRenderer>().sortingOrder = convertChildSortOrder;
            children[i].seatbelt.transform.FindChild("buckle2").GetComponent<SpriteRenderer>().sortingOrder = convertChildSortOrder;
        }
        
	}

	public void SetCreaturesAlpha (float alpha) {
		CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
		for (int i = 0; i < children.Length; ++i) {
			children[i].inner.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, alpha);
		}
	}
	#endregion


	#region Input
	public void StartDrag () {
        if (isMultipleOfTen)
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.tensQueue"));
        else
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "gameplay.onesQueue"));

        isDragging = true;
		this.transform.SetParent(null, true);

		// change queue location art to being 'in-limbo', temp
		container.EndMove();
		container.Empty();

		container.controller.EndlessDragging(value);

		// animate creatures being picked up
		SetCreaturesBool("dragged", true);
		SetCreaturesSortOrder(1000);
		SetCreaturesAlpha(1.0f);

		//SoundManager.instance.PlayOneShot(SoundManager.instance.chickenDrag);
		SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenDrag);
	}

	public void StopDrag () {
        isDragging = false;

		// change queue location art back to 'containing' a drag group, temp
		container.Filled();

		SetCreaturesBool("dragged", false);
		SetCreaturesSortOrder(0);
	}

	void OnMouseDown () {
		if (!isDragging && isDraggingEnabled) {
			mDragDist = Camera.main.WorldToScreenPoint(this.transform.position);
			mDragOffset.x = Input.mousePosition.x - mDragDist.x;
			mDragOffset.y = Input.mousePosition.y - mDragDist.y;

			StartDrag();
		}
	}

	void OnMouseDrag () {
		if (!isDragging)
			return;

		Vector3 currPos = new Vector3(Input.mousePosition.x - mDragOffset.x,
		                              Input.mousePosition.y - mDragOffset.y,
		                              mDragDist.z);
		Vector3 worldPos = Camera.main.ScreenToWorldPoint(currPos);

		// clamp worldPos to stay within the bounds of the display
		worldPos.x = Mathf.Clamp(worldPos.x,
		                         CameraUtils.cameraRect.xMin + this.GetComponent<SpriteRenderer>().bounds.extents.x,
		                         CameraUtils.cameraRect.xMax - this.GetComponent<SpriteRenderer>().bounds.extents.x);
		worldPos.y = Mathf.Clamp(worldPos.y,
		                         CameraUtils.cameraRect.yMin + this.GetComponent<SpriteRenderer>().bounds.extents.y,
		                         CameraUtils.cameraRect.yMax - this.GetComponent<SpriteRenderer>().bounds.extents.x);
		this.transform.position = worldPos;

        PlaceValueCtrl ones = container.controller.screen.GetComponent<MyScreen>().onesColumn;
        PlaceValueCtrl tens = container.controller.screen.GetComponent<MyScreen>().tensColumn;

		// check for bounding box intersections between group and place value columns
		bool onesPlacement = this.GetComponent<SpriteRenderer>().bounds.Intersects(ones.bounds) && (ones.numCreatures + numOnes + numTens * 10) <= ones.creatureMax
            && (!isMultipleOfTen || ones.allowDragConvert);
		bool tensPlacement = this.GetComponent<SpriteRenderer>().bounds.Intersects(tens.bounds) && (tens.numCreatures + numTens) <= tens.creatureMax && !onesPlacement;

		// force a show of drop states if the number will be distributed between both columns
		if (numOnes != 0 && numTens != 0) {
			onesPlacement = tensPlacement = onesPlacement || tensPlacement;
		}

		// update place value column over states
        ones.UpdateDragOver(onesPlacement, (numOnes == 0 ? numTens * 10 : numOnes));
        if (numTens != 0)
            tens.UpdateDragOver(tensPlacement, numTens);
	}

	void OnMouseUp () {
        if (!isDragging)
            return;
        
		StopDrag();

        PlaceValueCtrl ones = container.controller.screen.GetComponent<MyScreen>().onesColumn;
        PlaceValueCtrl tens = container.controller.screen.GetComponent<MyScreen>().tensColumn;

		// update place value column over states
        ones.UpdateDragOver(false, (numOnes == 0 ? numTens * 10 : numOnes));
		tens.UpdateDragOver(false, numTens);

		// check for bounding box intersections between group and place value columns
        bool onesPlacement = this.GetComponent<SpriteRenderer>().bounds.Intersects(ones.bounds) && (ones.numCreatures + numOnes + numTens * 10) <= ones.creatureMax
            && (!isMultipleOfTen || ones.allowDragConvert);
        bool tensPlacement = this.GetComponent<SpriteRenderer>().bounds.Intersects(tens.bounds) && (tens.numCreatures + numTens) <= tens.creatureMax && !onesPlacement;

		if ((onesPlacement || tensPlacement) && !isMultipleOfTen && isGreaterThanNine)
        {
            //NOTE: this probably doesn't happen any more, should confirm
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.eitherColumn");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.eitherColumn"));
            ones.ForceDragOverFalse(value);
			RemoveFromContainerDistributeToPlaceValue();
		}
        else if (onesPlacement && !tensPlacement && isLessThanTen)
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn"));
            // if on the correct place-value column, snap to place-value column (remove from queue)
            // try to place in ones column
            ones.ForceDragOverFalse(value);
			RemoveFromContainerAddToPlaceValue(ones);
        }
        else if (tensPlacement && !onesPlacement && isMultipleOfTen)
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensColumn");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensColumn"));
            // try to place in tens column
            tens.ForceDragOverFalse(value);
			RemoveFromContainerAddToPlaceValue(tens);
        }
        else if (tensPlacement && !onesPlacement && !isMultipleOfTen)
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensColumn");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.tensColumn"));
            // highlight tens column as incorrect placement (reminder)
            tens.ShowIncorrectFeedback();
            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSnapBack);
            // try to place in ones column
            ones.ForceDragOverFalse(value);
            RemoveFromContainerAddToPlaceValue(ones, true);
        }
        else if (onesPlacement && isMultipleOfTen && ones.numCreatures < 10 && ones.allowDragConvert)
        {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "gameplay.onesColumn"));
            EnlearnInstance.I.LogActions(EnlearnInstance.Action.AddNestToOnes);
            // place a 10s in 1s column
            ones.ForceDragOverFalse(value);

            CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
            bool bIsBrown = children[0].color == CreatureCtrl.COLOR_BROWN;

            SoundManager.instance.PlayOneShot(SoundManager.instance.chickenConvertToOnes);
            ones.Add(10, bIsBrown);
            // add explosion effects
            container.controller.screen.Poof();
            Cleanup();
        }
        else
        {
			if (onesPlacement) {
				// update place value column over states
				ones.ForceDragOverFalse(value);
				ones.GetComponent<Animator>().SetTrigger("correct");
			}
			if (tensPlacement) {
				tens.ForceDragOverFalse(value);
				tens.GetComponent<Animator>().SetTrigger("correct");
			}
            
            // else, if we're overlapping with both or neither
            // snap back to QueueContainer
            //SnapBack();
            CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
            children[0].Drop();
            Cleanup();
        }
	}
	#endregion
    
}
