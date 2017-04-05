using UnityEngine;
using System.Collections;

public class ExpressionCtrl : MonoBehaviour {

	#region Prefabs
	#endregion

	#region Inspector

    public Color brownColor = Color.red;
    public Color whiteColor = Color.gray;

	#endregion

	#region Gui
	[HideInInspector]
	public Transform operationText;

	[HideInInspector]
	public Transform topTenText;
	[HideInInspector]
	public Transform topOneText;

	[HideInInspector]
	public Transform bottomTenText;
	[HideInInspector]
	public Transform bottomOneText;

    [HideInInspector]
    public Transform borrowTensText;

    [HideInInspector]
    public Transform borrowedOnesText;
    #endregion

    #region Members
    bool _hasDoubleDigit = false;
	#endregion

	#region Ctrl
	void Awake () {
		operationText = this.transform.Find("BubbleHolder/ExpOp");

		topTenText = this.transform.Find("BubbleHolder/ExpTensColumn/ExpTopTen/ExpTopTen");
		topOneText = this.transform.Find("BubbleHolder/ExpOnesColumn/ExpTopOne");

		bottomTenText = this.transform.Find("BubbleHolder/ExpTensColumn/ExpBottomTen");
		bottomOneText = this.transform.Find("BubbleHolder/ExpOnesColumn/ExpBottomOne");
		
		borrowTensText = this.transform.Find("BubbleHolder/ExpTensColumn/ExpTopTen/BorrowTens");
        borrowedOnesText = this.transform.Find("BubbleHolder/ExpOnesColumn/ExpTopOne/BorrowOnes");
    }
	#endregion

	#region Methods
	public void SetGoal (Level currentLevel) {
        // set the goal
		_hasDoubleDigit = false;
		if (currentLevel.isExpression) {
			operationText.GetComponent<TextMesh>().text = currentLevel.expOp;
			AssignDoubleDigit(topTenText, topOneText,currentLevel.expTop);
			AssignDoubleDigit(bottomTenText, bottomOneText, currentLevel.expBottom);
			this.GetComponent<Animator>().SetBool("isTarget", false);
		} else if (currentLevel.isTargetNumber) {
			operationText.GetComponent<TextMesh>().text = "";
			AssignDoubleDigit(bottomTenText, bottomOneText, "");
			AssignDoubleDigit(topTenText, topOneText, currentLevel.expTop);
			this.GetComponent<Animator>().SetBool("isTarget", true);
		}

        FixDoubleDigitZeroes(currentLevel);
        FixColors(currentLevel);
	}

    public void SetBorrowValue (Level currentLevel) {
        borrowTensText.GetComponent<TextMesh>().text = (currentLevel.expTop.Length == 2) ? (int.Parse(currentLevel.expTop.Substring(0, 1)) - 1).ToString() : "";
    }

    void AssignDoubleDigit (Transform tensDigitText, Transform onesDigitText, string value) {
		if (value.Length == 1) {
			tensDigitText.GetComponent<TextMesh>().text = "";
			onesDigitText.GetComponent<TextMesh>().text = value;
		} else if (value.Length == 2) {
			tensDigitText.GetComponent<TextMesh>().text = value.Substring(0,1);
			onesDigitText.GetComponent<TextMesh>().text = value.Substring(1,1);
			_hasDoubleDigit = true;
		}
	}

    void FixDoubleDigitZeroes (Level level) {
        if (level.isExpression) {
            bool topShowZero = level.expTop.Length > 1;
            bool bottomShowZero = level.expBottom.Length > 1;

            this.GetComponent<Animator>().SetBool("hasNoTenDigits", !topShowZero && !bottomShowZero);
            this.GetComponent<Animator>().SetBool("hasNoTenTop", !topShowZero);
            this.GetComponent<Animator>().SetBool("hasNoTenBottom", !bottomShowZero);
            this.GetComponent<Animator>().SetBool("hasBorrowing", level.isSubtractionProblem && level.requiresRegrouping);
        }
        else {
            this.GetComponent<Animator>().SetBool("hasNoTenDigits", false);
            this.GetComponent<Animator>().SetBool("hasNoTenTop", false);
            this.GetComponent<Animator>().SetBool("hasNoTenBottom", false);
            this.GetComponent<Animator>().SetBool("hasBorrowing", false);
        }
    }

