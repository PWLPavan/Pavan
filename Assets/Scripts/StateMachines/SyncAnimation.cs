using UnityEngine;
using System.Collections;

public class SyncAnimation : StateMachineBehaviour {
	private float startPoint;
	
	// Use to override the random frame Behaviour
	// Transition Duration must be 0
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		startPoint = 0f;
		animator.Play(stateInfo.tagHash, layerIndex, startPoint);
	}
}
