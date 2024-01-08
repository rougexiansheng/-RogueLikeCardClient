using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAY
{
    /// <summary>
    /// 紀錄MapData資料的位置
    /// </summary>

    public class MapDataManger : MonoBehaviour
    {
        public List<MapData> mapDatas = new List<MapData>();
        public List<SceneSettings> sceneSettings = new List<SceneSettings>();

        static MapDataManger instance;

        public static MapDataManger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MapDataManger>();

                    if (instance == null)
                    {
                        GameObject managerGO = new GameObject("MapDataManger");
                        instance = managerGO.AddComponent<MapDataManger>();
                    }

                }
                return instance;
            }
            set { }
        }

        /// <summary>
        /// 透過名稱或編號識別獲取特定的地圖Data(ScriptableObject)
        /// </summary>
        /// <param name="Level"></param>
        /// <returns></returns>
        public MapData GetMapData(string Level)
        {
            // 檢查傳入的 Level 是否是數字格式
            if (int.TryParse(Level, out int intIdentifier))
            {
                // 如果為整數，根據整數查找對應的 MapData
                return mapDatas.Find(obj => obj.mapNumber == intIdentifier);
            }
            else
            {
                // 否則，當作字符串處理，根據名稱查找對應的 MapData
                return mapDatas.Find(obj => obj.name == Level);

            }

        }

        public SceneSettings GetMapSettign(string MapSettingName)
        {
            // 檢查傳入的 MapSettingName 是否是數字格式
            if (int.TryParse(MapSettingName, out int intIdentifier))
            {
                // 如果為整數，根據整數查找對應的 SceneSettings
                return sceneSettings.Find(obj => obj.Number == intIdentifier);
            }
            else
            {
                // 否則，當作字符串處理，根據名稱查找對應的 SceneSettings
                return sceneSettings.Find(obj => obj.name == MapSettingName);

            }

        }
    }
}


