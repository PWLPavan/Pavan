using Enlearn.Client;
using FGUnity.Utils;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnlearnInstance : LazySingletonBehavior<EnlearnInstance>
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private const bool DUMMY = false;
#else
    private const bool DUMMY = true;
#endif

    public enum Action
    {
        AddToOnes,
        RemoveFromOnes,
        AddNestToOnes,
        AddChickenToTens,
        AddToTens,
        RemoveFromTens,
        ConvertToTens,
        ConvertToOnes,
        Submit,
        ResetProblem,
        ColumnCount
    }

    private UnityEnlearnClientBuilder m_ClientBuilder;
    private IUnityEnlearnClient m_Client;
    private Guid m_StudentID;
    private JSONNode m_StudentInfo;
    private Level m_NextLevel;

    protected override void Awake()
    {
        base.Awake();

        if (Ekstep.Genie.instance.IsNullUser)
        {
            m_StudentID = Guid.Empty;
            m_StudentInfo = null;
        }
        else if (Ekstep.Genie.instance.IsAnonymousUser)
        {
            m_StudentID = new Guid(Ekstep.Genie.instance.UserID);
            m_StudentInfo = null;
        }
        else
        {
            m_StudentID = new Guid(Ekstep.Genie.instance.UserID);
            m_StudentInfo = Ekstep.Genie.instance.UserData;
        }

        m_ClientBuilder = GetComponent<UnityEnlearnClientBuilder>();
        if (m_ClientBuilder == null)
            m_ClientBuilder = gameObject.AddComponent<UnityEnlearnClientBuilder>();

        Logger.Log("Student GUID: {0}", m_StudentID);

        MessageHook.instance.OnUpdate += UpdateCallQueue;
    }

    public Action<JSONNode> OnLogActions;

    public IUnityEnlearnClient Client
    {
        get { return m_Client; }
    }

    public static EnlearnInstance I
    {
        get { return instance; }
    }

    #region Initialization

    public static IEnumerator LoadSequence()
    {
        if (Exists && instance.IsInitialized())
            yield break;

        EnlearnInstance.CreateSingleton();
        yield return null;
        EnlearnInstance.instance.CreateClient();
        yield return CoroutineUtil.WaitForCondition(EnlearnInstance.instance.IsInitialized, 0.2f);
        Logger.Log("Finished loading enlearn.");
    }

    public bool IsInitialized()
    {
        return m_Client != null || DUMMY;
    }

    private void CreateClient()
    {
        if (!DUMMY)
            this.SmartCoroutine(CreateClientRoutine());
    }

    private IEnumerator CreateClientRoutine()
    {
        Logger.Log("Initializing Enlearn client...");
        m_ClientBuilder.CreateClient(BundleInfo.Identifier);
        while (!m_ClientBuilder.IsClientReady())
        {
            yield return 0.1f;
        }
        Logger.Log("Enlearn client is ready!");
        m_Client = m_ClientBuilder.GetEnlearnClient();

        if (m_StudentInfo != null)
            QueueCall(CallType.UpdateStudentInfo, m_StudentInfo, true, 2.0f);
    }

    #endregion

    #region GetNextProblem

    public void GetLevel()
    {
        if (m_Client != null)
        {
            QueueCall(CallType.GetNextProblem, null, true);
        }
        else
        {
            string levelJSON = "\"level\": { \"expression\": \"95\", \"tensCount\": 6, \"onesCount\": 7, \"tensColumnEnabled\": true, \"tensQueueEnabled\": true, \"onesQueueEnabled\": true, \"twoPartProblem\": false, \"seatbelts\": false, \"useNumberPad\": false, \"startHandhold\": true }";

            Level level = new Level();
            JSONNode json = JSON.Parse(levelJSON);
            level.ParseJSON(json, true);

            m_NextLevel = level;
        }
    }

    public bool TryGetLevel(out Level outLevel)
    {
        if (m_NextLevel != null)
        {
            outLevel = m_NextLevel;
            m_NextLevel = null;
            return true;
        }

        outLevel = null;
        return false;
    }

    private void GetNextProblemImpl(EnlearnCall inCall)
    {
        m_Client.GetNextProblem(m_StudentID, inCall.OnResponse);
    }

    private void GetNextProblemResponse(EnlearnCall inCall, JSONNode inData)
    {
        Level level = new Level();

        Logger.Log("Received level JSON:\n{0}", inData.ToFormattedString());
        level.ParseJSON(inData, true);

        m_NextLevel = level;
    }

    #endregion

    #region LogStudentActions

    private JSONNode CreateActionJSON(Action inAction, IList<string> inParameters)
    {
        JSONClass json = new JSONClass();
        json.Add("action", Enum.GetName(inAction.GetType(), inAction));

        JSONClass parameters = new JSONClass();

        if (inParameters != null)
        {
            for (int a = 0, b = 1; b < inParameters.Count; a += 2, b += 2)
                parameters.Add(inParameters[a], inParameters[b]);
        }

        json.Add("data", parameters);

        return json;
    }

    public void LogActions(Action action, params string[] parameters)
    {
        LogActions(action, (IList<string>)parameters);
    }

    public void LogActions(Action action, IList<string> parameters)
    {
        if (m_Client != null && Session.instance.currentLevel.fromEnlearn)
        {
            JSONNode enlearnData = CreateActionJSON(action, parameters);
            QueueCall(CallType.LogStudentActions, enlearnData, action == Action.Submit || action == Action.ResetProblem);
        }
    }

    private void LogStudentActionsImpl(EnlearnCall inCall)
    {
        Logger.Log("Sending LogStudentActions: {0}", inCall.Data.ToFormattedString());
        m_Client.LogStudentActions(m_StudentID, inCall.Data.ToString(), inCall.OnResponse);
    }

    private void LogStudentActionsResponse(EnlearnCall inCall, JSONNode inData)
    {
        if (OnLogActions != null)
            OnLogActions(inData);
    }

    #endregion

    #region UpdateStudentInfo

    private void UpdateStudentInfoImpl(EnlearnCall inCall)
    {
        m_Client.UpdateStudentInfo(m_StudentID, inCall.Data.ToString());
        this.WaitSecondsThen(1.0f, inCall.OnResponse, string.Empty);
    }

    private void UpdateStudentInfoResponse(EnlearnCall inCall)
    { }

    #endregion

    #region Call Queue

    private Queue<EnlearnCall> m_Calls = new Queue<EnlearnCall>();

    private enum CallType : byte
    {
        GetNextProblem,
        UpdateStudentInfo,
        LogStudentActions
    }

    private enum CallState : byte
    {
        Queued,
        InProgress,
        Finished
    }

    private class EnlearnCall : IDisposable
    {
        private const int RETRY_LIMIT = 3;

        public CallType Type;
        public JSONNode Data;
        public bool Required;

        public CallState State { get; private set; }
        public int Attempts { get; private set; }

        public float Delay = 0.0f;

        public EnlearnCall(CallType inCall, JSONNode inData, bool inbRequired)
        {
            Type = inCall;
            Data = inData;
            Required = inbRequired;

            Attempts = 0;
            Delay = 0.0f;
        }

        public void Invoke()
        {
            EnlearnInstance enlearn = EnlearnInstance.instance;

            State = CallState.InProgress;
            switch(Type)
            {
                case CallType.GetNextProblem:
                    enlearn.GetNextProblemImpl(this);
                    break;
                case CallType.LogStudentActions:
                    enlearn.LogStudentActionsImpl(this);
                    break;
                case CallType.UpdateStudentInfo:
                    enlearn.UpdateStudentInfoImpl(this);
                    break;
            }
        }

        public void OnResponse(string inResponse)
        {
            EnlearnInstance enlearn = EnlearnInstance.instance;

            if (Type == CallType.UpdateStudentInfo)
            {
                State = CallState.Finished;
                return;
            }

            JSONNode response = JSON.Parse(inResponse);
            bool bSuccess = CheckErrors(response);

            if (bSuccess)
            {
                State = CallState.Finished;
                switch (Type)
                {
                    case CallType.GetNextProblem:
                        enlearn.GetNextProblemResponse(this, response);
                        break;
                    case CallType.LogStudentActions:
                        enlearn.LogStudentActionsResponse(this, response);
                        break;
                    case CallType.UpdateStudentInfo:
                        enlearn.UpdateStudentInfoResponse(this);
                        break;
                }
            }
        }

        public void NeedsRetry()
        {
            State = CallState.Queued;
        }

        private bool CheckErrors(JSONNode inResponse)
        {
            if (inResponse == null)
            {
                NeedsRetry();
                return false;
            }

            string responseError = inResponse["error"].Value;
            if (responseError != null && !String.IsNullOrEmpty(responseError))
            {
                Logger.Warn("Received error from '{0}' call:\n{1}", Type, inResponse.ToFormattedString());

                if (responseError == "jni")
                {
                    CrashScreen.Create("Unfortunately, the Enlearn adaptive engine has crashed.", true);
                    Assert.Fail("A '{0}' call produced the following error:\n{1}", Type, inResponse.ToFormattedString());
                    State = CallState.Finished;
                }
                else if (responseError.Contains("progress"))
                {
                    NeedsRetry();
                    Delay = 1.5f;
                    return false;
                }
                else
                {
                    if (Attempts++ < RETRY_LIMIT)
                    {
                        NeedsRetry();
                    }
                    else
                    {
                        if (Required)
                        {
                            CrashScreen.Create("Unfortunately, the Enlearn adaptive engine has crashed.", true);
                            Assert.Fail("A '{0}' call timed out for {1} attempts.", Type, Attempts.ToStringLookup());
                            State = CallState.Finished;
                        }
                        else
                        {
                            Logger.Warn("A non-essential '{0}' call timed out {1} attempts. Discarding...", Type, Attempts.ToStringLookup());
                            State = CallState.Finished;
                        }
                    }
                }

                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (Data != null)
            {
                Data.Clear();
                Data = null;
            }
        }
    }

    private void QueueCall(CallType inCall, JSONNode inData, bool inbRequired, float inDelay = 0.0f)
    {
        EnlearnCall call = new EnlearnCall(inCall, inData, inbRequired);
        call.Delay = inDelay;
        m_Calls.Enqueue(call);
    }

    private void UpdateCallQueue()
    {
        if (m_Calls.Count == 0)
            return;

        EnlearnCall nextCall = m_Calls.Peek();
        if (nextCall.Delay > 0)
        {
            nextCall.Delay -= Time.deltaTime;
        }
        else if (nextCall.State == CallState.Queued)
        {
            nextCall.Invoke();
        }
        else if (nextCall.State == CallState.Finished)
        {
            nextCall.Dispose();
            m_Calls.Dequeue();
        }
    }

    #endregion

    static public bool CheckInstalled()
    {
#if UNITY_ANDROID

        if (AndroidHelper.IsInstalled("org.enlearn.enlearnService"))
            return true;

        var dialog = new AndroidDialog.AlertDialogBuilder();
        dialog.SetMessage("Enlearn Service is not installed.\nPlease make sure it is installed before launching this game.");
        dialog.SetPositiveButton("Okay.", (int a) => { AndroidHelper.KillActivity(); });
        dialog.SetTitle("Unable to start.");
        dialog.ShowAndDispose();

        return false;
#else
        return true;
#endif
    }
}
