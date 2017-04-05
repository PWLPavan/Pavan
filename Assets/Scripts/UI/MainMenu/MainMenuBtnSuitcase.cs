using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FGUnity.Utils;
using Ekstep;

public class MainMenuBtnSuitcase : MonoBehaviour {

    public SuitcaseCtrl Suitcase;
    public Button SuitcaseButton;

    void Start ()
    {
        this.WaitOneFrameThen(Check);
    }

    private void Check()
    {
        SaveData.instance.UpdateData();
        if (SaveData.instance.Eggs == 0)
            return;

		SuitcaseButton.transform.FindChild("BtnSuitcase").gameObject.SetActive(true);
		SuitcaseButton.onClick.AddListener(Button_Click);
    }

    void Button_Click()
    {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.suitcase"));
		SuitcaseButton.GetComponent<Animator>().SetBool("isOpen", true);
        Suitcase.gameObject.SetActive(true);
        Suitcase.GetComponent<Animator>().SetBool("showCollectionFromBottom", true);
        Suitcase.Show(true);
        Suitcase.onExited = Button_Close;
    }

    void Button_Close()
    {
		SuitcaseButton.GetComponent<Animator>().SetBool("isOpen", false);
        Suitcase.GetComponent<Animator>().SetBool("showCollectionFromBottom", false);
    }

    public void Reset()
    {
		SuitcaseButton.transform.FindChild("BtnSuitcase").gameObject.SetActive(false);
    }
}
