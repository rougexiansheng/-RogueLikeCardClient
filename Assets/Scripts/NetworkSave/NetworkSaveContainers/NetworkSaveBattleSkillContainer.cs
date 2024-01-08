using System;
using System.Collections.Generic;
using Zenject;
using System.Linq;

public class NetworkSaveBattleSkillContainer : NetworkSaveContainerBase
{
    // 儲存客戶端暫存的伺服器資料, 根據查找方便的方式撰寫Dictionary (原始index : 資料內容)
    private Dictionary<int, NetworkSaveBattleSkillData> m_datas = new Dictionary<int, NetworkSaveBattleSkillData>();


    public override void OnInit(INetworkSaveData data)
    {
        m_datas.Clear();
        m_datas.Add(data.GetKey().ToInt(), data as NetworkSaveBattleSkillData);
    }

    public override void OnInit(List<INetworkSaveData> datas)
    {
        m_datas.Clear();
        foreach (var data in datas)
        {
            m_datas.Add(data.GetKey().ToInt(), data as NetworkSaveBattleSkillData);
        }
    }

    public override void OnUpdate(INetworkSaveData data)
    {
        var key = data.GetKey().ToInt();
        if (m_datas.ContainsKey(key))
        {
            m_datas[key] = data as NetworkSaveBattleSkillData;
        }
        else
        {
            m_datas.Add(key, data as NetworkSaveBattleSkillData);
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
        var key = id.ToInt();
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
    /// 透過原始索引獲取單筆英雄資料
    /// </summary>
    public NetworkSaveBattleSkillData GetData(int originIndex)
    {
        if (!m_datas.ContainsKey(originIndex))
        {
            return null;
        }
        return m_datas[originIndex];
    }

    // ... 其他獲取存檔資料方式請自行擴充

    public List<ActorSkill> GetSortedActorSkillList()
    {
        var list = new ActorSkill[m_datas.Count];
        // 對 Dictionary 的鍵進行排序並提取值
        foreach (var v in m_datas.Values)
        {
            list[v.currentIdx] = new ActorSkill()
            {
                skillId = v.skillId,
                isUsed = false,
                originIndex = v.index,
            };
        }
        return list.ToList();
    }
}
