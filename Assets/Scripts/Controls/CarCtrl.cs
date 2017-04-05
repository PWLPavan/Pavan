using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;

public class CarCtrl : MonoBehaviour {

	#region Inspector
	public int value = 1;
    
    public GameObject dropZonePrefab;
	#endregion

	#region Gui
	[HideInInspector]
	public GameObject inner;
	public Bounds bounds {
		get { return inner.GetComponent<SpriteRenderer>().bounds; }
	}

	[HideInInspector]
	public Transform seats;
	#endregion

	#region Members
	[HideInInspector]
	public int size = 1;

	[HideInInspector]
	public bool isFilled = false;

	string mValueStr = "";
	List<Transform> mSeats;
	#endregion


	#region Ctrl
	void Awake () {
		if (value == 1)
			mValueStr = "One";
		else if (value == 10)
			mValueStr = "Ten";
	}

	public void SetSize (int sz) {
		if (mSeats != null) {
			mSeats.Clear();
		}
		if (inner) {
			Destroy (inner);
			inner = null;
		}

		size = sz;
		if (size == 0)
		    return;
        
		inner = (GameObject)Instantiate(dropZonePrefab, Vector3.zero, Quaternion.identity);
		inner.transform.SetParent(this.transform, false);
		
		// collect available seats
		seats = inner.transform.Find ("Seats");
        mSeats = new List<Transform>();
        for (int i = 1; i <= size; ++i) {
            mSeats.Add(seats.Find("CarSeat" + mValueStr + i.ToStringLookup()));
		}
	}
	#endregion

	#region Methods
	public void AddDragGroup (DragGroup group) {
		SoundManager.instance.PlayOneShot(SoundManager.instance.counterUpdates);

		CreatureCtrl[] children = group.GetComponentsInChildren<CreatureCtrl>();
		int startingSeatIdx = this.GetComponentsInChildren<CreatureCtrl>().Length;
		for (int i = 0; i < children.Length; ++i) {
			children[i].transform.localPosition = mSeats[startingSeatIdx++].transform.localPosition;
			children[i].transform.SetParent(seats.transform, false);
		}

		if (startingSeatIdx == size)
			isFilled = true;
	}

	public void AddCreature (CreatureCtrl creature) {
	}

	public void Clear () {
		CreatureCtrl[] children = this.GetComponentsInChildren<CreatureCtrl>();
		for (int i = 0; i < children.Length; ++i) {
			Destroy (children[i].gameObject);
		}
		isFilled = false;
		if (inner) {
			Destroy (inner);
			inner = null;
		}
	}

	public void SetHighlight (bool enabled) {
		if (inner)
			inner.GetComponent<Animator>().SetBool("carHighlight", enabled);
	}
	#endregion

}
