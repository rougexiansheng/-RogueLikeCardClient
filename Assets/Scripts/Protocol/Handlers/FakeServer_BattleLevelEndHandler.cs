using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask BattleLevelEnd(bool isWin, List<ActorSkill> skills = null)
    {
        fakeServerData.player.dungeonCache.isWin = isWin;
        if (!isWin) fakeServerData.player.dungeonCache.isDone = true;
        if (skills != null)
        {
            //Debug.Log($"BattleLevelEnd: {JsonConvert.SerializeObject(skills)}");
            fakeServerData.player.actorCache.actorSkills.Clear();
            for (int i = 0; i < skills.Count; i++)
            {
                ActorSkill skill = skills[i];
                if (skill.originIndex == -1) continue;
                fakeServerData.player.actorCache.actorSkills.Add(i, skill);
            }
        }

        // 回寫SaveContainer的資料
        var clientSave = new ClientSave(Cmd.update);
        clientSave.Add(ConvertSaveToBattleDungeonData());
        clientSave.Add(ConvertSaveToBattleSkillData());
        var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

        return EndProtocol(true, clientSaveResult);
    }
}