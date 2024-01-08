using SDKProtocol;
using System.Collections.Generic;
using Zenject;
using static NetworkSaveBattleHeroAttrContainer;

public class DataManager : IInitializable, ITickable
{
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    NetworkSaveManager saveManager;

    public void Initialize()
    {

    }

    public void Tick()
    {

    }

    public int GetCurrentDungeonLeveIndex()
    {
        var fightDungeonId = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().FightDungeonId;
        var lastCache = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().LastCache;
        var idx = lastCache.FindIndex(d => d.Find(dd => fightDungeonId == dd.dungeonId) != null);
        return idx;
    }

    public SDKProtocol.DungeonLevelData GetCurrentDungeonLeveData()
    {
        var lastCache = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().LastCache;
        var fightDungeonId = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().FightDungeonId;
        foreach (var levelLs in lastCache)
        {
            foreach (var level in levelLs)
            {
                if (level.dungeonId == fightDungeonId) return level;
            }
        }
        return null;
    }

    public BattleActor GainPlayerDataFromProfessionId(ActorProfessionEnum e, int buildingSet = 0)
    {
        var actor = new BattleActor();
        var define = dataTableManager.GetProfessionDataDefine(e);
        actor.isPlayer = true;
        actor.isDead = false;
        actor.currentHp = define.baseHp;
        actor.maxHp = define.baseHp;
        actor.skillMoveCount = 1;
        actor.skillRange = define.skillRange;
        actor.getColorCount = define.colorCount;
        // 已經解鎖被動
        actor.passives = new List<ActorPassive>();
        // 調配的技能組
        actor.skills = saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetSortedActorSkillList();
        return actor;
    }

    /// <summary>
    /// 用來檢查是否已經取得過技能(寶箱/商店)
    /// </summary>
    public bool IsGotSkill(int skillId)
    {
        return saveManager.GetContainer<NetworkSaveBattleHeroAttrContainer>().Exists(AttrType.Skill, skillId);
    }
}
