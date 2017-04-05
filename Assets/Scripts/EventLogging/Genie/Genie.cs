using System.Text;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;
using Ekstep;

namespace Ekstep
{
    public partial class Genie : LazySingletonBehavior<Genie>
    {
        private bool m_Disabled = false;
        private bool m_Killed = false;

        #region Singleton

        // AlexB: Shortcut for SingletonBehavior.instance, since "I" was previously used everywhere
        public static Genie I
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Unity Events

        protected override void Awake()
        {
            base.Awake();
            UserID = EMPTY_UID;
        }

        private void Start()
        {
            if (Exists && instance != this)
                return;

            KeepAlive.Apply(this);

            Assert.Initialize();
            Logger.Log("Initializing");
            Initialize();

            this.WaitOneFrameThen(InitializeProfile);

#if UNITY_ANDROID
            MessageHook.instance.OnUpdate += ProcessQueue;
#endif
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID
            if (MessageHook.Exists)
                MessageHook.instance.OnUpdate -= ProcessQueue;

            ShutdownTelemetry();
            ShutdownUserProfile();
#endif
        }

        #endregion

        #region Initialization and Shutdown

        private void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            m_Disabled = false;
#else
            m_Disabled = true;
#endif
            if (!m_Disabled)
            {
                if (IsLaunchValid())
                {
                    InitializeUserProfile();
                    InitializeTelemetry();
                }
                else
                {
                    m_Disabled = true;
                    m_Killed = true;
                    OnGenieNotLaunch();
                }
            }
            else
            {
                SetUserProfile(EMPTY_UID);
            }
        }

        private void Shutdown()
        {
            ShutdownTelemetry();
            ShutdownUserProfile();
        }

        public bool IsInitialized
        {
            get
            {
#if UNITY_ANDROID
                if (m_Killed)
                    return false;

                if (m_Disabled)
                    return true;

                return IsTelemetryInitialized() && IsUserProfileInitialized();
#else
                return true;
#endif
            }
        }

        #endregion

        #region Manager

        public void ForceShutdown()
        {
            Shutdown();
            DestroySingleton();
        }

        public void AttemptOpenGenie()
        {
            AndroidHelper.LaunchActivity("org.ekstep.android.genie");
        }

        private void OnApplicationQuit()
        {
            if (Exists)
            {
                LogEnd();
                ForceShutdown();
                AttemptOpenGenie();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && m_Killed)
            {
                AndroidHelper.KillActivity();
                return;
            }
            if (!paused && Exists && IsInitialized)
            {
                CheckProfileChanged();
            }
        }

        private void OnGenieNotInstalled()
        {
#if UNITY_ANDROID
            var dialog = new AndroidDialog.AlertDialogBuilder();
            dialog.SetMessage("Genie Services is not installed.\nPlease make sure it is installed before launching this game.");
            dialog.SetPositiveButton("Okay.", (int a) => { AndroidHelper.KillActivity(); });
            dialog.SetTitle("Unable to start.");
            dialog.ShowAndDispose();
#else
            CrashScreen.Create("Genie Services is not installed.\n\nPlease make sure Genie Services is installed on your device\nbefore launching this game.", true);
#endif
        }

        private bool IsLaunchValid()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Debug.isDebugBuild)
                return true;

            return AndroidHelper.GetIntentExtra("origin") == "Genie";
#else
            return true;
#endif
        }

        private void OnGenieNotLaunch()
        {
#if UNITY_ANDROID
            var dialog = new AndroidDialog.AlertDialogBuilder();
            dialog.SetMessage("Please launch the game from Genie.");
            dialog.SetPositiveButton("Okay", (int a) => { AndroidHelper.KillActivity(); });
            dialog.ShowAndDispose();
#endif
        }

        #endregion

        #region Queue

#if UNITY_ANDROID
        private object m_ActionQueueLock = new object();
        private Queue<ResponseProcessor> m_ActionQueue = new Queue<ResponseProcessor>();

        private void QueueAction(ResponseProcessor inAction)
        {
            lock(m_ActionQueueLock)
            {
                m_ActionQueue.Enqueue(inAction);
            }
        }

        private void ProcessQueue()
        {
            lock(m_ActionQueueLock)
            {
                if (m_ActionQueue.Count == 0)
                    return;
            }

            while(true)
            {
                ResponseProcessor action = null;
                bool bEmpty = false;
                lock(m_ActionQueueLock)
                {
                    action = m_ActionQueue.Dequeue();
                    bEmpty = m_ActionQueue.Count == 0;
                }
                action.Process();
                action.Dispose();

                if (bEmpty)
                    break;
            }
        }
#endif

        #endregion
    }
}