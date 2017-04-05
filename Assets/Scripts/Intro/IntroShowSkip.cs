using UnityEngine;
using System.Collections;

public class IntroShowSkip : MonoBehaviour {

    private void Start()
    {
        CleanupHook.instance.Cleanup();
    }

	public GameObject skipBtn;
	
	#region Input
	void OnMouseUp() {
		skipBtn.SetActive(true);
	}
	#endregion
}
