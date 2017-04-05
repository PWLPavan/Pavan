using UnityEngine;
using System.Collections;

public class ChickenSounds : MonoBehaviour
{
	public void audioChickenHappy()
    {
        var animator = GetComponent<Animator>();
        bool bIsAngry = animator.GetBool("fullTen") && !animator.GetBool("inTen");
        if (bIsAngry)
            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenAngryCount);
        else
		    SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenCount);
	} 

	public void audioNestHappy()
    {
		SoundManager.instance.PlayRandomOneShot(SoundManager.instance.nestCount);
	}
}
