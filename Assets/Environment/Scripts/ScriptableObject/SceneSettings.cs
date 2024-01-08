using UnityEngine;
using UnityEngine.Rendering;

namespace MAY
{

    /// <summary>
    /// 可在Project視窗，右鍵 > Environment/Scene Settings 新增要該場景要套用的設定檔案
    /// </summary>
    [CreateAssetMenu(fileName = "SceneSettings", menuName = "Environment/Scene Settings")]
    public class SceneSettings : ScriptableObject
    {
        [Header("編號")]
        public int Number = 0;

        [Header("天空盒子")]
        public Material skyBox = null;
        [Space(20)]
        [Header("Fog顏色")]
        public Color fogColor = Color.gray;
        [Header("Fog距離(Mode=Linear)")]
        public FogMode fogMode = FogMode.Linear;
        public float fogDensityStart = 30f;
        public float fogDensityEnd = 120f;

        [Space(20)]
        [Header("LightSetting")]
        public GameObject lightGroup_Prefab = null;
        [Space(20)]
        [Header("BackGround")]
        public GameObject BG_Prefab = null;


    }

}