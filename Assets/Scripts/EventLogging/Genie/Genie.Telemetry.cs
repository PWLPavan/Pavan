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
        // Current version of telemetry
        public const int TELEMETRY_VERSION = 1;
        private readonly string TELEMETRY_VERSION_STRING = TELEMETRY_VERSION.ToString("0.0");

        // Errors
        private const string kError_GenieNotInstalled = "GENIE_SERVICE_NOT_INSTALLED";

        private float m_StartTime = 0;

        #region Initialization and Shutdown

        private void InitializeTelemetry()
        {
#if UNITY_ANDROID
            m_SendResponseHandler = new SendEventResponseHandler();
            m_SyncResponseHandler = new SyncResponseHandler();

            m_TelemetryProxy = new TelemetryProxy();
            m_ExportProxy = new DataExportServiceProxy();
#endif
        }

        private void ShutdownTelemetry()
        {
#if UNITY_ANDROID
            Ref.Dispose(ref m_TelemetryProxy);
            Ref.Dispose(ref m_ExportProxy);

            m_SendResponseHandler = null;
            m_SyncResponseHandler = null;
#endif
        }

        private bool IsTelemetryInitialized()
        {
#if UNITY_ANDROID
            return m_TelemetryProxy != null && m_ExportProxy != null && m_ExportProxy.IsInitialized();
#else
            return true;
#endif
        }

        #endregion

        private double GetUTCTime()
        {
            DateTime now = DateTime.UtcNow;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Math.Floor((now - epoch).TotalMilliseconds);
        }

        private string GetTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK");
        }

        public void LogEvent(GenieEvent inEvent)
        {
            if (inEvent.MinVersion > TELEMETRY_VERSION)
                return;

            var json = new JSONClass();

            json["eid"] = inEvent.Name;
            if (TELEMETRY_VERSION >= 2)
                json["ts"].AsDouble = GetUTCTime();
            else
                json["ts"] = GetTimeString();
            json["ver"] = TELEMETRY_VERSION_STRING;
            json["gdata"]["id"] = BundleInfo.Identifier;
            json["gdata"]["ver"] = BundleInfo.Version;
            json["uid"] = UserID;

            var edata = json["edata"] = new JSONClass();
            inEvent.WriteEDATA(edata);

            Logger.Log(json.ToString());

            if (m_Disabled)
                return;

#if UNITY_ANDROID
            if (m_TelemetryProxy != null)
                m_TelemetryProxy.send(json.ToString(), m_SendResponseHandler);
#endif
        }

        public void SyncEvents()
        {
            if (m_Disabled || IsSyncing)
                return;
#if UNITY_ANDROID
            if (m_ExportProxy != null)
            {
                m_ExportProxy.sync(m_SyncResponseHandler);
                IsSyncing = true;
            }
#endif
        }

        public bool IsSyncing { get; private set; }

        #region Java

#if UNITY_ANDROID

        // Wrapper around telemetry calls
        private TelemetryProxy m_TelemetryProxy;
        private class TelemetryProxy : JavaWrapper
        {
            public TelemetryProxy()
                : base(new AndroidJavaObject("org.ekstep.genieservices.sdks.Telemetry", AndroidHelper.GetCurrentActivity()))
            {
            }

            public override void Dispose()
            {
                finish();
                base.Dispose();
            }

            public void send(string inEvent, ResponseHandler inCallback)
            {
                Logger.Log("Calling 'send'");
                m_InternalObject.Call("send", inEvent, inCallback);
            }

            public void finish()
            {
                //m_InternalObject.Call("finish");
            }
        }

        // Wrapper around data export calls
        private DataExportServiceProxy m_ExportProxy;
        private class DataExportServiceProxy : AndroidJavaProxy, IDisposable
        {
            private AndroidJavaObject m_DataExportService;

            public DataExportServiceProxy()
                : base("org.ekstep.genieservices.sdks.export.IManageDataExport")
            {
                AndroidJavaObject sdkService = new AndroidJavaObject("org.ekstep.genieservices.sdks.SdkFactory");
                sdkService.Call("initializeExportService", AndroidHelper.GetCurrentActivity(), this);
            }

            public void Dispose()
            {
                if (m_DataExportService != null)
                    m_DataExportService.Call("finish");
                Ref.Dispose(ref m_DataExportService);
            }

            public bool IsInitialized()
            {
                return m_DataExportService != null;
            }

            public void onSuccess(AndroidJavaObject inDataExport)
            {
                if (m_DataExportService != null)
                    return;

                Genie.instance.QueueAction(ResponseProcessor.Create(this.onSuccess_Event, inDataExport, false));
            }

            public void onFailure(AndroidJavaObject inResponse)
            {
                // TODO: Make this more meaningful
                // We're getting onFailure calls here when we're not
                // calling any functions to the service
                if (m_DataExportService != null)
                    return;

                Genie.instance.QueueAction(ResponseProcessor.Create(this.onFailure_Event, inResponse));
            }

            public void sync(ResponseHandler inResponseHandler)
            {
                Logger.Log("Calling 'sync'...");
                m_DataExportService.Call("sync", inResponseHandler);
            }

            private void onSuccess_Event(AndroidJavaObject inResponse)
            {
                Logger.Log("Successfully created Data Export service!");
                m_DataExportService = inResponse;
            }

            private void onFailure_Event(AndroidJavaObject inResponse)
            {
                GenieResponseWrapper response = new GenieResponseWrapper(inResponse);
                if (response.getError() == kError_GenieNotInstalled)
                {
                    Genie.instance.OnGenieNotInstalled();
                }
                else
                {
                    Assert.Fail("Error on DataExport: {0}", response.getError());
                }
            }
        }

        // Response for sending events
        private SendEventResponseHandler m_SendResponseHandler;
        private class SendEventResponseHandler : ResponseHandler
        {
            public override void Process(GenieResponseWrapper inWrapper)
            {
                if (inWrapper.getStatus() != "successful")
                {
                    Debug.LogFormat("Sending this event was unsuccessful: {0}", inWrapper.getError());
                }
                else
                {
                    Logger.Log("Send was successful!");
                }
            }
        }

        // Response for syncing events.
        private SyncResponseHandler m_SyncResponseHandler;
        private class SyncResponseHandler : ResponseHandler
        {
            public override void Process(GenieResponseWrapper inWrapper)
            {
                Genie.I.IsSyncing = false;
                if (inWrapper.getStatus() != "successful")
                {
                    Debug.LogWarningFormat("Unable to sync with Genie: {0}", inWrapper.getError());
                }
                else
                {
                    Logger.Log("Sync was successful!");
                }
            }
        }
#endif

        #endregion

        #region Time Management

        public void LogStart()
        {
            m_StartTime = Time.unscaledTime;
            LogEvent(new OE_START());
        }

        public void LogEnd()
        {
            LogEvent(new OE_END(Time.unscaledTime - m_StartTime));
        }

        #endregion
    }
}