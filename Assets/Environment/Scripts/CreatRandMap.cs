using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAY
{
    /// <summary>
    /// 隨機生成地圖10個
    /// </summary>
    public class CreatRandMap : MonoBehaviour
    {
        static CreatRandMap instance;

        public static CreatRandMap Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CreatRandMap>();

                    if (instance == null)
                    {
                        GameObject managerGo = new GameObject("CreatRandMap");
                        instance = managerGo.AddComponent<CreatRandMap>();
                    }
                }
                return instance;
            }
        }

        [Header("mapDataManger")]
        [SerializeField] MapDataManger mapDataManger;

        [Header("生成次數")]
        [SerializeField] int number = 10;
        [Header("地圖間距")]
        [SerializeField] float spacingZ = 50f;
        [Header("Boss在第幾關")]
        [SerializeField] int bossNumber = 10;


        void Start()
        {
            //獲取地圖資料的單例實例
            mapDataManger = MapDataManger.Instance;
        }


        /// <summary>
        /// 生成地圖Data
        /// </summary>
        /// <param name="name"></param>
        public void GenerateObjects(string name)
        {
            MapData mapData = mapDataManger.GetMapData(name);

      
            //1. 檢查mapData是否為空，不是則執行以下內容
            if (mapData != null)
            {

                for (int i = 0; i < number; i++)
                {
                    GameObject newObject = null;//初始化newObject


                    if (i == (bossNumber - 1) && mapData.Boss != null)
                    {
                        // 如果是Boss關卡(通常為最後一關)，且bossMap資料不為空，則生成Boss關卡
                        newObject = Instantiate(mapDataManger.GetMapData(name).Boss, gameObject.transform);

                    }
                    else if (mapData.normalMap != null && mapData.normalMap.Count > 0)
                    {
                        // 生成普通關卡，且普通關卡資料不為空，則生成
                        int r = Random.Range(0, mapDataManger.GetMapData(name).normalMap.Count);
                        newObject = Instantiate(mapDataManger.GetMapData(name).normalMap[r], gameObject.transform);
                    }
                    else
                    {
                        Debug.LogWarning("所指定的MapData資料missing 或 empty");
                    }

                    if (newObject != null)
                    {
                        // 設定物件位置
                        newObject.transform.position = new Vector3(0f, 0f, i * spacingZ);
                    }

                }
            }
            else
            {
                Debug.LogWarning("MapData資料missing或名稱不一致");

            }
        }














        /// <summary>
        /// 刪除物件
        /// </summary>
        public void clearObject()
        {
            //檢查是否有子物件，如果有則刪除
            if (gameObject.transform.childCount > 0)
            {
                foreach (Transform child in gameObject.transform)
                {
                    Destroy(child.gameObject);//刪除子物件
                }
            }
            else
            {
                //沒有則什麼也不做

            }
        }


    }
}