using UnityEngine;
using System.Collections;
using FGUnity.Utils;

public class CabinCounterCtrl : MonoBehaviour {
	//TODO: Animator states: revisit: correct, incorrect, checkTen, checkOne
	
	#region Gui
	public Transform counterHolder;
	public Transform numTen;
	public Transform numOne;
	public Transform starburst;
	
	public Transform kiwiHolder;
	public Transform kiwiInner;
	
	public Transform pilotBubble;
	public Transform pilot;
	
	public Transform onesColumn;
	public Transform tensColumn;
	
	//public Transform mainCamera;
	#endregion
	
	#region Members
	int mTotalCount = 0;
	int mTensCount = 0;
	int mOnesCount = 0;
	
	Color _textColor;
	
	public Vector3 midTensPosition = new Vector3(1.26f, -3.56f, 0f);
	public Vector3 midOnesPosition = new Vector3(1.26f, -3.56f, 0f);
	public Vector3 cockpitTensPosition = new Vector3(-7.41f, -0.85f, 0f);
	public Vector3 cockpitOnesPosition = new Vector3(-7.41f, -0.85f, 0f);
	public Vector3 offPosition = new Vector3(100f, 100f, 0f);
	
	[HideInInspector]
	public delegate void CabinCounting ();
	[HideInInspector]
	public CabinCounting onReadyToEval;
	
	#endregion

    private float m_CountSpeed = 1.0f;
	
	void Awake () {
		counterHolder = this.transform.Find ("CounterHolder");
		numTen = counterHolder.transform.Find ("CabinNumTen");
		numOne = counterHolder.transform.Find ("CabinNumOne");
		starburst = counterHolder.transform.Find ("Starburst");
		
		kiwiHolder = this.transform.Find ("KiwiHolder");
		kiwiInner = kiwiHolder.transform.Find ("kiwi");
		
		pilotBubble = transform.parent.FindChild ("GoalHolder/Bubble2");
		pilot = transform.parent.FindChild ("GoalHolder/PilotHolder/Pilot");
		
		onesColumn = transform.parent.FindChild ("ColumnOne");
		tensColumn = transform.parent.FindChild ("ColumnTen");
		
		_textColor = numTen.GetComponent<TextMesh>().color;
	}
	
	void Start () {
		this.transform.position = offPosition;
		
		counterHolder.GetComponent<Animator>().SetTrigger("counterToOff");
		kiwiHolder.GetComponent<Animator>().SetTrigger("attendantToOff");
	}
	
	public void Show (int submittedAnswer)
	{
		this.transform.position = (Session.instance.currentLevel.tensColumnEnabled) ? midTensPosition : midOnesPosition;
		
		if (Session.instance.currentLevel.isSingleDigitProblem) {
			GetComponent<Animator>().SetTrigger("beginCountOnes");
			counterHolder.GetComponent<Animator>().SetBool("isLarge", false);
			
			// display single digit kiwi bubble
			counterHolder.GetComponent<Animator>().SetTrigger("onesBubble");
			
			// if player's answer is single digit on single digit problem
			counterHolder.GetComponent<Animator>().SetBool("digitSingle", (submittedAnswer < 10));
			counterHolder.GetComponent<Animator>().SetBool("digitSingleDouble", false);
			
			kiwiHolder.GetComponent<Animator>().SetBool ("isSingle", true);
			
			if(!Session.instance.currentLevel.usesQueue){
				kiwiHolder.GetComponent<Animator>().SetTrigger("flipLeft");
				Logger.Log ("flipped");
			}
			
		} else if (Session.instance.currentLevel.isDoubleDigitProblem) {
			GetComponent<Animator>().SetTrigger("beginCountTens");
			counterHolder.GetComponent<Animator>().SetBool("isLarge", true);
			
			// display double digit kiwi bubble
			counterHolder.GetComponent<Animator>().SetTrigger("tensBubble");
			
			// if player's answer is single digit on double digit problem
			counterHolder.GetComponent<Animator>().SetBool("digitSingle", false);
			counterHolder.GetComponent<Animator>().SetBool("digitSingleDouble", (submittedAnswer < 10));
			
			kiwiHolder.GetComponent<Animator>().SetBool ("isSingle", false);
		}
		
		//Set the kiwi's bubble to the large version
		counterHolder.GetComponent<Animator>().SetTrigger("refreshCounter");
		counterHolder.GetComponent<Animator>().SetTrigger("stretchBubble");
		
		AlphaText(numTen, 0.3f);
		AlphaText(numOne, 0.3f);
		
		//counterHolder.GetComponent<Animator>().SetBool("isCounting", true);
		counterHolder.GetComponent<Animator>().SetTrigger("counterToOn");
		kiwiHolder.GetComponent<Animator>().SetTrigger("attendantToOn");

		if(Session.instance.currentLevel.twoPartProblem)
			GetComponent<Animator>().SetBool("isTwoPart", true);
		else
			GetComponent<Animator>().SetBool("isTwoPart", false);
	}
	
