using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.UIElements;

public class GameConfigSettingsProvider : SettingsProvider
{
    public static string SourcePath = Path.Combine(ConfigPath.AssetPath, $"{nameof(GameConfig)}.asset");

    private SerializedObject m_settings;

    public static Dictionary<string, GUIContent> PropertyDic = new Dictionary<string, GUIContent>()
    {
        { nameof(GameConfig.enableReporter), new GUIContent("啟用Reporter")},
        { nameof(GameConfig.enableStatsMonitor), new GUIContent("啟用狀態監視器")},
        { nameof(GameConfig.targetFrameRate), new GUIContent("預設畫面刷新率")}
    };

    public SerializedProperty GetPropertyField(string propertyName)
    {
        var property = m_settings.FindProperty(propertyName);
        if (property is null)
            return null;

        PropertyDic.TryGetValue(propertyName, out var displayName);
        EditorGUILayout.PropertyField(property, displayName);
        return property;
    }

    public GameConfigSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
    {
    }

    public static bool IsSettingsAvailable()
    {
        return File.Exists(SourcePath);
    }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        m_settings = GetSerializedSettings();
    }

    public override void OnGUI(string searchContext)
    {
        m_settings.Update();

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        GetPropertyField(nameof(GameConfig.enableReporter));
        GetPropertyField(nameof(GameConfig.enableStatsMonitor));

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Screen", EditorStyles.boldLabel);
        GetPropertyField(nameof(GameConfig.targetFrameRate));

        m_settings.ApplyModifiedProperties();
    }


    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        if (IsSettingsAvailable())
        {
            var provider = new GameConfigSettingsProvider(ConfigPath.ProjectPath + "Game Config", SettingsScope.Project);
            provider.keywords = PropertyDic.Keys;
            return provider;
        }

        return null;
    }

    public static GameConfig GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<GameConfig>(SourcePath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(settings, SourcePath);
            AssetDatabase.SaveAssets();
        }

        return settings;
    }
    public static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
}
