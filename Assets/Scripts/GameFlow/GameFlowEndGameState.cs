using Cysharp.Threading.Tasks;
using SDKProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameFlowEndGameState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    IProtocolBridge sdk;
    [Inject]
    UIManager uIManager;
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    FakeServer fakeServer;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    BattleManager battleManager;
    public override UniTask End()
    {
        RxEventBus.UnRegister(this);
        uIManager.RemoveUI<UIBattle>();
        environmentManager.RestScene();
        for (int i = 0; i < environmentManager.monsterPoints.Length; i++)
        {
            environmentManager.monsterPoints[i].Clear();
        }
        return default;
    }

    public override void OnAbort()
    {

    }

    public override async UniTask Start()
    {
        // 暫時直接離開 TODO
        RxEventBus.Register(EventBusEnum.UIBattleEnum.OnClickReview, OnClickReview, this);
        RxEventBus.Register(EventBusEnum.UIBattleEnum.OnClickRevive, () => GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.Empty), this);
        await sdk.BattleGiveUp();
        var ui = uIManager.FindUI<UIBattle>();
        Time.timeScale = 1;
        if (saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().IsWin)
            GetController().AddPerformanceData(new PUIAnimatonStateData { stateEnum = UIBattle.UIAnimatonStateEnum.UIGameWin, autoClose = false });
        else
            GetController().AddPerformanceData(new PUIAnimatonStateData { stateEnum = UIBattle.UIAnimatonStateEnum.UIBattlePhaseGameOver, autoClose = false });
        // TODO 遊戲結束後 流程處理
    }

    public override void Update()
    {

    }

    private async void OnClickReview()
    {
        var dungeonId = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().FightDungeonId;
        var level = dataTableManager.GetDungeonDataDefine(dungeonId).mapLayer;
        var spendTime = (int)(battleManager.GetElapsedTime() * 1000);
        var playerName = "";
        // CG UI
        if (saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().IsWin == false)
        {
            var cgUI = await uIManager.OpenUI<UICG>();
            cgUI.Init();
            await cgUI.PlayerCGAnimation();
        }
        // open remane box ui
        var renameUi = await uIManager.OpenUI<UIRenameBox>();
        await renameUi.Init(onConfirm: (name) =>
        {
            playerName = name;
            Debug.Log($"name: {name}");
        }, onCancel: (name) =>
        {
            playerName = name;
            Debug.Log($"name: {name}");
        });
        await UniTask.WaitUntil(() => { return renameUi.IsDone; });
        uIManager.RemoveUI(renameUi);

        // build battle end request data
        var battleEndData = new BattleEndData()
        {
            stageProgress = level,
            spendTime = spendTime,
            profession = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().SelectProfession,
            playerName = playerName
        };

        // client battle end -> server -> response result
        var battleResultData = await fakeServer.BattleResult(battleEndData);

        // open ranking ui
        var rankingUi = await uIManager.OpenUI<UIRanking>();
        await rankingUi.Init(battleResultData.ranking.fullRankings, battleResultData.ranking.lastRanking, () =>
        {
            Debug.Log("UIRanking OnClose");
        });
        await UniTask.WaitUntil(() => { return rankingUi.IsDone; });
        await uIManager.FadeIn(0.2f);
        uIManager.RemoveUI(rankingUi);
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.Empty);
    }
}
