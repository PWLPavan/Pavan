using UnityEngine;
using System.Collections;
using Ekstep;
using FGUnity.Utils;

public class CreatureCtrl : SteerableBehavior {

	#region Inspector
	public int value = 1;
    #endregion

    #region Gui
    [HideInInspector]
	public Transform inner;
	public Bounds bounds {
		get { return inner.GetComponent<SpriteRenderer>().bounds; }
	}

    public Transform seatbelt;
    #endregion

    #region Members
    [HideInInspector]
	public bool selected = false;

	[HideInInspector]
	public Vector3 prevLocalPosition = new Vector3();

    public const string COLOR_BROWN = "Brown";
    public const string COLOR_WHITE = "White";
    public const string COLOR_GOLD = "Gold";
    public string color = COLOR_BROWN;
	#endregion


	#region Ctrl
	override public void Awake () {
        //TODO: don't hardcode this
        if (value == 1) {
            inner = this.transform.Find("CreatureOneInner");
            seatbelt = this.transform.FindChild("onesSeatbelt");
        }
        if (value == 10) {
            inner = this.transform.Find("CreatureTenInner");
            seatbelt = this.transform.FindChild("tensSeatbelt");
        }
	}

	override public void Start () {

	}

	override public void Update () {
        base.Update();
    }
	#endregion


	#region Methods
	public void Select (bool value) {
		selected = value;
		SetBool("selected", value);
	}
	#endregion
    

	#region Animations
	public void SetTrigger (string trigger) {
		if (inner) {
			inner.GetComponent<Animator>().SetTrigger(trigger);
		}
	}

    public void ResetTrigger(string trigger)
    {
        if (inner)
        {
            inner.GetComponent<Animator>().ResetTrigger(trigger);
        }
    }

	public void SetBool (string param, bool val) {
		if (inner) {
			inner.GetComponent<Animator>().SetBool(param, val);
		}
	}
    #endregion

    #region Seatbelts
    private bool m_SeatbeltQueued = false;

    public void SetSeatbelt (bool seatbelted, bool overrideColor = false) {
        if (color == COLOR_WHITE && !overrideColor)
            return;

        if (seatbelted && IsMoving && !isSeatbelted)
        {
            m_SeatbeltQueued = true;
        }
        else
        {
            seatbelt.GetComponent<Animator>().SetBool("isSeatbelted", seatbelted);
            SetBool("isSeatbelted", seatbelted);
        }
    }

    public bool isSeatbelted {
        get { return seatbelt.GetComponent<Animator>().GetBool("isSeatbelted"); }
    }

    public void UpdateQueuedSeatbelt()
    {
        if (m_SeatbeltQueued)
        {
            seatbelt.GetComponent<Animator>().SetBool("isSeatbelted", m_SeatbeltQueued);
            SetBool("isSeatbelted", m_SeatbeltQueued);
            m_SeatbeltQueued = false;
            this.WaitSecondsThen(0.25f, () => { SoundManager.instance.PlayOneShot(SoundManager.instance.chickenSeatbeltBuckle); });
        }
    }
    #endregion
    
    #region ISteerable Methods
    /*override public void EndMove() {
        if (color == COLOR_ADDED) {
            color = COLOR_WHITE;
        }
        base.EndMove();
    }*/
    /*override public void EndMove() {
        state = STATE_IDLE;
        if (onMoveEnd != null)
            onMoveEnd(this);
    }*/
    #endregion

    public void SetSortingLayer(int inLayer)
    {
        if (this.GetComponent<ConvertedNestCtrl>() != null)
        {
            this.inner.FindChild("nestBG").GetComponent<SpriteRenderer>().sortingOrder = inLayer++;
            //TODO: fixme, only for CreatureTenHolder_Convert
            if (this.value == 10)
            {
                for (int j = 1; j <= 11; ++j)
                {
                    Transform child = this.inner.transform.Find("nestsChicken_" + j.ToStringLookup());
                    if (child)
                    {
                        child.GetComponent<SpriteRenderer>().sortingOrder = inLayer++;
                    }
                }
            }
            this.inner.FindChild("nestsChicken_11").GetComponent<SpriteRenderer>().sortingOrder = inLayer++;
        }
        else
        {
            this.inner.GetComponent<SpriteRenderer>().sortingOrder = inLayer++;

            //TODO: fixme, only for CreatureTenHolder_Convert
            if (this.value == 10)
            {
                for (int j = 1; j <= 11; ++j)
                {
                    Transform child = this.inner.transform.Find("nestsChicken_" + j.ToStringLookup());
                    if (child)
                    {
                        child.GetComponent<SpriteRenderer>().sortingOrder = inLayer++;
                    }
                }
            }
        }
        this.seatbelt.transform.FindChild("belt 1").GetComponent<SpriteRenderer>().sortingOrder = inLayer;
        this.seatbelt.transform.FindChild("belt 2").GetComponent<SpriteRenderer>().sortingOrder = inLayer;
        this.seatbelt.transform.FindChild("belt 3").GetComponent<SpriteRenderer>().sortingOrder = inLayer;
        this.seatbelt.transform.FindChild("buckle2").GetComponent<SpriteRenderer>().sortingOrder = inLayer;
    }


