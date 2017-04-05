using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class CrashScreen : MonoBehaviour
{
    static public void Create(string inCrashMessage, bool inAutoDie = false)
    {
        DestroyAllGameObjects();

        if (s_Messages == null)
            s_Messages = new List<string>();

        s_Messages.Add(inCrashMessage);

        if (s_CrashScreen.IsNull())
        {
            s_CrashScreen = new GameObject().AddComponent<CrashScreen>();
            s_CrashScreen.gameObject.AddComponent<Camera>().backgroundColor = Color.black;
        }

        if (s_CrashScreen.m_AutoDieDelay <= 0.0f)
        {
            s_CrashScreen.m_AutoDieDelay = inAutoDie ? AUTO_DIE_TIME : 0.0f;
        }
    }

    static private void DestroyAllGameObjects()
    {
        foreach(var obj in FindObjectsOfType<GameObject>())
        {
            if (s_CrashScreen == null || obj != s_CrashScreen.gameObject)
            {
                if (obj.GetComponent<Ekstep.Genie>() != null)
                    continue;

                try { Destroy(obj); }
                catch { Debug.Log("Unable to destroy " + obj.ToString()); }
            }
        }
    }

    private void Awake()
    {
        useGUILayout = false;

        m_BackgroundTexture = new Texture2D(2, 2);
        m_BackgroundTexture.SetPixels(new Color[]{Color.black, Color.black, Color.black, Color.black});
        m_BackgroundTexture.Apply();

        m_HeaderFooterStyle = new GUIStyle();
        m_HeaderFooterStyle.normal.textColor = Color.white;
        m_HeaderFooterStyle.fontSize = 48;
        m_HeaderFooterStyle.fontStyle = FontStyle.Bold;
        m_HeaderFooterStyle.alignment = TextAnchor.MiddleCenter;
        m_HeaderFooterStyle.normal.background = m_BackgroundTexture;

        m_MessageStyle = new GUIStyle(m_HeaderFooterStyle);
        m_MessageStyle.fontSize = Screen.height / 24;
        m_MessageStyle.fontStyle = FontStyle.Normal;
        m_MessageStyle.wordWrap = true;
    }

    private IEnumerator Start()
    {
        yield return null;
        if (Ekstep.Genie.Exists)
        {
            Ekstep.Genie.instance.SyncEvents();
        }
    }

    private void Update()
    {
        if (!m_MouseDown)
        {
            m_ScrollOffset *= 0.25f;

            if (UnityEngine.Input.GetMouseButton(0))
            {
                m_MouseDown = true;
                m_MouseY = Input.mousePosition.y;
                m_Dragging = false;
                m_ScrollOffset = 0.0f;
            }
        }
        else
        {
            if (!Input.GetMouseButton(0))
            {
                if (m_Dragging)
                {
                    m_Dragging = false;
                }
                else
                {
                    m_MessageIndex = (m_MessageIndex + 1) % s_Messages.Count;
                }

                m_MouseDown = false;
            }
            else
            {
                float mouseY = Input.mousePosition.y;
                float mouseDelta = mouseY - m_MouseY;
                if (!m_Dragging && Math.Abs(mouseDelta) >= Screen.height * DRAG_PERCENTAGE_THRESHOLD)
                {
                    m_Dragging = true;
                }

                if (m_Dragging)
                {
                    m_ScrollOffset = -mouseDelta;
                }
            }
        }

        if (UnityEngine.Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_ANDROID
            AndroidHelper.KillActivity();
#endif
        }

        if (m_AutoDieDelay > 0)
        {
            m_AutoDieDelay -= Time.deltaTime;
            if (m_AutoDieDelay <= 0.0f)
            {
                Application.Quit();
#if UNITY_ANDROID
                AndroidHelper.KillActivity();
#endif
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (m_AutoDieDelay > 0)
        {
            Application.Quit();
#if UNITY_ANDROID
            AndroidHelper.KillActivity();
#endif
        }
    }

    private void OnGUI()
    {
        UpdateGUIContent();
        GUI.Label(new Rect(64, 80 + m_ScrollOffset, Screen.width - 128, Screen.height - 160), m_ScreenMessage, m_MessageStyle);

        GUI.Label(new Rect(0, 0, Screen.width, 64), m_ScreenTop, m_HeaderFooterStyle);
        GUI.Label(new Rect(0, Screen.height - 64, Screen.width, 64), m_ScreenBottom, m_HeaderFooterStyle);
    }

    private void UpdateGUIContent()
    {
        if (m_ScreenTop == null)
        {
            m_ScreenTop = new GUIContent("Whoops! Something went wrong.");
            if (m_AutoDieDelay > 0)
                m_ScreenTop.text = "*** Error ***";
        }

        if (m_ScreenBottom == null)
            m_ScreenBottom = new GUIContent();

        if (m_ScreenMessage == null)
            m_ScreenMessage = new GUIContent();

        m_ScreenMessage.text = s_Messages[m_MessageIndex];
        m_ScreenBottom.text = "Viewing " + (m_MessageIndex + 1).ToString() + " / " + (s_Messages.Count).ToString();
    }

    static private CrashScreen s_CrashScreen;
    static private List<string> s_Messages;

    private int m_MessageIndex = 0;

    private GUIContent m_ScreenTop;
    private GUIContent m_ScreenMessage;
    private GUIContent m_ScreenBottom;

    private GUIStyle m_HeaderFooterStyle;
    private GUIStyle m_MessageStyle;

    private float m_AutoDieDelay = 0;

    private bool m_MouseDown = false;
    private float m_MouseY;
    private float m_ScrollOffset = 0.0f;
    private bool m_Dragging = false;

    private Texture2D m_BackgroundTexture;

    private const float DRAG_PERCENTAGE_THRESHOLD = 0.2f;
    private const float AUTO_DIE_TIME = 5.0f;
}
