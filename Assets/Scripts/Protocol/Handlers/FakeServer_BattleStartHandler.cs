using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using System.Diagnostics;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask<PlayerServerData> BattleStart(SelectDungeonData selectDungeonData, SelectProfessionData selectProfessionData)
    {
        //if (!fakeServerData.player.dungeonCache.isDone)
        //{
        //    //throw new RespondError();
        //    fakeServerData.player.dungeonCache.Clear();
        //}

        ResetBattleCache();

        fakeServerData.player.dungeonCache.isDone = false;
        fakeServerData.player.dungeonCache.dungeonGroupId = selectDungeonData.dungeonGroupId;
        fakeServerData.player.dungeonCache.professionEnum = (ActorProfessionEnum)selectDungeonData.professionId;
        var ls = GetProtoDungeonData(4);
        fakeServerData.player.dungeonCache.lastCache.AddRange(ls);
        fakeServerData.player.dungeonCache.fightDungeonId = ls[0][0].dungeonId;
        fakeServerData.player.actorCache.currentHp = selectDungeonData.currectHp;

        // 設定技能組
        DoSetActorSkill((ActorProfessionEnum)selectDungeonData.professionId, selectProfessionData.SkillGroupsIndex);

        // 回寫SaveContainer的資料
        var clientSaveUpdate = new ClientSave(Cmd.update);
        clientSaveUpdate.Add(ConvertSaveToPlayerData());

        // 透過init讓client重置
        var clientSaveInit = new ClientSave(Cmd.init);
        clientSaveInit.Add(ConvertSaveToBattleDungeonData());
        clientSaveInit.Add(ConvertSaveToBattleHeroAttrData());
        clientSaveInit.Add(ConvertSaveToBattleSkillData());

        var clientSaveResult = new List<JsonObject>() { clientSaveUpdate.ToJsonObject(), clientSaveInit.ToJsonObject() };

        return EndProtocol(JsonClone(fakeServerData.player), clientSaveResult);
    }
}