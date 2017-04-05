using UnityEngine;
using System.Collections;

public class SetTextSortingLayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<MeshRenderer>().sortingLayerName = "Ship";
		GetComponent<MeshRenderer>().sortingOrder = -1;
	}
}
