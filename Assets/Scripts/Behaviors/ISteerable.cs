using UnityEngine;
using System.Collections;

public interface ISteerable {

	void Move ();
	void StartMove (Vector3 targetPosition, bool isGlobal = false, float spd = 3.0f, bool factorDist = false);

}
