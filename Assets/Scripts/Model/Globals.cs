using UnityEngine;
using System.Collections;

public static class Globals {

	private static int sSortOrder = 1;

	public static int sortOrder {
		get { /*Debug.Log (sortOrder.ToString());*/ return sSortOrder++; }
	}

}
