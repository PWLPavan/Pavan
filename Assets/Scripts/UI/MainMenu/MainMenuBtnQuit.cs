using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;
using FGUnity.Utils;

public class MainMenuBtnQuit : MonoBehaviour
{
    public Button Button;
	public Transform ExitPrompt;

	public Button ConfirmButton;
	public Button CloseButton;

	public GameObject loadingScreen;

    private bool m_ShuttingDown = false;

    void Start ()
    {
        Button.onClick.AddListener(Button_Click);

		ConfirmButton.onClick.AddListener(exitConfirmBtn_onClick);
		CloseButton.onClick.AddListener(exitCloseBtn_onClick);
    }

    void Button_Click()
    {
        Genie.I.SyncEvents();
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.quit"));
		ExitPrompt.gameObject.SetActive(true);
		ExitPrompt.GetComponent<Animator>().SetTrigger("showPopup");
    }

	void exitConfirmBtn_onClick()
	{
        if (m_ShuttingDown)
            return;

        m_ShuttingDown = true;

        CloseButton.enabled = false;
        ConfirmButton.enabled = false;

        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.quit.confirm"));
        this.SmartCoroutine(ShutdownSequence());
		loadingScreen.SetActive(true);
	}

	void exitCloseBtn_onClick()
	{
        if (m_ShuttingDown)
            return;

        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.quit.cancel"));
		ExitPrompt.GetComponent<Animator>().SetTrigger("hidePopup");
	}

    private IEnumerator ShutdownSequence()
    {
        // To make sure we're separating ourselves from the previous interact event.
        yield return 0.1f;

        Genie.I.LogEnd();
        
        // To make sure we've had time to at least send the event out.
        yield return 0.2f;
        
        Genie.I.SyncEvents();

        float waitTimeRemaining = 10f;
        while (Genie.I.IsSyncing && waitTimeRemaining > 0)
        {
            yield return null;
            waitTimeRemaining -= Time.deltaTime;
        }

        Genie.I.AttemptOpenGenie();
        Genie.I.ForceShutdown();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();

#if UNITY_ANDROID
        AndroidHelper.KillActivity();
#endif
    }
}
