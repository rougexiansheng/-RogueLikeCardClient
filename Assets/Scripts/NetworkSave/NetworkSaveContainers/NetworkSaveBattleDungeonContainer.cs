using SDKProtocol;
using System;
using System.Collections.Generic;

public class NetworkSaveBattleDungeonContainer : NetworkSaveContainerBase
{
    private NetworkSaveBattleDungeonData m_data;

    /// <summary>
    /// 
    /// </summary>
    public int DungeonGroupId { get { return (m_data != null) ? m_data.dungeonGroupId : 0; } }

    /// <summary>
    /// 
    /// </summary>
    public int DungeonDataIndex { get { return (m_data != null) ? m_data.dungeonDataIndex : 0; } }

    /// <summary>
    /// 玩家現在的位置
    /// </summary>
    public int FightDungeonId { get { return (m_data != null) ? m_data.fightDungeonId : 0; } }

    /// <summary>
    /// 
    /// </summary>
    public List<List<DungeonLevelData>> LastCache { get { return (m_data != null) ? m_data.lastCache : null; } }

    /// <summary>
    /// 
    /// </summary>
    public bool IsDone { get { return (m_data != null) ? m_data.isDone : false; } }

    /// <summary>
    /// 
    /// </summary>
    public bool IsWin { get { return (m_data != null) ? m_data.isWin : false; } }

    /// <summary>
    /// 使用中的職業
    /// </summary>
    public ActorProfessionEnum SelectProfession { get { return (m_data != null) ? m_data.professionEnum : ActorProfessionEnum.None; } }

    public override void OnInit(INetworkSaveData data)
    {
        if (data == null) data = new NetworkSaveBattleDungeonData();

        m_data = data as NetworkSaveBattleDungeonData;
    }

    public override void OnInit(List<INetworkSaveData> datas)
    {
        throw new System.Exception("地城緩存資料沒有複數筆");
    }

    public override void OnUpdate(INetworkSaveData data)
    {
        if (data == null) return;

        m_data = data as NetworkSaveBattleDungeonData;
    }

    public override void OnUpdate(List<INetworkSaveData> datas)
    {
        throw new System.Exception("地城緩存資料沒有複數筆");
    }

    public override void OnRemove(string id)
    {
        throw new System.Exception("地城緩存資料不可移除");
    }

    public override void OnRemove(List<string> ids)
    {
        throw new System.Exception("地城緩存資料沒有複數筆");
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
}
