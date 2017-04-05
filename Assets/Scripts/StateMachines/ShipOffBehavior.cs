using UnityEngine;
using System.Collections;

public class ShipOffBehavior : StateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//Debug.Log ("ShipOffBehavior.OnStateEnter()");

		//Animate Canvas buttons Out
		GameObject canvas = GameObject.FindGameObjectWithTag ("Canvas");
		canvas.GetComponent<Animator> ().SetBool ("correct", true);
		
		//Animate Success Chickens
		GameObject successChicken1 = GameObject.Find ("Chicken_Success 1/Chicken_Traffic");
		GameObject successChicken2 = GameObject.Find ("Chicken_Success 2/Chicken_Traffic");
		GameObject successChicken3 = GameObject.Find ("Chicken_Success 3/Chicken_Traffic");

        successChicken1.GetComponent<Animator>().enabled = true;
		successChicken1.GetComponent<Animator>().Play("Chicken_Traffic", -1, 0f);

        successChicken2.GetComponent<Animator>().enabled = true;
		successChicken2.GetComponent<Animator>().Play("Chicken_Traffic", -1, -.05f);

        successChicken3.GetComponent<Animator>().enabled = true;
        successChicken3.GetComponent<Animator>().Play("Chicken_Traffic", -1, -.1f);
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//Debug.Log ("ShipOffBehavior.OnStateExit()");
	}

}
