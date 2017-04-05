using UnityEngine;
using System.Collections;

public class KiwiSounds : MonoBehaviour {
	public void KiwiChirp(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.kiwiChirp);
	}

	public void kiwiCount(){
		SoundManager.instance.PlayRandomOneShot(SoundManager.instance.kiwiCount, .4f);
	}
}
