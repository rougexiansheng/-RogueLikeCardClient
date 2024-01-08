using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MAY
{
    /// <summary>
    /// Demo場景暫時使用的UI
    /// </summary>

    public class UICtrl : MonoBehaviour
    {
        [SerializeField] CreatRandMap creatRandMap;
        [SerializeField] ApplySceneSettings applySceneSettings;

        [SerializeField] Button bt_GoblinMap = null;
        [SerializeField] Button bt_GrasslandMap = null;


        void Start()
        {
            //單例實例化
            creatRandMap = CreatRandMap.Instance;//接收場景生成的
            applySceneSettings = ApplySceneSettings.Instance;//接收場景燈光/濾鏡生成的

        }

        private void Update()
        {
            //接收下一個地圖的名字

        }


        #region 場景button


     
        /// <summary>
        /// 哥不林
        /// </summary>
        /// <param name="mapDataName"></param>
        public void _SetMap(string mapDataName)
        {
            //1.清除場景物件
            creatRandMap.clearObject();

            //2. 放上要的場景物件
            creatRandMap.GenerateObjects(mapDataName + "_MapData");

            //4. (呈上)生成燈光及套用場景設定等...
            applySceneSettings.FollProofCheck(mapDataName + "_SceneSettings");

            //3. 檢查腳本是否有多餘/少放的物件
            applySceneSettings.ApplySettings();


        }




        #endregion




    }

}
