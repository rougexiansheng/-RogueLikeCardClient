using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MAY
{
    /// <summary>
    /// 管理場景名稱與設定的對應資料
    /// </summary>
    
    public class SceneManger : MonoBehaviour
    {
        [SerializeField] SceneSettings[] sceneSettings;
        public Dictionary<string, SceneSettings> sceneDictionary = new Dictionary<string, SceneSettings>();

        private void Start()
        {
            // 初始化字典，將場景Key跟對應的SceneSettings
            sceneDictionary.Add("Map01", sceneSettings[0]);
            sceneDictionary.Add("Map02", sceneSettings[1]);
            sceneDictionary.Add("Map03", sceneSettings[2]);
            sceneDictionary.Add("Map04", sceneSettings[3]);
            //可以根據以上規則繼續新增

         
        }


        public void UseSceneBasedOnCondition(string condition)
        {
            if (sceneDictionary.TryGetValue(condition, out SceneSettings scene))
            {
                Debug.Log("Using " + condition );

            }
        }

    }
}

