using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FGUnity.Editor
{
    /// <summary>
    /// Controls additional global definitions.
    /// </summary>
    public class GlobalDefines
    {
        private GlobalDefines()
        {
            LoadConfigurations();
        }

        #region Singleton

        static private GlobalDefines s_Singleton;
        static public GlobalDefines Instance
        {
            get
            {
                if (s_Singleton == null)
                {
                    s_Singleton = new GlobalDefines();
                }
                return s_Singleton;
            }
        }

        #endregion

        #region Define Classes

        public class Define
        {
            public Define()
            {
                Name = String.Empty;
                Enabled = false;
            }

            public Define(string inName, bool inbEnabled)
            {
                Name = inName;
                Enabled = inbEnabled;
            }

            public string Name;
            public bool Enabled;

            public void Save(StreamWriter inWriter)
            {
                inWriter.WriteLine(Name);
                inWriter.WriteLine(Enabled);
            }

            public void Load(StreamReader inReader)
            {
                Name = inReader.ReadLine();
                Enabled = bool.Parse(inReader.ReadLine());
            }

            public Define Clone()
            {
                return new Define(Name, Enabled);
            }
        }

        // A specific configuration
        public class Configuration
        {
            public Configuration()
            {
                Name = string.Empty;
                Defines = new List<Define>();
            }

            public Configuration(string inName)
            {
                Name = inName;
                Defines = new List<Define>();
            }

            public string Name;
            public List<Define> Defines;

            public void Save(StreamWriter inWriter)
            {
                inWriter.WriteLine(Name);
                inWriter.WriteLine(Defines.Count);
                foreach (Define define in Defines)
                {
                    define.Save(inWriter);
                }
            }

            public void Load(StreamReader inReader)
            {
                Name = inReader.ReadLine();
                int numDefines = int.Parse(inReader.ReadLine());
                Defines = new List<Define>(numDefines);
                for (int i = 0; i < numDefines; ++i)
                {
                    Define newDefine = new Define();
                    newDefine.Load(inReader);
                    Defines.Add(newDefine);
                }
            }

            public Configuration Clone()
            {
                Configuration configuration = new Configuration(Name);
                foreach (Define define in Defines)
                    configuration.Defines.Add(define.Clone());
                return configuration;
            }
        }

        #endregion

        #region Configurations

        private List<Configuration> m_Configurations;
        private Configuration m_CurrentConfiguration;

        /// <summary>
        /// Currently used configuration.
        /// </summary>
        public Configuration CurrentConfiguration
        {
            get { return m_CurrentConfiguration; }
        }

        /// <summary>
        /// List of all configurations.
        /// </summary>
        public List<Configuration> Configurations
        {
            get { return m_Configurations; }
        }

        /// <summary>
        /// Saves all configurations to the file.
        /// </summary>
        public void SaveConfigurations()
        {
            using (StreamWriter writer = new StreamWriter(File.Open(EDITOR_FILE, FileMode.Create)))
            {
                writer.WriteLine(m_Configurations.Count);
                foreach (Configuration configuration in m_Configurations)
                {
                    configuration.Save(writer);
                }
                writer.WriteLine(m_CurrentConfiguration.Name);
                writer.Flush();
            }
        }

        /// <summary>
        /// Loads all configurations from the file.
        /// </summary>
        public void LoadConfigurations()
        {
            if (File.Exists(EDITOR_FILE))
            {
                using (StreamReader reader = new StreamReader(File.Open(EDITOR_FILE, FileMode.Open)))
                {
                    int numConfigurations = int.Parse(reader.ReadLine());
                    m_Configurations = new List<Configuration>(numConfigurations);
                    for (int i = 0; i < numConfigurations; ++i)
                    {
                        Configuration configuration = new Configuration();
                        configuration.Load(reader);
                        m_Configurations.Add(configuration);
                    }
                    string configurationName = reader.ReadLine();
                    m_CurrentConfiguration = FindConfiguration(configurationName);
                }
            }
            else
            {
                ResetConfigurations();
                SaveConfigurations();
            }
        }

        private void ResetConfigurations()
        {
            m_Configurations = new List<Configuration>();

            m_CurrentConfiguration = new Configuration("DEFAULT");
            m_Configurations.Add(m_CurrentConfiguration);

            Define defaultDefine = new Define("DEBUG", true);
            m_CurrentConfiguration.Defines.Add(defaultDefine);
        }

        /// <summary>
        /// Switches the current configuration.
        /// </summary>
        public void SwitchConfiguration(string inConfigName)
        {
            SwitchConfiguration(FindConfiguration(inConfigName));
        }

        /// <summary>
        /// Switches the current configuration.
        /// </summary>
        public void SwitchConfiguration(Configuration inConfiguration)
        {
            if (inConfiguration != null && inConfiguration != m_CurrentConfiguration && m_Configurations.Contains(inConfiguration))
            {
                m_CurrentConfiguration = inConfiguration;
                OutputDefineFiles();
            }
        }

        private Configuration FindConfiguration(string inConfigName)
        {
            foreach (Configuration config in m_Configurations)
            {
                if (config.Name == inConfigName)
                {
                    return config;
                }
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Writes all enabled defines to the Unity-specific files.
        /// </summary>
        public void OutputDefineFiles()
        {
            List<Define> activeDefines = new List<Define>(m_CurrentConfiguration.Defines.Count);
            foreach (Define define in m_CurrentConfiguration.Defines)
            {
                if (define.Enabled && !String.IsNullOrEmpty(define.Name))
                {
                    activeDefines.Add(define);
                }
            }

            foreach (string fileName in UNITY_OUTPUT_FILES)
            {
                if (activeDefines.Count > 0)
                {
                    using (StreamWriter writer = new StreamWriter(File.Open(fileName, FileMode.Create)))
                    {
                        writer.Write("-define:");
                        for (int i = 0; i < activeDefines.Count; ++i)
                        {
                            writer.Write(activeDefines[i].Name);
                            if (i < activeDefines.Count - 1)
                                writer.Write(';');
                        }
                        writer.Flush();
                    }
                }
                else
                {
                    File.Delete(fileName);
                }
            }
        }

        static private readonly string[] UNITY_OUTPUT_FILES = new string[]
        {
            Path.Combine(Application.dataPath, "smcs.rsp"),
            Path.Combine(Application.dataPath, "gmcs.rsp")
        };

        static private readonly string EDITOR_FILE = Path.Combine(Application.dataPath, "globalDefines");
    }
}