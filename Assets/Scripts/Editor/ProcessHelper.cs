using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using UnityEngine;

public class ProcessHelper
{
    public ProcessHelper(ProcessStartInfo inStartInfo, string inOutputFile = null)
    {
        StartInfo = inStartInfo;
        OutputFile = inOutputFile;

        ReceivedErrors = false;
    }

    public ProcessStartInfo StartInfo { get; private set; }
    public bool ReceivedErrors { get; private set; }
    public string OutputFile { get; private set; }

    private StreamWriter m_FileOutput;
    private Process m_Process;
    private bool m_Kill = false;

    public Action<string> OnLog;
    public Action<ProcessHelper> OnFinish;

    private void OnOutputReceived(object inSender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            string data = e.Data.Replace("\r", "").Trim();
            if (OnLog == null)
                UnityEngine.Debug.Log(data);
            else
                OnLog(data);

            if (m_FileOutput != null)
                m_FileOutput.Write(data + "\n");
        }
    }

    private void OnErrorReceived(object inSender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            string data = e.Data.Replace("\r", "").Trim();
            if (OnLog == null)
                UnityEngine.Debug.LogError(data);
            else
                OnLog(data);

            if (m_FileOutput != null)
                m_FileOutput.Write(data + "\n");

            ReceivedErrors = true;
        }
    }

    public void Run()
    {
        if (m_Process == null)
        {
            m_Kill = false;
            CreateFileRedirect();
            var thread = new Thread(Thread_Run);
            thread.Start();
        }
    }

    public void Stop()
    {
        if (m_Process != null)
        {
            ReceivedErrors = true;
            m_Kill = true;
        }
    }

    private void Thread_Run()
    {
        using (m_FileOutput)
        {
            using (m_Process = new Process())
            {
                m_Process.StartInfo = StartInfo;

                m_Process.OutputDataReceived += OnOutputReceived;
                m_Process.ErrorDataReceived += OnErrorReceived;

                m_Process.Start();
                m_Process.BeginErrorReadLine();
                m_Process.BeginOutputReadLine();

                while(true)
                {
                    Thread.Sleep(100);

                    if (m_FileOutput != null)
                        m_FileOutput.Flush();

                    if (m_Process.HasExited)
                        break;
                    
                    if (m_Kill)
                    {
                        try
                        {
                            m_Process.Kill();
                            m_Process.WaitForExit();
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogErrorFormat("Unable to kill process {0}: {1}", m_Process.ProcessName, e.Message);
                        }
                        break;
                    }
                }

                try
                {
                    if (!m_Process.HasExited || m_Process.ExitCode != 0)
                    {
                        ReceivedErrors = true;
                    }
                }
                finally
                {
                    if (OnFinish != null)
                        OnFinish(this);
                }
            }

            m_Process = null;
        }
    }

    private void CreateFileRedirect()
    {
        if (!String.IsNullOrEmpty(OutputFile))
        {
            string fullPath = Path.GetFullPath(OutputFile);
            string directoryName = Path.GetDirectoryName(fullPath);
            try
            {
                DirectoryInfo newDir = Directory.CreateDirectory(directoryName);
                m_FileOutput = new StreamWriter(File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogErrorFormat("Unable to create output file '{0}': {1}", fullPath, e.Message);
            }
        }
    }
}
