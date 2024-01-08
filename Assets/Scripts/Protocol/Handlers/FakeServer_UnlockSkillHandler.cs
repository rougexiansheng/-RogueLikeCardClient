using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask<bool> UnlockSkill(ActorProfessionEnum professionEnum, int skillID)
    {
        // Unlock Skill
        var result = DoUnlockSkill(professionEnum, skillID);

        // 回寫SaveContainer的資料
        if (result)
        {
            var clientSave = new ClientSave(Cmd.update);
            clientSave.Add(ConvertSaveToProfessionData(professionEnum));
            var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };
            return EndProtocol(result, clientSaveResult);
        }

        return EndProtocol(result);
    }
}
