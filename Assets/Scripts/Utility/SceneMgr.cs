using System;
using System.Collections;
using UnityEngine;
using FGUnity.Utils;

public class SceneMgr : LazySingletonBehavior<SceneMgr>
{
    public const string MAIN_MENU = "MainMenu";
    public const string INTRO = "Intro";
    public const string GAME = "Game";
	public const string CREDITS = "Credits";
    private const string LOADING_SCENE_NAME = "Loading";

    public void LoadGameScene()
    {
        SequenceBuilder gameLoadingSequence = new SequenceBuilder();
        gameLoadingSequence.Start(EnlearnInstance.LoadSequence()).Then(Session.LoadSequence());
        LoadScene(GAME, gameLoadingSequence);
    }

    private CoroutineHandle m_Routine;

    public bool IsLoading
    {
        get { return m_Routine.IsRunning(); }
    }

    protected override void Awake()
    {
        base.Awake();
        KeepAlive.Apply(this);
    }

    public void LoadScene(string inNextScene, SequenceBuilder inLoadSequence = null)
    {
        Assert.True(!m_Routine.IsRunning(), "Routine is not running.");
        m_Routine = this.SmartCoroutine(LoadScene_Routine(inNextScene, inLoadSequence));
    }

    private IEnumerator LoadScene_Routine(string inNextScene, SequenceBuilder inSequence)
    {
        var asyncOp = Application.LoadLevelAsync(LOADING_SCENE_NAME);
        while (!asyncOp.isDone)
            yield return null;

        GC.Collect();
        yield return 1.0f;

        asyncOp = Resources.UnloadUnusedAssets();
        while (!asyncOp.isDone)
            yield return null;

        yield return null;

        if (inSequence != null)
            yield return inSequence.Run();

        asyncOp = Application.LoadLevelAsync(inNextScene);
        while (!asyncOp.isDone)
            yield return null;
    }
}
