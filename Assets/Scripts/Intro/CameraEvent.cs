using UnityEngine;
using System.Collections;
using FGUnity.Utils;

public class CameraEvent : MonoBehaviour {

	public Transform boredBird;

	public Transform bird1;
	public Transform bird2;
	public Transform bird3;
	public Transform bird4;
	public Transform bird5;

	public Transform pebble;

	public Transform chicken1;

	public Transform chickenBG;

	public Transform crowd;

	public Transform busStop;

	public Transform CameraPoster;
	public Transform CameraIntro;

	public Transform pilotPoster;

	public Transform planeHolder;
	public Transform chickenPicture;

	public Transform loadingScreen;

	public Transform birdsFlyOff;

	public Transform introSoundManager;

	bool isAnimating = true;
	bool isTransitioning = false;

	public void PlayBirdsFlyOff(){
		birdsFlyOff.GetComponent<Animator>().SetTrigger ("isPlaying");
	}

	//Set the bored bird anim
	public void PlayBored () {
		//boredBird.GetComponent<Animator> ().SetBool ("isBored", true);
		boredBird.GetComponent<Animator> ().SetTrigger ("play");
		Logger.Log("start bored anim");
	}

	//Have birds look at posters
	public void LookAtPoster(){
		bird1.GetComponent<Animator> ().SetTrigger ("lookAtPoster");
		bird2.GetComponent<Animator> ().SetTrigger ("lookAtPoster");
		bird3.GetComponent<Animator> ().SetTrigger ("lookAtPoster");
		bird4.GetComponent<Animator> ().SetTrigger ("lookAtPoster");
		bird5.GetComponent<Animator> ().SetTrigger ("lookAtPoster");
	}

	public void BlueBirdFlyAway(){
		bird1.GetComponent<Animator> ().SetTrigger ("flyAway");
		bird2.GetComponent<Animator> ().SetTrigger ("flyAway");
		bird3.GetComponent<Animator> ().SetTrigger ("flyAway");
		bird4.GetComponent<Animator> ().SetTrigger ("flyAway");
		bird5.GetComponent<Animator> ().SetTrigger ("flyAway");
		boredBird.GetComponent<Animator> ().SetTrigger ("flyAway");
	}

	public void startFlying(){
		this.GetComponent<Animator> ().SetTrigger ("flyAway");
	}

	public void kickedPebble(){
		pebble.GetComponent<Animator> ().SetTrigger ("kickedPebble");
	}

	public void chickenEntrance(){
		chicken1.GetComponent<Animator> ().SetTrigger ("entrance");
	}

	public void chickenShowBG(){
		chickenBG.GetComponent<Animator> ().SetTrigger ("showBG");
	}

	public void chickenHideBG(){
		chickenBG.GetComponent<Animator> ().SetTrigger ("hideBG");
		chicken1.GetComponent<Animator> ().SetTrigger ("startFall");
	}

	public void crowdReact(){
		crowd.GetComponent<Animator> ().SetTrigger ("react");
	}

	public void busStopStartGlow(){
		busStop.GetComponent<Animator> ().SetTrigger ("startGlow");
	}

	public void toCamPoster(){
		CameraPoster.gameObject.SetActive (true);
		CameraIntro.gameObject.SetActive (false);
	}

	public void showPoster(){
		pilotPoster.GetComponent<Animator> ().SetTrigger ("showPoster");
	}

	public void startPlane(){
		planeHolder.GetComponent<Animator> ().SetTrigger ("startPlane");
	}

	public void holdPicture(){
		chickenPicture.GetComponent<Animator> ().SetTrigger ("holdPicture");
	}

	public void jumpInPlane(){
		planeHolder.GetComponent<Animator> ().SetTrigger ("jumpIn");
	}

	public void getLadder(){
		planeHolder.GetComponent<Animator> ().SetTrigger ("getLadder");
	}

	public void startLoading(){
		//loadingScreen.gameObject.SetActive (true);
		//transition ();
	}

	public void transition(){
        if (SaveData.instance.WatchedIntro)
        {
            SceneMgr.instance.LoadScene(SceneMgr.MAIN_MENU);
        }
        else
        {
            SceneMgr.instance.LoadGameScene();
            SaveData.instance.WatchedIntro = true;
        }
		isTransitioning = true;
		isAnimating = false;
	}

	public void playMusic(AudioClip clip){
		introSoundManager.GetComponent<IntroSoundManager>().PlayOneShot(clip, .5f);
	}

	public void stopMusic(){
		SoundManager.instance.StopMusic();
	}
}
