using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OpenWebpageButton : MonoBehaviour
{
	public string Address; 
	
	void Start () {
		GetComponent<Button>().onClick.AddListener(button_onClick);
	}
	
	void button_onClick()
    {
		Application.OpenURL(Address);
	}
}
