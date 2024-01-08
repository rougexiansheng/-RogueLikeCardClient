using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MAY
{
    /// <summary>
    /// 掛在mainCamera上
    /// 根據攝影機距離選擇要顯示/不顯示的物件
    /// </summary>
    public class CameraDisplayDistance : MonoBehaviour
    {

        public Camera mainCamera;
        static public float maxDisplayDistance = 120f;

        private void Update()
        {
            //找到場景中所有有Renderer 元件的object
            foreach (Renderer renderer in FindObjectsOfType<Renderer>())
            {
                // 計算該物件與攝影機的距離
                float distanceToCamera = Vector3.Distance(mainCamera.transform.position, renderer.transform.position);


                // 根據距離啟用或禁用渲染
                if (renderer.CompareTag("BG"))
                {
                    //背景永遠不禁用
                    renderer.enabled = true;
                }
                else if (renderer.CompareTag("Floor"))
                {
                     renderer.enabled = distanceToCamera <= maxDisplayDistance*2;
                }
                else
                {

                    renderer.enabled = distanceToCamera <= maxDisplayDistance;
                }

            }
        }

    }
}