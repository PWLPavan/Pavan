using UnityEngine;
using System.Collections;

/**
 * Simple timer counter for cooldowns.
 * Has a ton of options and can be set in the editor.
 *
 * Usage
 * =====
 * Set up the timer in the inspector or in the class.
 *
 * maxTime = time until timer first fires
 * fireForeverDelay = delay in the firing loop after first fire.
 *
 * Accumulate and fire check at once with:
 * if (timer.AccumulateFireOnce(Time.deltaTime)) { something(); }
 *
 * -or-
 *
 * Accumulate and then fire on a timed loop with:
 * if (timer.AccumulateFireLoop(Time.deltaTime)) { something(); }
 *
 * -Eric
 */

[System.Serializable]
public class CooldownTimer
{
	[Tooltip("The cooldown period in seconds.")]
	public float maxTime;
	
	[Tooltip("The loop delay for Fire Forever mode.")]
	public float fireForeverDelay;
	
	private float cooldown;
	
	public bool fired {get; private set;}
	
	public CooldownTimer(float maxTime, float fireForeverDelay = 0)
	{
		this.maxTime = maxTime;
		this.fireForeverDelay = fireForeverDelay;
		Reset();
	}
	
	public CooldownTimer()
	{
		Reset();
	}
	
	/**
	 * Reset the cooldown and fire state.
	 */
	public void Reset()
	{
		Reset(maxTime);
	}
	
	/**
	 * Reset with custom cooldown time for only this cycle.
	 */
	private void Reset(float maxTime)
	{
		cooldown = maxTime;
		fired = false;
	}
	
	/**
	 * Advance the cooldown by a certain amount of time.
	 */
	public void Accumulate(float timeDelta)
	{
		if (cooldown > 0)
			cooldown -= timeDelta;
	}
	
	/**
	 * Advance the cooldown by Time.deltaTime
	 */
	public void Accumulate()
	{
		Accumulate(Time.deltaTime);
	}
	
	/**
	 * Test if the cooldown is ready to fire.
	 */
	public bool canFire
	{
		get
		{
			return cooldown <= 0;
		}
	}
	
	/**
	 * Advance the timer and also do a fire check.
	 * Will never fire again unless reset.
	 */
	public bool AccumulateFireOnce(float timeDelta)
	{
		if (fired && noLoop) return false;
		
		Accumulate(timeDelta);
		
		if (canFire)
		{
			fired = true;
			return true;
		}
		
		return false;	
	}
	
	/**
	 * Advance the timer and also do a fire check.
	 * Will always fire after firing once.
	 * If you want a custom delay between each fire,
	 * set the fireForeverDelay field;
	 */
	public bool AccumulateFireLoop(float timeDelta)
	{
		if (fired && noLoop) return true;
		
		var fire = AccumulateFireOnce(timeDelta);
		
		if (fire)
		{
			Reset(fireForeverDelay);
			fired = true;
		}
		
		return fire;
	}

	/**
	 * Time until fire.
	 */
	public float timeRemaining
	{
		get
		{
			return cooldown;
		}
	}
	
	/**
	 * Cooldown ratio with maxTime
	 */
	public float completionRatio
	{
		get
		{
			if (cooldown <= 0) return 1;
			return 1 - (cooldown / maxTime);
		}
	}
	
	/**
	 * Is the loop delay zero?
	 */
	public bool noLoop
	{
		get
		{
			return fireForeverDelay == 0;
		}
	}
	
	/**
	 * Cooldown ratio with fireForeverDelay.
	 */
	public float loopCompletionRatio
	{
		get
		{
			if (canFire || noLoop) return 1;
			return 1 - (cooldown / fireForeverDelay);
		}
	}
	
	/**
	 * Cool the timer instantly. Ready to fire.
	 */
	public void Cool()
	{
		cooldown = 0;
	}
}
