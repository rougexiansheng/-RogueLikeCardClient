using Crystal;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    [Header("SafeArea")]
    [SerializeField]
    private bool m_enableSafeArea;

    private SafeArea safeArea;

    /// <summary>
    /// 請勿直接使用
    /// UI開啟時會自動執行
    /// </summary>
    /// <returns></returns>
    public virtual UniTask OnOpen()
    {
        if (m_enableSafeArea)
        {
            // 獲取SafeArea腳本
            safeArea = GetComponent<SafeArea>();
            if (safeArea == null)
                safeArea = gameObject.AddComponent<SafeArea>();

            // 設定SafeArea給擴展背景圖
            var extendImages = GetComponentsInChildren<ExtendImage>(true);
            if (extendImages != null)
                foreach (var extendImage in extendImages)
                {
                    extendImage.SetSafeArea(safeArea);
                }

            // 打開時刷新一次SafeArea
            safeArea.Refresh();
        }
        return default;
    }
    /// <summary>
    /// 請勿直接使用
    /// UI關閉時會自動執行
    /// </summary>
    public virtual void OnClose()
    {

    }
}
