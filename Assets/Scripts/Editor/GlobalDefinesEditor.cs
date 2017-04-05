using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

using FGUnity.Utils;

namespace FGUnity.Editor
{
    public class GlobalDefinesEditor : ScriptableWizard
    {
        static private GlobalDefinesEditor s_Editor;

        [MenuItem("File/Build Configuration")]
        static public void CreateEditorWindow()
        {
            if (s_Editor != null)
            {
                s_Editor.Show();
                return;
            }

            GlobalDefines defines = GlobalDefines.Instance;

            s_Editor = ScriptableWizard.DisplayWizard<GlobalDefinesEditor>("Global Defines");
            s_Editor.minSize = new Vector2(500, 500);
            s_Editor.maxSize = new Vector2(800, 800);

            s_Editor.m_HighlightedConfiguration = defines.CurrentConfiguration;
        }

        private GlobalDefines.Configuration m_HighlightedConfiguration;
        private List<GlobalDefines.Configuration> m_ConfigsToDelete = new List<GlobalDefines.Configuration>();
        private List<GlobalDefines.Define> m_DefinesToDelete = new List<GlobalDefines.Define>();

        private void OnGUI()
        {
            if (m_HighlightedConfiguration == null)
                m_HighlightedConfiguration = GlobalDefines.Instance.CurrentConfiguration;

            EditorGUILayout.BeginVertical();
            {
                // Top columns: config and define
                EditorGUILayout.BeginHorizontal();
                {
                    DisplayConfigurations();
                    DisplayDefines();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(40);

                // Lower buttons
                EditorGUILayout.BeginVertical();
                {
                    if (GUILayout.Button("Save and Switch"))
                    {
                        GlobalDefines.Instance.SwitchConfiguration(m_HighlightedConfiguration);
                        GlobalDefines.Instance.OutputDefineFiles();
                        GlobalDefines.Instance.SaveConfigurations();
                        ForceRecompile();
                        //Close();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnDisable()
        {
            s_Editor = null;
        }

        private RequestedAction DisplayNewButton()
        {
            RequestedAction action = RequestedAction.None;

            EditorGUILayout.BeginHorizontal(GUILayout.Height(LINE_HEIGHT));
            {
                // Button to delete it
                if (GUILayout.Button("+"))
                    action = RequestedAction.Create;
            }
            EditorGUILayout.EndHorizontal();

            return action;
        }

        private void DisplayConfigurations()
        {
            EditorGUILayout.BeginVertical();
            {
                // Title of column
                EditorGUILayout.BeginHorizontal(GUILayout.Height(LINE_HEIGHT));
                GUILayout.FlexibleSpace();
                GUILayout.Label("Configurations");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GlobalDefines.Configuration toSelect = null;
                m_ConfigsToDelete.Clear();

                // Each configuration
                List<GlobalDefines.Configuration> configurations = GlobalDefines.Instance.Configurations;
                for (int i = 0; i < configurations.Count; ++i)
                {
                    GlobalDefines.Configuration config = configurations[i];
                    RequestedAction action = DisplayConfiguration(config);
                    if (action == RequestedAction.Delete && configurations.Count > 1)
                    {
                        m_ConfigsToDelete.Add(config);
                        if (config == m_HighlightedConfiguration)
                        {
                            if (i == configurations.Count - 1)
                                toSelect = configurations[i - 1];
                            else
                                toSelect = configurations[i + 1];
                        }
                    }
                    else if (action == RequestedAction.Select)
                        toSelect = config;
                }

                if (DisplayNewButton() == RequestedAction.Create)
                {
                    GlobalDefines.Configuration newConfiguration = m_HighlightedConfiguration.Clone();
                    newConfiguration.Name = "NEW_CONFIG";
                    GlobalDefines.Instance.Configurations.Add(newConfiguration);
                    toSelect = newConfiguration;
                }

                foreach (GlobalDefines.Configuration config in m_ConfigsToDelete)
                {
                    GlobalDefines.Instance.Configurations.Remove(config);
                }

                if (toSelect != null)
                    m_HighlightedConfiguration = toSelect;
            }
            EditorGUILayout.EndVertical();
        }

        private RequestedAction DisplayConfiguration(GlobalDefines.Configuration inConfiguration)
        {
            RequestedAction action = RequestedAction.None;

            EditorGUILayout.BeginHorizontal(GUILayout.Height(LINE_HEIGHT));
            {
                // Button to delete it
                if (GlobalDefines.Instance.Configurations.Count > 1)
                {
                    if (GUILayout.Button("X", GUILayout.Width(24)))
                        action = RequestedAction.Delete;
                }

                // Name of the configuration
                string newName = EditorGUILayout.TextField(inConfiguration.Name);
                if (!string.IsNullOrEmpty(newName))
                    inConfiguration.Name = newName;

                if (m_HighlightedConfiguration != inConfiguration)
                {
                    // Button to select it
                    if (GUILayout.Button("...", GUILayout.Width(48)))
                        action = RequestedAction.Select;
                }
            }
            EditorGUILayout.EndHorizontal();

            return action;
        }

        private void DisplayDefines()
        {
            EditorGUILayout.BeginVertical();
            {
                // Title of column
                EditorGUILayout.BeginHorizontal(GUILayout.Height(LINE_HEIGHT));
                GUILayout.FlexibleSpace();
                GUILayout.Label("Defines");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                m_DefinesToDelete.Clear();

                // All defines
                foreach (GlobalDefines.Define define in m_HighlightedConfiguration.Defines)
                {
                    RequestedAction action = DisplayDefine(define);
                    if (action == RequestedAction.Delete)
                        m_DefinesToDelete.Add(define);
                }

                foreach (GlobalDefines.Define define in m_DefinesToDelete)
                {
                    m_HighlightedConfiguration.Defines.Remove(define);
                }

                if (DisplayNewButton() == RequestedAction.Create)
                {
                    m_HighlightedConfiguration.Defines.Add(new GlobalDefines.Define("NEW_DEFINE", false));
                }
            }
            EditorGUILayout.EndVertical();
        }

        private RequestedAction DisplayDefine(GlobalDefines.Define inDefine)
        {
            RequestedAction action = RequestedAction.None;

            EditorGUILayout.BeginHorizontal(GUILayout.Height(LINE_HEIGHT));
            {
                // Button to delete it
                if (GUILayout.Button("X", GUILayout.Width(24)))
                    action = RequestedAction.Delete;

                // Name of the configuration
                string newName = EditorGUILayout.TextField(inDefine.Name);
                if (!string.IsNullOrEmpty(newName) && newName.IndexOf(' ') < 0)
                    inDefine.Name = newName;

                // If it's enabled
                bool newEnabled = EditorGUILayout.Toggle(inDefine.Enabled, GUILayout.Width(16));
                inDefine.Enabled = newEnabled;
            }
            EditorGUILayout.EndHorizontal();

            return action;
        }

        private void ForceRecompile()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset("Assets/Scripts/Utility/Assert.cs", ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset("Assets/Scripts/Editor/GlobalDefines.cs", ImportAssetOptions.ForceUpdate);
        }

        private const int LINE_HEIGHT = 20;

        private enum RequestedAction
        {
            None,
            Delete,
            Select,
            Create
        }
    }

}