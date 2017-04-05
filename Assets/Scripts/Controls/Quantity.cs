using UnityEngine;
using System.Collections;

public class Quantity : MonoBehaviour {
	/*
	#region Members
	string mState;
	public const string STATE_QUEUE_IDLE = "idle";
	public const string STATE_QUEUE_MOVE = "move";
	public const string STATE_PICKUP = "move";

	float t;
	Vector3 mStart;
	Vector3 mTarget;

	QueueController mController;
	public QueueController controller {
		get { return mController; }
		set { mController = value; }
	}

	public GameObject dragPrefab;
	GameObject dragger;
	#endregion

	void Awake () {
		mState = STATE_QUEUE_IDLE;
		this.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
	}

	// Update is called once per frame
	void Update () {
	
		switch (mState) {
			case STATE_QUEUE_MOVE:
				Move ();
				break;
		}

	}

	void Move () {
		t += Time.deltaTime;
		this.transform.position = Vector3.Lerp(mStart, mTarget, t);
		
		if (this.transform.position == mTarget) {
			mState = STATE_QUEUE_IDLE;
			this.transform.rotation = Quaternion.identity;
		} else if (Random.value > 0.8f) {
			this.transform.Rotate(Vector3.forward, 45);
		}
	}

	public void StartMove (Vector3 newPos) {
		mStart = this.transform.position;
		mTarget = newPos;

		mState = STATE_QUEUE_MOVE;
		t = 0;
	}

	Vector3 dist;
	float posX;
	float posY;
	void OnMouseDown () {
		dragger = (GameObject)Instantiate(dragPrefab, this.transform.position, Quaternion.identity);
		dragger.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
		dist = Camera.main.WorldToScreenPoint(transform.position);
		posX = Input.mousePosition.x - dist.x;
		posY = Input.mousePosition.y - dist.y;
	}
	
	void OnMouseDrag () {
		Vector3 currPos = new Vector3(Input.mousePosition.x - posX, Input.mousePosition.y - posY, dist.z);
		Vector3 worldPos = Camera.main.ScreenToWorldPoint(currPos);
		dragger.transform.position = worldPos;
	}

	void OnMouseUp () {
		controller.RemoveQuantity(this);
	}
	*/

}
