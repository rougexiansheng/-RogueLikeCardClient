using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    /// <summary>
    /// 目前只有設定hp的需求，未來根據需求再擴充
    /// </summary>
    public UniTask<ActorCacheData> BattleGainHeroAttr(int hp)
    {
        fakeServerData.player.actorCache.currentHp = hp;

        // 回寫SaveContainer的資料
        var clientSave = new ClientSave(Cmd.update);
        clientSave.Add(ConvertSaveToBattleHeroAttrData(NetworkSaveBattleHeroAttrContainer.AttrType.Hp));
        var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

        return EndProtocol(fakeServerData.player.actorCache, clientSaveResult);
    }
}