using UnityEngine;
using System.Collections;

public class RandomizeAnimation : StateMachineBehaviour {

	// Use as a Behaviour on states that should start randomized
	// Transition Duration must be 0
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		animator.Play(stateInfo.tagHash, layerIndex, Random.value);
	}
}
