using UnityEngine;
using System.Collections;

public class RandomAnimationRange : StateMachineBehaviour {

	// Use as a Behaviour on states that should start randomized
	// Transition Duration must be 0

	public float minRange;
	public float maxRange;
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.Play(stateInfo.tagHash, layerIndex, Random.Range(minRange, maxRange));
	}
}