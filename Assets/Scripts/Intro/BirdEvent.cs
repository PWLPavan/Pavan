using UnityEngine;
using System.Collections;

public class BirdEvent : MonoBehaviour {

	public Transform introSoundManager;

	public void startFlapping(){
		this.GetComponent<Animator> ().SetTrigger ("startFlapping");
	}

	public void playSound(AudioClip clip){
		introSoundManager.GetComponent<IntroSoundManager>().PlayOneShot(clip, .5f);
	}
}
