using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuChickenCtrl : MonoBehaviour {

	private float randomScale;
	private float randomSize;
	private float randomSprite;

	public Sprite chickenA;
	public Sprite chickenB;
	
	void Start () {
		showChicken ();
	}

	public void showChicken(){
		transform.position = new Vector2(Random.Range(100, Screen.width-100), -165f);
		GetComponent<Animator>().SetTrigger("showCrazedJump");
		
		randomScale = Random.Range(-1,2);
		randomSize = Random.Range(.5f,1);

		randomSprite = Random.Range (-1,1);

		if(randomSprite >= 0){
			GetComponentInChildren<Image>().sprite = chickenA;
		}else{
			GetComponentInChildren<Image>().sprite = chickenB;
		}

		if(randomScale > 0){
			transform.localScale = new Vector3(1*randomSize,randomSize,1);
		}else{
			transform.localScale = new Vector3(-1*randomSize,randomSize,1);
		}
	}

	public void destroyChicken(){
		Destroy(this.gameObject);
	}
}
