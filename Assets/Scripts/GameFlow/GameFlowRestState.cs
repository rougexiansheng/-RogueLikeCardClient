using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameFlowRestState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    UIManager uIManager;

    [Inject]
    BattleManager battleManager;

    [Inject]
    GameFlowController gameFlow;
    [Inject]
    DiContainer diContainer;


    NumericalManager numericalManager = new NumericalManager();

    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    public async override UniTask Start()
    {
        diContainer.Inject(numericalManager);
        await OpenUIAsync();
    }

    public override void Update()
    {

    }

    private async UniTask OpenUIAsync()
    {
        var restUi = await uIManager.OpenUI<UIRest>();
        if (restUi != null)
        {
            await restUi.Init(0, 10);
            restUi.ShowEffect();
            await Recover();

            await restUi.RecoverFinish();
        }
        else
        {
            Debug.LogWarning("UIRest 開啟失敗");
            await Recover();
        }

        RecoverEnd();

        async UniTask Recover()
        {
            var battle = uIManager.FindUI<UIBattle>();
            if (battle != null)
            {
                int resetValue = numericalManager.GetRestHPValue(battleManager.player.maxHp);
                var value = battleManager.OnHeal(battleManager.player, resetValue);
                var p = new POnHealData();
                p.Init(battleManager.player, value);
                gameFlow.AddPerformanceData(p);

                //battleManager.RestStateHeal();
                await battle.OnStartRecover();
            }
        }
    }

    /// <summary>
    /// 恢復結束，執行下一階段 (Flow)
    /// </summary>
    private void RecoverEnd()
    {
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.NextLevel);
    }
}
