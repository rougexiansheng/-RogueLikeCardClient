using Cysharp.Threading.Tasks;
using SDKProtocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIRanking : UIBase, LoopScrollPrefabSource, LoopScrollDataSource
{
    [Inject]
    AssetManager assetManager;
    [Inject]
    DataTableManager dataTableManager;

    [SerializeField]
    private UIRankingItem m_rankingItemPrefab;
    [SerializeField]
    private LoopScrollRect m_loopScrollView;
    [SerializeField]
    private UIRankingItem m_playerRankingItem;
    [SerializeField]
    private Button m_buttonBack;

    public bool IsDone { get; private set; }

    private List<DungeonRankingDataItem> m_fullRankingDatas;
    private DungeonRankingDataItem m_playerRankingData;
    private int m_scrollRectCount;
    private Action m_onClose;

    public override UniTask OnOpen()
    {
        IsDone = false;

        m_buttonBack.onClick.AddListener(OnButtonBack);

        m_rankingItemPrefab.gameObject.SetActive(true);

        return base.OnOpen();
    }

    public override void OnClose()
    {
        m_buttonBack.onClick.RemoveListener(OnButtonBack);

        base.OnClose();
    }

    public async UniTask Init(List<DungeonRankingDataItem> fullRanking, DungeonRankingDataItem playerRanking, Action onClose = null)
    {
        await UniTask.DelayFrame(1);

#if UNITY_EDITOR
        Debug.Log($"FullRanking: {JsonUtility.ToJson(new Serialization<DungeonRankingDataItem>(fullRanking))}");
        Debug.Log($"PlayerRanking: {JsonUtility.ToJson(playerRanking)}");
#endif

        m_fullRankingDatas = fullRanking;
        m_playerRankingData = playerRanking;

        LoopScrollRectInit();
        SetPlayerRanking();

        m_onClose = onClose;
    }

    private void LoopScrollRectInit()
    {
        // 設定Prefab
        assetManager.SetDefaultObject(m_rankingItemPrefab);
        m_loopScrollView.prefabSource = this;
        m_loopScrollView.dataSource = this;

        // 設定長度
        m_scrollRectCount = m_fullRankingDatas.Count;
        m_loopScrollView.totalCount = m_scrollRectCount;

        // 刷新Cell
        m_loopScrollView.RefreshCells();

        // 關閉Prefab
        m_rankingItemPrefab.gameObject.SetActive(false);
    }

    private void SetPlayerRanking()
    {
        var table = dataTableManager.GetProfessionDataDefine(m_playerRankingData.profession);
        m_playerRankingData.professionName = table?.name;
        m_playerRankingItem.Init(m_playerRankingData);
    }

    private void OnButtonBack()
    {
        m_onClose?.Invoke();
        IsDone = true;
    }

    public GameObject GetObject(int index)
    {
        var item = assetManager.GetObject<UIRankingItem>();
        item.transform.localScale = Vector3.one;
        return item.gameObject;
    }

    public void ReturnObject(Transform trans)
    {
        assetManager.ReturnObjToPool(trans.gameObject);
    }

    public void ProvideData(Transform transform, int idx)
    {
        var item = transform.GetComponent<UIRankingItem>();
        var index = idx.TransformIndexNumber(m_scrollRectCount);
        var data = m_fullRankingDatas[index];
        var table = dataTableManager.GetProfessionDataDefine(data.profession);
        data.professionName = table?.name;
        item.Init(data);
    }
}
