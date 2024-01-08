using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer : IProtocolBridge, IInitializable
{
    private const string FAKE_SERVER_SAVE_KEY = "3f233a868deb64f3b26f8a03024a0693";
    private const int MAX_RANKING_SIZE = 100;
    private const string TAG = "<color=#9999ff>[FakeServer]</color>";
    private const string REQUEST_COLOR_FORMAT = "<color=#fff176>On{0}Request</color>";
    private const string RESPONSE_COLOR_FORMAT = "<color=#fdd835>On{0}Response</color>";

    [Inject]
    ItemManager itemManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    NetworkSaveManager saveManager;

    // 實際上伺服器的儲存資料
    private FakeServerData fakeServerData = new FakeServerData();

    public void Initialize()
    {
    }

    public ProtoReq<T> PackageReqest<T>(T data)
    {
        return default;
    }

    public T JsonClone<T>(T obj)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
    }

    /// <summary>
    /// 將假伺服器的存檔資料記錄到PlayerPrefs
    /// </summary>
    public void SaveFakeServerData()
    {
        var saveJson = JsonConvert.SerializeObject(fakeServerData);

        //Debug.Log($"{TAG} SaveFakeServerData: {saveJson}");

        PlayerPrefs.SetString(FAKE_SERVER_SAVE_KEY, saveJson);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 從PlayerPrefs讀取假伺服器資料
    /// </summary>
    public void LoadFakeServerData()
    {
        // 如果沒有存檔則創建需要儲存的資料結構
        if (!PlayerPrefs.HasKey(FAKE_SERVER_SAVE_KEY))
        {
            fakeServerData = JsonClone(new FakeServerData());
            var defaultJson = JsonConvert.SerializeObject(fakeServerData);
            PlayerPrefs.SetString(FAKE_SERVER_SAVE_KEY, defaultJson);
            PlayerPrefs.Save();

            Debug.Log($"{TAG} LoadFakeServerData: 建立新FakeServer存檔");
        }
        // 如果已經有存檔則跟PlayerPrefs獲取
        else
        {
            var saveJson = PlayerPrefs.GetString(FAKE_SERVER_SAVE_KEY);
            fakeServerData = JsonConvert.DeserializeObject<FakeServerData>(saveJson);
        }

        Debug.Log($"{TAG} LoadFakeServerData: {JsonConvert.SerializeObject(fakeServerData)}");
    }

    /// <summary>
    /// 刪除已存在的假伺服器資料
    /// </summary>
    public void DeleteFakeServerData()
    {
        if (!PlayerPrefs.HasKey(FAKE_SERVER_SAVE_KEY))
        {
            return;
        }

        PlayerPrefs.DeleteKey(FAKE_SERVER_SAVE_KEY);
        PlayerPrefs.Save();

        Debug.Log($"{TAG} DeleteFakeServerData: success!!");
    }

    private void BeginProtocol(params object[] value)
    {
#if UNITY_EDITOR
        var stackTrace = new StackTrace();
        var protocol = stackTrace.GetFrame(1).GetMethod().Name;
        Debug.Log($"{TAG} {string.Format(REQUEST_COLOR_FORMAT, protocol)}: {JsonConvert.SerializeObject(value)}");
#endif
    }

    /// <summary>
    /// 為了自動更新客戶端的DataManager裡面的PlayerData, 使用此方法替代UniTask.FromResult
    /// 之後alter資料可以拆成有需要的部分再更新，暫時Clone整份PlayerData
    /// </summary>
    private UniTask<T> EndProtocol<T>(T value, List<JsonObject> clientSaves = null)
    {
#if UNITY_EDITOR
        var stackTrace = new StackTrace();
        var protocol = stackTrace.GetFrame(1).GetMethod().Name;
        Debug.Log($"{TAG} {string.Format(RESPONSE_COLOR_FORMAT, protocol)}: {JsonConvert.SerializeObject(value)}");
#endif
        // 自動儲存當前FakeServer最新資料
        SaveFakeServerData();

        // 將伺服器資料Clone
        //var clonedData = JsonClone(fakeServerData.player);

        // 對NetworkSaveManager回傳存檔資料
        if (clientSaves != null)
        {
            foreach (var clientSave in clientSaves)
            {
                saveManager.OnServerResponse(clientSave);
            }
        }

        // 通用的UniTask回傳
        return new UniTask<T>(value);
    }

    [Serializable]
    private class FakeServerData
    {
        public PlayerServerData player = new PlayerServerData();
        public RankingServerData ranking = new RankingServerData();
    }
}
