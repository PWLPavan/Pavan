using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;

[Prefab("Session")]
public class Session : SingletonBehavior<Session>
{
    public Session()
    {
        currentLevel = new Level();
        currentLevelIndex = 0;
        numAttempts = 0;
        eggsEarned = 2;
    }

    protected override void Awake()
    {
        base.Awake();
        Input.multiTouchEnabled = false;

        ParseLevels();
        SyncWithSave();
        CreateInitialLevel();

        this.SmartCoroutine(LoadInitialLevel());
    }

    public void SyncWithSave()
    {
        numEggs = SaveData.instance.Eggs;
        numLevelsCompleted = SaveData.instance.NumLevelsCompleted;
        currentLevelIndex = SaveData.instance.LevelIndex;
    }

	#region Levels

	public TextAsset jsonLevels;
    public TextAsset enlearnPreludeLevels;
    public TextAsset tutorialLevels;
    public bool useEnlearn = false;
    public bool overrideHints = false;

	JSONNode _levels;
    JSONNode _tutorialLevels;

    public int currentLevelIndex { get; private set; }

    public delegate void OnLevelChange(int levelIndex);
    public OnLevelChange onLevelChanged;

    private void ParseLevels()
    {
        if (useEnlearn)
            _levels = JSON.Parse(enlearnPreludeLevels.text);
        else
            _levels = JSON.Parse(jsonLevels.text);

        _tutorialLevels = JSON.Parse(tutorialLevels.text);

        Assert.True(_levels != null, "Levels were parsed.", "Unable to load levels from JSON.");
        Assert.True(_tutorialLevels != null, "Tutorial levels were parsed.", "Unable to load tutorial levels from JSON.");
	}

    private IEnumerator LoadInitialLevel()
    {
        m_QueuedLevels.Clear();
        m_LoadNextLevelRoutine = this.SmartCoroutine(QueueLevelAt(currentLevelIndex));
        yield return m_LoadNextLevelRoutine;

        AdvanceLevel();
    }

    private void CreateInitialLevel()
    {
        currentLevel = GetPremadeLevel(currentLevelIndex);
    }

    #region Level Management

    public IEnumerator IncrementLevel(bool justBeat, bool fromSkip)
    {
        if (fromSkip)
            m_QueuedLevels.Clear();

        m_LoadNextLevelRoutine.Clear();
        m_LoadNextLevelRoutine = this.SmartCoroutine(QueueLevelAt(currentLevelIndex + 1));
        yield return m_LoadNextLevelRoutine;

        AdvanceLevel();

        ++currentLevelIndex;
        if (currentLevelIndex >= _levels.Count)
            currentLevelIndex = useEnlearn ? _levels.Count : 0;

        if (justBeat)
        {
            ++numLevelsCompleted;
            onLevelChanged(currentLevelIndex);
        }
	}

    public IEnumerator DecrementLevel()
    {
        m_QueuedLevels.Clear();
        m_LoadNextLevelRoutine.Clear();

        m_LoadNextLevelRoutine = this.SmartCoroutine(QueueLevelAt(currentLevelIndex - 1));
        yield return m_LoadNextLevelRoutine;

        AdvanceLevel();

        --currentLevelIndex;
        if (currentLevelIndex < 0)
            currentLevelIndex = useEnlearn ? 0 : _levels.Count - 1;
    }

    public IEnumerator GotoLevel(int levelIdx)
    {
        m_QueuedLevels.Clear();
        m_LoadNextLevelRoutine.Clear();

        m_LoadNextLevelRoutine = this.SmartCoroutine(QueueLevelAt(levelIdx));
        yield return m_LoadNextLevelRoutine;

        AdvanceLevel();
        if (useEnlearn)
            currentLevelIndex = levelIdx;
        else
            currentLevelIndex = (levelIdx + _levels.Count) % _levels.Count;
    }

    #endregion

    #region Level Queue

    private Queue<Level> m_QueuedLevels = new Queue<Level>();
    public Level currentLevel { get; private set; }

    private CoroutineHandle m_LoadNextLevelRoutine;

    /// <summary>
    /// Consumes the next available level.
    /// </summary>
    public void AdvanceLevel()
    {
        Assert.True(m_QueuedLevels.Count > 0, "Next level is ready.");
        currentLevel = m_QueuedLevels.Dequeue();
    }

    /// <summary>
    /// Starts loading the next available level.
    /// </summary>
    public void LoadNextLevel()
    {
        if (!m_LoadNextLevelRoutine.IsRunning())
        {
            m_LoadNextLevelRoutine = this.SmartCoroutine(QueueLevelAt(currentLevelIndex + 1));
        }
    }

    /// <summary>
    /// Peeks at the next available level.
    /// </summary>
    public Level PeekNextLevel()
    {
        Assert.True(m_QueuedLevels.Count > 0, "Next level is ready.");
        return m_QueuedLevels.Peek();
    }

    /// <summary>
    /// If there are levels in the queue.
    /// </summary>
    public bool IsNextLevelLoaded
    {
        get { return m_QueuedLevels.Count > 0 && !m_LoadNextLevelRoutine.IsRunning(); }
    }

