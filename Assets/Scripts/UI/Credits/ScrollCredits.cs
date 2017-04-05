using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FGUnity.Utils;

public class ScrollCredits : MonoBehaviour, IEndDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler {

	private float speed = 30f;
	private bool crawling = true;

	private Transform content;
    private Text text;
    private Canvas canvas;

    private string m_Spacing;

	void Start ()
    {
        using (PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
        {
            stringBuilder.Builder.Append('\n', 4);
            m_Spacing = stringBuilder.Builder.ToString();
        }

		content = transform.FindChild("Viewport/Content");
        text = content.GetComponentInChildren<Text>();
        canvas = GetComponentInParent<Canvas>();

        LanguageConfig lang = LanguageMgr.instance.FindLanguage("en");

        text.text = lang["credits"] + m_Spacing;
        text.font = lang.Font;

        foreach(Transform t in text.transform)
        {
            if (t.gameObject.name != lang.Code)
                t.gameObject.SetActive(false);
            else
                t.gameObject.SetActive(true);
        }
	}

	void Update () {
		if(!crawling)
			return;

		content.Translate(Vector3.up*Time.deltaTime*speed);
	}

	public void OnBeginDrag(PointerEventData data){
		crawling = false;
	}

	public void OnEndDrag(PointerEventData data){
		crawling = true;
	}

	public void OnPointerDown(PointerEventData data){
		crawling = false;
	}

	public void OnPointerUp(PointerEventData data){
		crawling = true;
	}
}