	public void ForceAnswerText(int inAnswer)
	{
		numTen.GetComponent<TextMesh>().text = (inAnswer / 10).ToStringLookup();
		numOne.GetComponent<TextMesh>().text = (inAnswer % 10).ToStringLookup();
		
		if (Session.instance.currentLevel.isDoubleDigitProblem){
			AlphaText(numTen, 1.0f);
		}

		AlphaText(numOne, 1.0f);
	}
	
	public void OnLevelStart(Level inLevel)
	{
		bool bShouldRespectChickens = inLevel.isSingleDigitProblem
			&& inLevel.usesSubZone;
		kiwiHolder.GetComponent<Animator>().SetBool("isOnesPlaneSub", bShouldRespectChickens);
		GetComponent<Animator>().SetBool("isOnesPlaneSub", bShouldRespectChickens);
	}

    public void SetCountMultiplier(float inTimeMultiplier)
    {
        m_CountSpeed = 1.0f / inTimeMultiplier;
    }
	
	public void Reset () {
		//counterHolder.GetComponent<Animator>().SetBool("isCounting", false);
		counterHolder.GetComponent<Animator>().SetBool("checkTen", false);
		counterHolder.GetComponent<Animator>().SetBool("checkOne", false);
		counterHolder.GetComponent<Animator>().SetBool("checkAnswer", false);
		//counterHolder.GetComponent<Animator>().SetBool("isLarge", false);
		
		//reset Kiwi conditions
		kiwiHolder.GetComponent<Animator>().SetBool("wrongAnswer", false);
		kiwiHolder.GetComponent<Animator>().SetBool("checkAnswer", false);
		kiwiHolder.GetComponent<Animator>().SetBool("correctAnswer", false);
		kiwiHolder.GetComponent<Animator>().SetBool("isSingle", false);
		//kiwiHolder.GetComponent<Animator>().SetBool("isCounting", false);
		
		mTensCount = 0;
		mOnesCount = 0;
		
		numTen.GetComponent<TextMesh>().text = "0";
		numOne.GetComponent<TextMesh>().text = "0";
		
		//Stretch the pilot bubble
		pilotBubble.GetComponent<Animator>().SetBool("showingAnswer", false);
		
		//reset incorrect bool
		Camera.main.GetComponent<Animator> ().SetBool ("incorrectAnswer", false);
		
		//Have pilot stop listening
		pilot.GetComponent<Animator> ().SetBool ("isListening", false);
		
		//Reset correct answer
		GetComponent<Animator>().SetBool("correct", false);
		
		//GetComponent<Animator>().SetTrigger ("reset");
		
		kiwiHolder.GetComponent<Animator>().SetBool("fixSub", false);
        kiwiHolder.GetComponent<Animator>().speed = 1.0f;
	}
	
	void AlphaText (Transform text, float alpha) {
		_textColor.a = alpha;
		text.GetComponent<TextMesh>().color = _textColor;
	}
	
	public void ToTensColumn () {
		AlphaText(numOne, 1.0f);
		
		counterHolder.GetComponent<Animator>().SetBool("counterRight", false);
		kiwiHolder.GetComponent<Animator>().SetTrigger("flipLeft");
		
		counterHolder.GetComponent<Animator>().SetBool("checkTen", true);
		counterHolder.GetComponent<Animator>().SetBool("checkOne", false);
		
		//Highlight tens column purple
		tensColumn.GetComponent<Animator> ().SetBool ("countingColumn", true);
		onesColumn.GetComponent<Animator> ().SetBool ("countingColumn", false);
	}
	
