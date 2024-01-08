using System;
using System.Collections.Generic;
using Zenject;

public class NetworkSaveBattleHeroAttrContainer : NetworkSaveContainerBase
{
    public enum AttrType
    {
        Unknwon = 0,


        /// <summary>
        /// 生命值，只會有一筆，ID為0
        /// </summary>
        Hp = 1,

        /// <summary>
        /// 被動能力，複數筆ID
        /// </summary>
        Passive = 2,

        /// <summary>
        /// 技能，複數筆ID
        /// </summary>
        Skill = 3,

        /// <summary>
        /// 已獲得商品(遺物、時裝，不能重複)，複數筆ID
        /// </summary>
        ItemGet = 4
    }

    // 儲存客戶端暫存的伺服器資料, 根據查找方便的方式撰寫Dictionary (屬性類型 : ( 屬性ID : 堆疊 ))
    private Dictionary<AttrType, Dictionary<int, NetworkSaveBattleHeroAttrData>> m_datas = new Dictionary<AttrType, Dictionary<int, NetworkSaveBattleHeroAttrData>>();


    public override void OnInit(INetworkSaveData data)
    {
        m_datas.Clear();
        var heroData = data as NetworkSaveBattleHeroAttrData;
        var key = heroData.type;
        var dict = new Dictionary<int, NetworkSaveBattleHeroAttrData>();
        dict.Add(heroData.id, heroData);
        m_datas.Add(key, dict);
    }

    public override void OnInit(List<INetworkSaveData> datas)
    {
        m_datas.Clear();
        foreach (var data in datas)
        {
            var heroData = data as NetworkSaveBattleHeroAttrData;
            var key = heroData.type;
            if (!m_datas.ContainsKey(key))
            {
                m_datas.Add(key, new Dictionary<int, NetworkSaveBattleHeroAttrData>());
            }
            if (!m_datas[key].ContainsKey(heroData.id))
            {
                m_datas[key].Add(heroData.id, heroData);
            }
            else
            {
                m_datas[key][heroData.id] = heroData;
            }
        }
    }

    public override void OnUpdate(INetworkSaveData data)
    {
        var heroData = data as NetworkSaveBattleHeroAttrData;
        var key = heroData.type;
        if (!m_datas.ContainsKey(key))
        {
            m_datas.Add(key, new Dictionary<int, NetworkSaveBattleHeroAttrData>());
        }
        if (!m_datas[key].ContainsKey(heroData.id))
        {
            m_datas[key].Add(heroData.id, heroData);
        }
        else
        {
            m_datas[key][heroData.id] = heroData;
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
        var split = id.Split("_"[0]);
        var key = (AttrType)split[0].ToInt();
        var attrId = split[1].ToInt();
        if (!m_datas.ContainsKey(key))
        {
            return;
        }
        if (!m_datas[key].ContainsKey(attrId))
        {
            return;
        }
        m_datas[key].Remove(attrId);
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
            var values = item.Value;
            foreach (var value in values)
            {
                datas.Add(value.Value);
            }
        }
        return datas;
    }

    /// <summary>
    /// 獲得當前HP
    /// </summary>
    public int GetCurrentHp()
    {
        var result = 0;
        if (!m_datas.ContainsKey(AttrType.Hp) || !m_datas[AttrType.Hp].ContainsKey(0))
        {
            return result;
        }
        return m_datas[AttrType.Hp][0].value;
    }

    /// <summary>
    /// 透過屬性獲得ID
    /// </summary>
    public List<int> GetAttrs(AttrType attrType)
    {
        var result = new List<int>();
        if (!m_datas.ContainsKey(attrType))
        {
            return result;
        }
        foreach (var data in m_datas[attrType])
        {
            result.Add(data.Value.id);
        }
        return result;
    }

    /// <summary>
    /// 查詢是否有某個屬性的ID
    /// </summary>
    public bool Exists(AttrType attrType, int id)
    {
        if (!m_datas.ContainsKey(attrType)) return false;
        return m_datas[attrType].ContainsKey(id);
    }

    // ... 其他獲取存檔資料方式請自行擴充
}
