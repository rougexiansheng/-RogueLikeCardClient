using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace EditorTool.CustomToolBar
{
    [InitializeOnLoad]
    public class CustomToolBar
    {
        static CustomToolBar()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);
            ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
        }

        private static GUIStyle m_styleLabel;
        private static GUIStyle m_styleButton;
        private static Texture m_buddhaTexture;


        private static void OnLeftToolbarGUI()
        {
            GUILayout.Space(50);

            if (m_styleLabel is null)
            {
                m_styleLabel = new GUIStyle(GUI.skin.label);
                m_styleLabel.fontSize = 14;
                m_styleLabel.richText = true;
            }
        }

        private static void OnRightToolbarGUI()
        {
            if (m_styleButton is null)
            {
                m_styleButton = new GUIStyle(GUI.skin.button);
                m_styleButton.fontSize = 14;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save Project", m_styleButton))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorApplication.ExecuteMenuItem("File/Save Project");
            }
            GUILayout.Space(8);

            if (GUILayout.Button("遊戲入口", m_styleButton))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
                Debug.Log("已切換至遊戲入口場景");
            }
            GUILayout.Space(8);

            if (GUILayout.Button("清本地紀錄", m_styleButton))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("已清除本地紀錄");
            }
            GUILayout.Space(25);
        }
    }
}