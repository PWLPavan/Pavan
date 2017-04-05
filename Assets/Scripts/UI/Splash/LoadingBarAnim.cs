using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingBarAnim : MonoBehaviour
{
    private float loadBarProgress = 0.0f;
    private const float loadBarSpeed = 2f;
    public Texture loadBarTexture = null;

    private RawImage img;
    private RectTransform rect;

    private float rectWidthMax;

    // Use this for initialization
    void Start ()
    {
        img = GetComponent<RawImage>();
        img.texture.wrapMode = TextureWrapMode.Repeat;

        rect = GetComponent<RectTransform>();
        rectWidthMax = rect.sizeDelta.x;

        StartCoroutine(LoadNextLevel());
    }
    
    // Update is called once per frame
    void Update ()
    {
        //Move the bar along; keep it's position between zero and one for best float point precision
        loadBarProgress += Time.deltaTime * loadBarSpeed % 1;

        //Toms Help in understanding the mod op
        //if (loadBarProgress >= 1.0f) loadBarProgress -= 1.0f;
        //loadBarProgress = loadBarProgress % 1;
        //loadBarProgress %= 1;

        img.uvRect = new Rect(loadBarProgress, 0, (rect.sizeDelta.x / img.texture.width), (rect.sizeDelta.y / img.texture.height));
    }

    private void SetPercentDone(float inPercentDone)
    {
        //Debug.LogFormat("Percent Done: {0}", inPercentDone);
        rect.sizeDelta = new Vector2(inPercentDone * rectWidthMax, rect.sizeDelta.y);
    }

    private IEnumerator LoadNextLevel()
    {
        Input.multiTouchEnabled = false;

        SetPercentDone(0);

        Optimizer.CreateSingleton();
        yield return null;

        Debug.Log("Loading genie...");

        Ekstep.Genie.CreateSingleton();

        while (!Ekstep.Genie.instance.IsInitialized)
            yield return null;

        Debug.Log("Finished loading genie...");

        if (!EnlearnInstance.CheckInstalled())
            yield break;

        LanguageMgr.CreateSingleton();

        FPSCounter.CreateSingleton();

        AsyncOperation loadOp = Application.LoadLevelAsync(Application.loadedLevel + 1);
        while(!loadOp.isDone)
        {
            SetPercentDone(loadOp.progress);
            yield return null;
        }
    }
}
