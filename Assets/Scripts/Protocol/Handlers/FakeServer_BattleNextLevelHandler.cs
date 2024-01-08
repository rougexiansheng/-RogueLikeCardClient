using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask<int> BattleNextLevel(int dungeonId)
    {
        var d = new NextDungeonData();
        fakeServerData.player.dungeonCache.fightDungeonId = dungeonId;
        d.nextDungeonId = fakeServerData.player.dungeonCache.fightDungeonId;

        // 回寫SaveContainer的資料
        var clientSave = new ClientSave(Cmd.update);
        clientSave.Add(ConvertSaveToBattleDungeonData());
        var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

        return EndProtocol(fakeServerData.player.dungeonCache.fightDungeonId, clientSaveResult);
    }

}