    // Queues up the next level and sends a signal that it's complete
    private IEnumerator QueueLevelAt(int inLevelIndex)
    {
        Logger.Log("Current levels in queue: {0}", m_QueuedLevels.Count);
        if (m_QueuedLevels.Count == 0)
        {
            if (SaveData.instance.CurrentTutorial != 0)
            {
                Level tutorialLevel = GetTutorialLevel(SaveData.instance.CurrentTutorial);
                QueueLevel(tutorialLevel);
            }
            else if (useEnlearn && inLevelIndex >= _levels.Count)
            {
                Level nextLevel = null;
                bool bWasValidLevel = false;
                while (!bWasValidLevel)
                {
                    EnlearnInstance.I.GetLevel();
                    yield return null;

                    while (!EnlearnInstance.I.TryGetLevel(out nextLevel))
                        yield return null;

                    if (nextLevel.twoPartProblem && nextLevel.isTargetNumber)
                    {
                        Logger.Warn("Invalid level: two part problem is target number.");
                        bWasValidLevel = false;
                    }
                    else
                    {
                        bWasValidLevel = nextLevel.isValid;
                    }

                    if (!bWasValidLevel)
                    {
                        Logger.Log("Invalid level generated: getting the next one.");
                    }
                }

                nextLevel.SubstituteMechanicsTutorials();

                if (nextLevel.tutorialPrelude != 0)
                {
                    SaveData.instance.CurrentTutorial = nextLevel.tutorialPrelude;
                    Level tutorialLevel = GetTutorialLevel(nextLevel.tutorialPrelude);
                    QueueLevel(tutorialLevel);
                }

                if (nextLevel.twoPartProblem)
                {
                    Level secondPart = nextLevel.SplitTwoPart();
                    QueueLevel(nextLevel);
                    QueueLevel(secondPart);
                }
                else
                {
                    QueueLevel(nextLevel);
                }
            }
            else
            {
                Level nextLevel = GetPremadeLevel(inLevelIndex);
                QueueLevel(nextLevel);
                while (nextLevel.twoPartProblem)
                {
                    yield return null;
                    nextLevel = GetPremadeLevel(++inLevelIndex);
                    QueueLevel(nextLevel);
                }
            }

            Logger.Log("Finished loading {0} levels.", m_QueuedLevels.Count);
        }
    }

    // Retrieves a premade level with the given index
    private Level GetPremadeLevel(int inIndex)
    {
        Level nextLevel = new Level();
        inIndex = (inIndex + _levels.Count) % _levels.Count;
        nextLevel.ParseJSON(_levels[inIndex.ToString()], false);
        return nextLevel;
    }

    // Retrieves a premade level for the given tutorial
    private Level GetTutorialLevel(SaveData.FlagType inTutorial)
    {
        Level nextLevel = new Level();
        string tutorialName = GetTutorialName(inTutorial);
        nextLevel.ParseJSON(_tutorialLevels[tutorialName], false);
        nextLevel.AddMechanicFlag(inTutorial);
        return nextLevel;
    }

    public bool HasTutorialLevel(SaveData.FlagType inTutorial)
    {
        return _tutorialLevels[GetTutorialName(inTutorial)] != null;
    }

    private string GetTutorialName(SaveData.FlagType inTutorial)
    {
        switch(inTutorial)
        {
            case SaveData.FlagType.Tutorial_TensPlane:
                return "tensPlane";
            case SaveData.FlagType.Tutorial_Borrowing:
                return "borrowing";
            case SaveData.FlagType.Tutorial_Carryover:
                return "carryover";
            case SaveData.FlagType.Tutorial_Subtract:
                return "subtract";
        }
        return null;
    }

    // Clears up the level queue
    private void ClearLevelQueue()
    {
        currentLevel = null;
        m_QueuedLevels.Clear();
    }

    // Queues a level
    private void QueueLevel(Level inLevel)
    {
        m_QueuedLevels.Enqueue(inLevel);
    }

    #endregion

    #endregion

    #region Persistent Gameplay

    public int numEggs { get; set; } // = 0
	public int numStamps {
		get { return (int)(numEggs / 10); }
	}

    public int numLevelsCompleted { get; private set; }

	#endregion

	#region Realtime Gameplay

	public int numAttempts { get; set; } // = 0
    public int eggsEarned { get; set; } // = 2
    public HintingType currentHint { get; set; }

    public float timeTaken
    {
        get { return Time.unscaledTime - m_ProblemStartTimestamp; }
    }

    public void MarkLevelStart()
    {
        m_ProblemStartTimestamp = Time.unscaledTime;
        m_PauseStartTimestamp = m_ProblemStartTimestamp;
    }

    public void MarkPauseStart()
    {
        m_PauseStartTimestamp = Time.unscaledTime;
    }

    public void MarkPauseEnd()
    {
        float pauseDuration = Time.unscaledTime - m_PauseStartTimestamp;
        m_ProblemStartTimestamp += pauseDuration;
    }

    public void ResetProgress(bool inbReset)
    {
        if (inbReset)
        {
            numAttempts = 0;
            eggsEarned = 2;
            currentHint = HintingType.None;
            MarkLevelStart();
        }
    }

    private float m_ProblemStartTimestamp;
    private float m_PauseStartTimestamp;

	#endregion

    public static IEnumerator LoadSequence()
    {
        if (Exists)
            yield break;

        Session.CreateSingleton();
        yield return null;
        while (!Session.instance.IsNextLevelLoaded)
            yield return null;
    }
}
