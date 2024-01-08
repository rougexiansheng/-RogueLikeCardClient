using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask<bool> BattleReplaceSkill(int index, ActorSkill skill)
    {
        // 添加英雄
        var result = DoReplaceBattleSkill(index, skill);

        // 回寫SaveContainer的資料
        if (result)
        {
            var clientSave = new ClientSave(Cmd.update);
            clientSave.Add(ConvertSaveToBattleSkillData());
            var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };
            return EndProtocol(result, clientSaveResult);
        }

        return EndProtocol(result);
    }
}