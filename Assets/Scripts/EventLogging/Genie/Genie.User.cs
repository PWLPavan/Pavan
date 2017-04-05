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
        public const string EMPTY_UID = "none";

        public string UserID { get; private set; }
        public string UserLanguage { get; private set; }

        public JSONNode UserData { get; private set; }

        public bool IsAnonymousUser { get; private set; }
        public bool IsNullUser { get { return UserID == EMPTY_UID; } }

        #region Initialization and Shutdown

        private void InitializeUserProfile()
        {
#if UNITY_ANDROID
            m_GetUserHandler = new GetUserResponseHandler();
            m_SetAnonUserHandler = new SetAnonUserResponseHandler();
            m_CheckUserChangedHandler = new CheckUserChangedResponseHandler();

            m_UserProfileProxy = new UserProfileProxy();
#endif
        }

        private void ShutdownUserProfile()
        {
#if UNITY_ANDROID
            Ref.Dispose(ref m_UserProfileProxy);

            m_GetUserHandler = null;
            m_SetAnonUserHandler = null;
            m_CheckUserChangedHandler = null;
#endif
        }

        private bool IsUserProfileInitialized()
        {
#if UNITY_ANDROID
            return m_UserProfileProxy != null && m_UserProfileLoaded;
#else
            return true;
#endif
        }

        #endregion

        private bool m_UserProfileLoaded = false;

        private void InitializeProfile()
        {
#if UNITY_ANDROID
            if (!m_Disabled)
                m_UserProfileProxy.getCurrentUser(m_GetUserHandler);
#else
            SetUserProfile(EMPTY_UID);
#endif
            
        }

        private void CheckProfileChanged()
        {
#if UNITY_ANDROID
            if (!m_Disabled)
                m_UserProfileProxy.getCurrentUser(m_CheckUserChangedHandler);
#endif
        }

#if UNITY_ANDROID
        private void GetUserResult(bool inbSuccess, GenieResponseWrapper inResponse)
        {
            if (inbSuccess)
            {
                SetUserProfile(inResponse);
            }
            else
            {
                m_UserProfileProxy.setAnonymousUser(m_SetAnonUserHandler);
            }
        }

        private void MakeAnonymousResult(bool inbSuccess)
        {
            if (inbSuccess)
            {
                m_UserProfileProxy.getCurrentUser(m_GetUserHandler);
            }
            else
            {
                SetUserProfile(EMPTY_UID);
            }
        }

        private void SetUserProfile(GenieResponseWrapper inResponse)
        {
            UserID = inResponse.getStringResult("uid");
            SaveData.instance.SetUserID(UserID);
            m_UserProfileLoaded = true;

            string handle = inResponse.getStringResult("handle", string.Empty);
            IsAnonymousUser = UserID == EMPTY_UID || String.IsNullOrEmpty(handle);

            if (!IsAnonymousUser)
            {
                UserData = new JSONClass();
                UserData["gender"] = inResponse.getStringResult("gender", string.Empty);
                UserData["age"] = inResponse.getIntResult("age").ToStringLookup();
                UserData["standard"] = inResponse.getIntResult("standard").ToStringLookup();
                UserLanguage = inResponse.getStringResult("language", string.Empty);

                Logger.Log("Handle: \"{0}\"\nLanguage: \"{1}\"\nData: {2}", handle, UserLanguage, UserData.ToFormattedString());
            }

            LogStart();
        }
#endif

        private void SetUserProfile(string inUID)
        {
            UserID = inUID;
            SaveData.instance.SetUserID(UserID);
            m_UserProfileLoaded = true;

            IsAnonymousUser = UserID == EMPTY_UID;

            LogStart();
        }

        #region Java

#if UNITY_ANDROID

        // User profile operations
        private UserProfileProxy m_UserProfileProxy;
        private class UserProfileProxy : JavaWrapper
        {
            public UserProfileProxy()
                : base(new AndroidJavaObject("org.ekstep.genieservices.sdks.UserProfile", AndroidHelper.GetCurrentActivity()))
            { }

            public override void Dispose()
            {
                finish();
                base.Dispose();
            }

            public void getCurrentUser(ResponseHandler inCallback)
            {
                m_InternalObject.Call("getCurrentUser", inCallback);
            }

            public void setAnonymousUser(ResponseHandler inCallback)
            {
                m_InternalObject.Call("setAnonymousUser", inCallback);
            }

            public void finish()
            {
                m_InternalObject.Call("finish");
            }
        }

        private GetUserResponseHandler m_GetUserHandler;
        private class GetUserResponseHandler : ResponseHandler
        {
            public override void Process(GenieResponseWrapper inWrapper)
            {
                if (inWrapper.getStatus() == "successful")
                {
                    Logger.Log("Successfully found the current user: {0}!", inWrapper.getStringResult("uid"));
                    Genie.instance.GetUserResult(true, inWrapper);
                }
                else
                {
                    Logger.Log("Unable to find current user.");
                    Genie.instance.GetUserResult(false, inWrapper);
                }
            }
        }

        private CheckUserChangedResponseHandler m_CheckUserChangedHandler;
        private class CheckUserChangedResponseHandler : ResponseHandler
        {
            public override void Process(GenieResponseWrapper inWrapper)
            {
                if (inWrapper.getStatus() == "successful")
                {
                    string uid = inWrapper.getStringResult("uid");
                    if (uid != Genie.I.UserID)
                    {
                        Logger.Log("User changed partway through - restarting the application.");
                        Genie.I.LogEnd();
                        Genie.instance.GetUserResult(true, inWrapper);
                        AndroidHelper.RestartActivity();
                    }
                }
                else
                {
                    AndroidToast.Show("Please sign back into Genie.");
                    AndroidHelper.KillActivity();
                }
            }
        }

        private SetAnonUserResponseHandler m_SetAnonUserHandler;

        private class SetAnonUserResponseHandler : ResponseHandler
        {
            public override void Process(GenieResponseWrapper inWrapper)
            {
                if (inWrapper.getStatus() != "successful")
                {
                    Genie.instance.MakeAnonymousResult(false);
                }
                else
                {
                    Genie.instance.MakeAnonymousResult(true);
                    Logger.Log("Successfully created an anonymous user!");
                }
            }
        }

#endif

        #endregion
    }
}