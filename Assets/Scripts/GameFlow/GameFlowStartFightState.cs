using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameFlowStartFightState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    DataManager dataManager;
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    MonsterManager monsterManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    BattleManager battleManager;
    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    public override UniTask Start()
    {
        // UI技能初始化
        var d = new PPlayerSkillDataInit();
        for (int i = 0; i < battleManager.player.currentActorBaseAttribute.currentSkillRange; i++)
        {
            if (i < battleManager.player.skills.Count)
                d.skills.Add(battleManager.player.skills[i]);
        }
        GetController().AddPerformanceData(d);
        GetController().UpdatePlayerSkillItem();
        var doungeonData = dataManager.GetCurrentDungeonLeveData();
        // 怪物登場
        var p = new PMonsterAppearData();
        p.isBoss = doungeonData.nodeEnum == MapNodeEnum.Boss;
        for (int i = 0; i < battleManager.monsters.Count; i++)
        {
            p.positions.Add(battleManager.monsters[i].monsterPos);
        }
        GetController().AddPerformanceData(p);
        GetController().AddPerformanceData(new PUIAnimatonStateData { stateEnum = UIBattle.UIAnimatonStateEnum.UIBattlePhaseBegin });
        // 怪物AI生成
        for (int i = 0; i < battleManager.monsters.Count; i++)
        {
            monsterManager.DoMonsterAI(battleManager.monsters[i]);
        }
        for (int i = 0; i < battleManager.monsters.Count; i++)
        {
            passiveManager.OnActorPassive(battleManager.monsters[i], PassiveTriggerEnum.BeginFightAfter);
        }
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.BeginFightAfter);
        // 相機歸位
        GetController().AddPerformanceData(new PCameraMoveData() { isAll = false, monsterPosition = BattleActor.MonsterPositionEnum.None });
        if (GetController().CheckWinAndLose())
            return default;
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.PlayerRound);
        return default;
    }

    public override void Update()
    {

    }
}
