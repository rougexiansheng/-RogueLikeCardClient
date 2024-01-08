using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniRx;
using UnityEngine.InputSystem;
using Zenject;
using Debug = UnityEngine.Debug;

public class NetworkSaveBattleItemContainer : NetworkSaveContainerBase
{
    public const int COIN = 1;
    public const int SKILL_FRAGMENT = 2; 

    [Inject]
    DataTableManager m_dataTableManager;

    // 儲存客戶端暫存的伺服器資料, 根據查找方便的方式撰寫Dictionary
    private Dictionary<int, NetworkSaveBattleItemData> m_datas = new Dictionary<int, NetworkSaveBattleItemData>();
    private Dictionary<ItemTpyeEnum, List<NetworkSaveBattleItemData>> m_datasByItemType = new Dictionary<ItemTpyeEnum, List<NetworkSaveBattleItemData>>();

    // 通知外部客戶端有道具更新的事件
    public Action<int, int> onItemUpdate;

    // 通知外部客戶端有道具移除的事件
    public Action<int> onItemRemove;

    public override void OnInit(INetworkSaveData data)
    {
        var key = data.GetKey().ToInt();

        m_datas.Clear();
        m_datas.Add(key, data as NetworkSaveBattleItemData);

        m_datasByItemType.Clear();
        var table = m_dataTableManager.GetItemDataDefine(key);
        if (table == null)
        {
            Debug.LogError($"NetworkSaveItemContainer can't found key '{key}' in item table.");
            return;
        }
        m_datasByItemType.Add(table.itemType, new List<NetworkSaveBattleItemData>()
        {
            data as NetworkSaveBattleItemData
        });
    }

    public override void OnInit(List<INetworkSaveData> datas)
    {
        m_datas.Clear();
        m_datasByItemType.Clear();
        foreach (var data in datas)
        {
            var key = data.GetKey().ToInt();
            m_datas.Add(key, data as NetworkSaveBattleItemData);
            var table = m_dataTableManager.GetItemDataDefine(key);
            if (table == null)
            {
                Debug.LogError($"NetworkSaveItemContainer can't found key '{key}' in item table.");
                continue;
            }
            if (m_datasByItemType.ContainsKey(table.itemType))
            {
                m_datasByItemType[table.itemType].Add(data as NetworkSaveBattleItemData);
            }
            else
            {
                var list = new List<NetworkSaveBattleItemData>();
                list.Add(data as NetworkSaveBattleItemData);
                m_datasByItemType.Add(table.itemType, list);
            }
        }
    }

    public override void OnUpdate(INetworkSaveData data)
    {
        var key = data.GetKey().ToInt();
        var value = data as NetworkSaveBattleItemData;
        if (m_datas.ContainsKey(key))
        {
            m_datas[key] = value;
        }
        else
        {
            m_datas.Add(key, value);
        }

        onItemUpdate?.Invoke(key, value.count);
    }

    public override void OnUpdate(List<INetworkSaveData> datas)
    {
        foreach (var data in datas)
        {
            OnUpdate(data);
        }
    }

    public override void OnRemove(string id)
    {
        var key = id.ToInt();
        if (!m_datas.ContainsKey(key))
        {
            Debug.LogError($"NetworkSaveItemContainer can't found key '{key}' in item table.");
            return;
        }
        m_datas.Remove(key);

        onItemRemove?.Invoke(key);
    }

    public override void OnRemove(List<string> ids)
    {
        foreach (var id in ids)
        {
            OnRemove(id);
        }
    }

    /// <summary>
    /// 獲取此容器全部存檔資料
    /// </summary>
    public override List<INetworkSaveData> GetDatas()
    {
        var datas = new List<INetworkSaveData>();
        foreach (var item in m_datas)
        {
            datas.Add(item.Value);
        }
        return datas;
    }

    /// <summary>
    /// 透過ID獲取單筆道具
    /// </summary>
    public NetworkSaveBattleItemData GetData(int id)
    {
        if (!m_datas.ContainsKey(id))
        {
            return null;
        }
        return m_datas[id];
    }

    /// <summary>
    /// 透過道具類型獲取道具清單
    /// </summary>
    public List<NetworkSaveBattleItemData> GetDatas(ItemTpyeEnum type)
    {
        if (!m_datasByItemType.ContainsKey(type))
        {
            return null;
        }
        return m_datasByItemType[type];
    }

    /// <summary>
    /// 透過ID獲取道具清單
    /// </summary>
    public List<NetworkSaveBattleItemData> GetDatas(List<int> ids)
    {
        var result = new List<NetworkSaveBattleItemData>();
        foreach (var id in ids)
        {
            if (m_datas.ContainsKey(id))
            {
                result.Add(m_datas[id]);
            }
        }
        return result;
    }

    /// <summary>
    /// 透過ID獲取單筆道具
    /// </summary>
    public int GetCount(int id)
    {
        if (!m_datas.ContainsKey(id))
        {
            return 0;
        }
        return m_datas[id].count;
    }

    // ... 其他獲取存檔資料方式請自行擴充
}
