using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameFlowProfitState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    DataManager dataManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    NetworkSaveManager saveManager;


    [Inject]
    SDKProtocol.IProtocolBridge sdk;
    public override UniTask End()
    {
        battleManager.StopTimer();
        return default;
    }

    public override void OnAbort()
    {

    }

    public override async UniTask Start()
    {
        GetController().AddPerformanceData(new PModifyShieldData()
        {
            isPlayer = true,
            beforeValue = battleManager.player.shield,
            shieldValue = 0,
        });
        battleManager.player.shield = 0;
        battleManager.StartTimer();
        var ui = uIManager.FindUI<UIBattle>();
        ui.ClearPassive();
        battleManager.player.colors.Clear();

        var p = new PModifyColorData();
        p.SetColorEffectEnum(SkillCostColorEnum.Red, PModifyColorData.PerformanceColorEffectEnum.Depletion);
        p.SetColorEffectEnum(SkillCostColorEnum.Green, PModifyColorData.PerformanceColorEffectEnum.Depletion);
        p.SetColorEffectEnum(SkillCostColorEnum.Blue, PModifyColorData.PerformanceColorEffectEnum.Depletion);
        p.Init(battleManager.player);
        GetController().AddPerformanceData(p);

        var acquisitionItemsList = dataManager.GetCurrentDungeonLeveData().acquisitionItems;

        for (int i = 0; i < acquisitionItemsList.Count; i++)
        {
            var itemsList = acquisitionItemsList[i];
            if (itemsList == null) continue;
            for (int j = 0; j < itemsList.Count; j++)
            {
                await sdk.BattleGainItem(itemsList[j].id, itemsList[j].count);
            }
        }
        ui.SetBlock(false);
        if (!saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().IsDone)
        {

            await GetController().Save();
            GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.ChoiceReward);
        }
        else
        {
            GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.EndGame);
        }
    }

    public override void Update()
    {

    }
}
