using System;
using System.Collections.Generic;

public class NetworkSavePlayerContainer : NetworkSaveContainerBase
{
    private NetworkSavePlayerData m_data;

    /// <summary>
    /// 獲取玩家名稱
    /// </summary>
    public string Username { get { return m_data?.username; } }

    /// <summary>
    /// 獲取玩家唯一ID
    /// </summary>
    public string UID { get { return m_data?.uid; } }


    /// <summary>
    /// 已解鎖的角色ID
    /// </summary>
    public List<int> unlockProfessionIds { get { return m_data?.unlockProfessionIds; } }

    /// <summary>
    /// 已解鎖的時裝ID
    /// </summary>
    public List<int> unlockSkinIds { get { return m_data?.unlockSkinIds; } }

    public override void OnInit(INetworkSaveData data)
    {
        if (data == null) data = new NetworkSavePlayerData();

        m_data = data as NetworkSavePlayerData;
    }

    public override void OnInit(List<INetworkSaveData> datas)
    {
        throw new System.Exception("玩家資料沒有複數筆");
    }

    public override void OnUpdate(INetworkSaveData data)
    {
        if (data == null) return;

        m_data = data as NetworkSavePlayerData;
    }

    public override void OnUpdate(List<INetworkSaveData> datas)
    {
        throw new System.Exception("玩家資料沒有複數筆");
    }

    public override void OnRemove(string id)
    {
        throw new System.Exception("玩家資料不可移除");
    }

    public override void OnRemove(List<string> ids)
    {
        throw new System.Exception("玩家資料沒有複數筆");
    }

    /// <summary>
    /// 獲取此容器全部存檔資料
    /// </summary>
    public override List<INetworkSaveData> GetDatas()
    {
        var datas = new List<INetworkSaveData>()
        {
            m_data
        };
        return datas;
    }

    /// <summary>
    /// 查詢某角色是否已解鎖
    /// </summary>
    public bool IsCharacterUnlock(int characterId)
    {
        return unlockProfessionIds.Contains(characterId);
    }
}
