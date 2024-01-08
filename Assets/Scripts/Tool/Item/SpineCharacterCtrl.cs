using Cysharp.Threading.Tasks;
using Spine.Unity;
using System.IO;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Button))]
public class SpineCharacterCtrl : MonoBehaviour
{
    public SkeletonGraphic spine;
    public BoneFollowerGraphic centerPoint;
    public BoneFollowerGraphic topPoint;
    public BoneFollowerGraphic bottomPoint;
    public BoneFollowerGraphic weapenPoint;
    public BoneFollowerGraphic bodyPoint;
    public Button button;
    public SpineSkinEnum currentSkin;

    public enum SpineSkinEnum
    {
        Default,
        Origin,
        Damage
    }

    public UniTask AttackTrigger()
    {
        var task = new UniTaskCompletionSource();
        spine.AnimationState.Event += (t, e) =>
        {
            if (e.Data.Name == "Attack")
            {
                task.TrySetResult();
            }
        };
        return task.Task;
    }

    /// <summary>
    /// 檢查當前skin是否有合法
    /// </summary>
    /// <returns></returns>
    public void SetSkin(SpineSkinEnum skinEnum)
    {
        currentSkin = skinEnum;
        spine.initialSkinName = skinEnum.ToString();
        spine.Initialize(true);

        centerPoint.Initialize();
        topPoint.Initialize();
        bottomPoint.Initialize();
        weapenPoint.Initialize();
        bodyPoint.Initialize();
    }

    public bool HaveAnimaton(SpineAnimationEnum animationEnum)
    {
        var animationNameAhead = currentSkin == SpineSkinEnum.Default ? "" : currentSkin.ToString() + "_";
        var animationName = $"{animationNameAhead}{animationEnum}";
        return spine.Skeleton.Data.Animations.Find(a => a.Name == animationName) != null;
    }

    public UniTask PlayAnimation(SpineAnimationEnum animationEnum, float speed = 1)
    {
        var t = new UniTaskCompletionSource();
        var animationNameAhead = currentSkin == SpineSkinEnum.Default ? "" : currentSkin.ToString() + "_";
        spine.AnimationState.ClearTrack(0);
        spine.timeScale = speed;
        spine.AnimationState.SetAnimation(0, $"{animationNameAhead}{animationEnum}", false).Complete += _ =>
        {
            t.TrySetResult();
            spine.timeScale = 1;
            PlayIdle(false);
        };
        return t.Task;
    }

    /// <summary>
    /// 預設播完 都會執行Idle01
    /// </summary>
    /// <param name="animationEnum"></param>
    public void PlayAnimationOneShot(SpineAnimationEnum animationEnum, SpineAnimationEnum defalutEnum = SpineAnimationEnum.None, bool isRandomStartTime = false)
    {
        var animationNameAhead = currentSkin == SpineSkinEnum.Default ? "" : currentSkin.ToString() + "_";
        var currentEnum = animationEnum;
        if (!HaveAnimaton(currentEnum)) currentEnum = defalutEnum;
        if (HaveAnimaton(currentEnum))
        {
            spine.AnimationState.ClearTrack(0);
            spine.AnimationState.SetAnimation(0, $"{animationNameAhead}{currentEnum}", false).Complete += _ =>
            {
                PlayIdle(isRandomStartTime);
            };
        }
        else
        {
            Debug.LogError($"沒有這個動畫名稱 :{animationNameAhead} - {animationEnum} / {defalutEnum}");
        }
    }

