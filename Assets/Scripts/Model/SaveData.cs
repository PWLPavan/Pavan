using UnityEngine;
using System.Collections;
using SimpleJSON;
using FGUnity.Utils;
using Ekstep;

public class SaveData : LazySingletonBehavior<SaveData>
{
    private const uint VERSION = 1;

    public enum FlagType
    {
        Seen_Intro = 0,
        
        Tutorial_Subtract = 4,
        Tutorial_TensPlane = 5,
        Tutorial_Carryover = 6,
        Tutorial_Borrowing = 7
    }

    private bool m_Modified = false;
    private string m_UserKey;

    private uint m_Flags = 0;
    private FlagType m_TutorialLevel;

    private int m_Eggs = 0;
    private bool m_MuteMusic = false;
    private bool m_MuteSound = false;
    private string m_LanguageCode = string.Empty;
    private JSONNode m_StampData = null;

    private int m_NumLevelsCompleted = 0;
    private int m_LevelIndex = 0;

    private JSONNode m_UserData;

    #region Flags

    public bool GetFlag(FlagType inFlag)
    {
        return Bits.Contains(m_Flags, (byte)inFlag);
    }

    public void SetFlag(FlagType inFlag, bool inbOn)
    {
        if (inbOn != Bits.Contains(m_Flags, (byte)inFlag))
        {
            Bits.Set(ref m_Flags, (byte)inFlag, inbOn);
            m_Modified = true;
        }
    }

    public bool WatchedIntro
    {
        get { return GetFlag(FlagType.Seen_Intro); }
        set { SetFlag(FlagType.Seen_Intro, value); }
    }

    public bool SeenTens
    {
        get { return GetFlag(FlagType.Tutorial_TensPlane); }
        set { SetFlag(FlagType.Tutorial_TensPlane, value); }
    }

    #endregion

    public int Eggs
    {
        get { return m_Eggs; }
        set
        {
            if (value != m_Eggs)
            {
                m_Eggs = value;
                m_Modified = true;
            }
        }
    }

    public int Stamps
    {
        get { return (int)(Eggs / 10); }
    }

    public bool MuteMusic
    {
        get { return m_MuteMusic;  }
        set
        {
            if (value != m_MuteMusic)
            {
                m_MuteMusic = value;
                m_Modified = true;
                SoundManager.instance.UpdateMute();
            }
        }
    }

    public bool MuteSound
    {
        get { return m_MuteSound; }
        set
        {
            if (value != m_MuteSound)
            {
                m_MuteSound = value;
                m_Modified = true;
                SoundManager.instance.UpdateMute();
            }
        }
    }

    public string LanguageCode
    {
        get { return m_LanguageCode; }
        set
        {
            if (value != m_LanguageCode)
            {
                m_LanguageCode = value;
                m_Modified = true;
            }
        }
    }

    public JSONNode StampData
    {
        get { return m_StampData; }
        set
        {
            if (m_StampData != value)
            {
                m_StampData = value;
                m_Modified = true;
            }
        }
    }

    public int NumLevelsCompleted
    {
        get { return m_NumLevelsCompleted; }
        set
        {
            if (m_NumLevelsCompleted != value)
            {
                m_NumLevelsCompleted = value;
                m_Modified = true;
            }
        }
    }

    public int LevelIndex
    {
        get { return m_LevelIndex; }
        set
        {
            if (m_LevelIndex != value)
            {
                m_LevelIndex = value;
                m_Modified = true;
            }
        }
    }

    public FlagType CurrentTutorial
    {
        get { return m_TutorialLevel; }
        set
        {
            if (m_TutorialLevel != value)
            {
                m_TutorialLevel = value;
                m_Modified = true;
            }
        }
    }

    public void UpdateData()
    {
        if (Session.Exists)
        {
            Eggs = Session.instance.numEggs;
            NumLevelsCompleted = Session.instance.numLevelsCompleted;
            LevelIndex = Session.instance.currentLevelIndex;
        }
    }

    public void SyncAndSave()
    {
        UpdateData();
        SaveChanges();
    }

    private void WriteToJSON()
    {
        m_UserData["version"].AsUInt = VERSION;
        m_UserData["flags"].AsUInt = m_Flags;
        m_UserData["eggs"].AsInt = m_Eggs;
        m_UserData["muteMusic"].AsBool = m_MuteMusic;
        m_UserData["muteSound"].AsBool = m_MuteSound;
        m_UserData["language"].Value = m_LanguageCode;
        m_UserData["numLevelsCompleted"].AsInt = m_NumLevelsCompleted;
        m_UserData["levelIndex"].AsInt = m_LevelIndex;
        m_UserData["tutorialLevel"].AsUInt = (uint)m_TutorialLevel;

        if (m_StampData != null)
            m_UserData["stampData"] = m_StampData;
        else
            m_UserData.Remove("stampData");

        PlayerPrefs.SetString(m_UserKey, m_UserData.ToString());
        PlayerPrefs.Save();
    }

    private void ReadFromJSON()
    {
        string jsonString = PlayerPrefs.GetString(m_UserKey);
        if (!string.IsNullOrEmpty(jsonString))
        {
            m_UserData = JSON.Parse(jsonString);

            uint version = m_UserData["version"].AsUInt;

            m_Flags = m_UserData["flags"].AsUInt;
            m_Eggs = m_UserData["eggs"].AsInt;
            m_MuteMusic = m_UserData["muteMusic"].AsBool;
            m_MuteSound = m_UserData["muteSound"].AsBool;
            m_LanguageCode = m_UserData["language"].Value;
            m_NumLevelsCompleted = m_UserData["numLevelsCompleted"].AsInt;
            m_StampData = m_UserData["stampData"];
            m_LevelIndex = m_UserData["levelIndex"].AsInt;
            m_TutorialLevel = (FlagType)m_UserData["tutorialLevel"].AsUInt;

            // If we're dealing with old save data, the flags are invalid.
            if (version == 0)
            {
                // We keep the seen flags.  Tutorials have changed.
                m_Flags &= (uint)(FlagType.Seen_Intro);
                m_TutorialLevel = 0;
            }
        }
        else
        {
            m_UserData = new JSONClass();
        }
    }

    public void SaveChanges()
    {
        if (m_Modified)
        {
            WriteToJSON();
            m_Modified = false;
        }
    }

    public void ResetProfile()
    {
        Eggs = 0;
        m_Flags = 0;
        m_TutorialLevel = 0;
        StampData = null;
        NumLevelsCompleted = 0;
        LevelIndex = 0;
        m_Modified = true;
        SaveChanges();
    }

    public void ResetSettings()
    {
        MuteMusic = false;
        MuteSound = false;
        SaveChanges();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SyncAndSave();
        }
    }

    private void OnApplicationQuit()
    {
        SyncAndSave();
    }

    public void SetUserID(string inID)
    {
        m_UserKey = "data.user." + inID;
        ReadFromJSON();

        if (SoundManager.Exists)
            SoundManager.instance.UpdateMute();
    }

    private void Start()
    {
        SetUserID(Genie.instance.UserID);
    }

    private void OnLevelWasLoaded(int level)
    {
        this.WaitOneFrameThen(UpdateData);
    }

    protected override void Awake()
    {
        base.Awake();
        KeepAlive.Apply(this);
    }
}
