using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask<bool> BattleGetMonsterAcquistion()
    {
        var result = UpdateMonsterAcquistionList();

        if (result)
        {
            // 回寫SaveContainer的資料
            var clientSave = new ClientSave(Cmd.update);
            clientSave.Add(ConvertSaveToBattleDungeonData());
            var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

            return EndProtocol(result, clientSaveResult);
        }

        return EndProtocol(false);

    }

    public UniTask<bool> BattleGetEventAcquistion()
    {
        var result = UpdateEvetAcquisitionList();

        if (result)
        {
            // 回寫SaveContainer的資料
            var clientSave = new ClientSave(Cmd.update);
            clientSave.Add(ConvertSaveToBattleDungeonData());
            var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

            return EndProtocol(result, clientSaveResult);
        }

        return EndProtocol(false);

    }

}