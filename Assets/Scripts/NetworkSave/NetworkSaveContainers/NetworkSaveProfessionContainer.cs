using System;
using System.Collections.Generic;
using Zenject;

public class NetworkSaveProfessionContainer : NetworkSaveContainerBase
{
    // 儲存客戶端暫存的伺服器資料, 根據查找方便的方式撰寫Dictionary
    private Dictionary<ActorProfessionEnum, NetworkSaveProfessionData> m_datas = new Dictionary<ActorProfessionEnum, NetworkSaveProfessionData>();


    public override void OnInit(INetworkSaveData data)
    {
        m_datas.Clear();
        m_datas.Add((ActorProfessionEnum)data.GetKey().ToInt(), data as NetworkSaveProfessionData);
    }

    public override void OnInit(List<INetworkSaveData> datas)
    {
        m_datas.Clear();
        foreach (var data in datas)
        {
            m_datas.Add((ActorProfessionEnum)data.GetKey().ToInt(), data as NetworkSaveProfessionData);
        }
    }

    public override void OnUpdate(INetworkSaveData data)
    {
        var key = (ActorProfessionEnum)data.GetKey().ToInt();
        if (m_datas.ContainsKey(key))
        {
            m_datas[key] = data as NetworkSaveProfessionData;
        }
        else
        {
            m_datas.Add(key, data as NetworkSaveProfessionData);
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
        var key = (ActorProfessionEnum)id.ToInt();
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
    public NetworkSaveProfessionData GetData(ActorProfessionEnum profession)
    {
        if (!m_datas.ContainsKey(profession))
        {
            return null;
        }
        return m_datas[profession];
    }

    // ... 其他獲取存檔資料方式請自行擴充

    public Dictionary<ActorProfessionEnum, SDKProtocol.PlayerProfessionData> GetSkillGroupsDic()
    {
        //Dictionary<ActorProfessionEnum, NetworkSaveProfessionData>
        Dictionary<ActorProfessionEnum, SDKProtocol.PlayerProfessionData> result = new Dictionary<ActorProfessionEnum, SDKProtocol.PlayerProfessionData>();

        foreach (var dic in m_datas)
        {
            result.Add(dic.Key, new SDKProtocol.PlayerProfessionData()
            {
                UnlockCharacterSkinIds = m_datas[dic.Key].unlockCharacterSkinIds,
                UnlockSkillIDs = m_datas[dic.Key].unlockSkillIDs,
                SkillGroups = m_datas[dic.Key].skillGroups,
                unlockPassiveGourpIds = m_datas[dic.Key].unlockPassiveGourpIds,
                DefalutProfessionDatas = m_datas[dic.Key].defalutProfessionDatas,
            });
        }

        return result;
    }
}
