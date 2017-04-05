using UnityEngine;
using System.Collections;
using Ekstep;

public class PilotSounds : MonoBehaviour {
	public void audioPilotHappy(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.pilotHappy, .2f);
	}
	public void audioPilotSad(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.pilotSad, .2f);
	}
	public void audioPilotSayProblem(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.pilotSayProblem, .2f);
	}
	public void audioPilotAngry(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.pilotAngry, .2f);
	}
	public void audioPilotFixed(){
		SoundManager.instance.PlayOneShot(SoundManager.instance.pilotFixed, .2f);
	}

    public bool AllowTap = true;

    void OnMouseDown()
    {
        if (AllowTap){
			if(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.PilotIdle") ||
			   GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.PilotUnhappy") )
            {
                Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.tapPilot"));
		    	GetComponent<Animator>().SetTrigger("press");
			}
		}
	}
}
