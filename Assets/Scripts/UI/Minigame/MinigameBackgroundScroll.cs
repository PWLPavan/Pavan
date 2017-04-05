using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinigameBackgroundScroll : MonoBehaviour {

	private float animProgress = 0.0f;
    private float currentAnimSpeed;
	public float animSpeed = 2f;
    public float fastSpeed = 5f;

    public void SetSpeed(float inSpeed)
    {
        currentAnimSpeed = inSpeed;
    }

	private RawImage img;
	private RectTransform rect;
    private Vector2 uvSize;

    public bool clampY = true;

	// Use this for initialization
	void Start ()
    {
        currentAnimSpeed = animSpeed;

		img = GetComponent<RawImage>();
		img.texture.wrapMode = TextureWrapMode.Repeat;

		rect = GetComponent<RectTransform>();
        uvSize = new Vector2(img.texture.width, img.texture.height);
	}
	
	// Update is called once per frame
	void Update () {

		animProgress = (animProgress + (Time.deltaTime * currentAnimSpeed)) % 1;

        float scaleX = (rect.sizeDelta.x / uvSize.x);
        float scaleY = (rect.sizeDelta.y / uvSize.y);

        if (clampY)
        {
            scaleX /= scaleY;
            scaleY = 1.0f;
        }
		img.uvRect = new Rect(animProgress, 0, scaleX, scaleY);
	}
}
