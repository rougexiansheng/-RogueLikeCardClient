using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameFlowPlayerRoundState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    UIManager uIManager;
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

    public override async UniTask Start()
    {
        GetController().AddPerformanceData(new PUIAnimatonStateData { stateEnum = UIBattle.UIAnimatonStateEnum.YourTurn });
        // 回合開始
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.RoundStart);

        // 玩家回合開始
        battleManager.player.canAttack = true;
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.PlayerRoundStartBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerRoundStartBefore);
        // 重製玩家已使用過技能
        for (int i = 0; i < battleManager.player.skills.Count; i++)
        {
            battleManager.player.skills[i].isUsed = false;
        }
        var pShield = new PModifyShieldData() { isPlayer = true, beforeValue = battleManager.player.shield };
        battleManager.player.shield = 0;
        pShield.shieldValue = battleManager.player.shield;
        GetController().AddPerformanceData(pShield);
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.PlayerRoundStartAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerRoundStartAfter);



        // 取得能能量階段
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.GetColorCostBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.GetColorCostBefore);
        GetColor();

        if (GetController().CheckWinAndLose()) return;
        // 取的能量階段後
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.GetColorCostAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.GetColorCostAfter);

        if (GetController().CheckWinAndLose()) return;

        //技能推進階段 前
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MoveSkillStateBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MoveSkillStateBefore);

        //技能推進 前
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MoveSkillBySkillBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MoveSkillBySkillBefore);
        GetController().DoPending(() =>
        {
            var ui = uIManager.FindUI<UIBattle>();
            ui.ShowSkillItme(true);
            return default;
        });
        //技能推進
        passiveManager.GetCurrentActorAttribute(battleManager.player);
        battleManager.PlayerPushSkills(battleManager.player.currentActorBaseAttribute.currentMove, battleManager.player);
        //技能推進 後
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MoveSkillBySkillAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MoveSkillBySkillAfter);
        //技能推進階段 後
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.MoveSkillStateAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.MoveSkillStateAfter);

        if (GetController().CheckWinAndLose()) return;
        GetController().AddPerformanceData(new PSwitchGameStateData() { state = GameFlowController.GameFlowState.PlayerAction });
        return;
    }

    /// <summary>
    /// 玩家取得能量球
    /// </summary>
    void GetColor()
    {
        passiveManager.GetCurrentActorAttribute(battleManager.player);
        var battleData = new BattleData();
        battleData.isSkill = false;
        battleData.currentColorCount.RestValue(battleManager.player.currentActorBaseAttribute.currentColorCount);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerGetCostColorBefore, battleData);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.OnGetColor, battleData);
        battleManager.GetColors(battleManager.player, battleData.currentColorCount.GetValue());
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerGetCostColorAfter, battleData);
    }

    public override void Update()
    {

    }
}