    #region Dropping Chickens
    bool isReturningToSeat = false;
    Transform parent = null;
    public void Drop (Transform returnTransform = null, bool plop = true) {
        isReturningToSeat = (returnTransform != null);
        parent = returnTransform;
        
        // reparent creature &
        // have chicken drop to 'ground level'
        if (isReturningToSeat) {
            this.transform.SetParent(parent, true);
        } else {
            //Genie.instance.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.thrownAway");
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.thrownAway"));
            this.transform.SetParent(null, true);
        }

        UpdateQueuedSeatbelt();

        if (plop) {
            this.transform.localScale = ChickenSettings.instance.dropScale;//new Vector3(0.66f, 2.5f, 1f);
            Vector3 pos = new Vector3(this.transform.position.x, -2.5f, this.transform.position.z);
            this.StartMove(pos, true, ChickenSettings.instance.dropSpeed, true);
            this.onMoveEnd = Creature_onFellToGround;
        } else if (isSeatbelted) {
            this.StartMove(prevLocalPosition, false, ChickenSettings.instance.seatbeltSpeed, true);
            this.onMoveEnd = Creature_onSnappedBack;
        } else {
            PrepWalkAnims(plop && isReturningToSeat);
            this.StartMove(prevLocalPosition, false, ChickenSettings.instance.snapBackSpeed, true);
            //this.StartMove(pos, true, ChickenSettings.instance.dropSpeed, true);
            this.onMoveEnd = Creature_onSnappedBack;
        }
    }
    
    void Creature_onFellToGround (SteerableBehavior obj) {
        // calculate which way to run
        // move off screen
        PrepWalkAnims(!isReturningToSeat);
        this.SetTrigger("plop");
        this.StartMove(this.transform.position, true, ChickenSettings.instance.plopWait, false);
        this.onMoveEnd = Creature_onPlopped;

        UpdateQueuedSeatbelt();
    }

    void Creature_onPlopped (SteerableBehavior obj) {
        
        if (isReturningToSeat) {
            this.StartMove(prevLocalPosition, false, ChickenSettings.instance.snapBackSpeed, true);
            this.onMoveEnd = Creature_onSnappedBack;
        } else {
            float xPos = (this.transform.position.x > 0f) ? 10f : -10f;
            Vector3 pos = new Vector3(xPos, this.transform.position.y, this.transform.position.z);
            this.StartMove(pos, true, ChickenSettings.instance.leaveSpeed);
            this.onMoveEnd = Creature_onLeaveScreen;
        }

        UpdateQueuedSeatbelt();
    }

    void Creature_onSnappedBack(SteerableBehavior obj) {
        this.SetBool("isWalking", false);
        this.SetTrigger("reset");
        this.transform.localScale = (value == 1) ? new Vector3(1f, 1f, 1f) : new Vector3(1.28f, 1.28f, 1f);
        //TODO: set local position
        //TODO: set local position (when picked up each time)
        //obj.transform.localPosition = obj.prevLocalPosition;
        this.onMoveEnd = null;
        this.transform.localPosition = this.prevLocalPosition;
        //TODO: should we set local (or global) to be SteerableBehavior.targetPosition instead?

        if (isSeatbelted) {
            seatbelt.GetComponent<Animator>().SetTrigger("dragFeedback");
            this.SetTrigger("plop");
        }

        UpdateQueuedSeatbelt();
    }

    void Creature_onLeaveScreen (SteerableBehavior obj) {
        // destroy creature
        this.onMoveEnd = null;
        Destroy(this.gameObject);
    }

    void PrepWalkAnims(bool global = true) {
        this.SetBool("dragged", false);
        this.SetBool("isWalking", true);
        this.SetTrigger("reset");

        float xPos = (global) ? (this.transform.position.x > 0f) ? 10f : -10f :
                                (this.transform.localPosition.x > 0f) ? -10f : 10f;
        if (this.transform.localPosition.x - xPos < 0) {
            this.transform.localScale = new Vector3(-1, 1, 1);
        } else {
            this.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    #endregion

}
