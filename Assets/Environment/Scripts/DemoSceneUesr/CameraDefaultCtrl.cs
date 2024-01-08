using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MAY
{

    /// <summary>
    /// 攝影機控制設定
    /// </summary>
    public class CameraDefaultCtrl : MonoBehaviour
    {
        [Header("要控制的攝影機")]
        [SerializeField] Camera mainCamera;


        [Header("攝影機設定參數")]
        [SerializeField] Vector3 pos = new Vector3(40f, 6f, 0f);
        [SerializeField] Vector3 rot = new Vector3(30, 0, 0);

        [Header("攝影機可視距離")]
        [SerializeField] float FOV = 75f;
        [SerializeField] float far = 250f;

        [Header("攝影機濾鏡功能打開")]
        [SerializeField] bool postProcessing = true;

        void Awake()
        {
            //查找攝影機
            mainCamera = Camera.main;

            //攝影機初始位置
            mainCamera.transform.position = pos;
            //攝影機初始旋轉
            mainCamera.transform.eulerAngles = rot;

            //攝影機可視距離
            mainCamera.fieldOfView = FOV;
            mainCamera.farClipPlane = far;

            //攝影機濾鏡開關
            mainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = postProcessing;


        }


        void Update()
        {

        }
    }

}
