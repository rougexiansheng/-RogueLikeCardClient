using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask<NextDungeonData> BattleLevelRewards(List<string> logs)
    {
        var d = new NextDungeonData();
        var ls = GetProtoDungeonData(1);
        if (ls != null && ls.Count > 0)
        {
            fakeServerData.player.dungeonCache.lastCache.AddRange(ls);
            d.dungeonDatas = ls[0];
        }
        var idx = fakeServerData.player.dungeonCache.lastCache.FindIndex(d => d.Find(dd => fakeServerData.player.dungeonCache.fightDungeonId == dd.dungeonId) != null);
        if (idx >= fakeServerData.player.dungeonCache.lastCache.Count - 1)
        {
            fakeServerData.player.dungeonCache.isDone = true;
        }
        d.isDone = fakeServerData.player.dungeonCache.isDone;

        // 原本寫在GameFlowChoiceRewardState
        //if (d.dungeonDatas != null && d.dungeonDatas.Count > 0)
        //    fakeServerData.player.dungeonData.lastCache.Add(d.dungeonDatas);

        // 回寫SaveContainer的資料
        var clientSave = new ClientSave(Cmd.update);
        clientSave.Add(ConvertSaveToBattleDungeonData());
        var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

        return EndProtocol(JsonClone(d), clientSaveResult);
    }

}