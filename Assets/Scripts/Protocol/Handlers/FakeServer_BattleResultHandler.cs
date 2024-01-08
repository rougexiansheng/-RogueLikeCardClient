using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using Spine;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    /// <summary>
    /// 戰鬥結算，處理完畢後會回傳排行榜資料
    /// </summary>
    public UniTask<BattleResultData> BattleResult(BattleEndData battleEndData)
    {
        fakeServerData.player.dungeonCache.isWin = battleEndData.isWin;
        if (!battleEndData.isWin) fakeServerData.player.dungeonCache.isDone = true;

        // TODO: 處理戰鬥結算

        // ...

        // 將最新一筆傳進來的戰鬥結算資料，做成排行榜物件
        var newItem = new DungeonRankingDataItem()
        {
            ranking = 0,
            stageProgress = battleEndData.stageProgress,
            spendTime = battleEndData.spendTime,
            profession = battleEndData.profession,
            playerName = battleEndData.playerName
        };

        // 將新排行榜物件加入當前排行榜，並進行排序
        var dungeonRankings = fakeServerData.ranking.dungeonData.fullRankings;
        dungeonRankings.Add(newItem);
        dungeonRankings = dungeonRankings.OrderByDescending(x => x.stageProgress).ThenBy(x => x.spendTime).ToList();

        // 更新排名順序
        for (int i = 0; i < dungeonRankings.Count; i++)
        {
            var rankData = dungeonRankings[i];
            rankData.ranking = i + 1;
        }

        // 移除多餘的排行榜名次
        var currentCount = dungeonRankings.Count;
        if (currentCount > MAX_RANKING_SIZE)
        {
            dungeonRankings.RemoveRange(MAX_RANKING_SIZE, currentCount - MAX_RANKING_SIZE);
        }

        // 伺服器的資料
        fakeServerData.ranking.dungeonData.fullRankings = dungeonRankings;
        fakeServerData.ranking.dungeonData.lastRanking = newItem;

        // 直接回傳的資料
        var result = new BattleResultData();
        result.ranking = JsonClone(fakeServerData.ranking.dungeonData);

        // 回寫SaveContainer的資料
        var clientSave = new ClientSave(Cmd.update);
        clientSave.Add(ConvertSaveToRankData(NetworkSaveRankContainer.RankType.Dungeon));
        var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

        // 回傳伺服器處理好的排行榜資料
        return EndProtocol(result, clientSaveResult);
    }
}