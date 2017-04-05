using UnityEngine;
using System.Collections;

public class PlaneSounds : MonoBehaviour {
	public void audioPlaneEnter(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.planeEnter);
	}

	public void audioPlaneExit(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.planeExit);
	}
}