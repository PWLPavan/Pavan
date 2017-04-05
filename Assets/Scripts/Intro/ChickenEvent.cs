using UnityEngine;
using System.Collections;

public class ChickenEvent : MonoBehaviour {

	public Transform introSoundManager;

	public void chickenStartFlapping(){
		this.GetComponent<Animator> ().SetTrigger ("startFlapping");
	}

	public void chickenStopFlapping(){
		this.GetComponent<Animator> ().SetTrigger ("stopFlapping");
	}

	public void playSound(AudioClip clip){
		introSoundManager.GetComponent<IntroSoundManager>().PlayOneShot(clip);
	}
}
