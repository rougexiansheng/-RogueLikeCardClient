using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MAY
{

    /// <summary>
    /// 自動套用場景設定。
    /// </summary>
    public class ApplySceneSettings : MonoBehaviour
    {
        static ApplySceneSettings instance;

        public static ApplySceneSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ApplySceneSettings>();

                    if (instance == null)
                    {
                        GameObject managerGo = new GameObject("ApplySceneSettings");
                        instance = managerGo.AddComponent<ApplySceneSettings>();
                    }
                }
                return instance;
            }
        }



        [Header("當前使用的設定檔案(自動放置)")]
        [SerializeField] SceneSettings sceneSettings = null;

        [SerializeField] GameObject mainCamera = null;

        List<Light> dirLight = new List<Light>();
        MapDataManger mapDataManger;
        CreatRandMap creatRandMap;


        private void Awake()
        {
            //獲取地圖資料的單例實例
            mapDataManger = MapDataManger.Instance;
            creatRandMap = CreatRandMap.Instance;

            //(防呆)檢查sceneSettings是否清空
            sceneSettings = null;
        }

        /// <summary>
        /// 防呆檢查。是否套用設定值/刪除重複物件
        /// </summary>

        public void FollProofCheck(string sceneSettingName)
        {
            //1. 放入應該要有的設定檔
            sceneSettings = mapDataManger.GetMapSettign(sceneSettingName);
            if (sceneSettings == null)
            {
                //放入指定設定檔
                Debug.Log($"<color=yellow> Prefab.{name} 欄位沒有放 Map(SceneSetting)設定檔!</color>");
                return;
            }

            //2. 判斷設定檔內的物件是否有放置，沒有放置就報log
            if (sceneSettings.skyBox == null)
            {
                Debug.Log($"<color=yellow> {sceneSettings.name}(SceneSetting).skyBox欄位為null</color>");
            }

            //3. 自動抓取場上tag是mainCamera的物件
            if (mainCamera == null)
            {
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            //4. 判斷Fog是否打開
            if (RenderSettings.fog == false)
            {
                RenderSettings.fog = true;
            }


            //5. 判斷是否場景有預設天光

            dirLight.AddRange(FindObjectsOfType<Light>());

            foreach (Light light in dirLight)
            {
                if (light.type == LightType.Directional)
                {
                    // 刪除天光
                    Destroy(light.gameObject);
                    //Debug.Log($"刪除天光 {light.gameObject.name}");
                }
            }

            if (dirLight.Count == 0)
            {
                //Debug.Log("沒有要刪除的光");
            }



            // 在 mainCamera 下遞迴查找帶有"BG"標籤的子物件
            Transform childTransform = FindChildWithTag(mainCamera.transform, "BG");

            if (childTransform != null)
            {
                Debug.Log("找到符合標籤的子物件：" + childTransform.name + "並刪除");
                // 销毁子物体
                Destroy(childTransform.gameObject);
            }
            else
            {
                Debug.Log("找不到符合BG標籤的子物件");
            }
        }

        // 遞迴查找帶有"BG"標籤的子物件
        Transform FindChildWithTag(Transform parent, string tag)
        {
            foreach (Transform child in parent)
            {
                if (child.CompareTag(tag))
                {
                    return child;
                }

                // 遞迴查找子物件的子物件
                Transform foundChild = FindChildWithTag(child, tag);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }
            return null; // 如果找不到符合標籤的子物件

        }

        /// <summary>
        /// 套用設定檔參數
        /// </summary>
        public void ApplySettings()
        {




            //1. 套用Skybox材質球
            RenderSettings.skybox = sceneSettings.skyBox;

            //2. 套用Fog
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = sceneSettings.fogColor;
            RenderSettings.fogStartDistance = sceneSettings.fogDensityStart;
            RenderSettings.fogEndDistance = sceneSettings.fogDensityEnd;


            //4. 生成環境光物件在場上
            Instantiate(sceneSettings.lightGroup_Prefab);


            //5. 生成環境設定(含遠景BG圖)Prefabe在MainCamera底下
            Instantiate(sceneSettings.BG_Prefab, mainCamera.transform);


            //重新渲染場中所有物體
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            DynamicGI.UpdateEnvironment();


        }


        #region 地圖套用按鈕

        string mapDataName = "";

        [InspectorButton("草原")]
        public void _SetGrassLandMap()
        {
            string mapDataName = "GrasslandMap";
            //1.清除場景物件
            creatRandMap.clearObject();

            //2. 放上要的場景物件
            creatRandMap.GenerateObjects(mapDataName + "_MapData");

            //4. (呈上)生成燈光及套用場景設定等...
            FollProofCheck(mapDataName + "_SceneSettings");

            //3. 檢查腳本是否有多餘/少放的物件
            ApplySettings();



        }
        [InspectorButton("哥布林")]
        public void _SetGoblinMap()
        {
            string mapDataName = "GoblinMap";


            //1.清除場景物件
            creatRandMap.clearObject();

            //2. 放上要的場景物件
            creatRandMap.GenerateObjects(mapDataName + "_MapData");

            //4. (呈上)生成燈光及套用場景設定等...
            FollProofCheck(mapDataName + "_SceneSettings");

            //3. 檢查腳本是否有多餘/少放的物件
            ApplySettings();


        }


        [InspectorButton("火山")]
        public void _SetVolcanoMap()
        {
            string mapDataName = "VolcanoMap";


            //1.清除場景物件
            creatRandMap.clearObject();

            //2. 放上要的場景物件
            creatRandMap.GenerateObjects(mapDataName + "_MapData");

            //4. (呈上)生成燈光及套用場景設定等...
            FollProofCheck(mapDataName + "_SceneSettings");

            //3. 檢查腳本是否有多餘/少放的物件
            ApplySettings();


        }


        [InspectorButton("神廟")]
        public void _SetTempleMap()
        {
            string mapDataName = "TempleMap";


            //1.清除場景物件
            creatRandMap.clearObject();

            //2. 放上要的場景物件
            creatRandMap.GenerateObjects(mapDataName + "_MapData");

            //4. (呈上)生成燈光及套用場景設定等...
            FollProofCheck(mapDataName + "_SceneSettings");

            //3. 檢查腳本是否有多餘/少放的物件
            ApplySettings();


        }

        #endregion







    }


}



//////Log//////
///20230614 新增 天光曝光與顏色
///20230615 新增 天光旋轉值
///20230824 更新成正式版本，燈光物件等全部包成Prefab執行