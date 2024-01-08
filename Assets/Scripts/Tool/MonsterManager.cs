using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MonsterManager : IInitializable
{
    [Inject]
    GameFlowController gameFlow;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    DataTableManager dataTableManager;

    Dictionary<AIConsiderationEnum, Func<BattleActor, AIConsideration, bool>> aiFuncDic = new Dictionary<AIConsiderationEnum, Func<BattleActor, AIConsideration, bool>>();

    public void Initialize()
    {
        aiFuncDic.Add(AIConsiderationEnum.None, (a, b) => true);
        aiFuncDic.Add(AIConsiderationEnum.HpPercentageLess, HpPercentageLess);
        aiFuncDic.Add(AIConsiderationEnum.EveryRound, EveryRound);
    }

    

    /// <summary>
    ///產生怪物 並 加上環境被動
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BattleActor GainMonsterActor(int id)
    {
        var actor = new BattleActor();
        var define = dataTableManager.GetMonsterDefine(id);
        actor.maxHp = define.maxHp;
        actor.currentHp = actor.maxHp;
        actor.actorName = define.name;
        actor.attackPower = define.atk;
        actor.defense = define.def;
        actor.monsterId = id;
        var passiveLs = new List<ActorPassive>();
        for (int i = 0; i < define.passives.Count; i++)
        {
            var p = passiveManager.GainActorPassive(define.passives[i], 1);
            p.owner = actor;
            p.sender = actor;
            passiveLs.Add(p);
        }
        actor.passives = passiveLs;
        actor.behaviorData = new MonsterAIbehaviorData() { aiId = define.aiId };
        actor.isDead = false;
        actor.isPlayer = false;
        var doungeonData = dataManager.GetCurrentDungeonLeveData();
        for (int i = 0; i < doungeonData.passives.Count; i++)
        {
            var p = passiveManager.GainActorPassive(doungeonData.passives[i], 1);
            p.owner = actor;
            p.sender = actor;
            actor.passives.Add(p);
        }
        return actor;
    }  

    public void DoMonsterAI(BattleActor monster)
    {
        if (monster.isPlayer || monster.isDead)
        {
            Debug.Log($"無法執行怪物AI name:{monster.actorName} Player:{monster.isPlayer} Dead:{monster.isDead}");
            return;
        }
        monster.behaviorData.roundCount++;
        var define = dataTableManager.GetMonsterAIDefine(monster.behaviorData.aiId);
        for (int i = 0; i < define.aIConsiderations.Count; i++)
        {
            var data = define.aIConsiderations[i];
            if (aiFuncDic[data.consideration](monster, data))
            {
                monster.monsterNextSkill = data.skillId;
                break;
            }
        }
        var p = new PMonsterNextSkillData();
        p.positionEnum = monster.monsterPos;
        p.skillId = monster.monsterNextSkill;
        gameFlow.AddPerformanceData(p);
    }

    #region
    bool EveryRound(BattleActor monster, AIConsideration data)
    {
        return monster.behaviorData.roundCount % data.arg == 0;
    }
    bool HpPercentageLess(BattleActor monster, AIConsideration data)
    {
        passiveManager.GetCurrentActorAttribute(monster);
        var maxHp = monster.currentActorBaseAttribute.maxHp.GetValue();
        var p = monster.currentHp * 100f / maxHp;
        return p < data.arg;
    }
    #endregion
}