	public void ToOnesColumn () {
		counterHolder.GetComponent<Animator>().SetBool("counterRight", true);
		if(Session.instance.currentLevel.isDoubleDigitProblem)
			kiwiHolder.GetComponent<Animator>().SetTrigger("flipRight");
		
		counterHolder.GetComponent<Animator>().SetBool("checkOne", true);
		counterHolder.GetComponent<Animator>().SetBool("checkTen", false);
		
		//Highlight ones column purple
		tensColumn.GetComponent<Animator> ().SetBool ("countingColumn", false);
		onesColumn.GetComponent<Animator> ().SetBool ("countingColumn", true);
	}

    public void ToNumpadCount()
    {
        counterHolder.GetComponent<Animator>().SetBool("counterRight", true);

        counterHolder.GetComponent<Animator>().SetBool("checkOne", true);
        counterHolder.GetComponent<Animator>().SetBool("checkTen", false);
    }

    public void IncrementTensCount(bool inbChangeNumber = true)
    {
        if (inbChangeNumber)
        {
            mTensCount++;
            numTen.GetComponent<TextMesh>().text = mTensCount.ToStringLookup();
            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.nestCount);
        }
		kiwiHolder.GetComponent<Animator>().SetTrigger("attendantCount");
        counterHolder.GetComponent<Animator>().SetTrigger("update");
		AlphaText(numTen, 1.0f);
        kiwiHolder.GetComponent<Animator>().speed = m_CountSpeed;
	}
	
	public void IncrementOnesCount (bool inbChangeNumber = true)
    {
        if (inbChangeNumber)
        {
            mOnesCount++;
            numOne.GetComponent<TextMesh>().text = mOnesCount.ToStringLookup();
            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenCount);
        }
		kiwiHolder.GetComponent<Animator>().SetTrigger("attendantCount");
        counterHolder.GetComponent<Animator>().SetTrigger("update");
		AlphaText(numOne, 1.0f);
        kiwiHolder.GetComponent<Animator>().speed = m_CountSpeed;
	}
	
	public void TransitionToTotal () {
		AlphaText(numOne, 1.0f);

        kiwiHolder.GetComponent<Animator>().speed = 1.0f;
		counterHolder.GetComponent<Animator>().SetBool("checkOne", false);
		counterHolder.GetComponent<Animator>().SetBool("checkTen", false);
		
		//counterHolder.GetComponent<Animator>().SetTrigger("counterToOff");
		//kiwiHolder.GetComponent<Animator>().SetTrigger("attendantToOff");
		
		//collapse bubble
		counterHolder.GetComponent<Animator>().SetBool ("checkAnswer", true);
		
		//transition to pilot
		GetComponent<Animator>().SetTrigger("transitionToPilot");
		
		//transition if ones plane
		if(kiwiHolder.GetComponent<Animator>().GetBool("isOnesPlaneSub"))
			kiwiHolder.GetComponent<Animator>().SetBool("fixSub", true);
		
		//counterHolder.GetComponent<MecanimEventHandler>().RegisterOnStateBegin(Animator.StringToHash("Base Layer.CabinCounterOff"), CounterHolder_OnTransition);
		
		//Stop highlighting columns
		tensColumn.GetComponent<Animator> ().SetBool ("countingColumn", false);
		onesColumn.GetComponent<Animator> ().SetBool ("countingColumn", false);
	}
	
	public void CounterHolder_OnTransition () {
		//counterHolder.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.CabinCounterToOff"), CounterHolder_OnTransition);
		//counterHolder.GetComponent<MecanimEventHandler>().UnRegisterOnStateBegin(Animator.StringToHash("Base Layer.CabinCounterOff"), CounterHolder_OnTransition);
		
		// put the kiwi in the right position
		this.transform.position = (Session.instance.currentLevel.tensColumnEnabled) ? cockpitTensPosition : cockpitOnesPosition;
		
		//set the kiwi's bubble to the smaller version
		//counterHolder.GetComponent<Animator>().SetBool("isLarge", false);
		counterHolder.GetComponent<Animator>().SetTrigger("refreshCounter");
		
		//Zoom in to check answer
		Camera.main.GetComponent<Animator> ().SetBool ("checkAnswer", true);
		
		//Have pilot listen to kiwi
		pilot.GetComponent<Animator> ().SetBool ("isListening", true);
		
		//Stretch pilot bubble back to small version
		pilotBubble.GetComponent<Animator>().SetBool("showingAnswer", true);
        pilotBubble.GetComponent<Animator>().SetBool("hintShowBorrow", false);
        pilotBubble.GetComponent<Animator>().SetBool("hintShowCarry", false);
		
		counterHolder.GetComponent<Animator>().SetBool("counterRight", false);
		//kiwiHolder.GetComponent<Animator>().SetBool("attendantFlip", false);
		
		//counterHolder.GetComponent<Animator>().SetTrigger("counterToOn");
		//kiwiHolder.GetComponent<Animator>().SetTrigger("attendantToOn");
		
		//Use delayed animation
		kiwiHolder.GetComponent<Animator>().SetBool("checkAnswer", true);
		//kiwiHolder.GetComponent<Animator>().SetBool("isOnesPlaneSub", false);
		
		//counterHolder.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.CabinCounterAnswerToOn"), CounterHolder_On);
	}
	
	public void CounterHolder_On () {
		//counterHolder.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.CabinCounterAnswerToOn"), CounterHolder_On);
		onReadyToEval();
	}
	
	public void Correct () {
		counterHolder.GetComponent<Animator>().SetTrigger("correct");
		
		//Happy Kiwi
		kiwiHolder.GetComponent<Animator>().SetTrigger("correctAnswer");
		
		//Correct answer, zoom out
		Camera.main.GetComponent<Animator> ().SetBool ("checkAnswer", false);
		Camera.main.GetComponent<Animator> ().SetBool ("incorrectAnswer", false);
		
		GetComponent<Animator>().SetBool("correct", true);
		GetComponent<Animator>().SetTrigger("checkAnswer");
		
		kiwiHolder.GetComponent<Animator>().SetBool("isOnesPlaneSub", false);
		GetComponent<Animator>().SetBool("isOnesPlaneSub", false);
	}
	
	public void Incorrect () {
		counterHolder.GetComponent<Animator>().SetTrigger("incorrect");
		kiwiHolder.GetComponent<Animator>().SetTrigger("wrongAnswer");
		
		//Wrong answer, delay zoom out
		Camera.main.GetComponent<Animator> ().SetBool ("incorrectAnswer", true);
		Camera.main.GetComponent<Animator> ().SetBool ("checkAnswer", false);
		pilotBubble.GetComponent<Animator> ().SetTrigger("stretchBubble");
		
		GetComponent<Animator>().SetBool("correct", false);
		GetComponent<Animator>().SetTrigger("checkAnswer");
	}
	
	public void ToOff () {
		counterHolder.GetComponent<Animator>().SetTrigger("counterToOff");
		kiwiHolder.GetComponent<Animator>().SetTrigger("attendantToOff");
		
		Reset ();
		//kiwiHolder.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.AttendantToOff"), KiwiHolder_Off);
		//kiwiHolder.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.AttendantToOffBump"), KiwiHolder_Off);
	}
	
	void KiwiHolder_Off () {
		//kiwiHolder.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.AttendantToOff"), KiwiHolder_Off);
		//kiwiHolder.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.AttendantToOffBump"), KiwiHolder_Off);
		
		Reset ();
		//this.transform.position = offPosition;
	}
	
	public void flipRight(){
		kiwiHolder.GetComponent<Animator>().SetTrigger ("flipRight");
	}
	public void flipLeft(){
		kiwiHolder.GetComponent<Animator>().SetTrigger ("flipLeft");
	}
	
	// init:    Session.instance.currentLevel.onesCount + (Session.instance.currentLevel.tensCount * 10)
	// updated: UpdateTotalCount((tensColumn.numCreatures * 10) + onesColumn.numCreatures);
	/*public void UpdateTotalCount (int cnt, bool forceColumnsUpdate = true) {
		if (cnt == mTotalCount)
			return;
		mTotalCount = cnt;
		// split to tens and ones
		if (forceColumnsUpdate) {
			mTensCount = (int)(mTotalCount / 10);
			mOnesCount = mTotalCount - (mTensCount * 10);
		}
		numTen.GetComponent<TextMesh>().text = (mTensCount != 0) ? mTensCount.ToString() : "";
		numOne.GetComponent<TextMesh>().text = mOnesCount.ToString();
		this.GetComponent<Animator>().SetTrigger("update");
	}*/
	
}
