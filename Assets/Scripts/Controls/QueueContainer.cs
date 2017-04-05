using UnityEngine;
using System.Collections;

public class QueueContainer : SteerableBehavior {

	#region Gui
	[HideInInspector]
	public Transform dragGroup;
	#endregion

	#region Members
	public const string STATE_PICKUP = "pickup";

	public QueueController controller { get; set; }

	[HideInInspector]
	public delegate void ContainerEndMove(QueueContainer container);
	[HideInInspector]
	public ContainerEndMove onContainerEndMove;
	#endregion


	#region Ctrl
	override public void Awake () {
		base.Awake ();

		dragGroup = this.transform.Find ("DragGroupPref");
		dragGroup.GetComponent<DragGroup>().container = this;
	}

	override public void Start () {
		base.Start ();

	}

	override public void Update () {
		base.Update ();

	}
	#endregion


	#region Methods
	public void Filled () {
		this.GetComponent<Animator>().SetBool("dragged", false);
	}

	public void Empty () {
		this.GetComponent<Animator>().SetBool("dragged", true);
	}

	public void RemoveFromQueue () {
		controller.RemoveFromQueue(this);
	}
	#endregion


	#region Steerables
	override public void StartMove (Vector3 targetPosition, bool isGlobal = false, float spd = 3.0f, bool factorDist = false) {
		base.StartMove(targetPosition, isGlobal);
		//TODO: once there's a transition, SetBool("waiting", false);
	}

	override public void EndMove () {
		base.EndMove();
		dragGroup.GetComponent<DragGroup>().SetCreaturesBool("waiting", true);
		if (onContainerEndMove != null)
			onContainerEndMove(this);
	}
	#endregion


	#region Input

	#endregion


}
