using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FGUnity.Utils;

public class FPSCounter : LazySingletonBehavior<FPSCounter>
{
	public  float updateInterval = 0.5F;
	
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval

	private float fps = 0;
	private string m_Text = "";

    public float Framerate { get { return fps; } }

    private Texture2D m_BackgroundTexture;
    private GUIStyle m_FontStyle;

    private bool m_Visible = false;

	// Use this for initialization
	void Start ()
    {
        useGUILayout = false;
		timeleft = updateInterval;

        m_BackgroundTexture = new Texture2D(2, 2);
        m_BackgroundTexture.SetPixels(new Color[] { Color.black, Color.black, Color.black, Color.black });
        m_BackgroundTexture.Apply();

        m_FontStyle = new GUIStyle();
        m_FontStyle.normal.textColor = Color.white;
        m_FontStyle.fontSize = 24;
        m_FontStyle.fontStyle = FontStyle.Bold;
        m_FontStyle.alignment = TextAnchor.MiddleLeft;
        m_FontStyle.normal.background = m_BackgroundTexture;

        KeepAlive.Apply(this);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            m_Visible = !m_Visible;

		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		
		// Interval ended - update GUI text and start new interval
		if (timeleft <= 0.0)
        {
			// display two fractional digits (f2 format)
			fps = accum/frames;

            using (PooledStringBuilder builder = PooledStringBuilder.Create())
            {
                builder.Builder.AppendFormat("  FPS: {0}", ((int)fps).ToStringLookup());
                m_Text = builder.Builder.ToString();
            }

			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
	}

#if DEVELOPMENT || ALLOW_FPS
	void OnGUI ()
    {
        if (!m_Visible)
            return;

		GUI.Label(new Rect(0, 0, Screen.width, 32), m_Text, m_FontStyle);
	}
#endif

}
