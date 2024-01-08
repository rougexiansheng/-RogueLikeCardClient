using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// bundle物件管理下載
/// GameObject物件池管理
/// 音樂播放
/// </summary>
public class AssetManager : MonoBehaviour
{
    public enum AudioMixerVolumeEnum
    {
        /// <summary>背景音樂</summary>
        BGM,
        /// <summary>特效音樂</summary>
        SoundEffect,
        /// <summary>角色語音</summary>
        Speak,
        /// <summary>主音量 無法撥放音樂</summary>
        Master
    }
    public enum LocalBGMEnum
    {
        Lobby,
    }
    [SerializeField]
    AudioMixer audioMixer;
    [SerializeField]
    List<AudioSource> audioSources;
    Dictionary<string, DefaultPoolsData> poolObjDatas = new Dictionary<string, DefaultPoolsData>();
    void Awake()
    {
        // 設置 addressable system download 加入 時間戳記
        Addressables.InternalIdTransformFunc = (url) =>
        {
            if (url.InternalId.StartsWith("http"))
            {
                return url.InternalId + "?t=" + DateTime.Now.Ticks;
            }
            return url.InternalId;
        };
    }

    /// <summary>
    /// 釋放資源
    /// </summary>
    /// <param name="obj"></param>
    public void Release(object obj)
    {
        Addressables.Release(obj);
    }

