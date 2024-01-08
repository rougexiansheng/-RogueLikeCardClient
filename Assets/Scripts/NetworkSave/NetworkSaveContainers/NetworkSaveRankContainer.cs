using System;
using System.Collections.Generic;
using Zenject;

public class NetworkSaveRankContainer : NetworkSaveContainerBase
{
    public enum RankType
    {
        Unknwon = 0,

        Dungeon = 1,
    }

    // 儲存客戶端暫存的伺服器資料, 根據查找方便的方式撰寫Dictionary
    private Dictionary<RankType, NetworkSaveRankData> m_datas = new Dictionary<RankType, NetworkSaveRankData>();


    public override void OnInit(INetworkSaveData data)
    {
        m_datas.Clear();
        m_datas.Add((RankType)data.GetKey().ToInt(), data as NetworkSaveRankData);
    }

    public override void OnInit(List<INetworkSaveData> datas)
    {
        m_datas.Clear();
        foreach (var data in datas)
        {
            m_datas.Add((RankType)data.GetKey().ToInt(), data as NetworkSaveRankData);
        }
    }

    public override void OnUpdate(INetworkSaveData data)
    {
        var key = (RankType)data.GetKey().ToInt();
        if (m_datas.ContainsKey(key))
        {
            m_datas[key] = data as NetworkSaveRankData;
        }
        else
        {
            m_datas.Add(key, data as NetworkSaveRankData);
        }
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
        var key = (RankType)id.ToInt();
        if (!m_datas.ContainsKey(key))
        {
            return;
        }
        m_datas.Remove(key);
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
    /// 透過UID獲取單筆英雄資料
    /// </summary>
    public NetworkSaveRankData GetData(RankType rankType)
    {
        if (!m_datas.ContainsKey(rankType))
        {
            return null;
        }
        return m_datas[rankType];
    }

    // ... 其他獲取存檔資料方式請自行擴充
}
