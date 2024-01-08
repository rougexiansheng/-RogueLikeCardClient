using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Zenject;

public class UIManager : MonoBehaviour
{
    [Inject]
    AssetManager assetManager;
    [Inject]
    DiContainer diContainer;
    Dictionary<string, GameObject> prefabes = new Dictionary<string, GameObject>();
    [SerializeField]
    List<GameObject> OpenedUI = new List<GameObject>();
    [SerializeField]
    GameObject loadingUI;
    [SerializeField]
    Image fadeUI;
    string debugColorString = "<color=#006000>{0}</color>";
    [SerializeField]
    RectTransform overlayCanvas;

    void Awake()
    {
        if (loadingUI != null)
        {
            var loadImg = loadingUI.transform.GetChild(0);
            loadImg.DOLocalRotate(new Vector3(0, 0, -360), 1.5f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
        }
    }

    public void LoadingUI(bool isShow)
    {
        loadingUI.gameObject.SetActive(isShow);
        Debug.LogFormat(debugColorString, "LoadingUI : " + isShow);
    }

    async public UniTask FadeIn(float s)
    {
        fadeUI.gameObject.SetActive(true);
        fadeUI.color = Color.clear;
        await fadeUI.DOFade(1f, s).AsyncWaitForCompletion();
    }

    async public UniTask FadeOut(float s)
    {
        fadeUI.gameObject.SetActive(true);
        await fadeUI.DOFade(1f, 0).AsyncWaitForCompletion();
        await fadeUI.DOFade(0f, s).AsyncWaitForCompletion();
        fadeUI.gameObject.SetActive(false);
    }

    public T FindUI<T>() where T : UIBase
    {
        var uiName = typeof(T).ToString();
        var obj = OpenedUI.Find(o => o.name == uiName);
        T ui = null;
        if (!obj)
        {
            Debug.LogWarning("Find UI not Opened : " + uiName);
        }
        else
        {
            ui = obj.GetComponent<T>();
            if (!ui) Debug.LogWarning("Find UI GameObject But not Script: " + uiName);
        }
        return ui;
    }

    public async UniTask PreloadUI(params Type[] uiType)
    {
        var ls = new List<UniTask>();
        for (int i = 0; i < uiType.Length; i++)
        {
            Debug.LogFormat(debugColorString, "預載UI : " + uiType[i].Name);
            ls.Add(LoadUIPrefab(uiType[i].Name));
        }
        await UniTask.WhenAll(ls);
    }

    async UniTask<GameObject> LoadUIPrefab(string prefabName)
    {
        if (!prefabes.TryGetValue(prefabName, out GameObject prefab))
        {
            prefab = await assetManager.AcyncLoadAsset<GameObject>($"Dialogs/{prefabName}.prefab");
            prefabes.Add(prefabName, prefab);
        }
        return prefab;
    }

    public async UniTask<T> OpenUI<T>(bool showLoading = false) where T : UIBase
    {
        if (showLoading) LoadingUI(true);
        string prefabName = typeof(T).ToString();
        T ui = null;
        try
        {
            GameObject prefab = await LoadUIPrefab(prefabName);
            var obj = GameObject.Instantiate(prefab, overlayCanvas);
            if (obj)
            {
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                obj.name = prefabName;
                OpenedUI.Add(obj);
                ui = obj.GetComponent<T>();
            }
            else Debug.LogWarningFormat("UI 開啟失敗: {0} 不存在或 物件名稱與腳本名稱不同 obj is null", typeof(T).ToString());
            Debug.LogFormat(debugColorString, "開啟 UI : " + prefabName);
            if (ui != null)
            {
                diContainer.InjectGameObjectForComponent<T>(ui.gameObject);
                await ui.OnOpen();
            }
            else Debug.LogWarningFormat("UI: {0} 不存在 或 物件名稱與腳本名稱不同 obj name is: {1}", typeof(T).ToString(), obj.name);
            return ui;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            Debug.LogError(ex.StackTrace);
        }
        finally
        {
            if (showLoading) LoadingUI(false);
        }
        return null;
    }

    /// <summary>
    /// 移除UI
    /// </summary>
    /// <param name="ui"></param>
    public void RemoveUI(UIBase ui)
    {
        if (ui != null)
        {
            if (OpenedUI.Remove(ui.gameObject))
            {
                ui.OnClose();
                Destroy(ui.gameObject);
                Debug.LogFormat(debugColorString, "Close UI : " + ui.name);
            }
            else
            {
                Debug.LogFormat(debugColorString, "NotInOpenedUI : " + ui.name);
            }
        }
        else Debug.LogWarning("Remove UI is NUll");
    }

    /// <summary>
    /// 移除UI
    /// </summary>
    /// <param name="ui"></param>
    public void RemoveUI<T>() where T : UIBase
    {
        var ui = FindUI<T>();
        if (ui != null)
        {
            if (OpenedUI.Remove(ui.gameObject))
            {
                ui.OnClose();
                Destroy(ui.gameObject);
                Debug.LogFormat(debugColorString, "Close UI : " + ui.name);
            }
            else
            {
                Debug.LogFormat(debugColorString, "NotInOpenedUI : " + ui.name);
            }
        }
        else Debug.LogWarning("Remove UI is NUll");
    }

    /// <summary>
    /// 顯示通用訊息視窗
    /// </summary>
    /// <param name="message"></param>
    /// <param name="onConfirm"></param>
    /// <returns></returns>
    public async UniTask ShowOneBottonMessageBox(string message, Action onConfirm = null)
    {
        var oldTimeScale = Time.timeScale;
        Time.timeScale = 0;
        var renameUi = await OpenUI<UIMessageBox>();
        await renameUi.ShowOneBottonMessageBox(message,
        onConfirm: () =>
        {
            Time.timeScale = oldTimeScale;
            onConfirm?.Invoke();
        });
        await UniTask.WaitUntil(() => { return renameUi.IsDone; });
        RemoveUI(renameUi);
    }

    /// <summary>
    /// 顯示通用訊息視窗
    /// </summary>
    /// <param name="message"></param>
    /// <param name="onConfirm"></param>
    /// <param name="onCancel"></param>
    /// <returns></returns>
    public async UniTask ShowTwoBottonMessageBox(string message, Action onConfirm = null, Action onCancel = null)
    {
        var oldTimeScale = Time.timeScale;
        Time.timeScale = 0;
        var renameUi = await OpenUI<UIMessageBox>();
        await renameUi.ShowTwoBottonMessageBox(message,
        onConfirm: () =>
        {
            Time.timeScale = oldTimeScale;
            onConfirm?.Invoke();
        },
        onCancel: () =>
        {
            Time.timeScale = oldTimeScale;
            onCancel?.Invoke();
        });
        await UniTask.WaitUntil(() => { return renameUi.IsDone; });
        RemoveUI(renameUi);
    }
}