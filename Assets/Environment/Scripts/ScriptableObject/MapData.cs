using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAY
{
    /// <summary>
    /// 生成的關卡物件清單
    /// </summary>
    /// 
    [CreateAssetMenu(fileName = "MapData", menuName = "Environment/MapData")]
    public class MapData : ScriptableObject
    {
        [Header("關卡編號")]
        public int mapNumber = 0;
        [Header("關卡代號")]
        public string mapCode ="Map0";

        [Header("小關卡")]
        public List<GameObject> normalMap = new List<GameObject>();

        [Header("Boss")]
        public GameObject Boss;

    }

}
