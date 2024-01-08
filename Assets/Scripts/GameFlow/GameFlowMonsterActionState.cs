using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameFlowMonsterActionState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    MonsterManager monsterManager;
    public override UniTask End()
    {
        //GetController().AddPerformanceData(new PCameraMoveData() { isAll = false, monsterPosition = battleManager.selectTargetEnum });
        return default;
    }

    public override void OnAbort()
    {

    }

    public override UniTask Start()
    {
        // 怪物行動前
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MonsterActionBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MonsterActionBefore);

        for (int i = 0; i < battleManager.monsters.Count; i++)
        {
            var monster = battleManager.monsters[i];
            // 怪物是否死亡
            if (monster.isDead == false) 
            {
                // 怪物可否攻擊
                if (monster.canAttack)
                {
                    var skillData = new ActorSkill();
                    skillData.skillId = monster.monsterNextSkill;
                    skillData.originIndex = -1;
                    skillData.isUsed = true;

                    var battleData = new BattleData();
                    battleData.isSkill = true;
                    battleData.selectTarget = battleManager.player;
                    battleData.originSkill = skillData;
                    battleData.skillId = battleData.originSkill.skillId;
                    battleData.sender = monster;
                    POnAttackData attackData = new POnAttackData();
                    attackData.Init(monster, battleData.originSkill.skillId);
                    GetController().AddPerformanceData(attackData);
                    skillManager.OnSkill(battleData);
                    monster.lastBattleData = battleData;
                    monsterManager.DoMonsterAI(monster);
                }
                else
                {
                    GetController().AddPerformanceData(new PCameraMoveData() {monsterPosition= monster.monsterPos});
                    GetController().DoPending(() => UniTask.Delay(400));
                }
                // 檢查輸贏
                if (GetController().CheckWinAndLose()) return default;
            }
        }
        // 怪物行動後
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MonsterActionAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MonsterActionAfter);

        // 怪物回合結束 前
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MonsterRoundEndBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MonsterRoundEndBefore);

        // 怪物回合結束 後
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MonsterRoundEndAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MonsterRoundEndAfter);
        // 回合結束 後
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.RoundEnd);
        if (GetController().CheckWinAndLose()) return default;
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.PlayerRound);
        return default;
    }

    public override void Update()
    {

    }
}
