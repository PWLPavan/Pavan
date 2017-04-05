using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;

public class NumberPadCtrl : MonoBehaviour
{
    #region Inspector

    public MyScreen Screen;
    public Button SubmitButton;
    public Button ToggleButton;

    [Header("Tens Column")]
    public Text TensText;
    public Button TensPlusButton;
    public Button TensMinusButton;
    public PlaceValueCtrl TensCtrl;

    [Header("Ones Column")]
    public Text OnesText;
    public Button OnesPlusButton;
    public Button OnesMinusButton;
    public PlaceValueCtrl OnesCtrl;

    #endregion

    public bool Open { get; private set; }

    public int TensCount { get; private set; }
    public int OnesCount { get; private set; }

    public int CalculateTotalValue()
    {
        return TensCount * 10 + OnesCount;
    }

    public void Reset()
    {
        TensCount = 0;
        OnesCount = 0;
        UpdateNumberDisplay();
        SubmitButton.interactable = true;
    }

    public void Show(bool inbShow)
    {
        if (Open != inbShow)
        {
            Open = inbShow;
            GetComponent<Animator>().SetBool("isOpen", inbShow);
            UpdateSeatHighlights();
        }

        PauseInputs(inbShow);
    }

    public void PauseInputs(bool pause)
    {
        if (pause)
            Screen.input.PauseInputs(GameplayInput.ONES_QUEUE, GameplayInput.ONES_SUB_ADD, GameplayInput.TENS_QUEUE, GameplayInput.TENS_SUB_ADD);
        else
            Screen.input.ResumeInputs();
    }

    public void ShowIncorrect()
    {
        int desiredOnes = Session.instance.currentLevel.valueOnes;
        int desiredTens = Session.instance.currentLevel.valueTens;

        if (OnesCount != desiredOnes)
            this.GetComponent<Animator>().SetTrigger("incorrectOne");
        if (TensCount != desiredTens)
            this.GetComponent<Animator>().SetTrigger("incorrectTen");
    }

    public void UpdateNumberDisplay()
    {
        TensText.text = TensCount.ToString();
        OnesText.text = OnesCount.ToString();
    }

    public void UpdateSeatHighlights()
    {
        if (Open)
        {
            TensCtrl.ShowNumpadHighlight(TensCount);
            OnesCtrl.ShowNumpadHighlight(OnesCount);
        }
        else
        {
            TensCtrl.ShowNumpadHighlight(0);
            OnesCtrl.ShowNumpadHighlight(0);
        }
    }

    #region Events

    private void OnClickTensPlus()
    {
        TensCount = (TensCount + 1) % 10;
        UpdateNumberDisplay();
        UpdateSeatHighlights();
		this.GetComponent<Animator>().SetTrigger("countTenUp");
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.numberPad.addTen"));
    }

    private void OnClickTensMinus()
    {
        TensCount = (TensCount + 9) % 10;
        UpdateNumberDisplay();
        UpdateSeatHighlights();
		this.GetComponent<Animator>().SetTrigger("countTenDown");
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.numberPad.subTen"));
    }

    private void OnClickOnesPlus()
    {
        OnesCount = (OnesCount + 1) % 10;
        UpdateNumberDisplay();
        UpdateSeatHighlights();
		this.GetComponent<Animator>().SetTrigger("countOneUp");
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.numberPad.addOne"));
    }

    private void OnClickOnesMinus()
    {
        OnesCount = (OnesCount + 9) % 10;
        UpdateNumberDisplay();
        UpdateSeatHighlights();
		this.GetComponent<Animator>().SetTrigger("countOneDown");
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "hud.numberPad.subOne"));
    }

    private void OnClickSubmit()
    {
        Screen.hud.SimulateLaunchClick();
    }

    #endregion

    // Use this for initialization
    void Start ()
    {
        ToggleButton.onClick.AddListener(toggleNumpad);
        TensPlusButton.onClick.AddListener(OnClickTensPlus);
        TensMinusButton.onClick.AddListener(OnClickTensMinus);
        OnesPlusButton.onClick.AddListener(OnClickOnesPlus);
        OnesMinusButton.onClick.AddListener(OnClickOnesMinus);
        SubmitButton.onClick.AddListener(OnClickSubmit);
    }
    
    // Update is called once per frame
    void toggleNumpad ()
    {
        Show(!Open);
    }
}
