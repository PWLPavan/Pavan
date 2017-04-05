using UnityEngine;
using System.Collections;

public class ScreenResize : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Resolution[] resolutions = Screen.resolutions;
		// Print the resolutions
		/*for (Resolution res in resolutions) {
			Debug.Log(res.width + "x" + res.height);
		}*/
		// Switch to the lowest supported fullscreen resolution
		Screen.SetResolution (resolutions[0].width, resolutions[0].height, true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
