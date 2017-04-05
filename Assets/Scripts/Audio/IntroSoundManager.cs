using UnityEngine;
using System.Collections;

public class IntroSoundManager : MonoBehaviour {

	#region Inspector
	public AudioSource sfx;

	public AudioClip cityNoise;
	public AudioClip kickPebble;
	public AudioClip birdChirp;
	public AudioClip birdsFlapAway;
	public AudioClip whoosh;
	public AudioClip chickenCrash;
	public AudioClip chickenFlying;
	public AudioClip revealPoster;
	public AudioClip whistle;
	#endregion

	public void PlayOneShot (AudioClip clip, float volume = 1.0f)
    {
		if (clip == null)
			return;
		if (sfx == null)
			return;
        if (SaveData.instance.MuteSound)
            return;
		sfx.PlayOneShot(clip, volume);
	}

	public void StopAudio (){
		transform.parent.Find ("SoundManager").GetComponent<SoundManager>().StopMusic();
	}
}
