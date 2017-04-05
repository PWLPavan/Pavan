using UnityEngine;
using System.Collections;

public class ShipOnBehavior : StateMachineBehaviour {

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//Debug.Log ("ShipOnBehavior.OnStateEnter()");

		// play new bubble on
		GameObject screen = GameObject.FindGameObjectWithTag("GameScreen");
		screen.GetComponent<MyScreen>().StartLevel();
		
		//Animate Canvas buttons In
		GameObject canvas = GameObject.FindGameObjectWithTag ("Canvas");
		canvas.GetComponent<Animator> ().SetTrigger ("newProblem");
		canvas.GetComponent<Animator> ().SetBool ("correct", false);

		animator.ResetTrigger("newProblem");
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//Debug.Log ("ShipOnBehavior.OnStateExit()");
	}
}
