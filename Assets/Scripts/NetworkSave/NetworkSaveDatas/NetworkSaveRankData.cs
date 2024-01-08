using SDKProtocol;
using System.Collections.Generic;
using static NetworkSaveRankContainer;

/// <summary>
/// 這邊儲存玩家的排行榜
/// </summary>
public class NetworkSaveRankData : NetworkSaveDataBase
{
    /// <summary>
    /// 排行榜ID
    /// </summary>
    public RankType id;

    /// <summary>
    /// 整個排行榜資料
    /// </summary>
    public List<DungeonRankingDataItem> fullRankings;

    /// <summary>
    /// 最後排行榜
    /// </summary>
    public DungeonRankingDataItem lastRanking;

    /// <summary>
    /// 此資料的Key為ID
    /// </summary>
    public override string GetKey()
    {
        return ((int)id).ToString();
    }
}