    void FixColors(Level inLevel)
    {
        if (inLevel.isTargetNumber)
        {
            SetTopColors(inLevel.twoPartProblem || inLevel.isSubtractionProblem ? brownColor : whiteColor);
        }
        else
        {
            SetTopColors(brownColor);
            SetBottomColors(whiteColor);
        }
    }

    void SetTopColors(Color inColor)
    {
        topOneText.GetComponent<TextMesh>().color = topTenText.GetComponent<TextMesh>().color
                = borrowTensText.GetComponent<TextMesh>().color = borrowedOnesText.GetComponent<TextMesh>().color
                = inColor;
    }

    void SetBottomColors(Color inColor)
    {
        bottomOneText.GetComponent<TextMesh>().color = bottomTenText.GetComponent<TextMesh>().color
                = inColor;
    }

	public void ToOff () {
		// turn bubble off
		operationText.GetComponent<TextMesh>().text = "";
		AssignDoubleDigit(topTenText, topOneText, "");
		AssignDoubleDigit(bottomTenText, bottomOneText, "");
		_hasDoubleDigit = false;
	}

	public void Reset (bool shouldReset) {
		if (shouldReset)
			this.GetComponent<Animator>().SetTrigger("reset");
        /*if (fromSkip) {
			this.GetComponent<Animator>().SetTrigger("correct");
			this.GetComponent<Animator>().SetTrigger("newProblem");
		}*/

        this.GetComponent<Animator>().SetBool("isCounting", false);
        this.GetComponent<Animator>().SetBool("hintFadeTens", false);
		this.GetComponent<Animator>().SetBool("hintFadeOnes", false);
		this.GetComponent<Animator>().SetBool("hintShowCarry", false);
        this.GetComponent<Animator>().SetBool("hintShowBorrow", false);
        this.GetComponent<Animator>().ResetTrigger("countTopTen");
		this.GetComponent<Animator>().ResetTrigger("countTopTenBorrow");
		this.GetComponent<Animator>().ResetTrigger("countTopTenCarry");
        this.GetComponent<Animator>().ResetTrigger("countBottomTen");
        this.GetComponent<Animator>().ResetTrigger("countTopOne");
        this.GetComponent<Animator>().ResetTrigger("countBottomOne");
        this.GetComponent<Animator>().SetTrigger("stopHinting");
        this.GetComponent<Animator>().SetBool("hintFadeTensExtra", false);
    }

    public void ResetHintingFades()
    {
        this.GetComponent<Animator>().SetBool("hintFadeTens", false);
        this.GetComponent<Animator>().SetBool("hintFadeOnes", false);
        this.GetComponent<Animator>().SetBool("isCounting", false);
        this.GetComponent<Animator>().ResetTrigger("countTopTen");
		this.GetComponent<Animator>().ResetTrigger("countTopTenBorrow");
		this.GetComponent<Animator>().ResetTrigger("countTopTenCarry");
        this.GetComponent<Animator>().ResetTrigger("countBottomTen");
        this.GetComponent<Animator>().ResetTrigger("countTopOne");
        this.GetComponent<Animator>().ResetTrigger("countBottomOne");
        this.GetComponent<Animator>().SetTrigger("stopHinting");
        this.GetComponent<Animator>().SetBool("hintFadeTensExtra", false);
    }

	#endregion

	public void stretchBubble(){
		this.GetComponent<Animator>().SetTrigger("stretchBubble");
	}

}
