using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameFlowMonsterRoundState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
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
        // 回合開始
        for (int i = 0; i < battleManager.monsters.Count; i++)
        {
            // 回合開始前怪物一定可以攻擊
            var monster = battleManager.monsters[i];
            monster.canAttack = true;
            passiveManager.OnActorPassive(battleManager.monsters[i], PassiveTriggerEnum.RoundStart);
        }

        
        // 怪物回合開始 並清除護盾
        for (int i = 0; i < battleManager.monsters.Count; i++)
        {
            passiveManager.OnActorPassive(battleManager.monsters[i], PassiveTriggerEnum.MonsterRoundStartBefore);
            var monster = battleManager.monsters[i];
            var pShield = new PModifyShieldData() { isPlayer = false, monsterPosition = monster.monsterPos, beforeValue = monster.shield };
            monster.shield = 0;
            pShield.shieldValue = monster.shield;
            GetController().AddPerformanceData(pShield);
        }
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MonsterRoundStartBefore);

        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MonsterRoundStartAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MonsterRoundStartAfter);

        if (GetController().CheckWinAndLose()) return default;
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.MonsterAction);
        return default;
    }

    public override void Update()
    {

    }
}
