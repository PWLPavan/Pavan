using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GUIConsole : MonoBehaviour
{
    public Text text;

    private const int maxlogs = 10;
    private Queue<string> log;

    void Awake()
    {
        if (I != null) DontDestroyOnLoad(gameObject);
        else Destroy(gameObject);

        log = new Queue<string>();
        Application.logMessageReceived += LogMessageReceived;
    }

    void LogMessageReceived(string condition, string stackTrace, LogType type)
    {
        log.Enqueue(type.ToString() + ": " + condition);
        while (log.Count > maxlogs) log.Dequeue();

        text.text = string.Empty;
        foreach (var s in log) text.text += s + '\n';
    }

    public static GUIConsole I
    {
        get { return GameObject.FindObjectOfType<GUIConsole>(); }
    }
}
