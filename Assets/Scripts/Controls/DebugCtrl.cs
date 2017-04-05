using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;

public class DebugCtrl : MonoBehaviour
{
    private MyScreen m_Screen;

    private void Awake()
    {
#if !DEVELOPMENT
        Destroy(this);
        return;
#endif
        m_Screen = GetComponent<MyScreen>();

        InitializeDummyResponses();
    }
    
    private void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha0))
            SendDummyReponse(s_StopHinting_Dummy);
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            SendDummyReponse(s_TooManyTooFew_Dummy);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SendDummyReponse(s_HighlightAnswer_Dummy);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SendDummyReponse(s_Handhold_Dummy);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            SendDummyReponse(s_CountTimingDefault_Dummy);
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            SendDummyReponse(s_CountTimingFast_Dummy);
        else if (Input.GetKeyDown(KeyCode.E))
            Session.instance.numEggs = 198;
        else if (Input.GetKeyDown(KeyCode.W))
            Session.instance.numEggs = 129;
        else if (Input.GetKeyDown(KeyCode.Q))
            Session.instance.numEggs = 7;

        else if (Input.GetKeyDown(KeyCode.P))
            SendDummyReponse(s_UseNumberPad_Dummy);
        else if (Input.GetKeyDown(KeyCode.O))
            SendDummyReponse(s_DisableNumberPad_Dummy);

        else if (Input.GetKeyDown(KeyCode.UpArrow) && (Time.timeScale * 2) < 100)
        {
            Time.timeScale *= 2.0f;
            UpdateSoundPitch();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && (Time.timeScale / 2) > 0)
        {
            Time.timeScale /= 2.0f;
            UpdateSoundPitch();
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Break();
#endif
    }

    private void SendDummyReponse(JSONNode inJSON)
    {
        m_Screen.ProcessEnlearnResponse(inJSON);
    }

    private void UpdateSoundPitch()
    {
        // For fun/hilarious effects
        SoundManager.instance.sfx.pitch = Time.timeScale;
        SoundManager.instance.music.pitch = Time.timeScale;
        SoundManager.instance.musicFX.pitch = Time.timeScale;
    }

    static private void InitializeDummyResponses()
    {
        if (s_StopHinting_Dummy != null)
            return;

        s_StopHinting_Dummy = new JSONClass();
        s_StopHinting_Dummy.Add("nextHint", "stopHinting");
        s_StopHinting_Dummy["startNow"].AsBool = true;

        s_TooManyTooFew_Dummy = new JSONClass();
        s_TooManyTooFew_Dummy.Add("nextHint", "tooManyTooFew");
        s_TooManyTooFew_Dummy["startNow"].AsBool = true;

        s_HighlightAnswer_Dummy = new JSONClass();
        s_HighlightAnswer_Dummy.Add("nextHint", "highlightAnswer");
        s_HighlightAnswer_Dummy["startNow"].AsBool = true;

        s_Handhold_Dummy = new JSONClass();
        s_Handhold_Dummy.Add("nextHint", "handHold");
        s_Handhold_Dummy["startNow"].AsBool = true;

        s_CountTimingDefault_Dummy = new JSONClass();
        s_CountTimingDefault_Dummy["countSpeed"].AsFloat = 1.0f;

        s_CountTimingFast_Dummy = new JSONClass();
        s_CountTimingFast_Dummy["countSpeed"].AsFloat = 2.0f;

        s_UseNumberPad_Dummy = new JSONClass();
        s_UseNumberPad_Dummy["useNumberPad"].AsBool = true;

        s_DisableNumberPad_Dummy = new JSONClass();
        s_DisableNumberPad_Dummy["useNumberPad"].AsBool = false;
    }

    static private JSONNode s_StopHinting_Dummy;
    static private JSONNode s_TooManyTooFew_Dummy;
    static private JSONNode s_HighlightAnswer_Dummy;
    static private JSONNode s_Handhold_Dummy;

    static private JSONNode s_CountTimingDefault_Dummy;
    static private JSONNode s_CountTimingFast_Dummy;

    static private JSONNode s_UseNumberPad_Dummy;
    static private JSONNode s_DisableNumberPad_Dummy;
}