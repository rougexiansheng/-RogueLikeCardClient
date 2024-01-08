using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

public class GameFlowPlayerActionState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    UIManager uIManager;
    public async override UniTask End()
    {
        RxEventBus.UnRegister(this);
        GetController().DoPending(() =>
        {
            var ui = uIManager.FindUI<UIBattle>();
            ui.isActiveControl = false;
            ui.ShowSkillItme(false);
            ui.SetBlock(true);
            return default;
        });
        GetController().UpdatePlayerSkillItem();
        if(saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().IsDone == false) 
            GetController().AddPerformanceData(new PUIAnimatonStateData { stateEnum = UIBattle.UIAnimatonStateEnum.YourTurnEnd });
        
        battleManager.StopTimer();
    }

    public override void OnAbort()
    {

    }

    void OncickSkillItem(int idx)
    {
        var ui = uIManager.FindUI<UIBattle>();
        ui.SetBlock(true);
        battleManager.player.skills[idx].isUsed = true;
        var battleData = new BattleData();
        battleData.isSkill = true;
        battleData.selectTarget = battleManager.monsters.Find(m => m.monsterPos == battleManager.selectTargetEnum);
        battleData.originSkill = battleManager.player.skills[idx];
        battleData.skillId = battleData.originSkill.skillId;
        battleData.sender = battleManager.player;
        // 消耗能量
        if (battleManager.CostColor(battleData))
        {
            GetController().UpdatePlayerSkillItem();
            POnAttackData attackData = new POnAttackData();
            attackData.Init(battleManager.player, battleData.originSkill.skillId);
            GetController().AddPerformanceData(attackData);
            skillManager.OnSkill(battleData);
            battleManager.player.lastBattleData = battleData;
            GetController().UpdatePlayerSkillItem();
            GetController().CheckWinAndLose();
        }
        GetController().DoPending(() => 
        {
            ui.SetBlock(false);
            return default;
        });
    }
    void PlayerRoundEnd()
    {
        // 玩家行動結束後階段
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.PlayerActionAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerActionAfter);

        // 清除能量前階段
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.PlayerClearCostColorBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerClearCostColorBefore);
        battleManager.player.colors.Clear();
        // UI表演
        var p = new PModifyColorData();
        p.SetColorEffectEnum(SkillCostColorEnum.Red, PModifyColorData.PerformanceColorEffectEnum.Depletion);
        p.SetColorEffectEnum(SkillCostColorEnum.Green, PModifyColorData.PerformanceColorEffectEnum.Depletion);
        p.SetColorEffectEnum(SkillCostColorEnum.Blue, PModifyColorData.PerformanceColorEffectEnum.Depletion);
        p.Init(battleManager.player);
        GetController().AddPerformanceData(p);
        // 清除能量後階段
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.PlayerClearCostColorAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerClearCostColorAfter);

        // 玩家會合結束前
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.PlayerRoundEndBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerRoundEndBefore);
        // 玩家會合結束後
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.PlayerRoundEndAfter);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerRoundEndAfter);
        // 回合結束
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.RoundEnd);
        if (GetController().CheckWinAndLose())
            return;
        else
            GetController().AddPerformanceData(new PSwitchGameStateData() { state = GameFlowController.GameFlowState.MonsterRound });
    }

    public async override UniTask Start()
    {
        RxEventBus.Register(EventBusEnum.UIBattleEnum.OnClickPlayerRoundEnd, PlayerRoundEnd, this);
        RxEventBus.Register(EventBusEnum.UIBattleEnum.OnClickSkillInfo, OnClickSkillInfo, this);
        RxEventBus.Register<int>(EventBusEnum.UIBattleEnum.OnClickSkillItem, OncickSkillItem, this);
        RxEventBus.Register(EventBusEnum.UIBattleEnum.OnLongPressMonster, OpenMonsterDetail, this);
        RxEventBus.Register<int>(EventBusEnum.UIBattleEnum.OnPressedSkillItem, OnSkillPopInfo, this);
        RxEventBus.Register(EventBusEnum.UIBattleEnum.OnPressUpSkillItem, () =>
        {
            var ui = uIManager.FindUI<UISkillPopupInfoPage>();
            if (ui) uIManager.RemoveUI(ui);
        }, this);
        RxEventBus.Register(EventBusEnum.UIBattleEnum.OnLongPressPlayer, OpenPlayerDetail, this);
        GetController().OnMonsterPassive(battleManager.monsters, PassiveTriggerEnum.PlayerActionBefore);
        passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.PlayerActionBefore);


        GetController().AddPerformanceData(new PCameraMoveData() { isAll = false, monsterPosition = BattleActor.MonsterPositionEnum.None });
        GetController().AddPerformanceData(new PUIAnimatonStateData { stateEnum = UIBattle.UIAnimatonStateEnum.ActionPhase });

        GetController().DoPending(() =>
        {
            var ui = uIManager.FindUI<UIBattle>();
            ui.isActiveControl = true;
            ui.ShowSkillItme(true);
            ui.SetBlock(false);
            return default;
        });
        GetController().UpdatePlayerSkillItem();
        if (battleManager.player.canAttack == false) GetController().DoPending(() =>
        {
            PlayerRoundEnd();
            return default;
        });
        battleManager.StartTimer();
    }

    /// <summary>
    /// 顯示技能說明
    /// </summary>
    /// <param name="skillId"></param>
    async void OnSkillPopInfo(int skillId)
    {
        var ui = await uIManager.OpenUI<UISkillPopupInfoPage>();
        ui.Init(skillId);
        ui.CancelButton.gameObject.SetActive(false);
        ui.eventTrigger.OnPointerDownAsObservable().Subscribe(_ => uIManager.RemoveUI(ui));
    }
    /// <summary>
    /// 開啟自身所有技能
    /// </summary>
    async void OnClickSkillInfo()
    {
        var ui = await uIManager.OpenUI<UISkill>();
        await ui.OpenCheckSkillPage(battleManager.player.skills);
    }
    /// <summary>
    /// 開啟怪物詳細資訊
    /// </summary>
    async void OpenMonsterDetail()
    {
        var monster = battleManager.monsters.Find(m => m.monsterPos == battleManager.selectTargetEnum);
        if (monster != null)
        {
            var ui = await uIManager.OpenUI<UITargetInfo>();
            ui.Init(monster);
        }
    }

    async void OpenPlayerDetail()
    {
        var ui = await uIManager.OpenUI<UITargetInfo>();
        ui.Init(battleManager.player);
    }

    public override void Update()
    {

    }
}
