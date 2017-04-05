using UnityEngine;
using System.Collections;

public class StampRevealSound : MonoBehaviour {

	public void PlayRevealSound(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.stampEarn, 1);
	}
}
