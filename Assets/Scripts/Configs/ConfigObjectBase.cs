using System.IO;
using UnityEngine;

[PreferBinarySerialization]
public abstract class ConfigObjectBase<T> : ScriptableObject where T : class
{
    private static T m_instance;
    public static T instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = LoadConfig<T>();
            }
            return m_instance;
        }
    }

    public virtual bool IsVaild()
    {
        return true;
    }

    public static T LoadConfig<T>() where T : class
    {
        string path = string.Format("Configs/{0}", typeof(T).Name);
        ConfigObjectBase<T> config = Resources.Load<ConfigObjectBase<T>>(path);

        if (config != null)
        {
            return config as T;
        }

#if UNITY_EDITOR
        //Note: If the Resources folder is an Editor subfolder, the Assets in it are loadable from Editor scripts, but are removed from builds.
        var editorAsset = UnityEditor.EditorGUIUtility.Load($"{path}.asset");
        config = (ConfigObjectBase<T>)editorAsset;
        if (config != null)
        {
            return config as T;
        }

        if (UnityEditor.EditorApplication.isUpdating || UnityEditor.EditorApplication.isCompiling)
        {
            return null;
        }

        if (!Application.isPlaying && !Application.isBatchMode)
        {
            int dialogComplex = UnityEditor.EditorUtility.DisplayDialogComplex("Config", $"Config¡i{typeof(T).Name}¡j doesn't exist @ Resource folder", "Skip", "Auto Add (Runtime)", "Auto Add (Editor)");

            if (dialogComplex != 0)
            {
                bool isEditorOnly = dialogComplex == 2;

                var configType = typeof(T);
                var configAssetPath = Path.Combine(isEditorOnly ? ConfigPath.EditorAssetPath : ConfigPath.AssetPath, $"{configType.Name}.asset");

                var createInstance = ScriptableObject.CreateInstance(configType);
                UnityEditor.AssetDatabase.CreateAsset(createInstance, configAssetPath);
                UnityEditor.AssetDatabase.SaveAssets();
                return createInstance as T;
            }
        }

        config?.IsVaild();
#endif

        Debug.Log($"Config¡i{typeof(T).Name}¡j doesn't exist @ Resource folder, LoadPath:{path}");
        return null;
    }
}

public class ConfigPath
{
    public const string ProjectPath = @"Project\";
    public const string AssetPath = @"Assets\Resources\Configs";
    public const string EditorAssetPath = @"Assets\Editor Default Resources\Configs";
}