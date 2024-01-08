using SDKProtocol;
using System.Collections.Generic;

/// <summary>
/// 這邊單純是玩家資料, 不儲存貨幣, 貨幣要到ItemContainer獲取
/// </summary>
public class NetworkSaveBattleDungeonData : NetworkSaveDataBase
{
    /// <summary>
    /// 
    /// </summary>
    public int dungeonGroupId;

    /// <summary>
    /// 
    /// </summary>
    public int dungeonDataIndex;

    /// <summary>
    /// 玩家現在的位置
    /// </summary>
    public int fightDungeonId;

    /// <summary>
    /// 
    /// </summary>
    public List<List<DungeonLevelData>> lastCache = new List<List<DungeonLevelData>>();

    /// <summary>
    /// 
    /// </summary>
    public bool isDone = true;

    /// <summary>
    /// 
    /// </summary>
    public bool isWin;

    /// <summary>
    /// 
    /// </summary>
    public ActorProfessionEnum professionEnum = ActorProfessionEnum.Witch;
}