    /// <summary>
    /// 執行完一次Idle01 會隨機執行 Idle01/Idle02(如果有)
    /// </summary>
    public void PlayIdle(bool isRandomStartTime = false)
    {
        var animationNameAhead = currentSkin == SpineSkinEnum.Default ? "" : currentSkin.ToString() + "_";
        var entry = spine.AnimationState.SetAnimation(0, $"{animationNameAhead}{SpineAnimationEnum.Idle01}", false);
        if (isRandomStartTime)
        {
            float totalDuration = entry.AnimationEnd - entry.AnimationStart;
            entry.TrackTime = entry.AnimationStart + Random.Range(0, totalDuration);
        }
        entry.Complete += _ =>
        {
            var animationEnum = (SpineAnimationEnum)Random.Range(4, 6);
            PlayAnimationOneShot(animationEnum, SpineAnimationEnum.Idle01);
        };
    }

#if UNITY_EDITOR
    string center = "Center";
    string body = "Body";
    string top = "Top";
    string bottom = "Bottom";
    string weapen = "Weapen";
    /// <summary>
    /// 物件名稱 跟腳本 BoneFollowerGraphic 都要定義好
    /// 才能執行
    /// </summary>
    [InspectorButton]
    private void Init()
    {
        spine = GetComponentInChildren<SkeletonGraphic>();

        centerPoint = SetBoneFollower(center);
        bodyPoint = SetBoneFollower(body);
        topPoint = SetBoneFollower(top);
        bottomPoint = SetBoneFollower(bottom);
        weapenPoint = SetBoneFollower(weapen);

        button = GetComponent<Button>();
        button.transition = Selectable.Transition.None;

        BoneFollowerGraphic SetBoneFollower(string name)
        {
            BoneFollowerGraphic point;
            var transformCenter = spine.transform.Find(name);
            if (transformCenter == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(BoneFollowerGraphic));
                go.transform.SetParent(spine.transform);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                transformCenter = go.transform;
            }
            point = transformCenter.GetComponent<BoneFollowerGraphic>() ?? transformCenter.AddComponent<BoneFollowerGraphic>();
            point.skeletonGraphic = spine;
            point.SetBone(name);
            Debug.Log($"{this.name}設定{name}{point.transform.localPosition}為參考點座標");
            return point;
        }
    }

    [MenuItem("Assets/AutoSettingSpineBoneFollower", priority= 100)]
    private static void AutoSettingSpineBoneFollower()
    {
        var spinePath = "Assets/DynamicAssets/Spine";
        var characterPath = "Assets/DynamicAssets/Spine/Prefabs/Character";
        var monsterPath = "Assets/DynamicAssets/Spine/Prefabs/Monster";
        var selectPath = GetSelectedPathOrFallback();
        //Debug.Log($"path: {selectPath}");

        if (selectPath != spinePath)
        {
            EditorUtility.DisplayDialog("Spine", @"請選擇DynamicAssets\Spine資料夾", "確定");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:prefab", new string[] { characterPath, monsterPath });
        var changeCount = 0;
        for (var i = 0; i < guids.Length; i++)
        {
            var guid = guids[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            EditorUtility.DisplayProgressBar("Spine", $"處理{prefab.name}中...({(i + 1)}/{guids.Length})", (float)(i + 1) / guids.Length);
            var prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            var ctrl = prefabInstance.GetComponent<SpineCharacterCtrl>() ?? prefabInstance.AddComponent<SpineCharacterCtrl>();
            ctrl.Init();
            if (PrefabUtility.HasPrefabInstanceAnyOverrides(prefabInstance, false))
            {
                PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.AutomatedAction);
                changeCount++;
            }
            DestroyImmediate(prefabInstance);
            Thread.Sleep(100);
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Spine", $"成功變更{changeCount}個Spine角色!", "確認");


        string GetSelectedPathOrFallback()
        {
            var path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }

    public void PrintPoint()
    {
        Debug.Log($"{name}參考點: " +
            $"Center: {centerPoint.transform.localPosition}, " +
            $"Body: {bodyPoint.transform.localPosition}, " +
            $"Top: {topPoint.transform.localPosition}, " +
            $"Bottom: {bottomPoint.transform.localPosition}, " +
            $"Weapen: {weapenPoint.transform.localPosition}");
    }
#endif
}