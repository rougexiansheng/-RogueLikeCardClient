using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask<bool> BattleGiveUp()
    {
        //fakeServerData.player = new PlayerServerData();
        ResetBattleCache();

        // 回寫SaveContainer的資料
        var clientSave = new ClientSave(Cmd.update);
        clientSave.Add(ConvertSaveToPlayerData());
        var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

        return EndProtocol(true, clientSaveResult);
    }

}