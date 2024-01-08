using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask<int> BattleLosesItem(int itemId, int count)
    {
        // 扣除道具
        var result = DoReduceItem(itemId, count);

        // 回寫SaveContainer的資料
        var clientSave = new ClientSave(Cmd.update);
        clientSave.Add(ConvertSaveToBattleItemData(itemId));
        var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

        return EndProtocol(result, clientSaveResult);
    }
}