using UnityEngine;
using System.Collections;

// This is redundant, since we can specify these
// in the Player Settings under AutoRotate
// Might not even be used in the project.
public class ScreenSettings : MonoBehaviour {

	void Awake () {
		// lock game to landscape mode only
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
	}

}
