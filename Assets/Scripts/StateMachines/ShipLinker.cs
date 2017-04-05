using UnityEngine;
using System.Collections;

public class ShipLinker : MonoBehaviour
{
    private MyScreen m_Screen;
	public Transform ship;

	void Awake()
    {
        m_Screen = GameObject.FindGameObjectWithTag("GameScreen").GetComponent<MyScreen>();
		ship = GameObject.FindGameObjectWithTag("GameScreen").transform.FindChild("Ship");
	}

    public void StopChirping()
    {
        m_Screen.onesColumn.StopShowingCorrectFeedback();
        m_Screen.tensColumn.StopShowingCorrectFeedback();
    }
}
