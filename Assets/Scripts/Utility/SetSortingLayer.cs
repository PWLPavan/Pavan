using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SetSortingLayer : MonoBehaviour
{
	[Tooltip("The desired sorting layer.")]
	public string sortingLayer = "Default";

	[Tooltip("The desired sorting layer position.")]
	public int sortingLayerNum = 0;

 	[Tooltip("Set for children as well")]
	public bool inChildren = true;

	private string oldSortingLayer = "Default";
	private bool oldInChildren = true;

	void Awake ()
	{
		UpdateIfDirty ();

		bool inEditor = false;

		#if UNITY_EDITOR
			inEditor = true;
		#endif

		if (!inEditor)
		{
			this.enabled = false;
		}
	}

#if UNITY_EDITOR
    void Update()
	{
			UpdateIfDirty ();
			if (Application.isPlaying)
			{
				this.enabled = false;
			}
	}
#endif

    private void UpdateIfDirty()
	{
		if (oldSortingLayer != sortingLayer || oldInChildren != inChildren)
		{
			oldSortingLayer = sortingLayer;
			oldInChildren = inChildren;
			
			SetSortingLayerInRenderer(GetComponent<Renderer>(), sortingLayer, sortingLayerNum);
			
			if (inChildren)
			{
				foreach (Renderer r in GetComponentsInChildren<Renderer>())
				{
					SetSortingLayerInRenderer(r, sortingLayer, sortingLayerNum);
				}
			}
		}
	}

	private static void SetSortingLayerInRenderer(Renderer r, string sLayer, int sNum)
	{
		if (r != null)
		{
			r.sortingLayerName = sLayer;
			r.sortingOrder = sNum;
		}
	}
}