    /// <summary>
    /// 設置音量
    /// </summary>
    /// <param name="volumeEnum"></param>
    /// <param name="v"></param>
    public void SetMasterVolume(AudioMixerVolumeEnum volumeEnum, float v)
    {
        audioMixer.SetFloat(volumeEnum.ToString(), v);
    }
    public void PlayerAudio(AudioMixerVolumeEnum volumeEnum, AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogError("Player Null AudioClip");
            return;
        }
        var audioSource = audioSources[(int)volumeEnum];
        switch (volumeEnum)
        {
            case AudioMixerVolumeEnum.BGM:
                audioSource.clip = audioClip;
                audioSource.loop = true;
                audioSource.Play();
                break;
            case AudioMixerVolumeEnum.SoundEffect:
                audioSource.loop = false;
                audioSource.PlayOneShot(audioClip);
                break;
            case AudioMixerVolumeEnum.Speak:
                audioSource.Stop();
                audioSource.clip = audioClip;
                audioSource.loop = false;
                audioSource.Play();
                break;
            case AudioMixerVolumeEnum.Master:
            default:
                break;
        }
    }

    /// <summary>
    /// 更新catalog
    /// 15s超時
    /// </summary>
    /// <returns></returns>
    public async UniTask UpdateCatalogs()
    {
        try
        {
            await Addressables.InitializeAsync().ToUniTask();

        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                Debug.LogError("InitializeAsync Timeout 資源包可能不法正常更新");
            }
            else
            {
                Debug.LogErrorFormat("Message : {0} \n Stack : {1}", ex.Message, ex.StackTrace);
            }
        }
    }

    /// <summary>
    /// 下載所有檔案
    /// </summary>
    /// <param name="loading"></param>
    /// <returns></returns>
    public async UniTask DownloadAllBoundle(Action<float, string> loading)
    {
        var resourceLocator = await Addressables.InitializeAsync().ToUniTask();
        // 更新catalog
        var catalogsHandler = Addressables.CheckForCatalogUpdates(false);
        await catalogsHandler.ToUniTask();
        var catalogs = catalogsHandler.Result;
        Addressables.Release(catalogsHandler);
        if (catalogs.Count > 0)
        {
            await Addressables.UpdateCatalogs(catalogs).ToUniTask();
        }
        // 開始下載資源包
        var keys = resourceLocator.Keys.ToList();
        long totalDownloadSizeKb = 0;
        for (int i = 0; i < keys.Count; i++)
        {
            totalDownloadSizeKb += (await Addressables.GetDownloadSizeAsync(keys[i])) / 1000;
        }
        var downloadHandle = Addressables.DownloadDependenciesAsync(keys);
        while (!downloadHandle.IsDone)
        {
            loading(downloadHandle.PercentComplete, $"{downloadHandle.PercentComplete * totalDownloadSizeKb} / {totalDownloadSizeKb}");
            await UniTask.Delay(200);
            if (downloadHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.Log("下載中斷");
                Addressables.Release(downloadHandle);
                throw downloadHandle.OperationException;
            }
        }
        loading(1, $"{totalDownloadSizeKb} / {totalDownloadSizeKb}");
        Addressables.Release(downloadHandle);
        Debug.Log(" 下載完成");
    }

    /// <summary>
    /// 非同步載入Aseet
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async UniTask<T> AcyncLoadAsset<T>(string name)
    {
        try
        {
            var assetHandle = Addressables.LoadAssetAsync<T>(name);
            await assetHandle.ToUniTask();
            if (assetHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return assetHandle.Result;
            }
            else
            {
                Debug.LogError(assetHandle.OperationException.Message);
                Debug.LogError(assetHandle.OperationException.StackTrace);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogError(e.StackTrace);
        }
        return default(T);
    }

    public async UniTask LoadAndSetInObjectPool<T>(int count = 20) where T : Component
    {
        var path = $"Assets/DynamicAssets/UI/Item/{typeof(T).ToString()}.prefab";
        var obj = await AcyncLoadAsset<GameObject>(path);
        var cmpt = obj.GetComponent<T>();
        SetDefaultObject(cmpt, null, count);
    }

    public bool IsExist(string objName)
    {
        return poolObjDatas.ContainsKey(objName);
    }

    public void ClearObjectPool<T>() where T : Component
    {
        var objName = typeof(T).ToString();
        if (poolObjDatas.ContainsKey(objName))
        {
            Debug.LogWarning(objName + " : not SetDefaultObject");
            return;
        }
        var poolData = poolObjDatas[objName];
        poolData.Release();
        poolObjDatas.Remove(objName);
    }

    public void SetDefaultObject<T>(T defaultObj, string objName = null, int defaultCount = 20) where T : Component
    {
        if (string.IsNullOrEmpty(objName)) objName = typeof(T).ToString();
        if (poolObjDatas.ContainsKey(objName))
        {
            Debug.LogWarning(objName + " : already SetDefaultObject");
            return;
        }
        var data = new DefaultPoolsData();
        var poolObj = new GameObject(objName);
        poolObj.SetActive(false);
        poolObj.transform.SetParent(transform);
        data.poolObj = poolObj;
        data.defaultObj = GameObject.Instantiate(defaultObj.gameObject, poolObj.transform);
        data.defaultObj.name = defaultObj.gameObject.name;
        data.AddObj(defaultCount);
        poolObjDatas.Add(objName, data);
    }

    public T GetObject<T>(string objName = null) where T : Component
    {
        if (string.IsNullOrEmpty(objName)) objName = typeof(T).ToString();
        if (poolObjDatas.TryGetValue(objName, out DefaultPoolsData data))
        {
            if (data.queue.Count == 0)
            {
                data.AddObj(20);
            }
            var obj = data.queue.Dequeue();
            return obj.GetComponent<T>();
        }
        Debug.LogWarning(objName + " : Not Set DefaultObj");
        return default;
    }

    public void ReturnObjToPool(GameObject obj)
    {
        if (poolObjDatas.TryGetValue(obj.name, out DefaultPoolsData data))
        {
            obj.SetActive(true);
            obj.transform.SetParent(data.poolObj.transform, false);
            obj.transform.localScale = Vector3.one;
            data.queue.Enqueue(obj);
        }
        else
        {
            Debug.LogError(obj.name + " not have pool");
        }
    }

    class DefaultPoolsData
    {
        public GameObject poolObj;
        public GameObject defaultObj;
        public Queue<GameObject> queue = new Queue<GameObject>();

        public void AddObj(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(defaultObj, poolObj.transform);
                obj.name = defaultObj.name;
                queue.Enqueue(obj);
            }
        }

        public void Release()
        {
            GameObject.Destroy(poolObj);
        }
    }
}
