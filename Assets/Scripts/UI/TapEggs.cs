using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FGUnity.Utils;
using Ekstep;

public class TapEggs : MonoBehaviour
{
	private CoroutineHandle secondTap;

	public void OnEggTap()
    {
        if (!AllowTap)
            return;

        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.tapEggs"));

		if(transform.parent.FindChild("egg1").GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.eggsIdle")){
			SoundManager.instance.PlayRandomOneShot(SoundManager.instance.hudTapEgg, .5f);
			transform.parent.FindChild("egg1").GetComponent<Animator>().SetTrigger("tapped");
			secondTap.Clear();
			if(Session.instance.eggsEarned != 1)
				secondTap = this.WaitSecondsThen(.25f, TapSecondEgg);
		}
	}

    public bool AllowTap = true;
	
	private void TapSecondEgg(){
		transform.parent.FindChild("egg2").GetComponent<Animator>().SetTrigger("tapped");
		SoundManager.instance.PlayRandomOneShot(SoundManager.instance.hudTapEgg, .5f);
	}
}
